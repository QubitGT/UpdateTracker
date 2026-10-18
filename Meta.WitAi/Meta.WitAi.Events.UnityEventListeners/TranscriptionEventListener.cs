using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Events.UnityEventListeners;

public class TranscriptionEventListener : MonoBehaviour, ITranscriptionEvent
{
	[SerializeField]
	private WitTranscriptionEvent onPartialTranscription = new WitTranscriptionEvent();

	[SerializeField]
	private WitTranscriptionEvent onFullTranscription = new WitTranscriptionEvent();

	private ITranscriptionEvent _events;

	public WitTranscriptionEvent OnPartialTranscription => onPartialTranscription;

	public WitTranscriptionEvent OnFullTranscription => onFullTranscription;

	private ITranscriptionEvent TranscriptionEvents
	{
		get
		{
			if (_events == null)
			{
				ITranscriptionEventProvider component = GetComponent<ITranscriptionEventProvider>();
				if (component != null)
				{
					_events = component.TranscriptionEvents;
				}
			}
			return _events;
		}
	}

	private void OnEnable()
	{
		ITranscriptionEvent transcriptionEvents = TranscriptionEvents;
		if (transcriptionEvents != null)
		{
			transcriptionEvents.OnPartialTranscription.AddListener(onPartialTranscription.Invoke);
			transcriptionEvents.OnFullTranscription.AddListener(onFullTranscription.Invoke);
		}
	}

	private void OnDisable()
	{
		ITranscriptionEvent transcriptionEvents = TranscriptionEvents;
		if (transcriptionEvents != null)
		{
			transcriptionEvents.OnPartialTranscription.RemoveListener(onPartialTranscription.Invoke);
			transcriptionEvents.OnFullTranscription.RemoveListener(onFullTranscription.Invoke);
		}
	}
}
