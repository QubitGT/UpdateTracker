using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Splines.Interpolators;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SlerpQuaternion : IInterpolator<quaternion>
{
	public quaternion Interpolate(quaternion a, quaternion b, float t)
	{
		return math.slerp(a, b, t);
	}
}
