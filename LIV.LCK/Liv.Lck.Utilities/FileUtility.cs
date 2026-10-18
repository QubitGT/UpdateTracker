using System;
using System.IO;
using System.Threading.Tasks;
using Liv.Lck.Settings;
using Liv.NativeGalleryBridge;
using UnityEngine;

namespace Liv.Lck.Utilities;

public static class FileUtility
{
	private const string EchoFileMarker = "_Echo_";

	public static bool IsFileLocked(string filePath)
	{
		FileStream fileStream = null;
		try
		{
			fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		}
		catch (IOException)
		{
			return true;
		}
		finally
		{
			fileStream?.Close();
		}
		return false;
	}

	public static async Task CopyToGallery(string sourceFilePath, string albumName, Action<bool, string> callback)
	{
		if (File.Exists(sourceFilePath))
		{
			bool flag = Path.GetExtension(sourceFilePath) == ".mp4";
			try
			{
				string fileName = Path.GetFileName(sourceFilePath);
				if (Application.platform == RuntimePlatform.Android)
				{
					NativeGallery.Permission permission = ((!flag) ? (await NativeGallery.SaveImageToGallery(sourceFilePath, albumName, fileName, WrappedMediaSaveCallback)) : (await NativeGallery.SaveVideoToGallery(sourceFilePath, albumName, fileName, WrappedMediaSaveCallback)));
					NativeGallery.Permission permission2 = permission;
					if (permission2 != NativeGallery.Permission.Granted)
					{
						callback(arg1: false, sourceFilePath);
						LckLog.LogError($"LCK Gallery permission not granted: {permission2}", "CopyToGallery", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\FileUtilities.cs", 68);
					}
					return;
				}
				string text = Path.Combine(Environment.GetFolderPath(flag ? Environment.SpecialFolder.MyVideos : Environment.SpecialFolder.MyPictures), albumName);
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				string destinationFilePath = Path.Combine(text, fileName);
				await Task.Run(delegate
				{
					File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
				});
				await DeleteMatchingFilesAsync(sourceFilePath);
				callback(arg1: true, destinationFilePath);
				return;
			}
			catch (Exception ex)
			{
				callback(arg1: false, sourceFilePath);
				LckLog.LogError("LCK Error reading file: " + ex.Message, "CopyToGallery", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\FileUtilities.cs", 94);
				return;
			}
		}
		callback(arg1: false, sourceFilePath);
		LckLog.LogError("LCK Source file does not exist: " + sourceFilePath, "CopyToGallery", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\FileUtilities.cs", 100);
		async void WrappedMediaSaveCallback(bool success, string path)
		{
			callback(success, path);
			if (success)
			{
				await DeleteMatchingFilesAsync(sourceFilePath);
			}
		}
	}

	public static string GenerateFilename(string extension)
	{
		string text = DateTime.Now.ToString(LckSettings.Instance.RecordingDateSuffixFormat);
		return LckSettings.Instance.RecordingFilenamePrefix + "_" + text + "." + extension;
	}

	public static string GenerateEchoFilename(string extension)
	{
		string text = DateTime.Now.ToString(LckSettings.Instance.RecordingDateSuffixFormat);
		return LckSettings.Instance.RecordingFilenamePrefix + "_Echo_" + text + "." + extension;
	}

	private static bool IsEchoFile(string filePath)
	{
		return Path.GetFileName(filePath).Contains("_Echo_");
	}

	private static async Task DeleteMatchingFilesAsync(string filePath)
	{
		try
		{
			string folderPath = Path.GetDirectoryName(filePath);
			string fileExtension = Path.GetExtension(filePath);
			bool sourceIsEcho = IsEchoFile(filePath);
			if (folderPath == null)
			{
				return;
			}
			await Task.Run(delegate
			{
				string[] files = Directory.GetFiles(folderPath, "*" + fileExtension);
				foreach (string text in files)
				{
					if (IsEchoFile(text) == sourceIsEcho)
					{
						try
						{
							File.Delete(text);
						}
						catch (Exception ex2)
						{
							LckLog.LogError("LCK Error deleting file " + text + ": " + ex2.Message, "DeleteMatchingFilesAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\FileUtilities.cs", 155);
						}
					}
				}
			});
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error during file deletion: " + ex.Message, "DeleteMatchingFilesAsync", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\FileUtilities.cs", 163);
		}
	}
}
