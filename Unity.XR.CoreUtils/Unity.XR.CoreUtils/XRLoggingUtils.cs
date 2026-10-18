using System;
using System.Linq;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class XRLoggingUtils
{
	private static readonly bool k_DontLogAnything;

	static XRLoggingUtils()
	{
		k_DontLogAnything = Environment.GetCommandLineArgs().Contains("-runTests");
	}

	public static void Log(string message, UnityEngine.Object context = null)
	{
		if (!k_DontLogAnything)
		{
			Debug.Log(message, context);
		}
	}

	public static void LogWarning(string message, UnityEngine.Object context = null)
	{
		if (!k_DontLogAnything)
		{
			Debug.LogWarning(message, context);
		}
	}

	public static void LogError(string message, UnityEngine.Object context = null)
	{
		if (!k_DontLogAnything)
		{
			Debug.LogError(message, context);
		}
	}

	public static void LogException(Exception exception, UnityEngine.Object context = null)
	{
		if (!k_DontLogAnything)
		{
			Debug.LogException(exception, context);
		}
	}
}
