using UnityEngine;

namespace Liv.Lck;

public static class LckRigidbodyExtensions
{
	public static void LookAtFromPivotPoint(this Rigidbody rigidbody, Vector3 pivot, Vector3 forward, Vector3 position, Quaternion currentRotation)
	{
		Quaternion quaternion = Quaternion.LookRotation(forward.normalized, Vector3.up);
		Quaternion quaternion2 = quaternion * Quaternion.Inverse(currentRotation);
		Vector3 vector = position - pivot;
		Vector3 vector2 = quaternion2 * vector;
		Vector3 position2 = pivot + vector2;
		rigidbody.MovePosition(position2);
		rigidbody.MoveRotation(quaternion);
	}
}
