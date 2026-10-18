using JetBrains.Annotations;
using UnityEngine;

namespace MathGeoLib;

[PublicAPI]
public struct Plane
{
	public readonly Vector3 Normal;

	public readonly float Distance;

	public Plane(Vector3 normal, float distance)
	{
		Normal = normal;
		Distance = distance;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}", "Normal", Normal, "Distance", Distance);
	}
}
