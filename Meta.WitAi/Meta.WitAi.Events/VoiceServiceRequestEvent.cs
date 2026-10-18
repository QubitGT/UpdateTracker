using System;
using Meta.WitAi.Requests;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class VoiceServiceRequestEvent : UnityEvent<VoiceServiceRequest>
{
}
