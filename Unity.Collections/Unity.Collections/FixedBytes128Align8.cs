using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 128)]
[GenerateTestsForBurstCompatibility]
internal struct FixedBytes128Align8
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

	[FieldOffset(64)]
	[SerializeField]
	internal FixedBytes16Align8 offset0064;

	[FieldOffset(80)]
	[SerializeField]
	internal FixedBytes16Align8 offset0080;

	[FieldOffset(96)]
	[SerializeField]
	internal FixedBytes16Align8 offset0096;

	[FieldOffset(112)]
	[SerializeField]
	internal FixedBytes16Align8 offset0112;
}
