using System;
using UnityEngine.Events;

namespace Meta.Voice;

[Serializable]
public class NLPRequestResponseEvent<TResponseData> : UnityEvent<TResponseData>
{
}
