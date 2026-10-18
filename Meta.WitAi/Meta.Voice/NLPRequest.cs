using System.Text;
using System.Threading.Tasks;
using Meta.Voice.Logging;
using Meta.WitAi;
using UnityEngine.Events;

namespace Meta.Voice;

[LogCategory(LogCategory.Network)]
public abstract class NLPRequest<TUnityEvent, TOptions, TEvents, TResults, TResponseData> : TranscriptionRequest<TUnityEvent, TOptions, TEvents, TResults> where TUnityEvent : UnityEventBase where TOptions : INLPRequestOptions where TEvents : NLPRequestEvents<TUnityEvent, TResponseData> where TResults : INLPRequestResults<TResponseData>
{
	private bool _initialized;

	private bool _finalized;

	private const int DECODE_DELAY_MS = 5;

	private string _rawResponseLast;

	private int _rawQueued;

	private int _rawDecoded;

	private bool _rawResponseFinal;

	private Task _lastDecode;

	private TResponseData _lastResponse;

	public override IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Network);


	public NLPRequestInputType InputType
	{
		get
		{
			if (base.Options != null)
			{
				return base.Options.InputType;
			}
			return NLPRequestInputType.Audio;
		}
	}

	public TResponseData ResponseData
	{
		get
		{
			if (base.Results != null)
			{
				return base.Results.ResponseData;
			}
			return default(TResponseData);
		}
	}

	protected virtual INLPRequestResponseDecoder<TResponseData> ResponseDecoder => null;

	protected virtual bool DecodeRawResponses => false;

	public virtual bool IsDecoding => _rawQueued > _rawDecoded;

	protected NLPRequest(NLPRequestInputType inputType, TOptions options, TEvents newEvents)
		: base(options, newEvents)
	{
		TOptions options2 = base.Options;
		options2.InputType = inputType;
		_initialized = true;
		_finalized = false;
		SetState(VoiceRequestState.Initialized);
	}

	protected override void SetState(VoiceRequestState newState)
	{
		if (_initialized)
		{
			base.SetState(newState);
		}
	}

	protected override void Log(string log, VLoggerVerbosity logLevel = VLoggerVerbosity.Info)
	{
		IVLogger logger = Logger;
		CorrelationID correlationID = Logger.CorrelationID;
		object[] obj = new object[6] { log, null, null, null, null, null };
		TOptions options = base.Options;
		obj[1] = ((options != null) ? options.RequestId : null);
		obj[2] = base.State;
		obj[3] = base.AudioInputState;
		TResults results = base.Results;
		obj[4] = ((results != null) ? results.Transcription : null);
		obj[5] = InputType;
		logger.Log(correlationID, logLevel, "{0}\nRequest Id: {1}\nRequest State: {2}\nAudio Input State: {3}\nTranscription: {4}\nInput: {5}", obj);
	}

	protected override string GetActivateAudioError()
	{
		if (InputType == NLPRequestInputType.Text)
		{
			return "Cannot activate audio on a text request";
		}
		return string.Empty;
	}

	protected override string GetSendError()
	{
		if (InputType == NLPRequestInputType.Audio && !base.IsAudioInputActivated)
		{
			return "Cannot send audio without activation";
		}
		return base.GetSendError();
	}

	protected virtual void HandleRawResponse(string rawResponse, bool final)
	{
		if (!base.IsActive)
		{
			return;
		}
		if (string.IsNullOrEmpty(rawResponse))
		{
			if (final && DecodeRawResponses)
			{
				HandleFailure("Final response is empty");
			}
		}
		else if (!string.Equals(_rawResponseLast, rawResponse))
		{
			_rawResponseFinal |= final;
			_rawResponseLast = rawResponse;
			OnRawResponse(rawResponse);
			if (DecodeRawResponses && ResponseDecoder != null)
			{
				EnqueueDecode(rawResponse, final);
			}
		}
	}

	protected virtual void OnRawResponse(string rawResponse)
	{
		ThreadUtility.CallOnMainThread(delegate
		{
			base.Events?.OnRawResponse?.Invoke(rawResponse);
		});
	}

	private void EnqueueDecode(string rawResponse, bool final)
	{
		_rawQueued++;
		Task blockingTask = _lastDecode;
		_lastDecode = ThreadUtility.BackgroundAsync(Logger, async delegate
		{
			if (blockingTask != null)
			{
				await blockingTask;
			}
			DecodeRawResponse(rawResponse, final);
		});
	}

	private void DecodeRawResponse(string rawResponse, bool final)
	{
		TResponseData val = ResponseDecoder.Decode(rawResponse);
		_rawDecoded++;
		if (base.IsActive)
		{
			final |= _rawResponseFinal && !IsDecoding;
			_lastResponse = val;
			ApplyResponseData(val, final);
		}
	}

	protected virtual void ApplyResponseData(TResponseData responseData, bool final)
	{
		if (!base.IsActive)
		{
			return;
		}
		if (final)
		{
			if (_finalized)
			{
				return;
			}
			_finalized = true;
		}
		if (responseData == null)
		{
			if (final)
			{
				HandleFailure("Failed to decode partial raw response");
			}
			return;
		}
		string text = ResponseDecoder?.GetResponseError(responseData);
		if (!string.IsNullOrEmpty(text))
		{
			int errorStatusCode = ((ResponseDecoder == null) ? (-1) : ResponseDecoder.GetResponseStatusCode(responseData));
			HandleFailure(errorStatusCode, text);
			return;
		}
		bool num = !responseData.Equals(base.Results.ResponseData);
		base.Results.SetResponseData(responseData);
		string transcription = ResponseDecoder?.GetResponseTranscription(responseData);
		bool flag = ResponseDecoder != null && ResponseDecoder.GetResponseHasTranscription(responseData);
		bool full = ResponseDecoder != null && ResponseDecoder.GetResponseIsTranscriptionFull(responseData);
		if (num && flag)
		{
			ApplyTranscription(transcription, full);
		}
		bool flag2 = ResponseDecoder != null && ResponseDecoder.GetResponseHasPartial(responseData);
		if (num && flag2)
		{
			OnPartialResponse(responseData);
		}
		if (final)
		{
			if (!flag2)
			{
				OnPartialResponse(responseData);
			}
			StringBuilder stringBuilder = new StringBuilder();
			base.Events.OnValidateResponse?.Invoke(responseData, stringBuilder);
			if (stringBuilder.Length > 0)
			{
				HandleFailure($"Response validation failed due to {stringBuilder}");
				return;
			}
			OnFullResponse(responseData);
			HandleSuccess();
		}
	}

	protected virtual void OnPartialResponse(TResponseData responseData)
	{
		ThreadUtility.CallOnMainThread(delegate
		{
			base.Events?.OnPartialResponse?.Invoke(responseData);
		});
	}

	protected virtual void OnFullResponse(TResponseData responseData)
	{
		ThreadUtility.CallOnMainThread(delegate
		{
			base.Events?.OnFullResponse?.Invoke(responseData);
		});
	}

	public virtual void CompleteEarly()
	{
		if (base.IsActive && !_finalized)
		{
			if (ResponseData == null)
			{
				Cancel("Cannot complete early without response data");
			}
			else
			{
				MakeLastResponseFinal();
			}
		}
	}

	protected virtual void MakeLastResponseFinal()
	{
		if (base.IsActive)
		{
			if (IsDecoding)
			{
				_rawResponseFinal = true;
			}
			else
			{
				ApplyResponseData(_lastResponse, final: true);
			}
		}
	}
}
