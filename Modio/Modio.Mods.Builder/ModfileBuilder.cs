using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Errors;

namespace Modio.Mods.Builder;

public class ModfileBuilder
{
	public enum Platform
	{
		Windows,
		Mac,
		Linux,
		Android,
		IOS,
		XboxOne,
		XboxSeriesX,
		PlayStation4,
		PlayStation5,
		Switch,
		Oculus
	}

	private readonly ModBuilder _parentModBuilder;

	public string FilePath { get; private set; }

	public string Version { get; private set; }

	public string ChangeLog { get; private set; }

	public string MetadataBlob { get; private set; }

	public Platform[] Platforms { get; private set; }

	private ModId ParentId => _parentModBuilder.EditTarget.Id;

	internal ModfileBuilder(ModBuilder parent)
	{
		_parentModBuilder = parent;
	}

	public ModfileBuilder SetSourceDirectoryPath(string filePath)
	{
		FilePath = filePath;
		return this;
	}

	public ModfileBuilder SetVersion(string version)
	{
		Version = version;
		return this;
	}

	public ModfileBuilder SetChangelog(string changelog)
	{
		ChangeLog = changelog;
		return this;
	}

	public ModfileBuilder SetMetadataBlob(string metadataBlob)
	{
		MetadataBlob = metadataBlob;
		return this;
	}

	public ModfileBuilder SetPlatform(Platform platform)
	{
		return SetPlatforms(new Platform[1] { platform });
	}

	public ModfileBuilder SetPlatforms(ICollection<Platform> platforms)
	{
		Platforms = platforms.ToArray();
		return this;
	}

	public ModfileBuilder AppendPlatform(Platform platform)
	{
		return AppendPlatforms(new Platform[1] { platform });
	}

	public ModfileBuilder AppendPlatforms(ICollection<Platform> platforms)
	{
		Platforms = Platforms.Concat(platforms).ToArray();
		return this;
	}

	public ModBuilder FinishModfile()
	{
		return _parentModBuilder;
	}

