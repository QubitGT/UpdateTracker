using System;

namespace UnityEngine.Splines;

[Obsolete("Replaced by GetTangentMode and SetTangentMode.")]
public enum SplineType : byte
{
	CatmullRom,
	Bezier,
	Linear
}
