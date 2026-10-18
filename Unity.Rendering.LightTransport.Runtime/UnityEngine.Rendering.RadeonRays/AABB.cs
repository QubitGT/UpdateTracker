using Unity.Mathematics;

namespace UnityEngine.Rendering.RadeonRays;

internal class AABB
{
	public float3 Min;

	public float3 Max;

	public AABB()
	{
		Min = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Max = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
	}

	public AABB(float3 min, float3 max)
	{
		Min = min;
		Max = max;
	}

	public void Encapsulate(AABB aabb)
	{
		Min = math.min(Min, aabb.Min);
		Max = math.max(Max, aabb.Max);
	}

	public void Encapsulate(float3 point)
	{
		Min = math.min(Min, point);
		Max = math.max(Max, point);
	}

	public bool Contains(AABB rhs)
	{
		if (rhs.Min.x >= Min.x && rhs.Min.y >= Min.y && rhs.Min.z >= Min.z && rhs.Max.x <= Max.x && rhs.Max.y <= Max.y)
		{
			return rhs.Max.z <= Max.z;
		}
		return false;
	}

	public bool IsValid()
	{
		if (Min.x <= Max.x && Min.y <= Max.y)
		{
			return Min.z <= Max.z;
		}
		return false;
	}
}
