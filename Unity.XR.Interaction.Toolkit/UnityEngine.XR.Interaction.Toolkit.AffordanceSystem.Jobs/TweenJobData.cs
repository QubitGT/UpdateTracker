using System;
using Unity.Collections;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public struct TweenJobData<T> where T : struct
{
	public const float squareSnapDistanceThreshold = 2.5000003E-07f;

	public const byte totalStateTransitionIncrements = byte.MaxValue;

	public T initialValue;

	public T stateOriginValue;

	public T stateTargetValue;

	public byte stateTransitionIncrement;

	public NativeCurve nativeCurve;

	public T tweenStartValue;

	public float tweenAmount;

	public NativeArray<T> outputData;

	public float stateTransitionAmountFloat => (float)(int)stateTransitionIncrement / 255f;
}
