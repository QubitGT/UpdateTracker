using System;
using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Dictation;
using Meta.WitAi.Dictation.Events;
using Meta.WitAi.Events;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;
using Oculus.Voice.Core.Bindings.Android;

namespace Oculus.Voice.Dictation.Bindings.Android;

public class PlatformDictationImpl : BaseAndroidConnectionImpl<PlatformDictationSDKBinding>, IDictationService, ITelemetryEventsProvider, IServiceEvents
{
	private readonly IDictationService _baseService;

	private bool _serviceAvailable = true;

	private WitDictationRuntimeConfiguration _dictationRuntimeConfiguration;

	private DictationListenerBinding _listenerBinding;

	public Action OnServiceNotAvailableEvent;

	public bool PlatformSupportsDictation
	{
		get
		{
			if (service != null && service.IsSupported)
			{
				return _serviceAvailable;
			}
			return false;
		}
	}

	public bool Active => service.Active;

	public bool IsRequestActive => service.IsRequestActive;

	public bool MicActive => service.Active;

	public ITranscriptionProvider TranscriptionProvider { get; set; }

	public DictationEvents DictationEvents
	{
		get
		{
			return _baseService.DictationEvents;
		}
		set
		{
			_baseService.DictationEvents = value;
		}
	}

	public TelemetryEvents TelemetryEvents
	{
		get
		{
			return _baseService.TelemetryEvents;
		}
		set
		{
			_baseService.TelemetryEvents = value;
		}
	}

	public PlatformDictationImpl(IDictationService dictationService)
		: base("com.oculus.assistant.api.unity.dictation.UnityDictationServiceFragment")
	{
		_baseService = dictationService;
	}

	public override void Connect(string version)
	{
		base.Connect(version);
		if (service != null)
		{
			_listenerBinding = new DictationListenerBinding(this, this);
			service.SetListener(_listenerBinding);
		}
	}

	public override void Disconnect()
	{
		base.Disconnect();
	}

	public void SetDictationRuntimeConfiguration(WitDictationRuntimeConfiguration configuration)
	{
		_dictationRuntimeConfiguration = configuration;
	}

	private void Activate()
	{
		service.StartDictation(new DictationConfigurationBinding(_dictationRuntimeConfiguration));
	}

	public VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		Activate();
		return null;
	}

	public VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		Activate();
		return null;
	}

	public void Deactivate()
	{
		service.StopDictation();
	}

	public void Cancel()
	{
		service.StopDictation();
	}

	public void OnServiceNotAvailable(string error, string message)
	{
		_serviceAvailable = false;
		OnServiceNotAvailableEvent?.Invoke();
	}
}
