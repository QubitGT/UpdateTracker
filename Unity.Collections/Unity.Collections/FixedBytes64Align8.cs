using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 64)]
[GenerateTestsForBurstCompatibility]
internal struct FixedBytes64Align8
{
	[FieldOffset(0)]
	[SerializeField]
	internal FixedBytes16Align8 offset0000;

	[FieldOffset(16)]
	[SerializeField]
	internal FixedBytes16Align8 offset0016;

	[FieldOffset(32)]
	[SerializeField]
	internal FixedBytes16Align8 offset0032;

	[FieldOffset(48)]
	[SerializeField]
	internal FixedBytes16Align8 offset0048;
}
