using System;
using System.Runtime.InteropServices;

namespace Meta.XR.Acoustics;

public struct MapParameters
{
	public UIntPtr thisSize;

	public SceneIRCallbacks callbacks;

	public UIntPtr threadCount;

	public UIntPtr reflectionCount;

	[MarshalAs(UnmanagedType.U4)]
	public AcousticMapFlags flags;

	public float minResolution;

	public float maxResolution;

	public float headHeight;

	public float maxHeight;

	public float gravityVectorX;

	public float gravityVectorY;

	public float gravityVectorZ;
}
