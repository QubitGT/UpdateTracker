using System;
using System.IO;

namespace Meta.WitAi.Utilities;

public static class IOUtility
{
	private static void LogError(string error)
	{
		VLog.E("IO Utility - " + error);
	}

	public static bool CreateDirectory(string directoryPath, bool recursively = true)
	{
		if (string.IsNullOrEmpty(directoryPath))
		{
			return false;
		}
		if (Directory.Exists(directoryPath))
		{
			return true;
		}
		if (recursively)
		{
			string directoryName = Path.GetDirectoryName(directoryPath);
			if (!string.IsNullOrEmpty(directoryName) && !CreateDirectory(directoryName))
			{
				return false;
			}
		}
		try
		{
			Directory.CreateDirectory(directoryPath);
		}
		catch (Exception arg)
		{
			LogError($"Create Directory Exception\nDirectory Path: {directoryPath}\n{arg}");
			return false;
		}
		return true;
	}

	public static bool DeleteDirectory(string directoryPath, bool forceIfFilled = true)
	{
		if (!Directory.Exists(directoryPath))
		{
			return true;
		}
		try
		{
			Directory.Delete(directoryPath, forceIfFilled);
		}
		catch (Exception arg)
		{
			LogError($"Delete Directory Exception\nDirectory Path: {directoryPath}\n{arg}");
			return false;
		}
		return true;
	}
}
