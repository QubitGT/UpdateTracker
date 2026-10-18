using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Meta.Conduit;
using Meta.Voice;
using Meta.Voice.TelemetryUtilities;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Data.Intents;
using Meta.WitAi.Events;
using Meta.WitAi.Events.UnityEventListeners;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using UnityEngine;

namespace Meta.WitAi;

public abstract class VoiceService : BaseSpeechService, IVoiceService, IVoiceEventProvider, ITelemetryEventsProvider, IVoiceActivationHandler, IInstanceResolver, IAudioEventProvider
{
	private WitConfiguration _witConfiguration;

	private readonly IParameterProvider _conduitParameterProvider = new ParameterProvider();

	[Tooltip("Events that will fire before, during and after an activation")]
	[SerializeField]
	protected VoiceEvents events = new VoiceEvents();

	protected TelemetryEvents telemetryEvents = new TelemetryEvents();

	protected bool _waitingForFirstPartialAudio = true;

	private bool UseIntentAttributes
	{
		get
		{
			if ((bool)WitConfiguration)
			{
				return WitConfiguration.useIntentAttributes;
			}
			return false;
		}
	}

	private bool UseConduit
	{
		get
		{
			if (UseIntentAttributes)
			{
				return WitConfiguration.useConduit;
			}
			return false;
		}
	}

	public virtual bool UsePlatformIntegrations
	{
		get
		{
			return false;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public WitConfiguration WitConfiguration
	{
		get
		{
			if (_witConfiguration == null)
			{
				_witConfiguration = GetComponent<IWitConfigurationProvider>()?.Configuration;
			}
			return _witConfiguration;
		}
		set
		{
			_witConfiguration = value;
		}
	}

	internal IConduitDispatcher ConduitDispatcher { get; set; }

	public virtual bool IsRequestActive => base.Active;

	public abstract ITranscriptionProvider TranscriptionProvider { get; set; }

	public abstract bool MicActive { get; }

	public virtual VoiceEvents VoiceEvents
	{
		get
		{
			return events;
		}
		set
		{
			events = value;
		}
	}

	public virtual TelemetryEvents TelemetryEvents
	{
		get
		{
			return telemetryEvents;
		}
		set
		{
			telemetryEvents = value;
		}
	}

	public IAudioInputEvents AudioEvents => VoiceEvents;

	public ITranscriptionEvent TranscriptionEvents => VoiceEvents;

	protected abstract bool ShouldSendMicData { get; }

	protected override SpeechEvents GetSpeechEvents()
	{
		return VoiceEvents;
	}

	protected VoiceService()
	{
		_conduitParameterProvider.SetSpecializedParameter("@WitResponseNode", typeof(WitResponseNode));
		_conduitParameterProvider.SetSpecializedParameter("@VoiceSession", typeof(VoiceSession));
		ConduitDispatcherFactory conduitDispatcherFactory = new ConduitDispatcherFactory(this);
		ConduitDispatcher = conduitDispatcherFactory.GetDispatcher();
	}

	public void Activate(string text)
	{
		ThreadUtility.BackgroundAsync(base.Logger, async () => await Activate(text, new WitRequestOptions())).WrapErrors();
	}

	public Task<VoiceServiceRequest> Activate(string text, WitRequestOptions requestOptions)
	{
		return Activate(text, requestOptions, new VoiceServiceRequestEvents());
	}

	public Task<VoiceServiceRequest> Activate(string text, VoiceServiceRequestEvents requestEvents)
	{
		return Activate(text, new WitRequestOptions(), requestEvents);
	}

	public abstract Task<VoiceServiceRequest> Activate(string text, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);

	public void Activate()
	{
		Activate(new WitRequestOptions());
	}

	public void Activate(WitRequestOptions requestOptions)
	{
		Activate(requestOptions, new VoiceServiceRequestEvents());
	}

	public VoiceServiceRequest Activate(VoiceServiceRequestEvents requestEvents)
	{
		return Activate(new WitRequestOptions(), requestEvents);
	}

	public abstract VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);

	public void ActivateImmediately()
	{
		ActivateImmediately(new WitRequestOptions());
	}

	public void ActivateImmediately(WitRequestOptions requestOptions)
	{
		ActivateImmediately(requestOptions, new VoiceServiceRequestEvents());
	}

	public VoiceServiceRequest ActivateImmediately(VoiceServiceRequestEvents requestEvents)
	{
		return ActivateImmediately(new WitRequestOptions(), requestEvents);
	}

	public abstract VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);

	protected override void OnRequestPartialResponse(VoiceServiceRequest request, WitResponseNode responseNode)
	{
		if (_waitingForFirstPartialAudio)
		{
			_waitingForFirstPartialAudio = false;
			RuntimeTelemetry.Instance.LogPoint((OperationID)request.Options.OperationId, RuntimeTelemetryPoint.FirstPartialAudioFromServer);
		}
		base.OnRequestPartialResponse(request, responseNode);
		OnValidateEarly(request, responseNode);
	}

	protected override void OnRequestSend(VoiceServiceRequest request)
	{
		_waitingForFirstPartialAudio = true;
		base.OnRequestSend(request);
	}

