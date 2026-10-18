using System.IO;
using System.Linq;

namespace ICSharpCode.SharpZipLib.Core;

public static class PathUtils
{
	public static string DropPathRoot(string path)
	{
		char[] invalidChars = Path.GetInvalidPathChars();
		bool cleanRootSep = path.Length >= 3 && path[1] == ':' && path[2] == ':';
		int j;
		for (j = Path.GetPathRoot(new string(path.Take(258).Select((char c, int i) => (!invalidChars.Contains(c) && !(i == 2 && cleanRootSep)) ? c : '_').ToArray())).Length; path.Length > j && (path[j] == '/' || path[j] == '\\'); j++)
		{
		}
		return path.Substring(j);
	}

	public static string GetTempFileName(string original = null)
	{
		string tempPath = Path.GetTempPath();
		string text;
		do
		{
			text = ((original == null) ? Path.Combine(tempPath, Path.GetRandomFileName()) : (original + "." + Path.GetRandomFileName()));
		}
		while (File.Exists(text));
		return text;
	}
}
