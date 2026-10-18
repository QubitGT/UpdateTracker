using System;
using UnityEngine.Events;

namespace Meta.WitAi.Speech;

[Serializable]
public class VoiceTextEvent : UnityEvent<string>
{
}
