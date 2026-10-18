using System;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class WitTranscriptionEvent : UnityEvent<string>
{
}
