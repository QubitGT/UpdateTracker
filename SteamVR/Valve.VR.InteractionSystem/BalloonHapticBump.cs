using UnityEngine;

namespace Valve.VR.InteractionSystem;

public class BalloonHapticBump : MonoBehaviour
{
	public GameObject physParent;

	private void OnCollisionEnter(Collision other)
	{
		if (other.collider.GetComponentInParent<Balloon>() != null)
		{
			Hand componentInParent = physParent.GetComponentInParent<Hand>();
			if (componentInParent != null)
			{
				componentInParent.TriggerHapticPulse(500);
			}
		}
	}
}
