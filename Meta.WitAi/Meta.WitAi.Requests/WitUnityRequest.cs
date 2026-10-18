using System;
using System.Threading.Tasks;
using Meta.Voice;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data.Configuration;

namespace Meta.WitAi.Requests;

[Serializable]
public class WitUnityRequest : VoiceServiceRequest
{
	private readonly WitVRequest _request;

	private bool _initialized;

	public WitConfiguration Configuration { get; private set; }

	public string Endpoint { get; set; }

	public bool ShouldPost { get; set; }

	protected override bool DecodeRawResponses => true;

	public WitUnityRequest(WitConfiguration newConfiguration, NLPRequestInputType newDataType, WitRequestOptions newOptions, VoiceServiceRequestEvents newEvents)
		: base(newDataType, newOptions, newEvents)
	{
		Configuration = newConfiguration;
		if (base.InputType == NLPRequestInputType.Text)
		{
			_request = new WitMessageVRequest(Configuration, newOptions.RequestId, newOptions.OperationId);
			_request.OnDownloadProgress += base.SetDownloadProgress;
			Endpoint = Configuration.GetEndpointInfo().Message;
			ShouldPost = false;
		}
		else if (base.InputType == NLPRequestInputType.Audio)
		{
			Endpoint = Configuration.GetEndpointInfo().Speech;
			ShouldPost = true;
		}
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
		if (_request == null)
		{
			return "Request creation failed.";
		}
		return base.GetSendError();
	}

	protected override void HandleSend()
	{
		WitVRequest request = _request;
		WitMessageVRequest messageRequest = request as WitMessageVRequest;
		if (messageRequest != null)
		{
			_request.TimeoutMs = base.Options.TimeoutMs;
			ThreadUtility.BackgroundAsync(Logger, async delegate
			{
				await SendMessageAsync(messageRequest);
			});
		}
	}

	private async Task SendMessageAsync(WitMessageVRequest messageRequest)
	{
		VRequestResponse<string> vRequestResponse = await messageRequest.MessageRequest(Endpoint, ShouldPost, base.Options.Text, base.Options.QueryParams, HandlePartialResponse);
		HandleFinalResponse(vRequestResponse.Value, vRequestResponse.Error);
	}

	private void HandlePartialResponse(string rawResponse)
	{
		HandleResponse(rawResponse, null, final: false);
	}

	private void HandleFinalResponse(string rawResponse, string error)
	{
		HandleResponse(rawResponse, error, final: true);
	}

	protected void HandleResponse(string rawResponse, string error, bool final)
	{
		if (!string.IsNullOrEmpty(error))
		{
			if (final)
			{
				int errorStatusCode = ((_request == null) ? (-1) : _request.ResponseCode);
				HandleFailure(errorStatusCode, error);
			}
		}
		else if (final)
		{
			MakeLastResponseFinal();
		}
		else
		{
			string[] array = rawResponse.Split("\r\n");
			for (int i = 0; i < array.Length; i++)
			{
				HandleRawResponse(array[i], final && i == array.Length - 1);
			}
		}
	}

	protected override void HandleCancel()
	{
		if (_request != null)
		{
			_request.Cancel();
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (!_request.IsComplete)
		{
			_request.Cancel();
		}
	}

	protected override string GetActivateAudioError()
	{
		return "Audio request not yet implemented";
	}

	protected override void HandleAudioActivation()
	{
		SetAudioInputState(VoiceAudioInputState.On);
	}

	protected override void HandleAudioDeactivation()
	{
		SetAudioInputState(VoiceAudioInputState.Off);
	}
}
