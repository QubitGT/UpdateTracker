using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem;

public class DistanceHaptics : MonoBehaviour
{
	public Transform firstTransform;

	public Transform secondTransform;

	public AnimationCurve distanceIntensityCurve = AnimationCurve.Linear(0f, 800f, 1f, 800f);

	public AnimationCurve pulseIntervalCurve = AnimationCurve.Linear(0f, 0.01f, 1f, 0f);

	private IEnumerator Start()
	{
		while (true)
		{
			float time = Vector3.Distance(firstTransform.position, secondTransform.position);
			Hand componentInParent = GetComponentInParent<Hand>();
			if (componentInParent != null)
			{
				float num = distanceIntensityCurve.Evaluate(time);
				componentInParent.TriggerHapticPulse((ushort)num);
			}
			float seconds = pulseIntervalCurve.Evaluate(time);
			yield return new WaitForSeconds(seconds);
		}
	}
}
