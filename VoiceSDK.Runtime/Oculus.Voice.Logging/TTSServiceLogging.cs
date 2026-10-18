using System;
using System.Collections.Generic;
using System.Globalization;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Interfaces;
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Data;
using Oculus.Voice.Core.Bindings.Android.PlatformLogger;
using Oculus.Voice.Core.Bindings.Interfaces;
using Oculus.Voice.Core.Utilities;
using UnityEngine;

namespace Oculus.Voice.Logging;

internal class TTSServiceLogging : MonoBehaviour
{
	private struct TTSServiceRequestLog
	{
		public DateTime startTime;

		public Dictionary<string, string> annotations;
	}

	public bool EnableConsoleLogging;

	private IVoiceSDKLogger _voiceSDKLoggerImpl;

	private Dictionary<string, TTSServiceRequestLog> _requests = new Dictionary<string, TTSServiceRequestLog>();

	private const string TTS_FILETYPE_ANNOTATION = "ttsFileType";

	private const string TTS_FILESTREAM_ANNOTATION = "ttsFileStream";

	private const string TTS_START_TIME_ANNOTATION = "ttsStartTime";

	private const string TTS_FIRST_TIME_ANNOTATION = "ttsFirstResponseTime";

	private const string TTS_READY_TIME_ANNOTATION = "ttsReadyTime";

	private const string TTS_FINISH_TIME_ANNOTATION = "ttsFinishedTime";

	private const string TTS_ERROR_ANNOTATION = "ttsError";

	private static bool _initialized;

	public TTSService Service { get; private set; }

	private void Awake()
	{
		Service = base.gameObject.GetComponent<TTSService>();
		InitLogger();
	}

	private void InitLogger()
	{
		_voiceSDKLoggerImpl = new VoiceSDKConsoleLoggerImpl();
		WitConfiguration witConfiguration = Service.GetComponent<IWitConfigurationProvider>()?.Configuration;
		if (witConfiguration != null)
		{
			_voiceSDKLoggerImpl.WitApplication = witConfiguration.GetLoggerAppId();
		}
	}

	private void OnEnable()
	{
		if (_voiceSDKLoggerImpl != null)
		{
			_voiceSDKLoggerImpl.ShouldLogToConsole = EnableConsoleLogging;
		}
		if ((bool)Service)
		{
			Service.Events.WebRequest.OnRequestBegin.AddListener(OnRequestBegin);
			Service.Events.WebRequest.OnRequestCancel.AddListener(OnRequestCancel);
			Service.Events.WebRequest.OnRequestError.AddListener(OnRequestError);
			Service.Events.WebRequest.OnRequestFirstResponse.AddListener(OnRequestFirstResponse);
			Service.Events.WebRequest.OnRequestReady.AddListener(OnRequestReady);
			Service.Events.WebRequest.OnRequestComplete.AddListener(OnRequestComplete);
		}
	}

	private void OnDisable()
	{
		if ((bool)Service)
		{
			Service.Events.WebRequest.OnRequestBegin.RemoveListener(OnRequestBegin);
			Service.Events.WebRequest.OnRequestCancel.RemoveListener(OnRequestCancel);
			Service.Events.WebRequest.OnRequestError.RemoveListener(OnRequestError);
			Service.Events.WebRequest.OnRequestFirstResponse.RemoveListener(OnRequestFirstResponse);
			Service.Events.WebRequest.OnRequestReady.RemoveListener(OnRequestReady);
			Service.Events.WebRequest.OnRequestComplete.RemoveListener(OnRequestComplete);
		}
	}

	private void OnRequestBegin(TTSClipData clipData)
	{
		LogStart(clipData);
	}

	private void OnRequestCancel(TTSClipData clipData)
	{
		LogComplete(clipData, "aborted");
	}

	private void OnRequestError(TTSClipData clipData, string error)
	{
		LogComplete(clipData, error);
	}

	private void OnRequestFirstResponse(TTSClipData clipData)
	{
		LogTimestamp(clipData, "ttsFirstResponseTime");
	}

	private void OnRequestReady(TTSClipData clipData)
	{
		LogTimestamp(clipData, "ttsReadyTime");
	}

	private void OnRequestComplete(TTSClipData clipData)
	{
		LogComplete(clipData);
	}

	private void LogStart(TTSClipData clipData)
	{
		TTSServiceRequestLog requestData = GetRequestData(clipData);
		requestData.startTime = DateTime.UtcNow;
		requestData.annotations = new Dictionary<string, string>();
		LogTimestamp(requestData, "ttsStartTime");
		LogAnnotate(requestData, "ttsFileType", clipData.extension);
		LogAnnotate(requestData, "ttsFileStream", clipData.queryStream.ToString(CultureInfo.InvariantCulture));
		_requests[clipData.queryRequestId] = requestData;
	}

	private TTSServiceRequestLog GetRequestData(TTSClipData clipData)
	{
		if (_requests.ContainsKey(clipData.queryRequestId))
		{
			return _requests[clipData.queryRequestId];
		}
		return default(TTSServiceRequestLog);
	}

	private void LogTimestamp(TTSClipData clipData, string key)
	{
		LogTimestamp(GetRequestData(clipData), key);
	}

	private void LogTimestamp(TTSServiceRequestLog requestData, string key)
	{
		LogAnnotate(requestData, key, DateTimeUtility.ElapsedMilliseconds.ToString());
	}

	private void LogAnnotate(TTSServiceRequestLog requestData, string key, string value)
	{
		if (requestData.annotations != null)
		{
			requestData.annotations[key] = value;
		}
	}

	private void LogComplete(TTSClipData clipData, string error = null)
	{
		TTSServiceRequestLog requestData = GetRequestData(clipData);
		if (requestData.annotations == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(error))
		{
			LogAnnotate(requestData, "ttsError", error);
		}
		LogTimestamp(requestData, "ttsFinishedTime");
		if (_voiceSDKLoggerImpl != null)
		{
			_voiceSDKLoggerImpl.LogInteractionStart(clipData.queryRequestId, "synthesize");
			foreach (string key in requestData.annotations.Keys)
			{
				_voiceSDKLoggerImpl.LogAnnotation(key, requestData.annotations[key]);
			}
			if (string.IsNullOrEmpty(error))
			{
				_voiceSDKLoggerImpl.LogInteractionEndSuccess();
			}
			else
			{
				_voiceSDKLoggerImpl.LogInteractionEndFailure(error);
			}
		}
		_requests.Remove(clipData.queryRequestId);
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			TTSService.OnServiceStart += OnServiceStart;
		}
	}

	private static void OnServiceStart(TTSService service)
	{
		if (service != null && service.GetComponent<TTSServiceLogging>() == null)
		{
			service.gameObject.AddComponent<TTSServiceLogging>();
		}
	}
}
