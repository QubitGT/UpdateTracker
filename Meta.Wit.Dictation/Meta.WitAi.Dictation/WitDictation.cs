using System.IO;
using Meta.Voice.Net.WebSockets;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Dictation.Events;
using Meta.WitAi.Events;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;
using UnityEngine;

namespace Meta.WitAi.Dictation;

public class WitDictation : DictationService, IWitRuntimeConfigProvider, IVoiceEventProvider, IVoiceServiceRequestProvider, IWitConfigurationProvider
{
	[SerializeField]
	private WitRuntimeConfiguration witRuntimeConfiguration;

	private WitService witService;

	private readonly VoiceEvents _voiceEvents = new VoiceEvents();

	public WitRuntimeConfiguration RuntimeConfiguration
	{
		get
		{
			return witRuntimeConfiguration;
		}
		set
		{
			witRuntimeConfiguration = value;
		}
	}

	public WitConfiguration Configuration => RuntimeConfiguration?.witConfiguration;

	public override bool Active
	{
		get
		{
			if (null != witService)
			{
				return witService.Active;
			}
			return false;
		}
	}

	public override bool IsRequestActive
	{
		get
		{
			if (null != witService)
			{
				return witService.IsRequestActive;
			}
			return false;
		}
	}

	public override ITranscriptionProvider TranscriptionProvider
	{
		get
		{
			return witService.TranscriptionProvider;
		}
		set
		{
			witService.TranscriptionProvider = value;
		}
	}

	public override bool MicActive
	{
		get
		{
			if (null != witService)
			{
				return witService.MicActive;
			}
			return false;
		}
	}

	protected override bool ShouldSendMicData
	{
		get
		{
			if (!witRuntimeConfiguration.sendAudioToWit)
			{
				return TranscriptionProvider == null;
			}
			return true;
		}
	}

	public VoiceEvents VoiceEvents => _voiceEvents;

	public override DictationEvents DictationEvents
	{
		get
		{
			return dictationEvents;
		}
		set
		{
			DictationEvents listener = dictationEvents;
			dictationEvents = value;
			if (base.gameObject.activeSelf)
			{
				VoiceEvents.RemoveListener(listener);
				VoiceEvents.AddListener(dictationEvents);
			}
		}
	}

	public VoiceServiceRequest CreateRequest(WitRuntimeConfiguration requestSettings, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		WitConfiguration witConfiguration = requestSettings?.witConfiguration;
		if (witConfiguration != null && witConfiguration.RequestType == WitRequestType.WebSocket)
		{
			return WitSocketRequest.GetDictationRequest(witConfiguration, GetComponent<WitWebSocketAdapter>(), AudioBuffer.Instance, requestOptions, requestEvents);
		}
		return witConfiguration.CreateDictationRequest(requestOptions, requestEvents);
	}

	public override VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		return witService.Activate(requestOptions, requestEvents);
	}

	public override VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		return witService.ActivateImmediately(requestOptions, requestEvents);
	}

	public override void Deactivate()
	{
		witService.Deactivate();
	}

	public override void Cancel()
	{
		witService.DeactivateAndAbortRequest();
	}

	protected override void Awake()
	{
		base.Awake();
		witService = base.gameObject.AddComponent<WitService>();
		witService.VoiceEventProvider = this;
		witService.ConfigurationProvider = this;
		witService.RequestProvider = this;
		witService.TelemetryEventsProvider = this;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		VoiceEvents.AddListener(DictationEvents);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		VoiceEvents.RemoveListener(DictationEvents);
	}

	public void TranscribeFile(string fileName)
	{
		if (CreateRequest(witRuntimeConfiguration, new WitRequestOptions(), new VoiceServiceRequestEvents()) is WitRequest witRequest)
		{
			byte[] postData = File.ReadAllBytes(fileName);
			witRequest.postData = postData;
			witService.ExecuteRequest(witRequest);
		}
	}
}
