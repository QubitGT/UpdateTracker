using System;
using System.Text;
using Meta.WitAi;
using UnityEngine;

namespace Oculus.Voice;

public static class VoiceSDKConstants
{
	private static bool _isInitialized;

	private static string _sdkVersion;

	private const string _userAgentPrefix = "voice-sdk-";

	public static string SdkVersion
	{
		get
		{
			if (string.IsNullOrEmpty(_sdkVersion))
			{
				_sdkVersion = "78.0.0";
			}
			return _sdkVersion;
		}
	}

	static VoiceSDKConstants()
	{
		_isInitialized = false;
		_sdkVersion = "78.0.0.8.295";
		Init();
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		if (!_isInitialized)
		{
			_isInitialized = true;
			WitRequestSettings.OnProvideCustomUserAgent = (Action<StringBuilder>)Delegate.Combine(WitRequestSettings.OnProvideCustomUserAgent, new Action<StringBuilder>(OnCustomUserAgent));
		}
	}

	private static void OnCustomUserAgent(StringBuilder sb)
	{
		if (!sb.ToString().StartsWith("voice-sdk-"))
		{
			sb.Insert(0, "voice-sdk-" + SdkVersion + ",");
		}
	}
}
