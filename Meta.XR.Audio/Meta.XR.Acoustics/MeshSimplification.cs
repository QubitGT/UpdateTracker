using System;
using System.Runtime.InteropServices;

namespace Meta.XR.Acoustics;

public struct MeshSimplification
{
	public UIntPtr thisSize;

	[MarshalAs(UnmanagedType.U4)]
	public MeshFlags flags;

	public float unitScale;

	public float maxError;

	public float minDiffractionEdgeAngle;

	public float minDiffractionEdgeLength;

	public float flagLength;

	public UIntPtr threadCount;
}
