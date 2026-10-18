using System;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class WitByteDataEvent : UnityEvent<byte[], int, int>
{
}
