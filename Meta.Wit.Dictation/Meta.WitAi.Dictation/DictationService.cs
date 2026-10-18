using Meta.WitAi.Configuration;
using Meta.WitAi.Dictation.Events;
using Meta.WitAi.Events;
using Meta.WitAi.Events.UnityEventListeners;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;
using UnityEngine;

namespace Meta.WitAi.Dictation;

public abstract class DictationService : BaseSpeechService, IDictationService, ITelemetryEventsProvider, IAudioEventProvider, ITranscriptionEventProvider
{
	[Tooltip("Events that will fire before, during and after an activation")]
	[SerializeField]
	protected DictationEvents dictationEvents = new DictationEvents();

	protected TelemetryEvents telemetryEvents = new TelemetryEvents();

	public virtual bool IsRequestActive => Active;

	public abstract ITranscriptionProvider TranscriptionProvider { get; set; }

	public abstract bool MicActive { get; }

	public virtual DictationEvents DictationEvents
	{
		get
		{
			return dictationEvents;
		}
		set
		{
			dictationEvents = value;
		}
	}

	public TelemetryEvents TelemetryEvents
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

	public IAudioInputEvents AudioEvents => DictationEvents;

	public ITranscriptionEvent TranscriptionEvents => DictationEvents;

	protected abstract bool ShouldSendMicData { get; }

	protected override SpeechEvents GetSpeechEvents()
	{
		return DictationEvents;
	}

	public void Activate()
	{
		Activate(new WitRequestOptions(), new VoiceServiceRequestEvents());
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
		ActivateImmediately(new WitRequestOptions(), new VoiceServiceRequestEvents());
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

	public abstract void Cancel();

	protected virtual void Awake()
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
}
