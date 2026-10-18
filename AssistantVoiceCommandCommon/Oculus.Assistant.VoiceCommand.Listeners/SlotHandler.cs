using System;
using UnityEngine;

namespace Oculus.Assistant.VoiceCommand.Listeners;

[Serializable]
public class SlotHandler
{
	[Tooltip("The name of the slot to listen for")]
	public string slotName;

	public OnCommandSlotReceived onCommandSlotReceived = new OnCommandSlotReceived();

	public override string ToString()
	{
		return slotName;
	}
}
