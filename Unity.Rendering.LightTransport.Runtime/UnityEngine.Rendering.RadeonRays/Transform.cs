using Unity.Mathematics;

namespace UnityEngine.Rendering.RadeonRays;

internal struct Transform
{
	public float4 row0;

	public float4 row1;

	public float4 row2;

	public Transform(float4 row0, float4 row1, float4 row2)
	{
		this.row0 = row0;
		this.row1 = row1;
		this.row2 = row2;
	}

	public static Transform Identity()
	{
		return new Transform(new float4(1f, 0f, 0f, 0f), new float4(0f, 1f, 0f, 0f), new float4(0f, 0f, 1f, 0f));
	}

	public static Transform Translation(float3 translation)
	{
		return new Transform(new float4(1f, 0f, 0f, translation.x), new float4(0f, 1f, 0f, translation.y), new float4(0f, 0f, 1f, translation.z));
	}

	public static Transform Scale(float3 scale)
	{
		return new Transform(new float4(scale.x, 0f, 0f, 0f), new float4(0f, scale.y, 0f, 0f), new float4(0f, 0f, scale.z, 0f));
	}

	public static Transform TRS(float3 translation, float3 rotation, float3 scale)
	{
		float3x3 float3x = float3x3.Euler(rotation);
		float3x.c0 *= scale.x;
		float3x.c1 *= scale.y;
		float3x.c2 *= scale.z;
		return new Transform(new float4(float3x.c0.x, float3x.c1.x, float3x.c2.x, translation.x), new float4(float3x.c0.y, float3x.c1.y, float3x.c2.y, translation.y), new float4(float3x.c0.z, float3x.c1.z, float3x.c2.z, translation.z));
	}

	public Transform Inverse()
	{
		float4x4 m = default(float4x4);
		m[0] = new float4(row0.x, row1.x, row2.x, 0f);
		m[1] = new float4(row0.y, row1.y, row2.y, 0f);
		m[2] = new float4(row0.z, row1.z, row2.z, 0f);
		m[3] = new float4(row0.w, row1.w, row2.w, 1f);
		m = math.fastinverse(m);
		Transform result = default(Transform);
		result.row0 = new float4(m[0].x, m[1].x, m[2].x, m[3].x);
		result.row1 = new float4(m[0].y, m[1].y, m[2].y, m[3].y);
		result.row2 = new float4(m[0].z, m[1].z, m[2].z, m[3].z);
		return result;
	}
}