	protected virtual void OnValidateEarly(VoiceServiceRequest request, WitResponseNode responseNode)
	{
		if (request == null || request.State != VoiceRequestState.Transmitting || responseNode == null || VoiceEvents.OnValidatePartialResponse == null)
		{
			return;
		}
		VoiceSession voiceSession = GetVoiceSession(responseNode);
		VoiceEvents.OnValidatePartialResponse.Invoke(voiceSession);
		if (UseConduit)
		{
			WitIntentData firstIntentData = responseNode.GetFirstIntentData();
			if (firstIntentData != null)
			{
				_conduitParameterProvider.PopulateParametersFromNode(responseNode);
				_conduitParameterProvider.AddParameter("@VoiceSession", voiceSession);
				_conduitParameterProvider.AddParameter("@WitResponseNode", responseNode);
				ConduitDispatcher.InvokeAction(_conduitParameterProvider, firstIntentData.name, _witConfiguration.relaxedResolution, firstIntentData.confidence, partial: true);
			}
		}
		if (voiceSession.validResponse)
		{
			VLog.I("Validated Early");
			request.CompleteEarly();
		}
	}

	public IEnumerable<object> GetObjectsOfType(Type type)
	{
		return UnityEngine.Object.FindObjectsByType(type, FindObjectsSortMode.None);
	}

	protected virtual void Awake()
	{
		InitializeEventListeners();
	}

	private void InitializeEventListeners()
	{
		if (!GetComponent<AudioEventListener>())
		{
			base.gameObject.AddComponent<AudioEventListener>();
		}
		if (!GetComponent<TranscriptionEventListener>())
		{
			base.gameObject.AddComponent<TranscriptionEventListener>();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (UseConduit)
		{
			InitializeConduit().WrapErrors();
		}
		else if (UseIntentAttributes)
		{
			MatchIntentRegistry.Initialize();
		}
		TranscriptionProvider?.OnFullTranscription.AddListener(OnFinalTranscription);
		VoiceEvents.OnResponse.AddListener(HandleResponse);
	}

	private async Task InitializeConduit()
	{
		await ConduitDispatcher.Initialize(_witConfiguration.ManifestLocalPath);
		if (!_witConfiguration.relaxedResolution)
		{
			return;
		}
		if (!ConduitDispatcher.Manifest.ResolveEntities())
		{
			VLog.E("Failed to resolve Conduit entities");
		}
		foreach (KeyValuePair<string, Type> customEntityType in ConduitDispatcher.Manifest.CustomEntityTypes)
		{
			_conduitParameterProvider.AddCustomType(customEntityType.Key, customEntityType.Value);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		TranscriptionProvider?.OnFullTranscription.RemoveListener(OnFinalTranscription);
		VoiceEvents.OnResponse.RemoveListener(HandleResponse);
	}

	protected virtual void OnFinalTranscription(string transcription)
	{
		if (TranscriptionProvider != null)
		{
			Activate(transcription);
		}
	}

	private VoiceSession GetVoiceSession(WitResponseNode response)
	{
		return new VoiceSession
		{
			service = this,
			response = response,
			validResponse = false
		};
	}

	protected virtual void HandleResponse(WitResponseNode response)
	{
		HandleIntents(response);
	}

	private void HandleIntents(WitResponseNode response)
	{
		WitIntentData[] intents = response.GetIntents();
		foreach (WitIntentData intent in intents)
		{
			HandleIntent(intent, response);
		}
	}

	private void HandleIntent(WitIntentData intent, WitResponseNode response)
	{
		if (UseConduit)
		{
			_conduitParameterProvider.PopulateParametersFromNode(response);
			_conduitParameterProvider.AddParameter("@WitResponseNode", response);
			ConduitDispatcher.InvokeAction(_conduitParameterProvider, intent.name, _witConfiguration.relaxedResolution, intent.confidence);
		}
		else
		{
			if (!UseIntentAttributes)
			{
				return;
			}
			foreach (RegisteredMatchIntent item in MatchIntentRegistry.RegisteredMethods[intent.name])
			{
				ExecuteRegisteredMatch(item, intent, response);
			}
		}
	}

	private void ExecuteRegisteredMatch(RegisteredMatchIntent registeredMethod, WitIntentData intent, WitResponseNode response)
	{
		if (!(intent.confidence >= registeredMethod.matchIntent.MinConfidence) || !(intent.confidence <= registeredMethod.matchIntent.MaxConfidence))
		{
			return;
		}
		foreach (object item in GetObjectsOfType(registeredMethod.type))
		{
			ParameterInfo[] parameters = registeredMethod.method.GetParameters();
			if (parameters.Length == 0)
			{
				registeredMethod.method.Invoke(item, Array.Empty<object>());
			}
			else if (parameters[0].ParameterType != typeof(WitResponseNode) || parameters.Length > 2)
			{
				VLog.E("Match intent only supports methods with no parameters or with a WitResponseNode parameter. Enable Conduit or adjust the parameters");
			}
			else if (parameters.Length == 1)
			{
				registeredMethod.method.Invoke(item, new object[1] { response });
			}
		}
	}
}
