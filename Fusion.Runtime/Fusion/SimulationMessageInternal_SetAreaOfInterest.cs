using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
internal struct SimulationMessageInternal_SetAreaOfInterest
{
	public const int SIZE = 16;

	[FieldOffset(0)]
	public Vector3 Center;

	[FieldOffset(12)]
	public float Radius;
}
