using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Modio.Errors;
using Modio.Mods;
using Modio.Users;
using Newtonsoft.Json;

namespace Modio.FileIO;

public class BaseDataStorage : IModioDataStorage
{
	protected bool Initialized;

	protected bool IsShuttingDown;

	protected long GameId;

	protected string Root;

	protected string UserRoot;

	protected int OngoingTaskCount;

	protected CancellationTokenSource ShutdownTokenSource;

	protected CancellationToken ShutdownToken;

	public virtual Task<Error> Init()
	{
		SetupRootPaths();
		OngoingTaskCount = 0;
		ShutdownTokenSource = new CancellationTokenSource();
		ShutdownToken = ShutdownTokenSource.Token;
		IsShuttingDown = false;
		Initialized = true;
		MigrateLegacyModInstalls();
		return Task.FromResult(Error.None);
	}

	protected virtual void SetupRootPaths()
	{
		GameId = ModioServices.Resolve<ModioSettings>().GameId;
		Root = string.Format("{0}{1}", Path.Combine(ModioServices.Resolve<IModioRootPathProvider>().Path, "mod.io", GameId.ToString()), Path.DirectorySeparatorChar);
		UserRoot = string.Format("{0}{1}", Path.Combine(ModioServices.Resolve<IModioRootPathProvider>().UserPath, "mod.io", GameId.ToString()), Path.DirectorySeparatorChar);
	}

	public virtual async Task Shutdown()
	{
		Stopwatch shutdownTimer = new Stopwatch();
		shutdownTimer.Start();
		ModioLog.Verbose?.Log($"Shutting down {typeof(BaseDataStorage)}");
		IsShuttingDown = true;
		ShutdownTokenSource?.Cancel();
		while (OngoingTaskCount > 0)
		{
			await Task.Yield();
		}
		shutdownTimer.Stop();
		ModioLog.Verbose?.Log($"{typeof(BaseDataStorage)} took {shutdownTimer.Elapsed.Milliseconds}ms to shut down");
	}

	[ModioDebugMenu]
	public static void DebugDeleteAllGameData()
	{
		ModioClient.DataStorage.DeleteAllGameData();
		User.LogOut();
	}

	public Task<Error> DeleteAllGameData()
	{
		if (!Initialized)
		{
			return Task.FromResult(new Error(ErrorCode.NOT_INITIALIZED));
		}
		Error error = DeleteDirectoryAndContents(Root);
		if ((bool)error)
		{
			return Task.FromResult(error);
		}
		error = DeleteDirectoryAndContents(UserRoot);
		if ((bool)error)
		{
			return Task.FromResult(error);
		}
		return Task.FromResult(Error.None);
	}

	protected virtual async Task<(Error error, T result)> ReadData<T>(string filePath)
	{
		var (error, value) = await ReadTextFile(filePath);
		if (!error && string.IsNullOrWhiteSpace(value))
		{
			error = new Error(ErrorCode.FILE_NOT_FOUND, "file exists, but is empty/null");
		}
		if ((bool)error)
		{
			ModioLog.Message?.Log("Error reading the " + typeof(T).Name + " file at path " + filePath + ": " + error.GetMessage());
			return (error: error, result: default(T));
		}
		try
		{
			T item = JsonConvert.DeserializeObject<T>(value);
			return (error: error, result: item);
		}
		catch (Exception exception)
		{
			return (error: new ErrorException(exception), result: default(T));
		}
	}

	protected virtual async Task<Error> WriteData<T>(T data, string filePath)
	{
		if (data == null)
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		string data2 = JsonConvert.SerializeObject(data, Formatting.Indented);
		Error error = await WriteTextFile(filePath, data2);
		if ((bool)error)
		{
			ModioLog.Error?.Log("Error writing the " + typeof(T).Name + " file at path " + filePath + ": " + error.GetMessage());
		}
		return error;
	}

	private Task<Error> DeleteData(string filePath)
	{
		Error error = DeleteFile(filePath);
		if ((bool)error)
		{
			ModioLog.Error?.Log($"Error deleting [{GameId}] game data: {error.GetMessage()}\nAt: {filePath}");
		}
		return Task.FromResult(error);
	}

	public virtual Task<(Error error, GameData result)> ReadGameData()
	{
		return ReadData<GameData>(GetGameDataFilePath());
	}

	public virtual Task<Error> WriteGameData(GameData gameData)
	{
		return WriteData(gameData, GetGameDataFilePath());
	}

	public virtual Task<Error> DeleteGameData()
	{
		return DeleteData(GetGameDataFilePath());
	}

	public virtual Task<(Error error, ModIndex index)> ReadIndexData()
	{
		return ReadData<ModIndex>(GetIndexFilePath());
	}

	public virtual Task<Error> WriteIndexData(ModIndex index)
	{
		return WriteData(index, GetIndexFilePath());
	}

	public virtual Task<Error> DeleteIndexData()
	{
		return DeleteData(GetIndexFilePath());
	}

	public virtual Task<(Error error, UserSaveObject result)> ReadUserData(string localUserId)
	{
		return ReadData<UserSaveObject>(GetUserDataFilePath(localUserId));
	}

