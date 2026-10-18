using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public static class SplineFactory
{
	public static Spline CreateLinear(IList<float3> positions, bool closed = false)
	{
		return CreateLinear(positions, null, closed);
	}

	public static Spline CreateLinear(IList<float3> positions, IList<quaternion> rotations, bool closed = false)
	{
		int count = positions.Count;
		Spline spline = new Spline(count, closed);
		for (int i = 0; i < count; i++)
		{
			float3 position = positions[i];
			quaternion rotation = rotations?[i] ?? quaternion.identity;
			float3 zero = float3.zero;
			float3 zero2 = float3.zero;
			spline.Add(new BezierKnot(position, zero, zero2, rotation), TangentMode.Linear);
		}
		return spline;
	}

	public static Spline CreateCatmullRom(IList<float3> positions, bool closed = false)
	{
		return CreateCatmullRom(positions, null, closed);
	}

	internal static Spline CreateCatmullRom(IList<float3> positions, IList<quaternion> rotations, bool closed = false)
	{
		int count = positions.Count;
		Spline spline = new Spline(count, closed);
		for (int i = 0; i < count; i++)
		{
			float3 position = positions[i];
			quaternion quaternion = rotations?[i] ?? quaternion.identity;
			int index = SplineUtility.NextIndex(i, count, closed);
			int index2 = SplineUtility.PreviousIndex(i, count, closed);
			float3 @float = math.rotate(math.inverse(quaternion), SplineUtility.GetAutoSmoothTangent(positions[index2], positions[i], positions[index], 0.5f));
			float3 tangentIn = -@float;
			spline.Add(new BezierKnot(position, tangentIn, @float, quaternion), TangentMode.AutoSmooth);
		}
		return spline;
	}

	public static Spline CreateRoundedSquare(float radius, float rounding)
	{
		float3 @float = new float3(-0.5f, 0f, -0.5f);
		float3 float2 = new float3(-0.5f, 0f, 0.5f);
		float3 float3 = new float3(0.5f, 0f, 0.5f);
		float3 float4 = new float3(0.5f, 0f, -0.5f);
		float3 float5 = new float3(0f, 0f, -1f);
		float3 float6 = new float3(0f, 0f, 1f);
		Spline spline = new Spline(new BezierKnot[4]
		{
			new BezierKnot(@float * radius, float5 * rounding, float6 * rounding, Quaternion.Euler(0f, -45f, 0f)),
			new BezierKnot(float2 * radius, float5 * rounding, float6 * rounding, Quaternion.Euler(0f, 45f, 0f)),
			new BezierKnot(float3 * radius, float5 * rounding, float6 * rounding, Quaternion.Euler(0f, 135f, 0f)),
			new BezierKnot(float4 * radius, float5 * rounding, float6 * rounding, Quaternion.Euler(0f, -135f, 0f))
		}, closed: true);
		for (int i = 0; i < spline.Count; i++)
		{
			spline.SetTangentMode(i, TangentMode.Mirrored);
		}
		return spline;
	}

	public static Spline CreateHelix(float radius, float height, int revolutions)
	{
		revolutions = math.max(1, revolutions);
		float num = height / (float)revolutions;
		float num2 = MathF.PI / 2f;
		float num3 = num / (MathF.PI * 2f);
		float num4 = radius * math.cos(num2);
		float num5 = radius * math.sin(num2);
		float num6 = num3 * num2 * (radius - num4) * (3f * radius - num4) / (num5 * (4f * radius - num4) * math.tan(num2));
		float num7 = num * 0.25f;
		float3 @float = new float3(num4, (0f - num2) * num3 + num7, 0f - num5);
		float3 float2 = new float3((4f * radius - num4) / 3f, 0f - num6 + num7, (0f - (radius - num4)) * (3f * radius - num4) / (3f * num5));
		float3 float3 = new float3((4f * radius - num4) / 3f, num6 + num7, (radius - num4) * (3f * radius - num4) / (3f * num5));
		float3 float4 = new float3(num4, num2 * num3 + num7, num5);
		Spline spline = new Spline();
		float3 x = float2 - @float;
		float num8 = math.length(x);
		float3 float5 = math.normalize(x);
		spline.Add(new BezierKnot(rotation: quaternion.LookRotation(float5, math.cross(math.cross(float5, math.up()), float5)), position: @float, tangentIn: new float3(0f, 0f, 0f - num8), tangentOut: new float3(0f, 0f, num8)));
		float5 = math.normalize(float4 - float3);
		spline.Add(new BezierKnot(rotation: quaternion.LookRotation(float5, math.cross(math.cross(float5, math.up()), float5)), position: float4, tangentIn: new float3(0f, 0f, 0f - num8), tangentOut: new float3(0f, 0f, num8)));
		quaternion q = quaternion.AxisAngle(math.up(), math.radians(180f));
		num7 = num * 0.5f;
		float4 = math.rotate(q, float4);
		float4.y += num7;
		float5 = math.normalize(float2 - @float);
		float3 up = math.cross(math.cross(float5, math.up()), float5);
		spline.Add(new BezierKnot(float4, new float3(0f, 0f, 0f - num8), new float3(0f, 0f, num8), quaternion.LookRotation(float5, up)));
		float3 float6 = new float3(0f, num, 0f);
		for (int i = 1; i < revolutions; i++)
		{
			BezierKnot item = spline[spline.Count - 1];
			item.Position += float6;
			BezierKnot item2 = spline[spline.Count - 2];
			item2.Position += float6;
			spline.Add(item2);
			spline.Add(item);
		}
		return spline;
	}

	public static Spline CreateRoundedCornerSquare(float size, float cornerRadius)
	{
		float num = size * 0.5f;
		cornerRadius = math.clamp(cornerRadius, 0f, num);
		if (cornerRadius == 0f)
		{
			return CreateSquare(size);
		}
		float3 @float = new float3(0f - num, 0f, num - cornerRadius);
		float3 float2 = new float3(0f - num + cornerRadius, 0f, num);
		float num2 = SplineMath.GetUnitCircleTangentLength() * cornerRadius;
		float num3 = 0f;
		Spline spline = new Spline();
		for (int i = 0; i < 4; i++)
		{
			Quaternion quaternion = Quaternion.Euler(0f, num3, 0f);
			if (cornerRadius < 1f)
			{
				spline.Add(new BezierKnot(quaternion * @float, new float3(0f, 0f, 0f - math.min(num2, 0f)), new float3(0f, 0f, num2), Quaternion.identity * quaternion));
				spline.Add(new BezierKnot(quaternion * float2, new float3(0f, 0f, 0f - num2), new float3(0f, 0f, math.min(num2, 0f)), Quaternion.Euler(0f, 90f, 0f) * quaternion));
			}
			else
			{
				spline.Add(new BezierKnot(quaternion * @float, new float3(0f, 0f, 0f - num2), new float3(0f, 0f, num2), Quaternion.identity * quaternion));
			}
			num3 += 90f;
		}
		spline.Closed = true;
		return spline;
	}

	public static Spline CreateSquare(float size)
	{
		float3 @float = new float3(-0.5f, 0f, -0.5f) * size;
		float3 float2 = new float3(-0.5f, 0f, 0.5f) * size;
		float3 float3 = new float3(0.5f, 0f, 0.5f) * size;
		float3 float4 = new float3(0.5f, 0f, -0.5f) * size;
		return CreateLinear(new float3[4] { @float, float2, float3, float4 }, closed: true);
	}

	public static Spline CreateCircle(float radius)
	{
		float3 v = new float3(0f - radius, 0f, 0f);
		float3 @float = new float3(0f, 0f, SplineMath.GetUnitCircleTangentLength() * radius);
		Spline spline = new Spline();
		quaternion quaternion = quaternion.identity;
		for (int i = 0; i < 4; i++)
		{
			spline.Add(new BezierKnot(math.rotate(quaternion, v), -@float, @float, quaternion));
			quaternion = math.mul(quaternion, quaternion.AxisAngle(math.up(), MathF.PI / 2f));
		}
		spline.Closed = true;
		return spline;
	}

	public static Spline CreatePolygon(float edgeSize, int sides)
	{
		sides = math.max(3, sides);
		float3[] array = new float3[sides];
		float num = MathF.PI * 2f / (float)sides;
		float z = edgeSize * 0.5f / math.sin(num * 0.5f);
		float3 v = new float3(0f, 0f, z);
		quaternion quaternion = quaternion.identity;
		for (int i = 0; i < sides; i++)
		{
			array[i] = math.rotate(quaternion, v);
			quaternion = math.mul(quaternion, quaternion.AxisAngle(math.up(), num));
		}
		return CreateLinear(array, closed: true);
	}

	public static Spline CreateStarPolygon(float edgeSize, int corners, float concavity)
	{
		concavity = math.clamp(concavity, 0f, 1f);
		if (concavity == 0f)
		{
			CreatePolygon(edgeSize, corners);
		}
		corners = math.max(3, corners);
		int num = corners * 2;
		float3[] array = new float3[num];
		float num2 = MathF.PI * 2f / (float)corners;
		float z = edgeSize * 0.5f / math.sin(num2 * 0.5f);
		float3 v = new float3(0f, 0f, z);
		quaternion quaternion = quaternion.identity;
		for (int i = 0; i < num; i += 2)
		{
			array[i] = math.rotate(quaternion, v);
			quaternion = math.mul(quaternion, quaternion.AxisAngle(math.up(), num2));
			if (i != 0)
			{
				array[i - 1] = (array[i - 2] + array[i]) * 0.5f * (1f - concavity);
			}
			if (i == num - 2)
			{
				array[i + 1] = (array[0] + array[i]) * 0.5f * (1f - concavity);
			}
		}
		return CreateLinear(array, closed: true);
	}
}
