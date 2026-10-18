using System;
using UnityEngine.Events;

namespace Meta.WitAi.TTS.Utilities;

[Serializable]
public class TTSSpeakerEvent : UnityEvent<TTSSpeaker, string>
{
}
