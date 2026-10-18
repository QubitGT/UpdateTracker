using System;
using System.Runtime.InteropServices;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 16)]
[GenerateTestsForBurstCompatibility]
public struct FixedBytes16
{
	[FieldOffset(0)]
	public byte byte0000;

	[FieldOffset(1)]
	public byte byte0001;

	[FieldOffset(2)]
	public byte byte0002;

	[FieldOffset(3)]
	public byte byte0003;

	[FieldOffset(4)]
	public byte byte0004;

	[FieldOffset(5)]
	public byte byte0005;

	[FieldOffset(6)]
	public byte byte0006;

	[FieldOffset(7)]
	public byte byte0007;

	[FieldOffset(8)]
	public byte byte0008;

	[FieldOffset(9)]
	public byte byte0009;

	[FieldOffset(10)]
	public byte byte0010;

	[FieldOffset(11)]
	public byte byte0011;

	[FieldOffset(12)]
	public byte byte0012;

	[FieldOffset(13)]
	public byte byte0013;

	[FieldOffset(14)]
	public byte byte0014;

	[FieldOffset(15)]
	public byte byte0015;
}
