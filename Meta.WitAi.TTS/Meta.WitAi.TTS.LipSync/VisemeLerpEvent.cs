using System;
using Meta.WitAi.TTS.Data;
using UnityEngine.Events;

namespace Meta.WitAi.TTS.LipSync;

[Serializable]
public class VisemeLerpEvent : UnityEvent<Viseme, Viseme, float>
{
}
