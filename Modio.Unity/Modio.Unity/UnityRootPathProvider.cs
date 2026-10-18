using System.IO;
using Modio.FileIO;
using UnityEngine;

namespace Modio.Unity;

public class UnityRootPathProvider : IModioRootPathProvider
{
	public string Path => Application.persistentDataPath;

	public string UserPath => System.IO.Path.Combine(Application.persistentDataPath, "UserData");
}
