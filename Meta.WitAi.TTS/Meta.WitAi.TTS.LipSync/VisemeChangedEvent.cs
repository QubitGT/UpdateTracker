using System;
using Meta.WitAi.TTS.Data;
using UnityEngine.Events;

namespace Meta.WitAi.TTS.LipSync;

[Serializable]
public class VisemeChangedEvent : UnityEvent<Viseme>
{
}
