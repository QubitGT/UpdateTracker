using System;
using System.Globalization;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Voice.Unity;

public class VoiceLogger : ILogger
{
	private readonly UnityEngine.Object context;

	public string Tag { get; set; }

	public DebugLevel LogLevel { get; set; }

	public bool IsErrorEnabled => (int)LogLevel >= 1;

	public bool IsWarningEnabled => (int)LogLevel >= 2;

	public bool IsInfoEnabled => (int)LogLevel >= 3;

	public bool IsDebugEnabled => LogLevel == DebugLevel.ALL;

	public VoiceLogger(UnityEngine.Object context, string tag, DebugLevel level = DebugLevel.ERROR)
	{
		this.context = context;
		Tag = tag;
		LogLevel = level;
	}

	public VoiceLogger(string tag, DebugLevel level = DebugLevel.ERROR)
	{
		Tag = tag;
		LogLevel = level;
	}

	public void LogError(string fmt, params object[] args)
	{
		if (IsErrorEnabled)
		{
			fmt = GetFormatString(fmt);
			if (context == null)
			{
				Debug.LogErrorFormat(fmt, args);
			}
			else
			{
				Debug.LogErrorFormat(context, fmt, args);
			}
		}
	}

	public void LogWarning(string fmt, params object[] args)
	{
		if (IsWarningEnabled)
		{
			fmt = GetFormatString(fmt);
			if (context == null)
			{
				Debug.LogWarningFormat(fmt, args);
			}
			else
			{
				Debug.LogWarningFormat(context, fmt, args);
			}
		}
	}

	public void LogInfo(string fmt, params object[] args)
	{
		if (IsInfoEnabled)
		{
			fmt = GetFormatString(fmt);
			if (context == null)
			{
				Debug.LogFormat(fmt, args);
			}
			else
			{
				Debug.LogFormat(context, fmt, args);
			}
		}
	}

	public void LogDebug(string fmt, params object[] args)
	{
		if (IsDebugEnabled)
		{
			LogInfo(fmt, args);
		}
	}

	private string GetFormatString(string fmt)
	{
		return $"[{Tag}] {GetTimestamp()}:{fmt}";
	}

	private string GetTimestamp()
	{
		return DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss", new CultureInfo("en-US"));
	}
}
