using System;
using System.Collections.Generic;
using System.Text;
using Meta.Voice.Audio.Decoding;
using Meta.WitAi.Configuration;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace Meta.WitAi;

public static class WitRequestSettings
{
	private static string _operatingSystem;

	private static string _deviceModel;

	private static string _appIdentifier;

	private static string _unityVersion;

	public static Func<UriBuilder, UriBuilder> OnProvideCustomUri;

	public static Action<Dictionary<string, string>> OnProvideCustomHeaders;

	public static Action<StringBuilder> OnProvideCustomUserAgent;

	private static string _localClientUserId;

	private const string PREF_CLIENT_USER_ID = "client-user-id";

	public static string LocalClientUserId
	{
		get
		{
			return _localClientUserId;
		}
		set
		{
			string text = value;
			if (string.IsNullOrEmpty(text))
			{
				text = Guid.NewGuid().ToString();
			}
			else if (string.Equals(text, _localClientUserId))
			{
				return;
			}
			_localClientUserId = text;
			ThreadUtility.CallOnMainThread(delegate
			{
				PlayerPrefs.SetString("client-user-id", _localClientUserId);
				PlayerPrefs.Save();
			});
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		if (_operatingSystem == null)
		{
			_operatingSystem = SystemInfo.operatingSystem;
		}
		if (_deviceModel == null)
		{
			_deviceModel = SystemInfo.deviceModel;
		}
		if (_appIdentifier == null)
		{
			_appIdentifier = Application.identifier;
		}
		if (_unityVersion == null)
		{
			_unityVersion = Application.unityVersion;
		}
		if (string.IsNullOrEmpty(_localClientUserId))
		{
			LocalClientUserId = PlayerPrefs.GetString("client-user-id");
		}
	}

	internal static string GetByteString(byte[] bytes)
	{
		return GetByteString(bytes, 0, bytes.Length);
	}

	internal static string GetByteString(byte[] bytes, int start, int length)
	{
		return BitConverter.ToString(bytes, start, length);
	}

	public static Uri GetUri(IWitRequestConfiguration configuration, string path, Dictionary<string, string> queryParams = null)
	{
		UriBuilder uriBuilder = new UriBuilder();
		IWitRequestEndpointInfo endpointInfo = configuration.GetEndpointInfo();
		uriBuilder.Scheme = endpointInfo.UriScheme;
		uriBuilder.Host = endpointInfo.Authority;
		uriBuilder.Port = endpointInfo.Port;
		uriBuilder.Path = path;
		string witApiVersion = endpointInfo.WitApiVersion;
		uriBuilder.Query = "v=" + witApiVersion;
		if (queryParams != null)
		{
			foreach (string key in queryParams.Keys)
			{
				string text = queryParams[key];
				if (!string.IsNullOrEmpty(text))
				{
					text = UnityWebRequest.EscapeURL(text).Replace("+", "%20");
					UriBuilder uriBuilder2 = uriBuilder;
					uriBuilder2.Query = uriBuilder2.Query + "&" + key + "=" + text;
				}
			}
		}
		if (OnProvideCustomUri != null)
		{
			Delegate[] invocationList = OnProvideCustomUri.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				uriBuilder = ((Func<UriBuilder, UriBuilder>)invocationList[i])(uriBuilder);
			}
		}
		return uriBuilder.Uri;
	}

	public static Dictionary<string, string> GetHeaders(IWitRequestConfiguration configuration, string requestId, bool useServerToken, string clientUserId = null)
	{
		return GetHeaders(configuration, new WitRequestOptions(requestId, clientUserId, (string)null, (VoiceServiceRequestOptions.QueryParam[])null), useServerToken);
	}

