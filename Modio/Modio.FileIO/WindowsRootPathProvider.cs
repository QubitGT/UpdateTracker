using System;
using UnityEngine;

namespace Modio.FileIO;

public class WindowsRootPathProvider : IModioRootPathProvider
{
	public string LegacyPath = Application.persistentDataPath ?? "";

	public string LegacyUserPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/mod.io";

	public string Path => Environment.GetEnvironmentVariable("public") ?? "";

	public string UserPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) ?? "";

	public static bool IsPublicEnvironmentVariableSet()
	{
		return Environment.GetEnvironmentVariable("public") != null;
	}
}
