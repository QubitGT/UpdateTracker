using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.Voice;
using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Events;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;
using Oculus.Voice.Core.Bindings.Android;
using Oculus.Voice.Interfaces;
using UnityEngine;

namespace Oculus.Voice.Bindings.Android;

public class VoiceSDKImpl : BaseAndroidConnectionImpl<VoiceSDKBinding>, IPlatformVoiceService, IVoiceService, IVoiceEventProvider, ITelemetryEventsProvider, IVoiceActivationHandler, IVCBindingEvents
{
	private bool _isServiceAvailable = true;

	public Action OnServiceNotAvailableEvent;

	private IVoiceService _baseVoiceService;

	private bool _isActive;

	private VoiceSDKListenerBinding eventBinding;

	public bool UsePlatformIntegrations
	{
		get
		{
			return true;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public bool PlatformSupportsWit
	{
		get
		{
			if (service.PlatformSupportsWit)
			{
				return _isServiceAvailable;
			}
			return false;
		}
	}

	public bool Active
	{
		get
		{
			if (service.Active)
			{
				return _isActive;
			}
			return false;
		}
	}

	public bool IsRequestActive => service.IsRequestActive;

	public bool MicActive => service.MicActive;

	public HashSet<VoiceServiceRequest> Requests { get; } = new HashSet<VoiceServiceRequest>();


	public ITranscriptionProvider TranscriptionProvider { get; set; }

	public VoiceEvents VoiceEvents
	{
		get
		{
			return _baseVoiceService.VoiceEvents;
		}
		set
		{
			_baseVoiceService.VoiceEvents = value;
		}
	}

	public TelemetryEvents TelemetryEvents
	{
		get
		{
			return _baseVoiceService.TelemetryEvents;
		}
		set
		{
			_baseVoiceService.TelemetryEvents = value;
		}
	}

	public VoiceSDKImpl(IVoiceService baseVoiceService)
		: base("com.oculus.assistant.api.unity.immersivevoicecommands.UnityIVCServiceFragment")
	{
		_baseVoiceService = baseVoiceService;
	}

	public void SetRuntimeConfiguration(WitRuntimeConfiguration configuration)
	{
		service.SetRuntimeConfiguration(configuration);
	}

	public bool CanActivateAudio()
	{
		return true;
	}

	public bool CanSend()
	{
		return true;
	}

	public override void Connect(string version)
	{
		base.Connect(version);
		eventBinding = new VoiceSDKListenerBinding(this, this);
		eventBinding.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);
		service.SetListener(eventBinding);
		service.Connect();
		Debug.Log("Platform integration initialization complete. Platform integrations are " + (PlatformSupportsWit ? "active" : "inactive"));
	}

	public override void Disconnect()
	{
		base.Disconnect();
		if (eventBinding != null)
		{
			eventBinding.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
		}
	}

	private void OnStoppedListening()
	{
		_isActive = false;
	}

	public Task<VoiceServiceRequest> Activate(string text, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		if (requestOptions == null)
		{
			requestOptions = new WitRequestOptions();
		}
		requestOptions.Text = text;
		VoiceServiceRequest request = GetRequest(requestOptions, requestEvents, NLPRequestInputType.Text);
		request.Send();
		return Task.FromResult(request);
	}

	public VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		if (_isActive)
		{
			return null;
		}
		_isActive = true;
		if (requestOptions == null)
		{
			requestOptions = new WitRequestOptions();
		}
		VoiceServiceRequest request = GetRequest(requestOptions, requestEvents, NLPRequestInputType.Audio);
		request.ActivateAudio();
		return request;
	}

	public VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		if (_isActive)
		{
			return null;
		}
		_isActive = true;
		if (requestOptions == null)
		{
			requestOptions = new WitRequestOptions();
		}
		VoiceServiceRequest request = GetRequest(requestOptions, requestEvents, NLPRequestInputType.Audio, audioImmediate: true);
		request.ActivateAudio();
		return request;
	}

	public void Deactivate()
	{
		_isActive = false;
		foreach (VoiceServiceRequest request in Requests)
		{
			if (request.InputType == NLPRequestInputType.Audio)
			{
				request.DeactivateAudio();
			}
		}
	}

	public void DeactivateAndAbortRequest()
	{
		_isActive = false;
		foreach (VoiceServiceRequest request in Requests)
		{
			if (request.InputType == NLPRequestInputType.Audio)
			{
				request.Cancel();
			}
		}
	}

	public void DeactivateAndAbortRequest(VoiceServiceRequest request)
	{
		if (Requests.Contains(request))
		{
			request.Cancel();
		}
	}

	public void OnServiceNotAvailable(string error, string message)
	{
		_isActive = false;
		_isServiceAvailable = false;
		OnServiceNotAvailableEvent?.Invoke();
	}

	private VoiceServiceRequest GetRequest(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents, NLPRequestInputType inputType, bool audioImmediate = false)
	{
		VoiceSDKImplRequest voiceSDKImplRequest = new VoiceSDKImplRequest(service, inputType, audioImmediate, requestOptions, requestEvents);
		Requests.Add(voiceSDKImplRequest);
		return voiceSDKImplRequest;
	}
}
