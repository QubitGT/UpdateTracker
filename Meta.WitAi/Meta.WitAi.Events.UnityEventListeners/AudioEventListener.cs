using Meta.WitAi.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.Events.UnityEventListeners;

[RequireComponent(typeof(IAudioEventProvider))]
public class AudioEventListener : MonoBehaviour, IAudioInputEvents
{
	[SerializeField]
	private WitMicLevelChangedEvent onMicAudioLevelChanged = new WitMicLevelChangedEvent();

	[SerializeField]
	private UnityEvent onMicStartedListening = new UnityEvent();

	[SerializeField]
	private UnityEvent onMicStoppedListening = new UnityEvent();

	private IAudioInputEvents _events;

	public WitMicLevelChangedEvent OnMicAudioLevelChanged => onMicAudioLevelChanged;

	public UnityEvent OnMicStartedListening => onMicStartedListening;

	public UnityEvent OnMicStoppedListening => onMicStoppedListening;

	private IAudioInputEvents AudioInputEvents
	{
		get
		{
			if (_events == null)
			{
				IAudioEventProvider component = GetComponent<IAudioEventProvider>();
				if (component != null)
				{
					_events = component.AudioEvents;
				}
			}
			return _events;
		}
	}

	private void OnEnable()
	{
		IAudioInputEvents audioInputEvents = AudioInputEvents;
		if (audioInputEvents != null)
		{
			audioInputEvents.OnMicAudioLevelChanged.AddListener(onMicAudioLevelChanged.Invoke);
			audioInputEvents.OnMicStartedListening.AddListener(onMicStartedListening.Invoke);
			audioInputEvents.OnMicStoppedListening.AddListener(onMicStoppedListening.Invoke);
		}
	}

	private void OnDisable()
	{
		IAudioInputEvents audioInputEvents = AudioInputEvents;
		if (audioInputEvents != null)
		{
			audioInputEvents.OnMicAudioLevelChanged.RemoveListener(onMicAudioLevelChanged.Invoke);
			audioInputEvents.OnMicStartedListening.RemoveListener(onMicStartedListening.Invoke);
			audioInputEvents.OnMicStoppedListening.RemoveListener(onMicStoppedListening.Invoke);
		}
	}
}
