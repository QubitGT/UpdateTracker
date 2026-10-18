using System;
using System.IO;
using System.Linq;

namespace g3;

public static class FileSystemUtils
{
	public static bool CanAccessFolder(string sPath)
	{
		try
		{
			Directory.GetDirectories(sPath);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidFilenameCharacter(char c)
	{
		return !Path.GetInvalidPathChars().Contains(c);
	}

	public static bool IsValidFilenameString(string s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (Path.GetInvalidPathChars().Contains(s[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsWebURL(string s)
	{
		if (Uri.TryCreate(s, UriKind.Absolute, out var result))
		{
			if (!(result.Scheme == Uri.UriSchemeHttp))
			{
				return result.Scheme == Uri.UriSchemeHttps;
			}
			return true;
		}
		return false;
	}

	public static bool IsFullFilesystemPath(string s)
	{
		return Path.IsPathRooted(s);
	}

	public static string GetTempFilePathWithExtension(string extension)
	{
		string tempPath = Path.GetTempPath();
		string path = Guid.NewGuid().ToString() + extension;
		return Path.Combine(tempPath, path);
	}
}
