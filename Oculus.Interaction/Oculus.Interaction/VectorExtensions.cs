using UnityEngine;

namespace Oculus.Interaction;

public static class VectorExtensions
{
	public static bool Approximately(this Vector3 a, Vector3 b, float epsilon = 1E-05f)
	{
		return (a - b).sqrMagnitude <= epsilon * epsilon;
	}
}
