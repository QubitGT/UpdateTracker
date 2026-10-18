using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Splines.Interpolators;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct LerpFloat2 : IInterpolator<float2>
{
	public float2 Interpolate(float2 a, float2 b, float t)
	{
		return math.lerp(a, b, t);
	}
}
