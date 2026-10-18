using System;
using UnityEngine.Events;

namespace Meta.WitAi.Utilities;

[Serializable]
public class CaptionEvent : UnityEvent<CaptionData>
{
}
