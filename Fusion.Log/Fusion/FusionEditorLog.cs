using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace Fusion;

public abstract class FusionEditorLog
{
	private static string s_prefixColor;

	[Conditional("FUSION_EDITOR_TRACE")]
	public static void TraceConfig(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Config]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void WarnConfig(string msg)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor/Config]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogConfig(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Config]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void ErrorConfig(string msg)
	{
		UnityEngine.Debug.LogError("<color" + s_prefixColor + ">[FusionEditor/Config]</color> " + msg);
	}

	[Conditional("FUSION_EDITOR_TRACE")]
	public static void TraceInstaller(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Installer]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void WarnInstaller(string msg)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor/Installer]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogInstaller(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Installer]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void ErrorInstaller(string msg)
	{
		UnityEngine.Debug.LogError("<color" + s_prefixColor + ">[FusionEditor/Installer]</color> " + msg);
	}

	public static void SetPrefixColor(Color color)
	{
		SetPrefixColor((Color32)color);
	}

	public static void SetPrefixColor(Color32 c)
	{
		s_prefixColor = "=" + Color32ToHex(c);
	}

	private static string Color32ToHex(Color32 color)
	{
		return $"#{(color.r << 16) | (color.g << 8) | color.b:X6}";
	}

	public static void Initialize(bool isDarkMode)
	{
		if (isDarkMode)
		{
			SetPrefixColor(FusionUnityLoggerBase.DefaultDarkPrefixColor);
		}
		else
		{
			SetPrefixColor(FusionUnityLoggerBase.DefaultLightPrefixColor);
		}
	}

	[Conditional("UNITY_ASSERTIONS")]
	[AssertionMethod]
	[ContractAnnotation("condition: false => halt")]
	public static void Assert(bool condition, string message)
	{
	}

	[Conditional("UNITY_ASSERTIONS")]
	[AssertionMethod]
	[ContractAnnotation("condition: false => halt")]
	public static void Assert(bool condition)
	{
	}

	[Conditional("FUSION_EDITOR_TRACE_IMPORT")]
	public static void TraceImport(string assetPath, string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Import]</color> " + assetPath + ": " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void WarnImport(string assetPath, string msg)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor/Import]</color> " + assetPath + ": " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogImport(string assetPath, string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Import]</color> " + assetPath + ": " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void ErrorImport(string assetPath, string msg)
	{
		UnityEngine.Debug.LogError("<color" + s_prefixColor + ">[FusionEditor/Import]</color> " + assetPath + ": " + msg);
	}

	[Conditional("FUSION_EDITOR_TRACE_IMPORT")]
	public static void TraceImport(string msg, UnityEngine.Object asset)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Import]</color>: " + msg, asset);
	}

	[Conditional("UNITY_EDITOR")]
	public static void WarnImport(string msg, UnityEngine.Object asset)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor/Import]</color>: " + msg, asset);
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogImport(string msg, UnityEngine.Object asset)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Import]</color>: " + msg, asset);
	}

	[Conditional("UNITY_EDITOR")]
	public static void ErrorImport(string msg, UnityEngine.Object asset)
	{
		UnityEngine.Debug.LogError("<color" + s_prefixColor + ">[FusionEditor/Import]</color>: " + msg, asset);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Warn(string msg, UnityEngine.Object obj)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor]</color>: " + msg, obj);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Log(string msg, UnityEngine.Object obj)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor]</color>: " + msg, obj);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Error(string msg, UnityEngine.Object obj)
	{
		UnityEngine.Debug.LogError("<color" + s_prefixColor + ">[FusionEditor]</color>: " + msg, obj);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Exception(string message, Exception ex)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor]</color>: " + message + " <i>See next error log entry for details.</i>");
		ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(ex);
		Thread thread = new Thread((ThreadStart)delegate
		{
			edi.Throw();
		});
		thread.Start();
		thread.Join();
	}

	[Conditional("UNITY_EDITOR")]
	public static void Exception(Exception ex)
	{
		UnityEngine.Debug.LogWarning($"<color{s_prefixColor}>[FusionEditor]</color>: {ex.GetType()} <i>See next error log entry for details.</i>");
		ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(ex);
		Thread thread = new Thread((ThreadStart)delegate
		{
			edi.Throw();
		});
		thread.Start();
		thread.Join();
	}

	[Conditional("FUSION_EDITOR_TRACE")]
	public static void Trace(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Warn(string msg)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Log(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Error(string msg)
	{
		UnityEngine.Debug.LogError("<color" + s_prefixColor + ">[FusionEditor]</color> " + msg);
	}

	[Conditional("FUSION_EDITOR_TRACE_IMPORT")]
	public static void TraceImport(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Import]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void WarnImport(string msg)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor/Import]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogImport(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Import]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void ErrorImport(string msg)
	{
		UnityEngine.Debug.LogError("<color" + s_prefixColor + ">[FusionEditor/Import]</color> " + msg);
	}

	[Conditional("FUSION_EDITOR_TRACE_INSPECTOR")]
	public static void TraceInspector(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Inspector]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void WarnInspector(string msg)
	{
		UnityEngine.Debug.LogWarning("<color" + s_prefixColor + ">[FusionEditor/Inspector]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogInspector(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Inspector]</color> " + msg);
	}

	[Conditional("UNITY_EDITOR")]
	public static void ErrorInspector(string msg)
	{
		UnityEngine.Debug.LogError("<color" + s_prefixColor + ">[FusionEditor/Inspector]</color> " + msg);
	}

	[Conditional("FUSION_EDITOR_TRACE_TEST")]
	public static void TraceTest(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Test]</color> " + msg);
	}

	[Conditional("FUSION_EDITOR_TRACE_MPPM")]
	public static void TraceMppm(string msg)
	{
		UnityEngine.Debug.Log("<color" + s_prefixColor + ">[FusionEditor/Mppm]</color> " + msg);
	}
}
