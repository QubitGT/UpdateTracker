using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 16)]
[GenerateTestsForBurstCompatibility]
internal struct FixedBytes16Align8
{
	[FieldOffset(0)]
	[SerializeField]
	public ulong byte0000;

	[FieldOffset(8)]
	[SerializeField]
	public ulong byte0008;
}
