using System;
using Oculus.Assistant.VoiceCommand.Configuration;
using Oculus.Assistant.VoiceCommand.Data;
using Oculus.Voice.Core.Utilities;

namespace Oculus.Assistant.VoiceCommand.Listeners;

[Serializable]
public class VoiceCommandResultHandler : VoiceCommandListener
{
	public Oculus.Assistant.VoiceCommand.Configuration.VoiceCommand voiceCommand;

	public VoiceCommandCallbackEvent onVoiceCommandReceived = new VoiceCommandCallbackEvent();

	[ArrayElementTitle("slotName", "Unassigned Slot")]
	public SlotHandler[] slotHandlers = Array.Empty<SlotHandler>();

	public void OnCallback(VoiceCommandResult result)
	{
		if (!(voiceCommand.actionId == result.ActionId))
		{
			return;
		}
		onVoiceCommandReceived.Invoke(result);
		SlotHandler[] array = slotHandlers;
		foreach (SlotHandler slotHandler in array)
		{
			if (result.TryGetSlot(slotHandler.slotName, out var slotValue))
			{
				slotHandler.onCommandSlotReceived.Invoke(slotValue);
			}
		}
	}
}
