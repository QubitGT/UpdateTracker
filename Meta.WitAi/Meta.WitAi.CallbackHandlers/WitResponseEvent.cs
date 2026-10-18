using System;
using Meta.WitAi.Json;
using UnityEngine.Events;

namespace Meta.WitAi.CallbackHandlers;

[Serializable]
public class WitResponseEvent : UnityEvent<WitResponseNode>
{
}
