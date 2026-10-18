using UnityEngine;

namespace Technie.PhysicsCreator;

public class Triangle
{
	public Vector3 normal;

	public float area;

	public Vector3 center;

	public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 lhs = p1 - p0;
		Vector3 rhs = p2 - p0;
		Vector3 vector = Vector3.Cross(lhs, rhs);
		area = vector.magnitude * 0.5f;
		normal = vector.normalized;
		center = (p0 + p1 + p2) / 3f;
	}
}
