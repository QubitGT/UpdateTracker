using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Unity.Mathematics;

[Unity.IL2CPP.CompilerServices.Il2CppEagerStaticClassConstruction]
public static class svd
{
	public const float k_EpsilonDeterminant = 1E-06f;

	public const float k_EpsilonRCP = 1E-09f;

	public const float k_EpsilonNormalSqrt = 1E-15f;

	public const float k_EpsilonNormal = 1E-30f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void condSwap(bool c, ref float x, ref float y)
	{
		float trueValue = x;
		x = math.select(x, y, c);
		y = math.select(y, trueValue, c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void condNegSwap(bool c, ref float3 x, ref float3 y)
	{
		float3 trueValue = -x;
		x = math.select(x, y, c);
		y = math.select(y, trueValue, c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static quaternion condNegSwapQuat(bool c, quaternion q, float4 mask)
	{
		return math.mul(q, math.select(quaternion.identity.value, mask * 0.70710677f, c));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void sortSingularValues(ref float3x3 b, ref quaternion v)
	{
		float x = math.lengthsq(b.c0);
		float y = math.lengthsq(b.c1);
		float y2 = math.lengthsq(b.c2);
		bool c = x < y;
		condNegSwap(c, ref b.c0, ref b.c1);
		v = condNegSwapQuat(c, v, math.float4(0f, 0f, 1f, 1f));
		condSwap(c, ref x, ref y);
		c = x < y2;
		condNegSwap(c, ref b.c0, ref b.c2);
		v = condNegSwapQuat(c, v, math.float4(0f, -1f, 0f, 1f));
		condSwap(c, ref x, ref y2);
		c = y < y2;
		condNegSwap(c, ref b.c1, ref b.c2);
		v = condNegSwapQuat(c, v, math.float4(1f, 0f, 0f, 1f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static quaternion approxGivensQuat(float3 pq, float4 mask)
	{
		float num = 2f * (pq.x - pq.y);
		float z = pq.z;
		return math.normalize(math.select(math.float4(0.38268343f, 0.38268343f, 0.38268343f, 0.9238795f), math.float4(z, z, z, num), 5.8284273f * z * z < num * num) * mask);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static quaternion qrGivensQuat(float2 pq, float4 mask)
	{
		float num = math.sqrt(pq.x * pq.x + pq.y * pq.y);
		float x = math.select(0f, pq.y, num > 1E-15f);
		float y = math.abs(pq.x) + math.max(num, 1E-15f);
		condSwap(pq.x < 0f, ref x, ref y);
		return math.normalize(math.float4(x, x, x, y) * mask);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static quaternion givensQRFactorization(float3x3 b, out float3x3 r)
	{
		quaternion obj = qrGivensQuat(math.float2(b.c0.x, b.c0.y), math.float4(0f, 0f, 1f, 1f));
		float3x3 a = math.float3x3(math.conjugate(obj));
		r = math.mul(a, b);
		quaternion quaternion2 = qrGivensQuat(math.float2(r.c0.x, r.c0.z), math.float4(0f, -1f, 0f, 1f));
		quaternion a2 = math.mul(obj, quaternion2);
		a = math.float3x3(math.conjugate(quaternion2));
		r = math.mul(a, r);
		quaternion2 = qrGivensQuat(math.float2(r.c1.y, r.c1.z), math.float4(1f, 0f, 0f, 1f));
		quaternion result = math.mul(a2, quaternion2);
		a = math.float3x3(math.conjugate(quaternion2));
		r = math.mul(a, r);
		return result;
	}

	private static quaternion jacobiIteration(ref float3x3 s, int iterations = 5)
	{
		quaternion quaternion2 = quaternion.identity;
		for (int i = 0; i < iterations; i++)
		{
			quaternion quaternion3 = approxGivensQuat(math.float3(s.c0.x, s.c1.y, s.c0.y), math.float4(0f, 0f, 1f, 1f));
			quaternion2 = math.mul(quaternion2, quaternion3);
			float3x3 float3x5 = math.float3x3(quaternion3);
			s = math.mul(math.mul(math.transpose(float3x5), s), float3x5);
			quaternion3 = approxGivensQuat(math.float3(s.c1.y, s.c2.z, s.c1.z), math.float4(1f, 0f, 0f, 1f));
			quaternion2 = math.mul(quaternion2, quaternion3);
			float3x5 = math.float3x3(quaternion3);
			s = math.mul(math.mul(math.transpose(float3x5), s), float3x5);
			quaternion3 = approxGivensQuat(math.float3(s.c2.z, s.c0.x, s.c2.x), math.float4(0f, 1f, 0f, 1f));
			quaternion2 = math.mul(quaternion2, quaternion3);
			float3x5 = math.float3x3(quaternion3);
			s = math.mul(math.mul(math.transpose(float3x5), s), float3x5);
		}
		return quaternion2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float3 singularValuesDecomposition(float3x3 a, out quaternion u, out quaternion v)
	{
		u = quaternion.identity;
		v = quaternion.identity;
		float3x3 s = math.mul(math.transpose(a), a);
		v = jacobiIteration(ref s);
		float3x3 b = math.float3x3(v);
		b = math.mul(a, b);
		sortSingularValues(ref b, ref v);
		u = givensQRFactorization(b, out var r);
		return math.float3(r.c0.x, r.c1.y, r.c2.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float3 rcpsafe(float3 x, float epsilon = 1E-09f)
	{
		return math.select(math.rcp(x), float3.zero, math.abs(x) < epsilon);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3x3 svdInverse(float3x3 a)
	{
		quaternion u;
		quaternion v;
		float3 x = singularValuesDecomposition(a, out u, out v);
		float3x3 v2 = math.float3x3(u);
		return math.mul(math.float3x3(v), math.scaleMul(rcpsafe(x, 1E-06f), math.transpose(v2)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static quaternion svdRotation(float3x3 a)
	{
		singularValuesDecomposition(a, out var u, out var v);
		return math.mul(u, math.conjugate(v));
	}
}
