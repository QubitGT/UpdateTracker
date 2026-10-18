using System;
using System.Collections.Generic;
using System.Linq;
using Meta.WitAi;
using Meta.WitAi.Events;
using Meta.WitAi.Requests;
using UnityEngine;

namespace Oculus.Voice.Bindings.Android;

public class VoiceSDKListenerBinding : AndroidJavaProxy
{
	public enum StoppedListeningReason
	{
		NoReasonProvided,
		Inactivity,
		Timeout,
		Deactivation
	}

	private IVoiceService _voiceService;

	private readonly IVCBindingEvents _bindingEvents;

	public VoiceEvents VoiceEvents => _voiceService.VoiceEvents;

	public TelemetryEvents TelemetryEvents => _voiceService.TelemetryEvents;

	public VoiceSDKListenerBinding(IVoiceService voiceService, IVCBindingEvents bindingEvents)
		: base("com.oculus.assistant.api.voicesdk.immersivevoicecommands.IVCEventsListener")
	{
		_voiceService = voiceService;
		_bindingEvents = bindingEvents;
	}

	private VoiceServiceRequest GetRequest(string requestId)
	{
		HashSet<VoiceServiceRequest> requests = _voiceService.Requests;
		if (requests == null || requests.Count == 0)
		{
			return null;
		}
		foreach (VoiceServiceRequest item in requests)
		{
			string b = item?.Options?.RequestId;
			if (string.Equals(requestId, b))
			{
				return item;
			}
		}
		return requests.First();
	}

	public void onStartListening(string requestId)
	{
		VoiceEvents.OnStartListening?.Invoke();
	}

	public void onStartListening()
	{
		onStartListening(null);
	}

	public void onStoppedListening(int reason, string requestId)
	{
		VoiceServiceRequest request = GetRequest(requestId);
		VoiceEvents.OnStoppedListening?.Invoke();
		switch ((StoppedListeningReason)reason)
		{
		case StoppedListeningReason.Inactivity:
			VoiceEvents.OnStoppedListeningDueToInactivity?.Invoke();
			request.Cancel();
			break;
		case StoppedListeningReason.Timeout:
			VoiceEvents.OnStoppedListeningDueToTimeout?.Invoke();
			request.Cancel();
			break;
		case StoppedListeningReason.Deactivation:
			VoiceEvents.OnStoppedListeningDueToDeactivation?.Invoke();
			break;
		case StoppedListeningReason.NoReasonProvided:
			break;
		}
	}

	public void onStoppedListening(int reason)
	{
		onStoppedListening(reason, null);
	}

	public void onRequestCreated(string requestId)
	{
		if (GetRequest(requestId) is VoiceSDKImplRequest voiceSDKImplRequest)
		{
			voiceSDKImplRequest.HandleTransmissionBegan();
		}
	}

	private void onRequestCreated()
	{
		onRequestCreated(null);
	}

	public void onPartialTranscription(string transcription, string requestId)
	{
		if (GetRequest(requestId) is VoiceSDKImplRequest voiceSDKImplRequest)
		{
			voiceSDKImplRequest.HandlePartialTranscription(transcription);
		}
	}

	public void onPartialTranscription(string transcription)
	{
		onPartialTranscription(transcription, null);
	}

	public void onFullTranscription(string transcription, string requestId)
	{
		if (GetRequest(requestId) is VoiceSDKImplRequest voiceSDKImplRequest)
		{
			voiceSDKImplRequest.HandleFullTranscription(transcription);
		}
	}

	public void onFullTranscription(string transcription)
	{
		onFullTranscription(transcription, null);
	}

	public void onPartialResponse(string responseJson, string requestId)
	{
		if (GetRequest(requestId) is VoiceSDKImplRequest voiceSDKImplRequest)
		{
			voiceSDKImplRequest.HandlePartialResponse(responseJson);
		}
	}

	public void onPartialResponse(string responseJson)
	{
		onPartialResponse(responseJson, null);
	}

	public void onAborted(string requestId)
	{
		if (GetRequest(requestId) is VoiceSDKImplRequest voiceSDKImplRequest)
		{
			voiceSDKImplRequest.HandleCanceled();
		}
	}

	public void onAborted()
	{
		onAborted(null);
	}

	public void onError(string error, string message, string errorBody, string requestId)
	{
		if (GetRequest(requestId) is VoiceSDKImplRequest voiceSDKImplRequest)
		{
			voiceSDKImplRequest.HandleError(error, message, errorBody);
		}
	}

	public void onError(string error, string message, string errorBody)
	{
		onError(error, message, errorBody, null);
	}

	public void onResponse(string responseJson, string requestId)
	{
		if (GetRequest(requestId) is VoiceSDKImplRequest voiceSDKImplRequest)
		{
			voiceSDKImplRequest.HandleResponse(responseJson);
		}
	}

	public void onResponse(string responseJson)
	{
		onResponse(responseJson, null);
	}

	public void onMicLevelChanged(float level, string requestId)
	{
		VoiceEvents.OnMicLevelChanged?.Invoke(level);
	}

	public void onMicLevelChanged(float level)
	{
		onMicLevelChanged(level, null);
	}

	public void onMicDataSent(string requestId)
	{
		VoiceEvents.OnMicDataSent?.Invoke();
	}

	public void onMicDataSent()
	{
		onMicDataSent(null);
	}

	public void onMinimumWakeThresholdHit(string requestId)
	{
		VoiceEvents.OnMinimumWakeThresholdHit?.Invoke();
	}

	public void onMinimumWakeThresholdHit()
	{
		onMinimumWakeThresholdHit(null);
	}

	public void onRequestCompleted(string requestId)
	{
	}

	public void onRequestCompleted()
	{
		onRequestCompleted(null);
	}

	public void onServiceNotAvailable(string error, string message)
	{
		VLog.W("Platform service is not available: " + error + " - " + message);
		_bindingEvents.OnServiceNotAvailable(error, message);
	}

	public void onAudioDurationTrackerFinished(long timestamp, double duration)
	{
		long arg = NativeTimestampToDateTime(timestamp).Ticks / 10000;
		TelemetryEvents.OnAudioTrackerFinished?.Invoke(arg, duration);
	}

	private DateTime NativeTimestampToDateTime(long javaTimestamp)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(javaTimestamp);
	}
}
