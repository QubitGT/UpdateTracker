using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 32)]
[GenerateTestsForBurstCompatibility]
internal struct FixedBytes32Align8
{
	[FieldOffset(0)]
	[SerializeField]
	internal FixedBytes16Align8 offset0000;

	[FieldOffset(16)]
	[SerializeField]
	internal FixedBytes16Align8 offset0016;
}
