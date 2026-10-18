using System;
using UnityEngine.Events;

namespace Meta.WitAi.Requests;

[Serializable]
public class VoiceServiceRequestEvent : UnityEvent<VoiceServiceRequest>
{
}
