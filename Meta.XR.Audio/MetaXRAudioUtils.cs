using System;
using System.IO;
using System.Linq;

internal class MetaXRAudioUtils
{
	internal static string GetCaseSensitivePathForFile(string path)
	{
		if (!File.Exists(path))
		{
			return path;
		}
		string text = Path.GetPathRoot(path);
		string[] array = path.Substring(text.Length).Split(Path.DirectorySeparatorChar);
		foreach (string searchPattern in array)
		{
			text = Directory.EnumerateFileSystemEntries(text, searchPattern).First();
		}
		return text;
	}

	internal static void CreateDirectoryForFilePath(string absPath)
	{
		int num = Math.Max(absPath.LastIndexOf('/'), absPath.LastIndexOf('\\'));
		if (num >= 0)
		{
			string path = absPath.Substring(0, num);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}
}
