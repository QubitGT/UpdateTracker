using System;
using System.Runtime.InteropServices;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 30)]
[GenerateTestsForBurstCompatibility]
public struct FixedBytes30
{
	[FieldOffset(0)]
	public FixedBytes16 offset0000;

	[FieldOffset(16)]
	public byte byte0016;

	[FieldOffset(17)]
	public byte byte0017;

	[FieldOffset(18)]
	public byte byte0018;

	[FieldOffset(19)]
	public byte byte0019;

	[FieldOffset(20)]
	public byte byte0020;

	[FieldOffset(21)]
	public byte byte0021;

	[FieldOffset(22)]
	public byte byte0022;

	[FieldOffset(23)]
	public byte byte0023;

	[FieldOffset(24)]
	public byte byte0024;

	[FieldOffset(25)]
	public byte byte0025;

	[FieldOffset(26)]
	public byte byte0026;

	[FieldOffset(27)]
	public byte byte0027;

	[FieldOffset(28)]
	public byte byte0028;

	[FieldOffset(29)]
	public byte byte0029;
}