	public static Dictionary<string, string> GetHeaders(IWitRequestConfiguration configuration, WitRequestOptions options, bool useServerToken)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["Authorization"] = GetAuthorizationHeader(configuration, useServerToken);
		dictionary["X-Wit-Client-Request-Id"] = ((!string.IsNullOrEmpty(options.RequestId)) ? options.RequestId : WitConstants.GetUniqueId());
		dictionary["X-Wit-Client-Operation-Id"] = ((!string.IsNullOrEmpty(options.OperationId)) ? options.OperationId : WitConstants.GetUniqueId());
		dictionary["client-user-id"] = ((!string.IsNullOrEmpty(options.ClientUserId)) ? options.ClientUserId : LocalClientUserId);
		dictionary["User-Agent"] = GetUserAgentHeader(configuration);
		if (OnProvideCustomHeaders != null)
		{
			Delegate[] invocationList = OnProvideCustomHeaders.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				((Action<Dictionary<string, string>>)invocationList[i])(dictionary);
			}
		}
		return dictionary;
	}

	private static string GetAuthorizationHeader(IWitRequestConfiguration configuration, bool useServerToken)
	{
		string text = configuration.GetClientAccessToken();
		if (useServerToken)
		{
			text = string.Empty;
		}
		text = (string.IsNullOrEmpty(text) ? "XXX" : text.Trim());
		return "Bearer " + text;
	}

	private static string GetUserAgentHeader(IWitRequestConfiguration configuration)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("wit-unity-78.0.0");
		stringBuilder.Append(",\"" + _operatingSystem + "\"");
		stringBuilder.Append(",\"" + _deviceModel + "\"");
		string text = configuration.GetConfigurationId();
		if (string.IsNullOrEmpty(text))
		{
			text = "not-yet-configured";
		}
		stringBuilder.Append("," + text);
		stringBuilder.Append("," + _appIdentifier);
		stringBuilder.Append(",Runtime");
		stringBuilder.Append("," + _unityVersion);
		if (OnProvideCustomUserAgent != null)
		{
			Delegate[] invocationList = OnProvideCustomUserAgent.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				((Action<StringBuilder>)invocationList[i])(stringBuilder);
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetTtsErrors(string textToSpeak, IWitRequestConfiguration configuration)
	{
		if (string.IsNullOrEmpty(textToSpeak))
		{
			return "No text provided";
		}
		if (configuration == null)
		{
			return "No WitConfiguration Set";
		}
		if (string.IsNullOrEmpty(configuration.GetClientAccessToken()))
		{
			return "No WitConfiguration Client Token";
		}
		return string.Empty;
	}

	public static bool CanStreamAudio(TTSWitAudioType witAudioType)
	{
		return witAudioType != TTSWitAudioType.WAV;
	}

	public static string GetAudioMimeType(TTSWitAudioType witAudioType)
	{
		return witAudioType switch
		{
			TTSWitAudioType.PCM => "audio/raw", 
			TTSWitAudioType.OPUS => "audio/opus-demo", 
			_ => "audio/" + witAudioType.ToString().ToLower(), 
		};
	}

	public static string GetAudioExtension(TTSWitAudioType witAudioType, bool includeEvents)
	{
		string text = witAudioType switch
		{
			TTSWitAudioType.MPEG => ".mp3", 
			TTSWitAudioType.PCM => ".raw", 
			TTSWitAudioType.OPUS => ".opusd", 
			_ => "." + witAudioType.ToString().ToLower(), 
		};
		if (includeEvents)
		{
			text += "v";
		}
		return text;
	}

	public static IAudioDecoder GetTtsAudioDecoder(TTSWitAudioType witAudioType)
	{
		return witAudioType switch
		{
			TTSWitAudioType.PCM => new AudioDecoderPcm(AudioDecoderPcmType.Int16), 
			TTSWitAudioType.MPEG => new AudioDecoderMp3(), 
			TTSWitAudioType.WAV => new AudioDecoderWav(), 
			TTSWitAudioType.OPUS => new AudioDecoderOpus(1, 24000), 
			_ => throw new ArgumentException($"{witAudioType} audio decoder not supported"), 
		};
	}

	public static IAudioDecoder GetTtsAudioDecoder(TTSWitAudioType witAudioType, AudioJsonDecodeDelegate onEventsDecoded)
	{
		IAudioDecoder ttsAudioDecoder = GetTtsAudioDecoder(witAudioType);
		if (ttsAudioDecoder != null && onEventsDecoded != null)
		{
			return new AudioDecoderJson(ttsAudioDecoder, onEventsDecoded);
		}
		return ttsAudioDecoder;
	}
}
