using System;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 512)]
[NetworkStructWeaved(128)]
public struct _128 : INetworkStruct, IFixedStorage
{
	public const int SIZE = 512;

	[FieldOffset(0)]
	public unsafe fixed uint Data[128];

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

	[FieldOffset(256)]
	[NonSerialized]
	private uint _data64;

	[FieldOffset(260)]
	[NonSerialized]
	private uint _data65;

	[FieldOffset(264)]
	[NonSerialized]
	private uint _data66;

	[FieldOffset(268)]
	[NonSerialized]
	private uint _data67;

	[FieldOffset(272)]
	[NonSerialized]
	private uint _data68;

	[FieldOffset(276)]
	[NonSerialized]
	private uint _data69;

	[FieldOffset(280)]
	[NonSerialized]
	private uint _data70;

	[FieldOffset(284)]
	[NonSerialized]
	private uint _data71;

	[FieldOffset(288)]
	[NonSerialized]
	private uint _data72;

	[FieldOffset(292)]
	[NonSerialized]
	private uint _data73;

	[FieldOffset(296)]
	[NonSerialized]
	private uint _data74;

	[FieldOffset(300)]
	[NonSerialized]
	private uint _data75;

	[FieldOffset(304)]
	[NonSerialized]
	private uint _data76;

	[FieldOffset(308)]
	[NonSerialized]
	private uint _data77;

	[FieldOffset(312)]
	[NonSerialized]
	private uint _data78;

	[FieldOffset(316)]
	[NonSerialized]
	private uint _data79;

	[FieldOffset(320)]
	[NonSerialized]
	private uint _data80;

	[FieldOffset(324)]
	[NonSerialized]
	private uint _data81;

	[FieldOffset(328)]
	[NonSerialized]
	private uint _data82;

	[FieldOffset(332)]
	[NonSerialized]
	private uint _data83;

	[FieldOffset(336)]
	[NonSerialized]
	private uint _data84;

	[FieldOffset(340)]
	[NonSerialized]
	private uint _data85;

	[FieldOffset(344)]
	[NonSerialized]
	private uint _data86;

	[FieldOffset(348)]
	[NonSerialized]
	private uint _data87;

	[FieldOffset(352)]
	[NonSerialized]
	private uint _data88;

	[FieldOffset(356)]
	[NonSerialized]
	private uint _data89;

	[FieldOffset(360)]
	[NonSerialized]
	private uint _data90;

	[FieldOffset(364)]
	[NonSerialized]
	private uint _data91;

	[FieldOffset(368)]
	[NonSerialized]
	private uint _data92;

	[FieldOffset(372)]
	[NonSerialized]
	private uint _data93;

	[FieldOffset(376)]
	[NonSerialized]
	private uint _data94;

	[FieldOffset(380)]
	[NonSerialized]
	private uint _data95;

	[FieldOffset(384)]
	[NonSerialized]
	private uint _data96;

	[FieldOffset(388)]
	[NonSerialized]
	private uint _data97;

	[FieldOffset(392)]
	[NonSerialized]
	private uint _data98;

	[FieldOffset(396)]
	[NonSerialized]
	private uint _data99;

	[FieldOffset(400)]
	[NonSerialized]
	private uint _data100;

	[FieldOffset(404)]
	[NonSerialized]
	private uint _data101;

	[FieldOffset(408)]
	[NonSerialized]
	private uint _data102;

	[FieldOffset(412)]
	[NonSerialized]
	private uint _data103;

	[FieldOffset(416)]
	[NonSerialized]
	private uint _data104;

	[FieldOffset(420)]
	[NonSerialized]
	private uint _data105;

	[FieldOffset(424)]
	[NonSerialized]
	private uint _data106;

	[FieldOffset(428)]
	[NonSerialized]
	private uint _data107;

	[FieldOffset(432)]
	[NonSerialized]
	private uint _data108;

	[FieldOffset(436)]
	[NonSerialized]
	private uint _data109;

	[FieldOffset(440)]
	[NonSerialized]
	private uint _data110;

	[FieldOffset(444)]
	[NonSerialized]
	private uint _data111;

	[FieldOffset(448)]
	[NonSerialized]
	private uint _data112;

	[FieldOffset(452)]
	[NonSerialized]
	private uint _data113;

	[FieldOffset(456)]
	[NonSerialized]
	private uint _data114;

	[FieldOffset(460)]
	[NonSerialized]
	private uint _data115;

	[FieldOffset(464)]
	[NonSerialized]
	private uint _data116;

	[FieldOffset(468)]
	[NonSerialized]
	private uint _data117;

	[FieldOffset(472)]
	[NonSerialized]
	private uint _data118;

	[FieldOffset(476)]
	[NonSerialized]
	private uint _data119;

	[FieldOffset(480)]
	[NonSerialized]
	private uint _data120;

	[FieldOffset(484)]
	[NonSerialized]
	private uint _data121;

	[FieldOffset(488)]
	[NonSerialized]
	private uint _data122;

	[FieldOffset(492)]
	[NonSerialized]
	private uint _data123;

	[FieldOffset(496)]
	[NonSerialized]
	private uint _data124;

	[FieldOffset(500)]
	[NonSerialized]
	private uint _data125;

	[FieldOffset(504)]
	[NonSerialized]
	private uint _data126;

	[FieldOffset(508)]
	[NonSerialized]
	private uint _data127;
}
