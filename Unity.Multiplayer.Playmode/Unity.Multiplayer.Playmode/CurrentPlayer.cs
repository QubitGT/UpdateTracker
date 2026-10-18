using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.Multiplayer.Playmode;

public static class CurrentPlayer
{
	private static bool s_Loaded;

	private static List<string> s_Tags = new List<string>();

	public static bool IsMainEditor => false;

	public static string[] ReadOnlyTags()
	{
		if (!s_Loaded)
		{
			s_Loaded = true;
			LoadTag();
		}
		return s_Tags.ToArray();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ReloadLatestTagsOnEnterPlaymode()
	{
		s_Loaded = false;
	}

	private static void LoadTag()
	{
	}

	public static void ReportResult(bool condition, string message = "", [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int lineNumber = 0)
	{
	}
}
