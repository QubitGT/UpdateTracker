using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Splines.Interpolators;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SlerpFloat2 : IInterpolator<float2>
{
	public float2 Interpolate(float2 a, float2 b, float t)
	{
		Vector3 vector = Vector3.Slerp(new Vector3(a.x, a.y, 0f), new Vector3(b.x, b.y, 0f), t);
		return new float2(vector.x, vector.y);
	}
}
