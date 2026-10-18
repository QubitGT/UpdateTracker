using System;
using System.Diagnostics;

namespace Fusion;

public static class Versioning
{
	public static readonly Version InvalidVersion = new Version(0, 0, 0);

	public static Version GetCurrentVersion
	{
		get
		{
			Version version = typeof(Versioning).Assembly.GetName().Version;
			return new Version(version.Major, version.Minor, version.Build);
		}
	}

	public static Version ShortVersion(this Version version)
	{
		return new Version(version.Major, version.Minor);
	}

	public static string AssemblyFileVersion()
	{
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(typeof(Versioning).Assembly.Location);
		return versionInfo.ProductVersion;
	}
}
