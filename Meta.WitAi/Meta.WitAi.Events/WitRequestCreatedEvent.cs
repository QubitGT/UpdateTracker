using System;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class WitRequestCreatedEvent : UnityEvent<WitRequest>
{
}
