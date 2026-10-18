using System.Runtime.InteropServices;

namespace UnityEngine.Splines.Interpolators;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct LerpColor : IInterpolator<Color>
{
	public Color Interpolate(Color a, Color b, float t)
	{
		return Color.Lerp(a, b, t);
	}
}
