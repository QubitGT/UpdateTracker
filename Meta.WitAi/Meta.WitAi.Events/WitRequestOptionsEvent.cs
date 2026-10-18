using System;
using Meta.WitAi.Configuration;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class WitRequestOptionsEvent : UnityEvent<WitRequestOptions>
{
}