	internal async Task<Error> PublishModfile()
	{
		if (_parentModBuilder.EditTarget == null)
		{
			ModioLog.Error?.Log($"Unable to publish modfile, no {typeof(ModId)} found to upload to. How did you get here?");
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		if (!Directory.Exists(FilePath))
		{
			ModioLog.Error?.Log("Unable to publish modfile, directory " + FilePath + " not found");
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		string installPath = ModioClient.DataStorage.GetInstallPath(ParentId, 0L);
		Directory.CreateDirectory(installPath);
		string temporaryFilePath = Path.Combine(installPath, "upload.zip");
		Error error;
		await using (Stream outputTo = File.Open(temporaryFilePath, FileMode.Create))
		{
			error = await ModioClient.DataStorage.CompressToZip(FilePath, outputTo);
			if ((bool)error)
			{
				return error;
			}
		}
		if (new FileInfo(temporaryFilePath).Length > 104857600)
		{
			await using Stream outputTo = File.Open(temporaryFilePath, FileMode.Open);
			error = await AddMultipartModfile(outputTo);
		}
		else
		{
			ModioAPIFileParameter modioAPIFileParameter = default(ModioAPIFileParameter);
			modioAPIFileParameter.Name = "upload.zip";
			modioAPIFileParameter.Path = temporaryFilePath;
			ModioAPIFileParameter filedata = modioAPIFileParameter;
			string[] platforms = Platforms.Select(GetPlatformHeader).ToArray();
			error = (await ModioAPI.Files.AddModfile(body: new AddModfileRequest(filedata, Version, ChangeLog, MetadataBlob, platforms, null), modId: ParentId)).Item1;
		}
		return error;
	}

	private async Task<Error> AddMultipartModfile(Stream readStream)
	{
		string nonce = $"{ParentId}_{readStream.Length}_{DateTime.UtcNow.Ticks}";
		(Error error, MultipartUploadObject? multipartUploadObject) session = await ModioAPI.FilesMultipartUploads.CreateMultipartUploadSession(ParentId, new CreateMultipartUploadSessionRequest("upload.zip", nonce));
		if ((bool)session.error)
		{
			return session.error;
		}
		if (!session.multipartUploadObject.HasValue)
		{
			return new Error(ErrorCode.NO_DATA_AVAILABLE);
		}
		string uploadId = session.multipartUploadObject.Value.UploadId;
		Error error = await AddAllMulipartUploadParts(uploadId, 0, readStream);
		if ((bool)error)
		{
			return error;
		}
		if ((bool)session.error)
		{
			return session.error;
		}
		(Error, MultipartUploadObject?) tuple = await ModioAPI.FilesMultipartUploads.CompleteMultipartUploadSession(uploadId, ParentId);
		if ((bool)tuple.Item1)
		{
			return tuple.Item1;
		}
		string[] platforms = Platforms.Select(GetPlatformHeader).ToArray();
		await ModioAPI.Files.AddModfile(ParentId, new AddModfileRequest(ModioAPIFileParameter.None, Version, ChangeLog, MetadataBlob, platforms, uploadId));
		return Error.None;
	}

	private async Task<Error> AddAllMulipartUploadParts(string uploadId, int partCount, Stream readStream)
	{
		int chunkSize = 52428800;
		int endByte = chunkSize - 1;
		int startByte = 52428800 * partCount;
		if (readStream.CanSeek)
		{
			readStream.Position = startByte;
		}
		byte[] buffer = new byte[chunkSize];
		while (await readStream.ReadAsync(buffer, 0, chunkSize) > 0)
		{
			if (endByte >= readStream.Length)
			{
				endByte = (int)readStream.Length - 1;
			}
			byte[] array;
			if (endByte + 1 - startByte < chunkSize)
			{
				array = new byte[endByte + 1 - startByte];
				Array.Copy(buffer, array, endByte + 1 - startByte);
			}
			else
			{
				array = buffer;
			}
			(Error, MultipartUploadPartObject?) tuple = await ModioAPI.FilesMultipartUploads.AddMultipartUploadPart(uploadId, ParentId, $"bytes {startByte}-{endByte}/{readStream.Length}", array);
			if ((bool)tuple.Item1)
			{
				return tuple.Item1;
			}
			startByte = endByte + 1;
			endByte = startByte + chunkSize - 1;
		}
		return Error.None;
	}

	private async Task<(Error, ModfileObject?)> RetryAddMultipartModfile(string uploadId, string version, string changelog, string metadataBlob, string[] platforms, Stream readStream)
	{
		ModioAPI.FilesMultipartUploads.GetMultipartUploadPartsFilter filter = new ModioAPI.FilesMultipartUploads.GetMultipartUploadPartsFilter(0, 410, uploadId);
		(Error, Pagination<MultipartUploadPartObject[]>?) tuple = await ModioAPI.FilesMultipartUploads.GetMultipartUploadParts(ParentId, filter);
		if ((bool)tuple.Item1)
		{
			return (tuple.Item1, null);
		}
		if (!tuple.Item2.HasValue)
		{
			return (new Error(ErrorCode.NO_DATA_AVAILABLE), null);
		}
		int partCount = tuple.Item2.Value.Data.Length;
		Error error = await AddAllMulipartUploadParts(uploadId, partCount, readStream);
		if ((bool)error)
		{
			return (error, null);
		}
		(Error, MultipartUploadObject?) tuple2 = await ModioAPI.FilesMultipartUploads.CompleteMultipartUploadSession(uploadId, ParentId);
		if ((bool)tuple2.Item1)
		{
			return (tuple2.Item1, null);
		}
		(Error, ModfileObject?) tuple3 = await ModioAPI.Files.AddModfile(ParentId, new AddModfileRequest(ModioAPIFileParameter.None, version, changelog, metadataBlob, platforms, uploadId));
		return (Error.None, tuple3.Item2);
	}

	private static string GetPlatformHeader(Platform platform)
	{
		return platform switch
		{
			Platform.Windows => "windows", 
			Platform.Mac => "mac", 
			Platform.Linux => "linux", 
			Platform.Android => "android", 
			Platform.IOS => "ios", 
			Platform.XboxOne => "xboxone", 
			Platform.XboxSeriesX => "xboxseriesx", 
			Platform.PlayStation4 => "ps4", 
			Platform.PlayStation5 => "ps5", 
			Platform.Switch => "switch", 
			Platform.Oculus => "oculus", 
			_ => string.Empty, 
		};
	}
}
