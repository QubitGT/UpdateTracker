using System;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice;
using Meta.Voice.Net.WebSockets;
using Meta.Voice.Net.WebSockets.Requests;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;

namespace Meta.WitAi.Requests;

[Serializable]
public class WitSocketRequest : VoiceServiceRequest, IAudioUploadHandler, IDataUploadHandler, ILogSource
{
	private bool _initialized;

	internal VoiceErrorSimulationType _simulatedErrorType = (VoiceErrorSimulationType)(-1);

	public WitConfiguration Configuration { get; private set; }

	public WitWebSocketAdapter WebSocketAdapter { get; private set; }

	public AudioBuffer AudioInput { get; private set; }

	public string Endpoint { get; set; }

	public WitAudioRequestOption AudioRequestOption { get; private set; }

	public AudioEncoding AudioEncoding { get; set; }

	public bool IsInputStreamReady { get; private set; }

	public Action OnInputStreamReady { get; set; }

	protected override bool DecodeRawResponses => false;

	public WitWebSocketMessageRequest WebSocketRequest { get; private set; }

	private WitSocketRequest(NLPRequestInputType inputType, WitRequestOptions options = null, VoiceServiceRequestEvents events = null)
		: base(NLPRequestInputType.Text, options, events)
	{
	}

	~WitSocketRequest()
	{
		SetWebSocketRequest(null);
	}

	public static WitSocketRequest GetMessageRequest(WitConfiguration configuration, WitWebSocketAdapter webSocketAdapter, WitRequestOptions options = null, VoiceServiceRequestEvents events = null)
	{
		WitSocketRequest witSocketRequest = new WitSocketRequest(NLPRequestInputType.Text, options, events);
		witSocketRequest.Init(configuration.GetEndpointInfo().Message, WitAudioRequestOption.None, configuration, webSocketAdapter, null);
		return witSocketRequest;
	}

	public static WitSocketRequest GetSpeechRequest(WitConfiguration configuration, WitWebSocketAdapter webSocketAdapter, AudioBuffer audioBuffer, WitRequestOptions options = null, VoiceServiceRequestEvents events = null)
	{
		WitSocketRequest witSocketRequest = new WitSocketRequest(NLPRequestInputType.Audio, options, events);
		witSocketRequest.Init(configuration.GetEndpointInfo().Speech, WitAudioRequestOption.Speech, configuration, webSocketAdapter, audioBuffer);
		return witSocketRequest;
	}

	public static WitSocketRequest GetExternalRequest(WitWebSocketMessageRequest webSocketRequest, WitConfiguration configuration, WitWebSocketAdapter webSocketAdapter, WitRequestOptions options = null, VoiceServiceRequestEvents events = null)
	{
		WitSocketRequest witSocketRequest = new WitSocketRequest(NLPRequestInputType.Text, options, events);
		witSocketRequest.SetWebSocketRequest(webSocketRequest);
		witSocketRequest.Init(webSocketRequest.Endpoint, WitAudioRequestOption.None, configuration, webSocketAdapter, null);
		witSocketRequest.Results.ResponseData = webSocketRequest.ResponseData;
		return witSocketRequest;
	}

	public static WitSocketRequest GetTranscribeRequest(WitConfiguration configuration, WitWebSocketAdapter webSocketAdapter, AudioBuffer audioBuffer, WitRequestOptions options = null, VoiceServiceRequestEvents events = null)
	{
		WitSocketRequest witSocketRequest = new WitSocketRequest(NLPRequestInputType.Audio, options, events);
		witSocketRequest.Init("transcribe", WitAudioRequestOption.Transcribe, configuration, webSocketAdapter, audioBuffer);
		return witSocketRequest;
	}

	public static WitSocketRequest GetDictationRequest(WitConfiguration configuration, WitWebSocketAdapter webSocketAdapter, AudioBuffer audioBuffer, WitRequestOptions options = null, VoiceServiceRequestEvents events = null)
	{
		WitSocketRequest witSocketRequest = new WitSocketRequest(NLPRequestInputType.Audio, options, events);
		witSocketRequest.Init("transcribe", WitAudioRequestOption.Dictation, configuration, webSocketAdapter, audioBuffer);
		return witSocketRequest;
	}

