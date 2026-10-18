using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Meta.Voice;
using Meta.Voice.Logging;
using Meta.Voice.TelemetryUtilities;
using Meta.WitAi.Configuration;
using Meta.WitAi.Events;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi;

[LogCategory(LogCategory.SpeechService)]
public abstract class BaseSpeechService : MonoBehaviour
{
	public bool ShouldWrap = true;

	public bool ShouldLog = true;

	private ConcurrentDictionary<int, object> _customRequestEvents = new ConcurrentDictionary<int, object>();

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.SpeechService);


	public HashSet<VoiceServiceRequest> Requests { get; } = new HashSet<VoiceServiceRequest>();


	public virtual bool Active
	{
		get
		{
			if (Requests != null)
			{
				return Requests.Count > 0;
			}
			return false;
		}
	}

	public virtual bool IsAudioInputActive => GetAudioRequest()?.IsAudioInputActivated ?? false;

	protected virtual SpeechEvents GetSpeechEvents()
	{
		return null;
	}

	protected virtual VoiceServiceRequest GetAudioRequest()
	{
		return Requests?.FirstOrDefault((VoiceServiceRequest request) => request.InputType == NLPRequestInputType.Audio);
	}

	public virtual string GetActivateAudioError()
	{
		if (IsAudioInputActive)
		{
			return "Audio input is already being performed for this service.";
		}
		return string.Empty;
	}

	public virtual bool CanActivateAudio()
	{
		return string.IsNullOrEmpty(GetActivateAudioError());
	}

	public virtual string GetSendError()
	{
		return string.Empty;
	}

	public virtual bool CanSend()
	{
		return string.IsNullOrEmpty(GetSendError());
	}

	protected virtual void OnEnable()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			Logger.Error("Unable to reach the internet. Check your connection.");
		}
		GetSpeechEvents()?.OnRequestInitialized.AddListener(OnRequestInit);
	}

	protected virtual void OnDisable()
	{
		GetSpeechEvents()?.OnRequestInitialized.RemoveListener(OnRequestInit);
	}

	public virtual void Deactivate()
	{
		VoiceServiceRequest[] array = Requests.ToArray();
		foreach (VoiceServiceRequest request in array)
		{
			Deactivate(request);
		}
	}

	public virtual void Deactivate(VoiceServiceRequest request)
	{
		if (request != null && request.IsLocalRequest)
		{
			request.DeactivateAudio();
		}
	}

	public virtual void DeactivateAndAbortRequest()
	{
		VoiceServiceRequest[] array = Requests.ToArray();
		foreach (VoiceServiceRequest request in array)
		{
			DeactivateAndAbortRequest(request);
		}
	}

	public virtual void DeactivateAndAbortRequest(VoiceServiceRequest request)
	{
		if (request != null && request.IsLocalRequest)
		{
			request.Cancel();
		}
	}

	public virtual void SetupRequestParameters(ref WitRequestOptions options, ref VoiceServiceRequestEvents events)
	{
		if (options == null)
		{
			options = new WitRequestOptions();
		}
		if (events == null)
		{
			events = new VoiceServiceRequestEvents();
		}
		if (ShouldWrap)
		{
			GetSpeechEvents().OnRequestOptionSetup?.Invoke(options);
		}
	}

	public virtual bool WrapRequest(VoiceServiceRequest request)
	{
		if (request == null)
		{
			Log(null, "Cannot wrap a null VoiceServiceRequest", warn: true);
			return false;
		}
		if (request.State == VoiceRequestState.Canceled)
		{
			RuntimeTelemetry.Instance.LogEventTermination((OperationID)request.Options.OperationId, TerminationReason.Canceled);
			OnRequestCancel(request);
			OnRequestComplete(request);
			return true;
		}
		if (request.State == VoiceRequestState.Failed)
		{
			RuntimeTelemetry.Instance.LogEventTermination((OperationID)request.Options.OperationId, TerminationReason.Failed);
			OnRequestFailed(request);
			OnRequestComplete(request);
			return true;
		}
		if (request.State == VoiceRequestState.Successful)
		{
			RuntimeTelemetry.Instance.LogEventTermination((OperationID)request.Options.OperationId);
			OnRequestPartialResponse(request, request?.ResponseData);
			OnRequestSuccess(request);
			OnRequestComplete(request);
			return true;
		}
		if (ShouldWrap)
		{
			GetSpeechEvents()?.OnRequestInitialized?.Invoke(request);
			if (request.State == VoiceRequestState.Transmitting)
			{
				OnRequestSend(request);
			}
		}
		return true;
	}

	protected virtual void Log(VoiceServiceRequest request, string log, bool warn = false)
	{
		if (ShouldLog)
		{
			if (warn)
			{
				Logger.Warning("{0}\nRequest Id: {1}", log, request?.Options?.RequestId);
			}
			else
			{
				Logger.Info(log, null, null, null, null, "Log", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\BaseSpeechService.cs", 275);
			}
		}
	}

	protected virtual void OnRequestInit(VoiceServiceRequest request)
	{
		if (!Requests.Contains(request))
		{
			SetEventListeners(request, addListeners: true);
			Requests.Add(request);
			Log(request, "Request Initialized");
			GetSpeechEvents()?.OnRequestCreated?.Invoke((request is WitRequest witRequest) ? witRequest : null);
		}
	}

	protected virtual void OnRequestStartListening(VoiceServiceRequest request)
	{
		Log(request, "Request Start Listening");
		GetSpeechEvents()?.OnStartListening?.Invoke();
	}

	protected virtual void OnRequestStopListening(VoiceServiceRequest request)
	{
		Log(request, "Request Stop Listening");
		GetSpeechEvents()?.OnStoppedListening?.Invoke();
	}

	protected virtual void OnRequestSend(VoiceServiceRequest request)
	{
		Log(request, "Request Send");
		GetSpeechEvents()?.OnSend?.Invoke(request);
	}

	protected virtual void OnRequestRawResponse(VoiceServiceRequest request, string rawResponse)
	{
		GetSpeechEvents()?.OnRawResponse?.Invoke(rawResponse);
	}

	protected virtual void OnRequestPartialTranscription(VoiceServiceRequest request, string transcription)
	{
		Log(request, "Request partial transcription received \nText: " + transcription);
		GetSpeechEvents()?.OnPartialTranscription?.Invoke(transcription);
		GetSpeechEvents()?.OnUserPartialTranscription?.Invoke(request.Options.ClientUserId, transcription);
	}

	protected virtual void OnRequestFullTranscription(VoiceServiceRequest request, string transcription)
	{
		Log(request, "Request Full Transcription received\nText: " + transcription);
		GetSpeechEvents()?.OnFullTranscription?.Invoke(transcription);
		GetSpeechEvents()?.OnUserFullTranscription?.Invoke(request.Options.ClientUserId, transcription);
	}

	protected virtual void OnRequestPartialResponse(VoiceServiceRequest request, WitResponseNode responseData)
	{
		if (responseData != null)
		{
			GetSpeechEvents()?.OnPartialResponse?.Invoke(responseData);
		}
	}

	protected virtual void OnRequestCancel(VoiceServiceRequest request)
	{
		string text = request?.Results?.Message;
		Log(request, "Request Canceled\nReason: " + text);
		GetSpeechEvents()?.OnCanceled?.Invoke(text);
		if (!string.Equals(text, "Request cancelled prior to transmission begin"))
		{
			GetSpeechEvents()?.OnAborted?.Invoke();
		}
	}

	protected virtual void OnRequestFailed(VoiceServiceRequest request)
	{
		string text = $"HTTP Error {request.Results.StatusCode}";
		string text2 = request?.Results?.Message;
		string text3 = text2;
		if (string.Equals(text3, "timeout"))
		{
			text3 += $"\nTimeout Ms: {request.Options.TimeoutMs}";
		}
		Log(request, "Request Failed\n" + text + ": " + text3, warn: true);
		GetSpeechEvents()?.OnError?.Invoke(text, text2);
		GetSpeechEvents()?.OnRequestCompleted?.Invoke();
	}

	protected virtual void OnRequestSuccess(VoiceServiceRequest request)
	{
		Log(request, "Request Success");
		GetSpeechEvents()?.OnResponse?.Invoke(request?.ResponseData);
		GetSpeechEvents()?.OnRequestCompleted?.Invoke();
	}

	protected virtual void OnRequestComplete(VoiceServiceRequest request)
	{
		if (Requests.Contains(request))
		{
			SetEventListeners(request, addListeners: false);
			Requests.Remove(request);
		}
		Log(request, $"Request Complete\nRemaining: {Requests.Count}");
		GetSpeechEvents()?.OnComplete?.Invoke(request);
	}

	protected virtual void SetEventListeners(VoiceServiceRequest request, bool addListeners)
	{
		VoiceServiceRequestEvents events = request.Events;
		events.OnStartListening.SetListener(OnRequestStartListening, addListeners);
		events.OnStopListening.SetListener(OnRequestStopListening, addListeners);
		events.OnSend.SetListener(OnRequestSend, addListeners);
		events.OnSuccess.SetListener(OnRequestSuccess, addListeners);
		events.OnFailed.SetListener(OnRequestFailed, addListeners);
		events.OnCancel.SetListener(OnRequestCancel, addListeners);
		events.OnComplete.SetListener(OnRequestComplete, addListeners);
		SetRequestEventListener(events.OnRawResponse, request, OnRequestRawResponse, addListeners);
		SetRequestEventListener(events.OnPartialTranscription, request, OnRequestPartialTranscription, addListeners);
		SetRequestEventListener(events.OnFullTranscription, request, OnRequestFullTranscription, addListeners);
		SetRequestEventListener(events.OnPartialResponse, request, OnRequestPartialResponse, addListeners);
	}

	private void SetRequestEventListener<TParam>(UnityEvent<TParam> baseEvent, VoiceServiceRequest request, UnityAction<VoiceServiceRequest, TParam> callbackWithRequest, bool addListener)
	{
		int hashCode = baseEvent.GetHashCode();
		object value;
		if (addListener)
		{
			UnityAction<TParam> unityAction = delegate(TParam param)
			{
				callbackWithRequest?.Invoke(request, param);
			};
			_customRequestEvents[hashCode] = unityAction;
			baseEvent.AddListener(unityAction);
		}
		else if (_customRequestEvents.TryRemove(hashCode, out value) && value is UnityAction<TParam> call)
		{
			baseEvent.RemoveListener(call);
		}
	}
}
