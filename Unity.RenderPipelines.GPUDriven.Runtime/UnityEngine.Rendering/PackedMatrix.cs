using Unity.Mathematics;

namespace UnityEngine.Rendering;

internal struct PackedMatrix
{
	public float4 packed0;

	public float4 packed1;

	public float4 packed2;

	public static PackedMatrix FromMatrix4x4(in Matrix4x4 m)
	{
		PackedMatrix result = default(PackedMatrix);
		result.packed0 = new float4(m.m00, m.m10, m.m20, m.m01);
		result.packed1 = new float4(m.m11, m.m21, m.m02, m.m12);
		result.packed2 = new float4(m.m22, m.m03, m.m13, m.m23);
		return result;
	}

	public static PackedMatrix FromFloat4x4(in float4x4 m)
	{
		PackedMatrix result = default(PackedMatrix);
		result.packed0 = new float4(m.c0.x, m.c0.y, m.c0.z, m.c1.x);
		result.packed1 = new float4(m.c1.y, m.c1.z, m.c2.x, m.c2.y);
		result.packed2 = new float4(m.c2.z, m.c3.x, m.c3.y, m.c3.z);
		return result;
	}
}
