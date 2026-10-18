using System;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class WitErrorEvent : UnityEvent<string, string>
{
}
