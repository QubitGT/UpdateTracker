using System;
using Meta.WitAi.Data;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class WitValidationEvent : UnityEvent<VoiceSession>
{
}
