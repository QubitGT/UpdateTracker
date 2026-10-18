using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Integrations;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.TTS.Events.EventHandlers;

public class EmoteEventHandler : TTSEventTrigger<TTSEmoteEvent, string>
{
	[SerializeField]
	private UnityEvent<string> onEmoteStart = new UnityEvent<string>();

	[SerializeField]
	private UnityEvent<string> onEmoteStop = new UnityEvent<string>();

	private TTSEmoteEvent _lastEmote;

	public UnityEvent<string> OnEmoteStart => onEmoteStart;

	public UnityEvent<string> OnEmoteStop => onEmoteStop;

	protected override void OnEventTriggered(TTSEmoteEvent queuedEvent)
	{
		if (_lastEmote != null)
		{
			onEmoteStop?.Invoke(_lastEmote.Data);
		}
		_lastEmote = queuedEvent;
		onEmoteStart?.Invoke(queuedEvent.Data);
	}
}
