using Meta.WitAi.Json;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Integrations;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.TTS.Events.EventHandlers;

public class ActionEventHandler : TTSEventTrigger<TTSActionEvent, string>
{
	[SerializeField]
	private UnityEvent<WitResponseNode> onEvent = new UnityEvent<WitResponseNode>();

	public UnityEvent<WitResponseNode> OnEvent => onEvent;

	protected override void OnEventTriggered(TTSActionEvent queuedEvent)
	{
		onEvent?.Invoke(queuedEvent.Response);
	}
}
