using System;
using UnityEngine.Events;

namespace Meta.WitAi.CallbackHandlers;

[Serializable]
public class ConfidenceRange
{
	public float minConfidence;

	public float maxConfidence;

	public UnityEvent onWithinConfidenceRange = new UnityEvent();

	public UnityEvent onOutsideConfidenceRange = new UnityEvent();
}
