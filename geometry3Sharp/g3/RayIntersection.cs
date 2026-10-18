using System;

namespace g3;

public class RayIntersection
{
	private RayIntersection()
	{
	}

	public static bool Sphere(Vector3f vOrigin, Vector3f vDirection, Vector3f vCenter, float fRadius, out float fRayT)
	{
		bool result = SphereSigned(ref vOrigin, ref vDirection, ref vCenter, fRadius, out fRayT);
		fRayT = Math.Abs(fRayT);
		return result;
	}

	public static bool SphereSigned(ref Vector3f vOrigin, ref Vector3f vDirection, ref Vector3f vCenter, float fRadius, out float fRayT)
	{
		fRayT = 0f;
		Vector3f v = vOrigin - vCenter;
		float num = v.Dot(vDirection);
		float num2 = v.Dot(v) - fRadius * fRadius;
		if (num2 > 0f && num > 0f)
		{
			return false;
		}
		float num3 = num * num - num2;
		if (num3 < 0f)
		{
			return false;
		}
		fRayT = 0f - num - (float)Math.Sqrt(num3);
		return true;
	}

	public static bool SphereSigned(ref Vector3d vOrigin, ref Vector3d vDirection, ref Vector3d vCenter, double fRadius, out double fRayT)
	{
		fRayT = 0.0;
		Vector3d v = vOrigin - vCenter;
		double num = v.Dot(ref vDirection);
		double num2 = v.Dot(v) - fRadius * fRadius;
		if (num2 > 0.0 && num > 0.0)
		{
			return false;
		}
		double num3 = num * num - num2;
		if (num3 < 0.0)
		{
			return false;
		}
		fRayT = 0.0 - num - Math.Sqrt(num3);
		return true;
	}

	public static bool InfiniteCylinder(Vector3f vOrigin, Vector3f vDirection, Vector3f vCylOrigin, Vector3f vCylAxis, float fRadius, out float fRayT)
	{
		bool result = InfiniteCylinderSigned(vOrigin, vDirection, vCylOrigin, vCylAxis, fRadius, out fRayT);
		fRayT = Math.Abs(fRayT);
		return result;
	}

	public static bool InfiniteCylinderSigned(Vector3f vOrigin, Vector3f vDirection, Vector3f vCylOrigin, Vector3f vCylAxis, float fRadius, out float fRayT)
	{
		fRayT = 0f;
		Vector3f vector3f = vCylAxis;
		Vector3f vector3f2 = vOrigin - vCylOrigin;
		if (vector3f2.DistanceSquared(vector3f2.Dot(vector3f) * vector3f) > fRadius * fRadius)
		{
			return false;
		}
		Vector3f v = vector3f2.Cross(vector3f);
		Vector3f v2 = vDirection.Cross(vector3f);
		float num = vector3f.Dot(vector3f);
		float num2 = v2.Dot(v2);
		float num3 = 2f * v2.Dot(v);
		float num4 = v.Dot(v) - fRadius * fRadius * num;
		double num5 = num3 * num3 - 4f * num2 * num4;
		if (num5 <= 0.0)
		{
			return false;
		}
		num5 = Math.Sqrt(num5);
		fRayT = (0f - num3 - (float)num5) / (2f * num2);
		return true;
	}
}
