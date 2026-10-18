using System;
using System.Runtime.InteropServices;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 126)]
[GenerateTestsForBurstCompatibility]
public struct FixedBytes126
{
	[FieldOffset(0)]
	public FixedBytes16 offset0000;

	[FieldOffset(16)]
	public FixedBytes16 offset0016;

	[FieldOffset(32)]
	public FixedBytes16 offset0032;

	[FieldOffset(48)]
	public FixedBytes16 offset0048;

	[FieldOffset(64)]
	public FixedBytes16 offset0064;

	[FieldOffset(80)]
	public FixedBytes16 offset0080;

	[FieldOffset(96)]
	public FixedBytes16 offset0096;

	[FieldOffset(112)]
	public byte byte0112;

	[FieldOffset(113)]
	public byte byte0113;

	[FieldOffset(114)]
	public byte byte0114;

	[FieldOffset(115)]
	public byte byte0115;

	[FieldOffset(116)]
	public byte byte0116;

	[FieldOffset(117)]
	public byte byte0117;

	[FieldOffset(118)]
	public byte byte0118;

	[FieldOffset(119)]
	public byte byte0119;

	[FieldOffset(120)]
	public byte byte0120;

	[FieldOffset(121)]
	public byte byte0121;

	[FieldOffset(122)]
	public byte byte0122;

	[FieldOffset(123)]
	public byte byte0123;

	[FieldOffset(124)]
	public byte byte0124;

	[FieldOffset(125)]
	public byte byte0125;
}
