using System;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 256)]
[NetworkStructWeaved(64)]
public struct _64 : INetworkStruct, IFixedStorage
{
	public const int SIZE = 256;

	[FieldOffset(0)]
	public unsafe fixed uint Data[64];

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

	[FieldOffset(128)]
	[NonSerialized]
	private uint _data32;

	[FieldOffset(132)]
	[NonSerialized]
	private uint _data33;

	[FieldOffset(136)]
	[NonSerialized]
	private uint _data34;

	[FieldOffset(140)]
	[NonSerialized]
	private uint _data35;

	[FieldOffset(144)]
	[NonSerialized]
	private uint _data36;

	[FieldOffset(148)]
	[NonSerialized]
	private uint _data37;

	[FieldOffset(152)]
	[NonSerialized]
	private uint _data38;

	[FieldOffset(156)]
	[NonSerialized]
	private uint _data39;

	[FieldOffset(160)]
	[NonSerialized]
	private uint _data40;

	[FieldOffset(164)]
	[NonSerialized]
	private uint _data41;

	[FieldOffset(168)]
	[NonSerialized]
	private uint _data42;

	[FieldOffset(172)]
	[NonSerialized]
	private uint _data43;

	[FieldOffset(176)]
	[NonSerialized]
	private uint _data44;

	[FieldOffset(180)]
	[NonSerialized]
	private uint _data45;

	[FieldOffset(184)]
	[NonSerialized]
	private uint _data46;

	[FieldOffset(188)]
	[NonSerialized]
	private uint _data47;

	[FieldOffset(192)]
	[NonSerialized]
	private uint _data48;

	[FieldOffset(196)]
	[NonSerialized]
	private uint _data49;

	[FieldOffset(200)]
	[NonSerialized]
	private uint _data50;

	[FieldOffset(204)]
	[NonSerialized]
	private uint _data51;

	[FieldOffset(208)]
	[NonSerialized]
	private uint _data52;

	[FieldOffset(212)]
	[NonSerialized]
	private uint _data53;

	[FieldOffset(216)]
	[NonSerialized]
	private uint _data54;

	[FieldOffset(220)]
	[NonSerialized]
	private uint _data55;

	[FieldOffset(224)]
	[NonSerialized]
	private uint _data56;

	[FieldOffset(228)]
	[NonSerialized]
	private uint _data57;

	[FieldOffset(232)]
	[NonSerialized]
	private uint _data58;

	[FieldOffset(236)]
	[NonSerialized]
	private uint _data59;

	[FieldOffset(240)]
	[NonSerialized]
	private uint _data60;

	[FieldOffset(244)]
	[NonSerialized]
	private uint _data61;

	[FieldOffset(248)]
	[NonSerialized]
	private uint _data62;

	[FieldOffset(252)]
	[NonSerialized]
	private uint _data63;
}
