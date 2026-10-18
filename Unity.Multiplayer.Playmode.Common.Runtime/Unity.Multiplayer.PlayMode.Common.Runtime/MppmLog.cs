using System.Diagnostics;
using UnityEngine;

namespace Unity.Multiplayer.PlayMode.Common.Runtime;

internal static class MppmLog
{
	private const string k_ToolsPrefix = "[MultiplayerPlaymode]";

	[Conditional("UNITY_MP_TOOLS_DEV")]
	public static void Debug(object message)
	{
		UnityEngine.Debug.Log(string.Format("{0}: {1}", "[MultiplayerPlaymode]", message));
	}

	public static void Warning(object message)
	{
		UnityEngine.Debug.LogWarning(string.Format("{0}: {1}", "[MultiplayerPlaymode]", message));
	}

	public static void Error(object message)
	{
		UnityEngine.Debug.LogError(string.Format("{0}: {1}", "[MultiplayerPlaymode]", message));
	}
}
