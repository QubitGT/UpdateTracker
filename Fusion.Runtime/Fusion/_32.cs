using System;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 128)]
[NetworkStructWeaved(32)]
public struct _32 : INetworkStruct, IFixedStorage
{
	public const int SIZE = 128;

	[FieldOffset(0)]
	public unsafe fixed uint Data[32];

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

	[FieldOffset(64)]
	[NonSerialized]
	private uint _data16;

	[FieldOffset(68)]
	[NonSerialized]
	private uint _data17;

	[FieldOffset(72)]
	[NonSerialized]
	private uint _data18;

	[FieldOffset(76)]
	[NonSerialized]
	private uint _data19;

	[FieldOffset(80)]
	[NonSerialized]
	private uint _data20;

	[FieldOffset(84)]
	[NonSerialized]
	private uint _data21;

	[FieldOffset(88)]
	[NonSerialized]
	private uint _data22;

	[FieldOffset(92)]
	[NonSerialized]
	private uint _data23;

	[FieldOffset(96)]
	[NonSerialized]
	private uint _data24;

	[FieldOffset(100)]
	[NonSerialized]
	private uint _data25;

	[FieldOffset(104)]
	[NonSerialized]
	private uint _data26;

	[FieldOffset(108)]
	[NonSerialized]
	private uint _data27;

	[FieldOffset(112)]
	[NonSerialized]
	private uint _data28;

	[FieldOffset(116)]
	[NonSerialized]
	private uint _data29;

	[FieldOffset(120)]
	[NonSerialized]
	private uint _data30;

	[FieldOffset(124)]
	[NonSerialized]
	private uint _data31;
}
