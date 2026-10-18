using System;
using Meta.WitAi.Json;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class WitResponseEvent : UnityEvent<WitResponseNode>
{
}
