using Meta.WitAi.Events;
using Meta.WitAi.Events.UnityEventListeners;
using Meta.WitAi.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.ServiceReferences;

public class CombinedAudioEventReference : AudioInputServiceReference, IAudioInputEvents
{
	private WitMicLevelChangedEvent _onMicAudioLevelChanged = new WitMicLevelChangedEvent();

	private UnityEvent _onMicStartedListening = new UnityEvent();

	private UnityEvent _onMicStoppedListening = new UnityEvent();

	private AudioEventListener[] _sourceListeners;

	public override IAudioInputEvents AudioEvents => this;

	public WitMicLevelChangedEvent OnMicAudioLevelChanged => _onMicAudioLevelChanged;

	public UnityEvent OnMicStartedListening => _onMicStartedListening;

	public UnityEvent OnMicStoppedListening => _onMicStoppedListening;

	private void Awake()
	{
		_sourceListeners = Object.FindObjectsByType<AudioEventListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	}

	private void OnEnable()
	{
		AudioEventListener[] sourceListeners = _sourceListeners;
		foreach (AudioEventListener obj in sourceListeners)
		{
			obj.OnMicAudioLevelChanged.AddListener(OnMicAudioLevelChanged.Invoke);
			obj.OnMicStartedListening.AddListener(OnMicStartedListening.Invoke);
			obj.OnMicStoppedListening.AddListener(OnMicStoppedListening.Invoke);
		}
	}

	private void OnDisable()
	{
		AudioEventListener[] sourceListeners = _sourceListeners;
		foreach (AudioEventListener obj in sourceListeners)
		{
			obj.OnMicAudioLevelChanged.RemoveListener(OnMicAudioLevelChanged.Invoke);
			obj.OnMicStartedListening.RemoveListener(OnMicStartedListening.Invoke);
			obj.OnMicStoppedListening.RemoveListener(OnMicStoppedListening.Invoke);
		}
	}
}
