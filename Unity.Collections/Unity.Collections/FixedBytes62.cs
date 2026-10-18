using System;
using System.Runtime.InteropServices;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 62)]
[GenerateTestsForBurstCompatibility]
public struct FixedBytes62
{
	[FieldOffset(0)]
	public FixedBytes16 offset0000;

	[FieldOffset(16)]
	public FixedBytes16 offset0016;

	[FieldOffset(32)]
	public FixedBytes16 offset0032;

	[FieldOffset(48)]
	public byte byte0048;

	[FieldOffset(49)]
	public byte byte0049;

	[FieldOffset(50)]
	public byte byte0050;

	[FieldOffset(51)]
	public byte byte0051;

	[FieldOffset(52)]
	public byte byte0052;

	[FieldOffset(53)]
	public byte byte0053;

	[FieldOffset(54)]
	public byte byte0054;

	[FieldOffset(55)]
	public byte byte0055;

	[FieldOffset(56)]
	public byte byte0056;

	[FieldOffset(57)]
	public byte byte0057;

	[FieldOffset(58)]
	public byte byte0058;

	[FieldOffset(59)]
	public byte byte0059;

	[FieldOffset(60)]
	public byte byte0060;

	[FieldOffset(61)]
	public byte byte0061;
}
