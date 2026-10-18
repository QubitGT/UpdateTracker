using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Liv.NativeGalleryBridge;

public static class NativeGallery
{
	public enum PermissionType
	{
		Read,
		Write
	}

	public enum Permission
	{
		Denied,
		Granted,
		ShouldAsk
	}

	[Flags]
	public enum MediaType
	{
		Video = 2,
		Image = 4
	}

	public delegate void PermissionCallback(Permission permission);

	public delegate void MediaSaveCallback(bool success, string path);

	public delegate void MediaPickCallback(string path);

	private const bool PermissionFreeMode = true;

	public static Permission CheckPermission(PermissionType permissionType, MediaType mediaTypes)
	{
		return Permission.Granted;
	}

	public static Permission RequestPermission(PermissionType permissionType, MediaType mediaTypes)
	{
		CheckPermission(permissionType, mediaTypes);
		_ = 1;
		return Permission.Granted;
	}

	public static void RequestPermissionAsync(PermissionCallback callback, PermissionType permissionType, MediaType mediaTypes)
	{
		callback(Permission.Granted);
	}

	public static Task<Permission> RequestPermissionAsync(PermissionType permissionType, MediaType mediaTypes)
	{
		TaskCompletionSource<Permission> tcs = new TaskCompletionSource<Permission>();
		RequestPermissionAsync(delegate(Permission permission)
		{
			tcs.SetResult(permission);
		}, permissionType, mediaTypes);
		return tcs.Task;
	}

	private static Permission ProcessPermission(Permission permission)
	{
		if (permission != (Permission)3)
		{
			return permission;
		}
		return Permission.Granted;
	}

	public static async Task<Permission> SaveVideoToGallery(byte[] mediaBytes, string album, string filename, MediaSaveCallback callback = null)
	{
		return await SaveToGallery(mediaBytes, album, filename, MediaType.Video, callback);
	}

	public static async Task<Permission> SaveVideoToGallery(string existingMediaPath, string album, string filename, MediaSaveCallback callback = null)
	{
		return await SaveToGallery(existingMediaPath, album, filename, MediaType.Video, callback);
	}

	public static async Task<Permission> SaveImageToGallery(string existingMediaPath, string album, string filename, MediaSaveCallback callback = null)
	{
		return await SaveToGallery(existingMediaPath, album, filename, MediaType.Image, callback);
	}

	public static string GetExternalStoragePublicDirectory()
	{
		return "";
	}

	private static async Task<Permission> SaveToGallery(byte[] mediaBytes, string album, string filename, MediaType mediaType, MediaSaveCallback callback)
	{
		Permission result = await RequestPermissionAsync(PermissionType.Write, mediaType);
		if (result == Permission.Granted)
		{
			if (mediaBytes == null || mediaBytes.Length == 0)
			{
				throw new ArgumentException("Parameter 'mediaBytes' is null or empty!");
			}
			if (album == null || album.Length == 0)
			{
				throw new ArgumentException("Parameter 'album' is null or empty!");
			}
			if (filename == null || filename.Length == 0)
			{
				throw new ArgumentException("Parameter 'filename' is null or empty!");
			}
			if (string.IsNullOrEmpty(Path.GetExtension(filename)))
			{
				Debug.LogWarning("LCK 'filename' doesn't have an extension, this might result in unexpected behaviour!");
			}
			string path = GetTemporarySavePath(filename);
			using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
			{
				await fileStream.WriteAsync(mediaBytes, 0, mediaBytes.Length);
			}
			SaveToGalleryInternal(path, album, mediaType, callback);
		}
		return result;
	}

	private static async Task<Permission> SaveToGallery(string existingMediaPath, string album, string filename, MediaType mediaType, MediaSaveCallback callback)
	{
		Permission result = await RequestPermissionAsync(PermissionType.Write, mediaType);
		if (result == Permission.Granted)
		{
			if (!File.Exists(existingMediaPath))
			{
				throw new FileNotFoundException("File not found at " + existingMediaPath);
			}
			if (album == null || album.Length == 0)
			{
				throw new ArgumentException("Parameter 'album' is null or empty!");
			}
			if (filename == null || filename.Length == 0)
			{
				throw new ArgumentException("Parameter 'filename' is null or empty!");
			}
			if (string.IsNullOrEmpty(Path.GetExtension(filename)))
			{
				string extension = Path.GetExtension(existingMediaPath);
				if (string.IsNullOrEmpty(extension))
				{
					Debug.LogWarning("LCK 'filename' doesn't have an extension, this might result in unexpected behaviour!");
				}
				else
				{
					filename += extension;
				}
			}
			string path = GetTemporarySavePath(filename);
			await Task.Run(delegate
			{
				File.Copy(existingMediaPath, path, overwrite: true);
			});
			SaveToGalleryInternal(path, album, mediaType, callback);
		}
		return result;
	}

	private static void SaveToGalleryInternal(string path, string album, MediaType mediaType, MediaSaveCallback callback)
	{
		callback?.Invoke(success: true, null);
	}

	private static string GetTemporarySavePath(string filename)
	{
		string text = Path.Combine(Application.persistentDataPath, "NGallery");
		Directory.CreateDirectory(text);
		return Path.Combine(text, filename);
	}
}
