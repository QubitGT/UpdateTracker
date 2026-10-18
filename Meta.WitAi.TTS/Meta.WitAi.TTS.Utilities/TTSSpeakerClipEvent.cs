using System;
using Meta.WitAi.TTS.Data;
using UnityEngine.Events;

namespace Meta.WitAi.TTS.Utilities;

[Serializable]
public class TTSSpeakerClipEvent : UnityEvent<TTSSpeaker, TTSClipData>
{
}
