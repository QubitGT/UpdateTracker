using UnityEngine;

namespace Oculus.Interaction;

public static class RigidbodyKinematicLockerExtension
{
	public static bool IsLocked(this Rigidbody rigidbody)
	{
		if (rigidbody.TryGetComponent<RigidbodyKinematicLocker>(out var component))
		{
			return component.IsLocked;
		}
		return false;
	}

	public static void LockKinematic(this Rigidbody rigidbody)
	{
		if (!rigidbody.TryGetComponent<RigidbodyKinematicLocker>(out var component))
		{
			component = rigidbody.gameObject.AddComponent<RigidbodyKinematicLocker>();
		}
		component.LockKinematic();
	}

	public static void UnlockKinematic(this Rigidbody rigidbody)
	{
		if (!rigidbody.TryGetComponent<RigidbodyKinematicLocker>(out var component))
		{
			Debug.LogError("Too many calls to UnlockKinematic.Expected calls to LockKinematic to balance the kinematic state.", rigidbody);
		}
		else
		{
			component.UnlockKinematic();
		}
	}
}
