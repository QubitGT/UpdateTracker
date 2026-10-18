using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Samples;

public class LocomotionTutorialAnimationUnityEventWrapper : MonoBehaviour
{
	public UnityEvent WhenEnableTeleportRay;

	public UnityEvent WhenDisableTeleportRay;

	public UnityEvent WhenEnableTurningRing;

	public UnityEvent WhenDisableTurningRing;

	public void EnableTeleportRay()
	{
		WhenEnableTeleportRay.Invoke();
	}

	public void DisableTeleportRay()
	{
		WhenDisableTeleportRay.Invoke();
	}

	public void EnableTurningRing()
	{
		WhenEnableTurningRing.Invoke();
	}

	public void DisableTurningRing()
	{
		WhenDisableTurningRing.Invoke();
	}
}