	private void Init(string endpoint, WitAudioRequestOption audioOption, WitConfiguration configuration, WitWebSocketAdapter webSocketAdapter, AudioBuffer audioBuffer)
	{
		Endpoint = endpoint;
		AudioRequestOption = audioOption;
		Configuration = configuration;
		WebSocketAdapter = webSocketAdapter;
		AudioInput = audioBuffer;
		base.Options.InputType = ((audioOption != 0) ? NLPRequestInputType.Audio : NLPRequestInputType.Text);
		_initialized = true;
		SetState(VoiceRequestState.Initialized);
	}

	protected override void SetState(VoiceRequestState newState)
	{
		if (_initialized)
		{
			base.SetState(newState);
		}
	}

	protected override string GetSendError()
	{
		if (Configuration == null)
		{
			return "Cannot send request without a valid configuration.";
		}
		if (AudioInput == null && base.Options.InputType == NLPRequestInputType.Audio)
		{
			return "No audio input provided";
		}
		return base.GetSendError();
	}

	protected override void HandleSend()
	{
		if (base.Options.InputType == NLPRequestInputType.Text)
		{
			base.Options.QueryParams["q"] = base.Options.Text;
			WitWebSocketMessageRequest webSocketRequest = new WitWebSocketMessageRequest(Endpoint, base.Options.QueryParams, base.Options.RequestId, base.Options.ClientUserId, base.Options.OperationId);
			SetWebSocketRequest(webSocketRequest);
		}
		else if (base.Options.InputType == NLPRequestInputType.Audio)
		{
			base.Options.QueryParams["content_type"] = AudioEncoding.ToString();
			WitWebSocketMessageRequest witWebSocketMessageRequest = CreateAudioWebSocketRequest();
			if (witWebSocketMessageRequest != null)
			{
				SetWebSocketRequest(witWebSocketMessageRequest);
			}
		}
		if (WebSocketRequest != null && !(WebSocketAdapter == null))
		{
			WebSocketRequest.TimeoutMs = base.Options.TimeoutMs;
			WebSocketAdapter.SendRequest(WebSocketRequest);
		}
	}

	private WitWebSocketMessageRequest CreateAudioWebSocketRequest()
	{
		return AudioRequestOption switch
		{
			WitAudioRequestOption.Speech => new WitWebSocketSpeechRequest(Endpoint, base.Options.QueryParams, base.Options.RequestId, base.Options.ClientUserId, base.Options.OperationId), 
			WitAudioRequestOption.Transcribe => new WitWebSocketTranscribeRequest(Endpoint, base.Options.QueryParams, base.Options.RequestId, base.Options.ClientUserId, base.Options.OperationId, multipleSegments: false), 
			WitAudioRequestOption.Dictation => new WitWebSocketTranscribeRequest(Endpoint, base.Options.QueryParams, base.Options.RequestId, base.Options.ClientUserId, base.Options.OperationId), 
			_ => null, 
		};
	}

	private void SetWebSocketRequest(WitWebSocketMessageRequest request)
	{
		if (WebSocketRequest != null)
		{
			WitWebSocketMessageRequest webSocketRequest = WebSocketRequest;
			webSocketRequest.OnRawResponse = (Action<string>)Delegate.Remove(webSocketRequest.OnRawResponse, new Action<string>(ReturnRawResponse));
			WitWebSocketMessageRequest webSocketRequest2 = WebSocketRequest;
			webSocketRequest2.OnFirstResponse = (Action<IWitWebSocketRequest>)Delegate.Remove(webSocketRequest2.OnFirstResponse, new Action<IWitWebSocketRequest>(ReturnInputReady));
			WebSocketRequest.OnDecodedResponse -= ReturnDecodedResponse;
			WitWebSocketMessageRequest webSocketRequest3 = WebSocketRequest;
			webSocketRequest3.OnComplete = (Action<IWitWebSocketRequest>)Delegate.Remove(webSocketRequest3.OnComplete, new Action<IWitWebSocketRequest>(ReturnSuccessOrError));
		}
		WebSocketRequest = request;
		if (WebSocketRequest != null)
		{
			WitWebSocketMessageRequest webSocketRequest4 = WebSocketRequest;
			webSocketRequest4.OnRawResponse = (Action<string>)Delegate.Combine(webSocketRequest4.OnRawResponse, new Action<string>(ReturnRawResponse));
			WitWebSocketMessageRequest webSocketRequest5 = WebSocketRequest;
			webSocketRequest5.OnFirstResponse = (Action<IWitWebSocketRequest>)Delegate.Combine(webSocketRequest5.OnFirstResponse, new Action<IWitWebSocketRequest>(ReturnInputReady));
			WebSocketRequest.OnDecodedResponse += ReturnDecodedResponse;
			WitWebSocketMessageRequest webSocketRequest6 = WebSocketRequest;
			webSocketRequest6.OnComplete = (Action<IWitWebSocketRequest>)Delegate.Combine(webSocketRequest6.OnComplete, new Action<IWitWebSocketRequest>(ReturnSuccessOrError));
		}
		if (_simulatedErrorType != (VoiceErrorSimulationType)(-1))
		{
			WebSocketRequest.SimulatedErrorType = _simulatedErrorType;
		}
	}

