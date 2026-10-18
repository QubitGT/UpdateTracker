using System;
using System.Text;
using UnityEngine.Events;

namespace Meta.Voice;

[Serializable]
public class NLPRequestResponseValidatorEvent<TResponseData> : UnityEvent<TResponseData, StringBuilder>
{
}
