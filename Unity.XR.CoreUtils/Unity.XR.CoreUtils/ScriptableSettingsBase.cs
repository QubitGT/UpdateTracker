using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public abstract class ScriptableSettingsBase : ScriptableObject
{
	private const string k_AbsolutePathMessage = "Path cannot be absolute";

	protected const string PathExceptionMessage = "Exception caught trying to create path.";

	internal const string NullPathMessage = "Path cannot be null";

	internal const string PathWithPeriodMessage = "Path cannot contain the character '.' before or after a directory separator";

	internal const string PathWithInvalidCharacterMessage = "Paths on Windows cannot contain the following characters: ':', '*', '?', '\"', '<', '>', '|'";

	private static readonly char[] k_PathTrimChars = new char[3]
	{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar,
		' '
	};

	private static readonly char[] k_InvalidCharacters = new char[8] { ':', '*', '?', '"', '<', '>', '|', '\\' };

	private static readonly string[] k_InvalidStrings = new string[4] { "\\.", "/.", ".\\", "./" };

	public static ScriptableSettingsBase GetInstanceByType(Type settingsType)
	{
		return (ScriptableSettingsBase)settingsType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetProperty).GetValue(null, null);
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
		OnLoaded();
	}

	protected virtual void OnLoaded()
	{
	}

	internal static bool ValidatePath(string path, out string cleanedPath)
	{
		cleanedPath = path;
		if (cleanedPath == null)
		{
			Debug.LogWarning("Path cannot be null");
			return false;
		}
		char[] array = k_InvalidCharacters;
		foreach (char c in array)
		{
			if (cleanedPath.Contains(c.ToString()))
			{
				Debug.LogWarning("Paths on Windows cannot contain the following characters: ':', '*', '?', '\"', '<', '>', '|'");
				return false;
			}
		}
		string[] array2 = k_InvalidStrings;
		foreach (string value in array2)
		{
			if (cleanedPath.Contains(value))
			{
				Debug.LogWarning("Path cannot contain the character '.' before or after a directory separator");
				return false;
			}
		}
		try
		{
			if (Path.IsPathRooted(cleanedPath))
			{
				Debug.LogWarning("Path cannot be absolute");
				return false;
			}
		}
		catch (Exception arg)
		{
			Debug.LogWarning(string.Format("{0}\n{1}", "Exception caught trying to create path.", arg));
			return false;
		}
		cleanedPath = cleanedPath.Trim(k_PathTrimChars);
		int num = 0;
		for (int num2 = cleanedPath.Length - 1; num2 >= 0; num2--)
		{
			if (cleanedPath[num2] == '\\' || cleanedPath[num2] == '/')
			{
				num++;
			}
			else if (num > 0)
			{
				cleanedPath = cleanedPath.Remove(num2 + 1, num - 1);
				num = 0;
			}
		}
		if (cleanedPath != "")
		{
			cleanedPath += "/";
		}
		return true;
	}
}
public abstract class ScriptableSettingsBase<T> : ScriptableSettingsBase where T : ScriptableObject
{
	protected static readonly bool HasCustomPath = typeof(T).IsDefined(typeof(ScriptableSettingsPathAttribute), inherit: true);

	protected static T BaseInstance;

	protected ScriptableSettingsBase()
	{
		if (BaseInstance != null)
		{
			XRLoggingUtils.LogWarning($"ScriptableSingleton {typeof(T)} already exists. This can happen if " + "there are two copies of the asset or if you query the singleton in a constructor.", BaseInstance);
		}
	}

	protected static void Save(string savePathFormat)
	{
	}

	protected static string GetFilePath()
	{
		return typeof(T).Name;
	}
}