	public virtual async Task<(Error error, LegacyUserSaveObject result)> ReadLegacyUserData(string localUserId)
	{
		try
		{
			IModioRootPathProvider modioRootPathProvider = ModioServices.Resolve<IModioRootPathProvider>();
			string text = "";
			WindowsRootPathProvider windowsRootPathProvider = (WindowsRootPathProvider)modioRootPathProvider;
			if (windowsRootPathProvider != null)
			{
				text = windowsRootPathProvider.LegacyUserPath;
				text = text.Replace('/', '\\');
			}
			string text2 = Path.Combine(text, GameId.ToString("00000"));
			ModioLog.Verbose?.Log("App Root Path: " + text);
			text2 += Path.DirectorySeparatorChar;
			ModioLog.Verbose?.Log("Attempting to load legacy user data from: " + text2);
			if (DoesDirectoryExist(text2))
			{
				string[] directories = Directory.GetDirectories(text2);
				if (directories.Length == 0)
				{
					ModioLog.Verbose?.Log("No legacy user data exists. No UserData sub-folders found...");
					return (error: new Error(ErrorCode.FILE_NOT_FOUND), result: null);
				}
				text2 = directories[0] + Path.DirectorySeparatorChar + "user.json";
				ModioLog.Verbose?.Log("Checking for User Data file: " + text2);
				if (DoesFileExist(text2))
				{
					ModioLog.Verbose?.Log("Found user.json file, loading...");
					return await ReadData<LegacyUserSaveObject>(text2);
				}
				ModioLog.Verbose?.Log("No legacy user data exists. No user.json file found...");
				return (error: new Error(ErrorCode.FILE_NOT_FOUND), result: null);
			}
			ModioLog.Verbose?.Log("No legacy user data exists. No UserData folder found...");
			return (error: new Error(ErrorCode.DIRECTORY_NOT_FOUND), result: null);
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception reading legacy data: {ex}");
			return (error: new Error(ErrorCode.UNKNOWN, ex.ToString()), result: null);
		}
	}

	public virtual Task<Error> WriteUserData(UserSaveObject userObject)
	{
		return WriteData(userObject, GetUserDataFilePath(userObject.LocalUserId));
	}

	public virtual Task<Error> DeleteUserData(string localUserId)
	{
		return DeleteData(GetUserDataFilePath(localUserId));
	}

	public virtual async Task<(Error error, UserSaveObject[] results)> ReadAllSavedUserData()
	{
		Error error = Error.None;
		List<UserSaveObject> output = new List<UserSaveObject>();
		if (!Directory.Exists(Root))
		{
			return (error: new Error(ErrorCode.DIRECTORY_NOT_FOUND), results: Array.Empty<UserSaveObject>());
		}
		try
		{
			foreach (string item in from fileName in (from fileName in Directory.GetFiles(Root)
					where fileName.Contains("_user_data")
					select fileName).Select(Path.GetFileName)
				select fileName.Split('_')[0])
			{
				(Error, UserSaveObject) tuple = await ReadUserData(item);
				if (!tuple.Item1)
				{
					output.Add(tuple.Item2);
				}
			}
			return (error: error, results: output.ToArray());
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception reading all User Data files : {ex}");
			return (error: new ErrorException(ex), results: Array.Empty<UserSaveObject>());
		}
	}

	public virtual Error DeleteLegacyUserData()
	{
		try
		{
			ModioServices.Resolve<IModioRootPathProvider>();
			string text = "";
			string text2 = "";
			WindowsRootPathProvider windowsRootPathProvider = new WindowsRootPathProvider();
			if (windowsRootPathProvider != null)
			{
				text = windowsRootPathProvider.LegacyUserPath;
				text = text.Replace('/', '\\');
			}
			text2 = text;
			ModioLog.Verbose?.Log("Attempting to delete legacy user data in: " + text2);
			if (DoesDirectoryExist(text2))
			{
				Directory.Delete(text2, recursive: true);
				return Error.None;
			}
			ModioLog.Warning?.Log("No legacy user data exists, nothing to delete...");
			return new Error(ErrorCode.DIRECTORY_NOT_FOUND);
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception deleting legacy UserData: {ex}");
			return new Error(ErrorCode.UNKNOWN, ex.ToString());
		}
	}

