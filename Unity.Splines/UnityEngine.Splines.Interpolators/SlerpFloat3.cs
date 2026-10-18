using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Splines.Interpolators;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SlerpFloat3 : IInterpolator<float3>
{
	public float3 Interpolate(float3 a, float3 b, float t)
	{
		return Vector3.Slerp(a, b, t);
	}
}
