using System;
using Oculus.Assistant.VoiceCommand.Data;
using UnityEngine.Events;

namespace Oculus.Assistant.VoiceCommand.Listeners;

[Serializable]
public class VoiceCommandCallbackEvent : UnityEvent<VoiceCommandResult>
{
}
