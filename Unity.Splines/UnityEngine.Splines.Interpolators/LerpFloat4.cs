using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Splines.Interpolators;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct LerpFloat4 : IInterpolator<float4>
{
	public float4 Interpolate(float4 a, float4 b, float t)
	{
		return math.lerp(a, b, t);
	}
}
