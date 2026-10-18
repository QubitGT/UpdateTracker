using System;
using UnityEngine.Events;

namespace Meta.WitAi.Events;

[Serializable]
public class WitSampleEvent : UnityEvent<float[], int, float>
{
}
