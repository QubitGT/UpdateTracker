using System;
using UnityEngine.Events;

namespace Oculus.Assistant.VoiceCommand.Listeners;

[Serializable]
public class OnCommandSlotReceived : UnityEvent<string>
{
}
