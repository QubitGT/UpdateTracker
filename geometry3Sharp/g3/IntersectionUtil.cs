using System;

namespace g3;

public static class IntersectionUtil
{
	public static bool Intersects(Vector3d rayOrigin, Vector3d rayDirection, Vector3d V0, Vector3d V1, Vector3d V2)
	{
		Vector3d v = rayOrigin - V0;
		Vector3d vector3d = V1 - V0;
		Vector3d v2 = V2 - V0;
		Vector3d v3 = vector3d.Cross(v2);
		double num = rayDirection.Dot(v3);
		double num2;
		if (num > 1E-08)
		{
			num2 = 1.0;
		}
		else
		{
			if (!(num < -1E-08))
			{
				return false;
			}
			num2 = -1.0;
			num = 0.0 - num;
		}
		double num3 = num2 * rayDirection.Dot(v.Cross(v2));
		if (num3 >= 0.0)
		{
			double num4 = num2 * rayDirection.Dot(vector3d.Cross(v));
			if (num4 >= 0.0 && num3 + num4 <= num && (0.0 - num2) * v.Dot(v3) >= 0.0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool LineSphereTest(ref Vector3d lineOrigin, ref Vector3d lineDirection, ref Vector3d sphereCenter, double sphereRadius)
	{
		Vector3d v = lineOrigin - sphereCenter;
		double num = v.LengthSquared - sphereRadius * sphereRadius;
		double num2 = lineDirection.Dot(v);
		return num2 * num2 - num >= 0.0;
	}

	public static bool LineSphere(ref Vector3d lineOrigin, ref Vector3d lineDirection, ref Vector3d sphereCenter, double sphereRadius, ref LinearIntersection result)
	{
		Vector3d v = lineOrigin - sphereCenter;
		double num = v.LengthSquared - sphereRadius * sphereRadius;
		double num2 = lineDirection.Dot(v);
		double num3 = num2 * num2 - num;
		if (num3 > 0.0)
		{
			result.intersects = true;
			result.numIntersections = 2;
			double num4 = Math.Sqrt(num3);
			result.parameter.a = 0.0 - num2 - num4;
			result.parameter.b = 0.0 - num2 + num4;
		}
		else if (num3 < 0.0)
		{
			result.intersects = false;
			result.numIntersections = 0;
		}
		else
		{
			result.intersects = true;
			result.numIntersections = 1;
			result.parameter.a = 0.0 - num2;
			result.parameter.b = 0.0 - num2;
		}
		return result.intersects;
	}

	public static LinearIntersection LineSphere(ref Vector3d lineOrigin, ref Vector3d lineDirection, ref Vector3d sphereCenter, double sphereRadius)
	{
		LinearIntersection result = default(LinearIntersection);
		LineSphere(ref lineOrigin, ref lineDirection, ref sphereCenter, sphereRadius, ref result);
		return result;
	}

	public static bool RaySphereTest(ref Vector3d rayOrigin, ref Vector3d rayDirection, ref Vector3d sphereCenter, double sphereRadius)
	{
		Vector3d v = rayOrigin - sphereCenter;
		double num = v.LengthSquared - sphereRadius * sphereRadius;
		if (num <= 0.0)
		{
			return true;
		}
		double num2 = rayDirection.Dot(v);
		if (num2 >= 0.0)
		{
			return false;
		}
		return num2 * num2 - num >= 0.0;
	}

	public static bool RaySphere(ref Vector3d rayOrigin, ref Vector3d rayDirection, ref Vector3d sphereCenter, double sphereRadius, ref LinearIntersection result)
	{
		LineSphere(ref rayOrigin, ref rayDirection, ref sphereCenter, sphereRadius, ref result);
		if (result.intersects)
		{
			if (result.parameter.b < 0.0)
			{
				result.intersects = false;
				result.numIntersections = 0;
			}
			else if (result.parameter.a < 0.0)
			{
				result.numIntersections--;
				result.parameter.a = result.parameter.b;
			}
		}
		return result.intersects;
	}

	public static LinearIntersection RaySphere(ref Vector3d rayOrigin, ref Vector3d rayDirection, ref Vector3d sphereCenter, double sphereRadius)
	{
		LinearIntersection result = default(LinearIntersection);
		LineSphere(ref rayOrigin, ref rayDirection, ref sphereCenter, sphereRadius, ref result);
		return result;
	}
}
