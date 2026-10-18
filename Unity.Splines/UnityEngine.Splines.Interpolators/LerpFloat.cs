using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Splines.Interpolators;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct LerpFloat : IInterpolator<float>
{
	public float Interpolate(float a, float b, float t)
	{
		return math.lerp(a, b, t);
	}
}