	private void ReturnRawResponse(string rawResponse)
	{
		HandleRawResponse(rawResponse, final: false);
	}

	private void ReturnInputReady(IWitWebSocketRequest request)
	{
		if (request is WitWebSocketSpeechRequest { IsReadyForInput: not false })
		{
			IsInputStreamReady = true;
			OnInputStreamReady?.Invoke();
		}
	}

	private void ReturnDecodedResponse(WitResponseNode responseNode)
	{
		ThreadUtility.CallOnMainThread(delegate
		{
			ApplyResponseData(responseNode, isFinal: false);
		});
	}

	private void ReturnSuccessOrError(IWitWebSocketRequest request)
	{
		if (!base.IsActive)
		{
			return;
		}
		if (string.IsNullOrEmpty(request.Error))
		{
			ApplyResponseData(base.ResponseData, isFinal: true);
			return;
		}
		int num = request.Code;
		if (num == 0)
		{
			num = -1;
		}
		HandleFailure(num, request.Error);
	}

	protected override void HandleCancel()
	{
		if (WebSocketRequest != null)
		{
			WebSocketRequest.Cancel();
		}
	}

	protected override string GetActivateAudioError()
	{
		string activateAudioError = base.GetActivateAudioError();
		if (!string.IsNullOrEmpty(activateAudioError))
		{
			return activateAudioError;
		}
		if (AudioInput == null && base.Options.InputType == NLPRequestInputType.Audio)
		{
			return "No audio input provided";
		}
		return string.Empty;
	}

	protected override void HandleAudioActivation()
	{
		SetAudioInputState(VoiceAudioInputState.On);
	}

	public void Write(byte[] buffer, int offset, int length)
	{
		if (base.IsListening && WebSocketRequest is WitWebSocketSpeechRequest witWebSocketSpeechRequest)
		{
			witWebSocketSpeechRequest.SendAudioData(buffer, offset, length);
		}
	}

	protected override void HandleAudioDeactivation()
	{
		bool flag = base.InputType == NLPRequestInputType.Audio && WebSocketRequest == null;
		if (WebSocketRequest is WitWebSocketSpeechRequest witWebSocketSpeechRequest)
		{
			witWebSocketSpeechRequest.CloseAudioStream();
			flag = !witWebSocketSpeechRequest.HasSentAudio && !witWebSocketSpeechRequest.IsComplete;
		}
		SetAudioInputState(VoiceAudioInputState.Off);
		if (flag)
		{
			Logger.Verbose("Audio input disabled prior to transmission\nRequest Id: {0}\n", base.Options.RequestId, null, null, null, "HandleAudioDeactivation", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Requests\\WitSocketRequest.cs", 458);
			Cancel("Request cancelled prior to transmission begin");
		}
	}

	internal override void SimulateError(VoiceErrorSimulationType errorType)
	{
		_simulatedErrorType = errorType;
		if (WebSocketRequest != null)
		{
			WebSocketRequest.SimulatedErrorType = _simulatedErrorType;
		}
	}
}
