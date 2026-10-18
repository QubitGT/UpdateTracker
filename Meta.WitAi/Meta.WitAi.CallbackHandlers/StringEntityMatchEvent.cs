using System;
using UnityEngine.Events;

namespace Meta.WitAi.CallbackHandlers;

[Serializable]
public class StringEntityMatchEvent : UnityEvent<string>
{
}