	public virtual async Task<Error> DownloadModFileFromStream(long modId, long modfileId, Stream downloadStream, string md5Hash, CancellationToken token, ModInstallProgressTracker progressTracker = null)
	{
		string filePath = GetModfilePath(modId, modfileId);
		Error error = CreateDirectory(filePath);
		if ((bool)error)
		{
			ModioLog.Error?.Log("Error attempting download Modfile: " + error.GetMessage() + "\nAt:" + filePath);
			downloadStream?.Dispose();
			return error;
		}
		byte[] buffer = new byte[1048576];
		OngoingTaskCount++;
		try
		{
			using CancellationTokenSource combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, ShutdownToken);
			token = combinedCts.Token;
			using MD5 md5 = MD5.Create();
			Stream writerStream = CreateFileStream(filePath, FileMode.Create);
			long totalBytesWritten = 0L;
			await using (writerStream)
			{
				while (true)
				{
					int num;
					int bytesRead = (num = await downloadStream.ReadAsync(buffer, 0, buffer.Length, token));
					if (num > 0)
					{
						if (token.IsCancellationRequested)
						{
							ModioLog.Verbose?.Log("Cancelling");
							error = new Error((ErrorCode)(IsShuttingDown ? (-2147483588) : (-2147483598)));
							break;
						}
						md5.TransformBlock(buffer, 0, bytesRead, null, 0);
						await writerStream.WriteAsync(buffer, 0, bytesRead, token);
						totalBytesWritten += bytesRead;
						progressTracker?.SetBytesRead(totalBytesWritten);
						continue;
					}
					break;
				}
			}
			downloadStream.Dispose();
			if ((bool)error)
			{
				Error error2 = await DeleteModfile(modId, modfileId);
				if ((bool)error2)
				{
					error = error2;
				}
				return error;
			}
			md5.TransformFinalBlock(buffer, 0, 0);
			string text = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
			if (string.IsNullOrEmpty(md5Hash))
			{
				ModioLog.Warning?.Log("Modfile has no MD5 hash in its metadata, skipping validation: At " + filePath);
			}
			else if (!string.Equals(md5Hash, text))
			{
				ModioLog.Error?.Log("Validation failed for Modfile: At " + filePath + "\nExpected MD5: " + md5Hash + " | Actual MD5: " + text + " | " + $"Bytes written: {totalBytesWritten}");
				error = new ModValidationError(ModValidationErrorCode.MD5DOES_NOT_MATCH);
				Error error3 = await DeleteModfile(modId, modfileId);
				if ((bool)error3)
				{
					error = error3;
				}
				return error;
			}
		}
		catch (TaskCanceledException arg)
		{
			ModioLog.Verbose?.Log($"Cancelled downloading Modfile: {arg}\nAt:{filePath}");
			error = new Error((ErrorCode)(IsShuttingDown ? (-2147483588) : (-2147483598)));
			Error error4 = DeleteFile(filePath);
			if ((bool)error4)
			{
				error = error4;
			}
			return error;
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception attempting to download Modfile: {ex}\nAt:{filePath}");
			error = new ErrorException(ex);
			Error error5 = DeleteFile(filePath);
			if ((bool)error5)
			{
				error = error5;
			}
			return error;
		}
		finally
		{
			OngoingTaskCount--;
		}
		return error;
	}

	protected virtual Stream CreateFileStream(string filePath, FileMode mode)
	{
		return new FileStream(filePath, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
	}

	public static async Task<byte[]> CalculateMd5Hash(string filePath, byte[] buffer)
	{
		using MD5 md5 = MD5.Create();
		byte[] hash;
		await using (Stream stream = File.OpenRead(filePath))
		{
			int inputCount;
			while ((inputCount = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
			{
				md5.TransformBlock(buffer, 0, inputCount, null, 0);
			}
			md5.TransformFinalBlock(buffer, 0, 0);
			hash = md5.Hash;
		}
		return hash;
	}

	public virtual Task<Error> DeleteModfile(long modId, long modfileId)
	{
		string modfilePath = GetModfilePath(modId, modfileId);
		Error error = DeleteFile(modfilePath);
		if ((bool)error)
		{
			ModioLog.Error?.Log($"Error deleting Modfile {modId}: {error.GetMessage()}\nAt: {modfilePath}");
		}
		return Task.FromResult(error);
	}

	public virtual Task<(Error error, List<(long modId, long modfileId)> results)> ScanForModfiles()
	{
		if (!Directory.Exists(Path.Combine(Root, "Modfiles")))
		{
			return Task.FromResult((Error.None, new List<(long, long)>()));
		}
		try
		{
			string[] files = Directory.GetFiles(Path.Combine(Root, "Modfiles"));
			List<(long, long)> list = new List<(long, long)>();
			string[] array = files;
			foreach (string text in array)
			{
				if (text.Contains("_modfile"))
				{
					string fileName = Path.GetFileName(text);
					string[] array2 = fileName.Split('_');
					if (array2.Length == 2 && long.TryParse(array2[0], out var result) && long.TryParse(array2[1], out var result2))
					{
						list.Add((result, result2));
					}
					else
					{
						ModioLog.Message?.Log("Invalid Modfile name: [" + fileName + "], skipping");
					}
				}
			}
			return Task.FromResult((Error.None, list));
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception scanning Modfiles: {ex}");
			return Task.FromResult(((Error)new ErrorException(ex), new List<(long, long)>()));
		}
	}

	public virtual async Task<Error> InstallMod(Mod mod, long modfileId, CancellationToken token)
	{
		string modFilePath = GetModfilePath(mod.Id, modfileId);
		Error error2 = Error.None;
		if (!DoesFileExist(modFilePath))
		{
			error2 = new Error(ErrorCode.FILE_NOT_FOUND);
		}
		if (!(await IsThereEnoughSpaceForExtracting(modFilePath)))
		{
			error2 = new Error(ErrorCode.INSUFFICIENT_SPACE);
		}
		if ((bool)error2)
		{
			ModioLog.Error?.Log("Unable to install mod: " + error2.GetMessage() + "\nModfile Path:" + modFilePath);
			return error2;
		}
		Stream stream = File.Open(modFilePath, FileMode.Open);
		error2 = await InstallModFromStream(mod, modfileId, stream, null, token);
		ErrorCode code = error2.Code;
		if (code != ErrorCode.OPERATION_CANCELLED && code != ErrorCode.BAD_PARAMETER && code != ErrorCode.SHUTTING_DOWN)
		{
			await DeleteModfile(mod.Id, modfileId);
		}
		return error2;
	}

	public virtual async Task<Error> InstallModFromStream(Mod mod, long modfileId, Stream stream, string md5Hash, CancellationToken token)
	{
		Error error = Error.None;
		string temporaryDirectoryPath = GetTemporaryInstallPath(mod.Id, modfileId);
		string installDirectoryPath = GetInstallPath(mod.Id, modfileId);
		Error error2 = CreateDirectory(temporaryDirectoryPath);
		if ((bool)error2)
		{
			error = error2;
		}
		ModioLog.Message?.Log($"Installing Modfile {mod} to {installDirectoryPath}");
		if ((bool)error)
		{
			ModioLog.Error?.Log("Unable to install mod: " + error.GetMessage() + "\nInstall Path:" + installDirectoryPath + "\nTemp Path:" + temporaryDirectoryPath + "\n");
			if (stream != null)
			{
				await stream.DisposeAsync();
			}
			return error;
		}
		OngoingTaskCount++;
		try
		{
			using CancellationTokenSource combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, ShutdownToken);
			token = combinedCts.Token;
			MD5ComputingStreamWrapper md5Stream = new MD5ComputingStreamWrapper(stream);
			try
			{
				Error result;
				await using (ZipInputStream zipStream = new ZipInputStream(md5Stream))
				{
					zipStream.IsStreamOwner = false;
					ModInstallProgressTracker tracker = new ModInstallProgressTracker(mod, mod.File.FileSize, () => md5Stream.TotalBytesRead);
					int numEntries = 0;
					while (true)
					{
						ZipEntry nextEntry = zipStream.GetNextEntry();
						if (nextEntry != null)
						{
							token.ThrowIfCancellationRequested();
							numEntries++;
							if (numEntries > 3)
							{
								ModioLog.Error?.Log("[BaseDataStorage::InstallModFromStream] Extraction disallowed: more than 3 files in modfile.");
								throw new TaskCanceledException("Extraction disallowed: More than 3 files present in modfile.");
							}
							if (nextEntry.IsDirectory)
							{
								ModioLog.Error?.Log("[BaseDataStorage::InstallModFromStream] Extraction disallowed: directory found in modfile.");
								throw new TaskCanceledException("Extraction disallowed: Directory present in modfile.");
							}
							if (nextEntry.IsDirectory || string.IsNullOrEmpty(nextEntry.Name))
							{
								continue;
							}
							error = await ExtractFileFromZipStream(zipStream, nextEntry, temporaryDirectoryPath + nextEntry.Name, tracker, token);
							if ((bool)error)
							{
								Error error3 = DeleteDirectoryAndContents(temporaryDirectoryPath);
								if ((bool)error3)
								{
									ModioLog.Message?.Log("Error cleaning up temporary download location: " + error3.GetMessage() + "\nAt: " + temporaryDirectoryPath);
								}
								result = error;
								break;
							}
							continue;
						}
						if (numEntries < 3)
						{
							ModioLog.Error?.Log("[BaseDataStorage::InstallModFromStream] Extraction disallowed: Less than 3 files in modfile.");
							throw new TaskCanceledException("Extraction disallowed: Less than 3 files present in modfile.");
						}
						string text = (await md5Stream.GetMD5HashAsync()).Replace("-", "").ToLowerInvariant();
						if (text != md5Hash && !string.IsNullOrEmpty(md5Hash))
						{
							ModioLog.Error?.Log("Error installing mod: md5 mismatch\n" + text + " != " + md5Hash + "\nInstall Path: " + installDirectoryPath + "\nTemp Path: " + temporaryDirectoryPath + "\n");
							error = new Error(ErrorCode.MD5DOES_NOT_MATCH);
						}
						goto end_IL_0297;
					}
					goto IL_068f;
					end_IL_0297:;
				}
				goto end_IL_0264;
				IL_068f:
				return result;
				end_IL_0264:;
			}
			finally
			{
				if (md5Stream != null)
				{
					await ((IAsyncDisposable)md5Stream).DisposeAsync();
				}
			}
		}
		catch (TaskCanceledException)
		{
			error = LogTaskCancelAndCleanup();
			return error;
		}
		catch (OperationCanceledException)
		{
			error = LogTaskCancelAndCleanup();
			return error;
		}
		catch (Exception arg)
		{
			ModioLog.Error?.Log($"Error installing mod: {arg}\nInstall Path: {installDirectoryPath}\nTemp Path: {temporaryDirectoryPath}\n");
			error = new Error((ErrorCode)(IsShuttingDown ? (-2147483588) : int.MinValue));
		}
		finally
		{
			OngoingTaskCount--;
		}
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Extraction operation for Modfile {mod.Id} aborted.");
			}
			Error error4 = DeleteDirectoryAndContents(temporaryDirectoryPath);
			if ((bool)error4)
			{
				ModioLog.Message?.Log("Error cleaning up temporary download location: " + error4.GetMessage() + "\nAt: " + temporaryDirectoryPath);
			}
			return error;
		}
		OngoingTaskCount++;
		try
		{
			return MoveTempInstallToCorrectLocation(mod, installDirectoryPath, temporaryDirectoryPath);
		}
		catch (Exception ex3)
		{
			ModioLog.Error?.Log($"Exception moving extracted files: {ex3}\nFrom: {temporaryDirectoryPath}\nTo: {installDirectoryPath}");
			return new ErrorException(ex3);
		}
		finally
		{
			OngoingTaskCount--;
		}
		Error LogTaskCancelAndCleanup()
		{
			error = new Error((ErrorCode)(IsShuttingDown ? (-2147483588) : (-2147483598)));
			ModioLog.Verbose?.Log("Cancelled installing mod: \nInstall Path: " + installDirectoryPath + "\nTemp Path: " + temporaryDirectoryPath + "\n");
			Error error5 = DeleteDirectoryAndContents(temporaryDirectoryPath);
			if ((bool)error5)
			{
				ModioLog.Message?.Log("Error cleaning up temporary download location: " + error5.GetMessage() + "\nAt: " + temporaryDirectoryPath);
			}
			return error;
		}
	}

	protected virtual Error MoveTempInstallToCorrectLocation(Mod mod, string installDirectoryPath, string temporaryDirectoryPath)
	{
		if (DoesDirectoryExist(installDirectoryPath))
		{
			DeleteDirectoryAndContents(installDirectoryPath);
		}
		string directoryName = Path.GetDirectoryName(installDirectoryPath);
		if (!DoesDirectoryExist(directoryName))
		{
			Error error = CreateDirectory(directoryName);
			if ((bool)error)
			{
				if (!error.IsSilent)
				{
					ModioLog.Error?.Log($"Install operation for Modfile {mod.Id} aborted. Failed to create directory {directoryName} with error {error}");
				}
				return error;
			}
		}
		Directory.Move(temporaryDirectoryPath, installDirectoryPath);
		return Error.None;
	}

	protected virtual async Task<Error> ExtractFileFromZipStream(ZipInputStream zipStream, ZipEntry entry, string filePath, ModInstallProgressTracker progressTracker, CancellationToken token)
	{
		if (entry.Name.Contains(".."))
		{
			ModioLog.Error?.Log("Invalid file path detected in zip entry: " + entry.Name);
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		if (!DoesDirectoryExist(filePath))
		{
			Error error = CreateDirectory(filePath);
			if ((bool)error)
			{
				if (!error.IsSilent)
				{
					ModioLog.Error?.Log($"Extraction operation for mod aborted. Failed to create directory {filePath} with error {error}");
				}
				return error;
			}
		}
		Stream writerStream = CreateFileStream(filePath, FileMode.Create);
		byte[] buffer = new byte[1048576];
		await using (writerStream)
		{
			while (true)
			{
				int num = await zipStream.ReadAsync(buffer, 0, buffer.Length, token);
				progressTracker.Update();
				if (num <= 0)
				{
					break;
				}
				await writerStream.WriteAsync(buffer, 0, num, token);
			}
		}
		return Error.None;
	}

	public virtual Task<Error> DeleteInstalledMod(Mod mod, long modfileId)
	{
		string installPath = GetInstallPath(mod.Id, modfileId);
		Error error = DeleteDirectoryAndContents(installPath);
		if ((bool)error)
		{
			ModioLog.Error?.Log($"Error deleting installed mod {mod}: {error.GetMessage()}\nAt: {installPath}");
		}
		return Task.FromResult(error);
	}

	public virtual Task<(Error error, List<(long modId, long modfileId)> results)> ScanForInstalledMods()
	{
		try
		{
			List<(long, long)> list = new List<(long, long)>();
			foreach (var (error, path) in IterateDirectoriesInDirectory(Path.Combine(Root, "mods")))
			{
				if (!error)
				{
					string fileName = Path.GetFileName(path);
					string[] array = fileName.Split('_');
					if (array.Length == 2 && long.TryParse(array[0], out var result) && long.TryParse(array[1], out var result2))
					{
						list.Add((result, result2));
					}
					else
					{
						ModioLog.Message?.Log("Invalid Install name: [" + fileName + "], skipping");
					}
				}
			}
			return Task.FromResult((Error.None, list));
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception scanning Mod Installations: {ex}");
			return Task.FromResult(((Error)new ErrorException(ex), new List<(long, long)>()));
		}
	}

	public virtual Task<(Error error, bool installed, long modfileId)> ScanForInstalledMod(Mod mod)
	{
		try
		{
			foreach (var (error, path) in IterateDirectoriesInDirectory(Path.Combine(Root, "mods")))
			{
				if (!error)
				{
					string fileName = Path.GetFileName(path);
					string[] array = fileName.Split('_');
					if (array.Length == 2 && long.TryParse(array[0], out var result) && long.TryParse(array[1], out var result2) && result == mod.Id)
					{
						return Task.FromResult((Error.None, true, result2));
					}
					ModioLog.Message?.Log("Invalid Install name: [" + fileName + "], skipping");
				}
			}
			return Task.FromResult((Error.None, false, -1L));
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception scanning Mod Installations: {ex}");
			return Task.FromResult(((Error)new ErrorException(ex), false, -1L));
		}
	}

	protected virtual void MigrateLegacyModInstalls()
	{
		MigrateLegacyModInstalls(Path.Combine(Root, "Installed"));
		try
		{
			ModioLog.Verbose?.Log("Checking for legacy mod installs... (windows only code path specific to GT)");
			WindowsRootPathProvider windowsRootPathProvider = new WindowsRootPathProvider();
			if (windowsRootPathProvider != null)
			{
				string text = Path.Combine(windowsRootPathProvider.LegacyPath, "mod.io");
				string text2 = Path.Combine(text, GameId.ToString("00000"), $"data{Path.DirectorySeparatorChar}mods") + Path.DirectorySeparatorChar;
				text2 = text2.Replace('/', '\\');
				if (DoesDirectoryExist(text2))
				{
					ModioLog.Verbose?.Log("Migrating legacy mods from: \"" + text2 + "\"");
					MigrateLegacyModInstalls(text2);
				}
				text += Path.DirectorySeparatorChar;
				text = text.Replace('/', '\\');
				if (DoesDirectoryExist(text))
				{
					ModioLog.Verbose?.Log("Deleting legacy mod.io folder at: \"" + text + "\"");
					Directory.Delete(text, recursive: true);
				}
			}
		}
		catch (Exception arg)
		{
			Console.WriteLine($"[mod.io] Exception while migrating legacy data: {arg}");
		}
	}

	protected void MigrateLegacyModInstalls(string legacyDirectoryPath)
	{
		try
		{
			foreach (var (error, text) in IterateDirectoriesInDirectory(legacyDirectoryPath))
			{
				if ((bool)error)
				{
					continue;
				}
				string fileName = Path.GetFileName(text);
				string[] array = fileName.Split('_');
				if (array.Length != 2 || !long.TryParse(array[0], out var result) || !long.TryParse(array[1], out var result2))
				{
					ModioLog.Message?.Log("Invalid Install name in legacy folder: [" + fileName + "], skipping");
					continue;
				}
				string installPath = GetInstallPath(result, result2);
				if (Directory.Exists(installPath))
				{
					ModioLog.Message?.Log("Deleting redundant legacy folder: " + installPath);
					Directory.Delete(text, recursive: true);
				}
				else
				{
					ModioLog.Message?.Log("Moving legacy folder: " + text + " to " + installPath);
					CreateDirectory(Path.GetFullPath(Path.Combine(installPath, "..") + Path.DirectorySeparatorChar));
					Directory.Move(text, installPath);
				}
			}
		}
		catch (Exception arg)
		{
			ModioLog.Error?.Log($"Exception scanning legacy Mod Installations: {arg}");
		}
	}

	public virtual async Task<(Error error, byte[] result)> ReadCachedImage(Uri serverPath)
	{
		if (serverPath == null)
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), result: null);
		}
		string path = GetImageDataFilePath(serverPath);
		byte[] item = Array.Empty<byte>();
		if (!IsValidPath(path))
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), result: item);
		}
		if (!DoesFileExist(path))
		{
			return (error: new Error(ErrorCode.FILE_NOT_FOUND), result: item);
		}
		Error error;
		(error, item) = await ReadFile(path);
		if ((bool)error)
		{
			ModioLog.Warning?.Log("Error reading image: " + error.GetMessage() + "\nAt: " + path);
		}
		return (error: error, result: item);
	}

	public virtual async Task<Error> WriteCachedImage(Uri serverPath, byte[] data)
	{
		if (serverPath == null)
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		string path = GetImageDataFilePath(serverPath);
		if (!IsValidPath(path) || data == null)
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		Error error = await WriteFile(path, data, data.Length);
		if ((bool)error)
		{
			ModioLog.Warning?.Log("Error writing image: " + error.GetMessage() + "\nAt: " + path);
		}
		return error;
	}

	public virtual Task<Error> DeleteCachedImage(Uri serverPath)
	{
		if (serverPath == null)
		{
			return Task.FromResult(new Error(ErrorCode.BAD_PARAMETER));
		}
		string imageDataFilePath = GetImageDataFilePath(serverPath);
		Error error = DeleteFile(imageDataFilePath);
		if ((bool)error)
		{
			ModioLog.Warning?.Log("Error deleting image: " + error.GetMessage() + "\nAt: " + imageDataFilePath);
		}
		return Task.FromResult(error);
	}

	public virtual Task<bool> IsThereAvailableFreeSpaceFor(long tempBytes, long persistentBytes)
	{
		return Task.FromResult(IsThereEnoughDiskSpaceFor(tempBytes + persistentBytes));
	}

	public virtual Task<bool> IsThereAvailableFreeSpaceForModfile(long bytes)
	{
		return Task.FromResult(IsThereEnoughDiskSpaceFor(bytes));
	}

	public virtual Task<long> GetAvailableFreeSpaceForModfile()
	{
		return Task.FromResult(GetAvailableFreeSpace());
	}

	public virtual Task<bool> IsThereAvailableFreeSpaceForModInstall(long bytes)
	{
		return Task.FromResult(IsThereEnoughDiskSpaceFor(bytes));
	}

	public virtual Task<long> GetAvailableFreeSpaceForModInstall()
	{
		return Task.FromResult(GetAvailableFreeSpace());
	}

	protected virtual async Task<bool> IsThereEnoughSpaceForExtracting(string archiveFilePath)
	{
		if (!DoesFileExist(archiveFilePath))
		{
			return new Error(ErrorCode.FILE_NOT_FOUND);
		}
		bool result;
		await using (Stream fileStream = File.Open(archiveFilePath, FileMode.Open))
		{
			bool flag;
			await using (ZipInputStream stream = new ZipInputStream(fileStream))
			{
				long num = 0L;
				while (true)
				{
					ZipEntry nextEntry = stream.GetNextEntry();
					if (nextEntry == null)
					{
						break;
					}
					if (nextEntry.Size == -1)
					{
						ModioLog.Verbose?.Log("Size unknown for file in zip: [" + nextEntry.Name + "]");
					}
					else
					{
						num += nextEntry.Size;
					}
				}
				flag = await IsThereAvailableFreeSpaceForModInstall(num);
			}
			result = flag;
		}
		return result;
	}

	protected virtual bool IsThereEnoughDiskSpaceFor(long bytes)
	{
		long availableFreeSpace = GetAvailableFreeSpace();
		if (availableFreeSpace > 0)
		{
			return bytes < availableFreeSpace;
		}
		return true;
	}

	protected virtual long GetAvailableFreeSpace()
	{
		if (ModioClient.Settings.TryGetPlatformSettings<ModioDiskTestSettings>(out var settings) && settings.OverrideDiskSpaceRemaining)
		{
			return settings.BytesRemaining;
		}
		if (!Initialized)
		{
			return 0L;
		}
		return new DriveInfo(Path.GetPathRoot(Root)).AvailableFreeSpace;
	}

	protected virtual bool IsValidPath(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
		{
			return false;
		}
		try
		{
			string pathRoot = Path.GetPathRoot(Path.GetFullPath(filePath));
			return pathRoot == "/" || !string.IsNullOrEmpty(pathRoot.Trim('\\', '/'));
		}
		catch
		{
			return false;
		}
	}

	protected virtual bool DoesDirectoryExist(string filePath)
	{
		return Directory.Exists(filePath);
	}

	protected virtual bool DoesFileExist(string filePath)
	{
		return File.Exists(filePath);
	}

	protected virtual Error CreateDirectory(string filePath)
	{
		if (!IsValidPath(filePath))
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		string directoryName = Path.GetDirectoryName(filePath);
		if (string.IsNullOrEmpty(directoryName))
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		try
		{
			Directory.CreateDirectory(directoryName);
		}
		catch (Exception exception)
		{
			return new ErrorException(exception);
		}
		return Error.None;
	}

	protected virtual Error DeleteDirectoryAndContents(string filePath)
	{
		if (!IsValidPath(filePath))
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		if (!DoesDirectoryExist(filePath))
		{
			return Error.None;
		}
		try
		{
			Directory.Delete(filePath, recursive: true);
		}
		catch (Exception exception)
		{
			return new ErrorException(exception);
		}
		return Error.None;
	}

	protected virtual Error DeleteFile(string filePath)
	{
		if (!IsValidPath(filePath))
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		if (!DoesFileExist(filePath))
		{
			return Error.None;
		}
		try
		{
			File.Delete(filePath);
		}
		catch (Exception exception)
		{
			return new ErrorException(exception);
		}
		return Error.None;
	}

	protected virtual async Task<Error> WriteFile(string path, byte[] data, int bytesToWrite)
	{
		if (!IsValidPath(path) || data == null)
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		Error error = CreateDirectory(path);
		if ((bool)error)
		{
			return error;
		}
		OngoingTaskCount++;
		try
		{
			Error none;
			await using (FileStream fileStream = File.Open(path, FileMode.Create))
			{
				fileStream.Position = 0L;
				await fileStream.WriteAsync(data, 0, bytesToWrite, CancellationToken.None);
				none = Error.None;
			}
			return none;
		}
		catch (Exception exception)
		{
			return new ErrorException(exception);
		}
		finally
		{
			OngoingTaskCount--;
		}
	}

	protected virtual async Task<(Error error, byte[] result)> ReadFile(string path)
	{
		byte[] output = Array.Empty<byte>();
		if (!IsValidPath(path))
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), result: output);
		}
		if (!DoesFileExist(path))
		{
			return (error: new Error(ErrorCode.FILE_NOT_FOUND), result: output);
		}
		OngoingTaskCount++;
		try
		{
			(Error error, byte[] result) result;
			await using (FileStream fileStream = File.Open(path, FileMode.Open))
			{
				output = new byte[fileStream.Length];
				await fileStream.ReadAsync(output, 0, output.Length);
				result = (error: Error.None, result: output);
			}
			return result;
		}
		catch (Exception exception)
		{
			return (error: new ErrorException(exception), result: output);
		}
		finally
		{
			OngoingTaskCount--;
		}
	}

	protected virtual async Task<Error> WriteTextFile(string path, string data)
	{
		if (!IsValidPath(path) || string.IsNullOrEmpty(data))
		{
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		var (error, array) = ConvertUTF8Data(data);
		if ((bool)error)
		{
			return error;
		}
		return await WriteFile(path, array, array.Length);
	}

	protected virtual async Task<(Error error, string result)> ReadTextFile(string path)
	{
		string output = string.Empty;
		if (ShutdownToken.IsCancellationRequested)
		{
			return (error: new Error(ErrorCode.SHUTTING_DOWN), result: output);
		}
		if (!IsValidPath(path))
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), result: output);
		}
		if (!DoesFileExist(path))
		{
			return (error: new Error(ErrorCode.FILE_NOT_FOUND), result: output);
		}
		var (error, data) = await ReadFile(path);
		if (!error)
		{
			(error, output) = TryParseUTF8Data(data);
		}
		return (error: error, result: output);
	}

	protected virtual (Error error, string result) TryParseUTF8Data(byte[] data)
	{
		if (data == null)
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), result: string.Empty);
		}
		try
		{
			string @string = Encoding.UTF8.GetString(data);
			return (error: Error.None, result: @string);
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception parsing bytes to string: {ex}");
			return (error: new ErrorException(ex), result: string.Empty);
		}
	}

	protected virtual (Error error, byte[] result) ConvertUTF8Data(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), result: Array.Empty<byte>());
		}
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			return (error: Error.None, result: bytes);
		}
		catch (Exception arg)
		{
			ModioLog.Error?.Log($"Exception parsing string to bytes: {arg}");
			return (error: new Error(ErrorCode.BAD_PARAMETER), result: Array.Empty<byte>());
		}
	}

	public virtual string GetModfilePath(long modId, long modfileId)
	{
		return Path.Combine(Root, "Modfiles", $"{modId}_{modfileId}_modfile.zip");
	}

	public virtual string GetInstallPath(long modId, long modfileId)
	{
		return string.Format("{0}{1}", Path.Combine(Root, "mods", $"{modId}_{modfileId}"), Path.DirectorySeparatorChar);
	}

	protected virtual string GetTemporaryInstallPath(long modId, long modfileId)
	{
		return string.Format("{0}{1}", Path.Combine(Root, "Temp", $"{modId}_{modfileId}"), Path.DirectorySeparatorChar);
	}

	protected virtual string GetGameDataFilePath()
	{
		return Path.Combine(Root, $"{GameId}_game_data.json");
	}

	protected virtual string GetIndexFilePath()
	{
		return Path.Combine(Root, $"{GameId}_mod_index.json");
	}

	protected virtual string GetUserDataFilePath(string localUserId)
	{
		return Path.Combine(UserRoot, localUserId + "_user_data.json");
	}

	protected virtual string GetImageDataFilePath(Uri serverPath)
	{
		return Path.Combine(Root, "ImageCache" + serverPath.LocalPath);
	}

	public virtual bool DoesModfileExist(long modId, long modfileId)
	{
		string modfilePath = GetModfilePath(modId, modfileId);
		return DoesFileExist(modfilePath);
	}

	public virtual bool DoesInstallExist(long modId, long modfileId)
	{
		string installPath = GetInstallPath(modId, modfileId);
		return DoesDirectoryExist(installPath);
	}

	public virtual async Task<Error> CompressToZip(string filePath, Stream outputTo)
	{
		if (!DoesDirectoryExist(filePath) && !DoesFileExist(filePath))
		{
			return new Error(ErrorCode.FILE_NOT_FOUND);
		}
		Error returnError = Error.None;
		filePath = Path.GetFullPath(filePath);
		Error result;
		await using (ZipOutputStream zipStream = new ZipOutputStream(outputTo))
		{
			foreach (var (error, path) in IterateFilesInDirectory(filePath))
			{
				if ((bool)error)
				{
					continue;
				}
				await using FileStream fileStream = File.Open(path, FileMode.Open);
				string text = Path.GetFullPath(path).Substring(filePath.Length);
				if (string.IsNullOrEmpty(text))
				{
					text = Path.GetFileName(filePath);
				}
				await CompressStream(text, fileStream, zipStream);
			}
			result = returnError;
		}
		return result;
	}

	protected virtual async Task CompressStream(string entryName, Stream stream, ZipOutputStream zipStream)
	{
		ZipEntry entry = new ZipEntry(entryName);
		zipStream.PutNextEntry(entry);
		await stream.CopyToAsync(zipStream, 4096);
		zipStream.CloseEntry();
	}

	protected virtual IEnumerable<(Error error, string fileName)> IterateFilesInDirectory(string directoryPath)
	{
		if (!DoesDirectoryExist(directoryPath))
		{
			if (DoesFileExist(directoryPath))
			{
				yield return (error: Error.None, fileName: directoryPath);
			}
			else
			{
				yield return (error: new Error(ErrorCode.FILE_NOT_FOUND), fileName: null);
			}
			yield break;
		}
		foreach (string item in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
		{
			yield return (error: Error.None, fileName: item);
		}
	}

	protected virtual IEnumerable<(Error error, string directoryPath)> IterateDirectoriesInDirectory(string directoryPath)
	{
		if (!DoesDirectoryExist(directoryPath))
		{
			yield return (error: new Error(ErrorCode.FILE_NOT_FOUND), directoryPath: null);
			yield break;
		}
		foreach (string item in Directory.EnumerateDirectories(directoryPath))
		{
			yield return (error: Error.None, directoryPath: item);
		}
	}
}
