using System;
using Meta.WitAi.TTS.Data;
using UnityEngine.Events;

namespace Meta.WitAi.TTS.Events;

[Serializable]
public class TTSClipEvent : UnityEvent<TTSClipData>
{
}
