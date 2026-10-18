using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public enum FingerFeature
{
	[Tooltip("Convex angle (in degrees) representing the top 2 joints of the fingers. Angle increases as finger curl becomes closed.")]
	Curl,
	[Tooltip("Convex angle (in degrees) of joint 1 of the finger. Angle increases as finger flexion becomes closed.")]
	Flexion,
	[Tooltip("Angle (in degrees) between the given finger, and the next finger towards the pinkie.")]
	Abduction,
	[Tooltip("Distance between the tip of the given finger and the tip of the thumb.\nCalculated tracking space, with a 1.0 hand scale.")]
	Opposition
}
