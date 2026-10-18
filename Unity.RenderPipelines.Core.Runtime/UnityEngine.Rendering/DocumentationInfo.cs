namespace UnityEngine.Rendering;

public class DocumentationInfo
{
	private const string fallbackVersion = "13.1";

	private const string packageDocumentationUrl = "https://docs.unity3d.com/Packages/{0}@{1}/manual/";

	private const string url = "https://docs.unity3d.com/Packages/{0}@{1}/manual/{2}.html{3}";

	public static string version => "13.1";

	public static string GetPackageLink(string packageName, string packageVersion, string pageName)
	{
		return string.Format("https://docs.unity3d.com/Packages/{0}@{1}/manual/{2}.html{3}", packageName, packageVersion, pageName, "");
	}

	public static string GetPackageLink(string packageName, string packageVersion, string pageName, string pageHash)
	{
		if (!string.IsNullOrEmpty(pageHash) && !pageHash.StartsWith("#"))
		{
			pageHash = "#" + pageHash;
		}
		return $"https://docs.unity3d.com/Packages/{packageName}@{packageVersion}/manual/{pageName}.html{pageHash}";
	}

	public static string GetPageLink(string packageName, string pageName)
	{
		return string.Format("https://docs.unity3d.com/Packages/{0}@{1}/manual/{2}.html{3}", packageName, version, pageName, "");
	}

	public static string GetPageLink(string packageName, string pageName, string pageHash)
	{
		if (!string.IsNullOrEmpty(pageHash) && !pageHash.StartsWith("#"))
		{
			pageHash = "#" + pageHash;
		}
		return $"https://docs.unity3d.com/Packages/{packageName}@{version}/manual/{pageName}.html{pageHash}";
	}

	public static string GetDefaultPackageLink(string packageName, string packageVersion)
	{
		return $"https://docs.unity3d.com/Packages/{packageName}@{packageVersion}/manual/";
	}

	public static string GetDefaultPackageLink(string packageName)
	{
		return $"https://docs.unity3d.com/Packages/{packageName}@{version}/manual/";
	}
}
