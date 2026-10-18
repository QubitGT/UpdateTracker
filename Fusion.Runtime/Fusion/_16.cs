using System;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 64)]
[NetworkStructWeaved(16)]
public struct _16 : INetworkStruct, IFixedStorage
{
	public const int SIZE = 64;

	[FieldOffset(0)]
	public unsafe fixed uint Data[16];

	[FieldOffset(0)]
	[NonSerialized]
	private uint _data0;

	[FieldOffset(4)]
	[NonSerialized]
	private uint _data1;

	[FieldOffset(8)]
	[NonSerialized]
	private uint _data2;

	[FieldOffset(12)]
	[NonSerialized]
	private uint _data3;

	[FieldOffset(16)]
	[NonSerialized]
	private uint _data4;

	[FieldOffset(20)]
	[NonSerialized]
	private uint _data5;

	[FieldOffset(24)]
	[NonSerialized]
	private uint _data6;

	[FieldOffset(28)]
	[NonSerialized]
	private uint _data7;

	[FieldOffset(32)]
	[NonSerialized]
	private uint _data8;

	[FieldOffset(36)]
	[NonSerialized]
	private uint _data9;

	[FieldOffset(40)]
	[NonSerialized]
	private uint _data10;

	[FieldOffset(44)]
	[NonSerialized]
	private uint _data11;

	[FieldOffset(48)]
	[NonSerialized]
	private uint _data12;

	[FieldOffset(52)]
	[NonSerialized]
	private uint _data13;

	[FieldOffset(56)]
	[NonSerialized]
	private uint _data14;

	[FieldOffset(60)]
	[NonSerialized]
	private uint _data15;
}
