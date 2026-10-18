using System;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Voice.Unity;

[HelpURL("https://doc.photonengine.com/en-us/voice/v2")]
public abstract class VoiceComponent : MonoBehaviour, ILoggableDependent, ILoggable
{
	private VoiceLogger logger;

	[SerializeField]
	protected DebugLevel logLevel = DebugLevel.WARNING;

	[SerializeField]
	[HideInInspector]
	private bool ignoreGlobalLogLevel;

	private static string currentPlatform;

	public VoiceLogger Logger
	{
		get
		{
			if (logger == null)
			{
				logger = new VoiceLogger(this, $"{base.name}.{GetType().Name}", logLevel);
			}
			return logger;
		}
		protected set
		{
			logger = value;
		}
	}

	public DebugLevel LogLevel
	{
		get
		{
			if (Logger != null)
			{
				logLevel = Logger.LogLevel;
			}
			return logLevel;
		}
		set
		{
			logLevel = value;
			if (Logger != null)
			{
				Logger.LogLevel = logLevel;
			}
		}
	}

	public bool IgnoreGlobalLogLevel
	{
		get
		{
			return ignoreGlobalLogLevel;
		}
		set
		{
			ignoreGlobalLogLevel = value;
		}
	}

	public static string CurrentPlatform
	{
		get
		{
			if (string.IsNullOrEmpty(currentPlatform))
			{
				currentPlatform = Enum.GetName(typeof(RuntimePlatform), Application.platform);
			}
			return currentPlatform;
		}
	}

	protected virtual void Awake()
	{
		if (logger == null)
		{
			logger = new VoiceLogger(this, $"{base.name}.{GetType().Name}", logLevel);
		}
	}
}
