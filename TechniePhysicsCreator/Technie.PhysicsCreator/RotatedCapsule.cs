using System;
using UnityEngine;

namespace Technie.PhysicsCreator;

public struct RotatedCapsule
{
	public Vector3 center;

	public Vector3 dir;

	public float radius;

	public float height;

	public float CalcVolume()
	{
		float num = Mathf.Max(height - radius * 2f, 0f);
		return MathF.PI * (radius * radius) * (1.3333334f * radius * num);
	}

	public void DrawWireframe()
	{
		Vector3 vector = center - dir * Mathf.Max(height * 0.5f - radius, 0f);
		Vector3 vector2 = center + dir * Mathf.Max(height * 0.5f - radius, 0f);
		float num = (vector2 - vector).magnitude * 0.5f;
		Vector3 vector3 = Vector3.Cross(dir, (Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.9f) ? Vector3.right : Vector3.up);
		Vector3 vector4 = Vector3.Cross(dir, vector3);
		Gizmos.DrawWireSphere(vector, radius);
		Gizmos.DrawWireSphere(vector2, radius);
		Gizmos.DrawLine(center + vector3 * radius - dir * num, center + vector3 * radius + dir * num);
		Gizmos.DrawLine(center - vector3 * radius - dir * num, center - vector3 * radius + dir * num);
		Gizmos.DrawLine(center + vector4 * radius - dir * num, center + vector4 * radius + dir * num);
		Gizmos.DrawLine(center - vector4 * radius - dir * num, center - vector4 * radius + dir * num);
	}
}
