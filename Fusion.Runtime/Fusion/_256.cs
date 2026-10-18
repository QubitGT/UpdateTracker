using System;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 1024)]
[NetworkStructWeaved(256)]
public struct _256 : INetworkStruct, IFixedStorage
{
	public const int SIZE = 1024;

	[FieldOffset(0)]
	public unsafe fixed uint Data[256];

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

	[FieldOffset(512)]
	[NonSerialized]
	private uint _data128;

	[FieldOffset(516)]
	[NonSerialized]
	private uint _data129;

	[FieldOffset(520)]
	[NonSerialized]
	private uint _data130;

	[FieldOffset(524)]
	[NonSerialized]
	private uint _data131;

	[FieldOffset(528)]
	[NonSerialized]
	private uint _data132;

	[FieldOffset(532)]
	[NonSerialized]
	private uint _data133;

	[FieldOffset(536)]
	[NonSerialized]
	private uint _data134;

	[FieldOffset(540)]
	[NonSerialized]
	private uint _data135;

	[FieldOffset(544)]
	[NonSerialized]
	private uint _data136;

	[FieldOffset(548)]
	[NonSerialized]
	private uint _data137;

	[FieldOffset(552)]
	[NonSerialized]
	private uint _data138;

	[FieldOffset(556)]
	[NonSerialized]
	private uint _data139;

	[FieldOffset(560)]
	[NonSerialized]
	private uint _data140;

	[FieldOffset(564)]
	[NonSerialized]
	private uint _data141;

	[FieldOffset(568)]
	[NonSerialized]
	private uint _data142;

	[FieldOffset(572)]
	[NonSerialized]
	private uint _data143;

	[FieldOffset(576)]
	[NonSerialized]
	private uint _data144;

	[FieldOffset(580)]
	[NonSerialized]
	private uint _data145;

	[FieldOffset(584)]
	[NonSerialized]
	private uint _data146;

	[FieldOffset(588)]
	[NonSerialized]
	private uint _data147;

	[FieldOffset(592)]
	[NonSerialized]
	private uint _data148;

	[FieldOffset(596)]
	[NonSerialized]
	private uint _data149;

	[FieldOffset(600)]
	[NonSerialized]
	private uint _data150;

	[FieldOffset(604)]
	[NonSerialized]
	private uint _data151;

	[FieldOffset(608)]
	[NonSerialized]
	private uint _data152;

	[FieldOffset(612)]
	[NonSerialized]
	private uint _data153;

	[FieldOffset(616)]
	[NonSerialized]
	private uint _data154;

	[FieldOffset(620)]
	[NonSerialized]
	private uint _data155;

	[FieldOffset(624)]
	[NonSerialized]
	private uint _data156;

	[FieldOffset(628)]
	[NonSerialized]
	private uint _data157;

	[FieldOffset(632)]
	[NonSerialized]
	private uint _data158;

	[FieldOffset(636)]
	[NonSerialized]
	private uint _data159;

	[FieldOffset(640)]
	[NonSerialized]
	private uint _data160;

	[FieldOffset(644)]
	[NonSerialized]
	private uint _data161;

	[FieldOffset(648)]
	[NonSerialized]
	private uint _data162;

	[FieldOffset(652)]
	[NonSerialized]
	private uint _data163;

	[FieldOffset(656)]
	[NonSerialized]
	private uint _data164;

	[FieldOffset(660)]
	[NonSerialized]
	private uint _data165;

	[FieldOffset(664)]
	[NonSerialized]
	private uint _data166;

	[FieldOffset(668)]
	[NonSerialized]
	private uint _data167;

	[FieldOffset(672)]
	[NonSerialized]
	private uint _data168;

	[FieldOffset(676)]
	[NonSerialized]
	private uint _data169;

	[FieldOffset(680)]
	[NonSerialized]
	private uint _data170;

	[FieldOffset(684)]
	[NonSerialized]
	private uint _data171;

	[FieldOffset(688)]
	[NonSerialized]
	private uint _data172;

	[FieldOffset(692)]
	[NonSerialized]
	private uint _data173;

	[FieldOffset(696)]
	[NonSerialized]
	private uint _data174;

	[FieldOffset(700)]
	[NonSerialized]
	private uint _data175;

	[FieldOffset(704)]
	[NonSerialized]
	private uint _data176;

	[FieldOffset(708)]
	[NonSerialized]
	private uint _data177;

	[FieldOffset(712)]
	[NonSerialized]
	private uint _data178;

	[FieldOffset(716)]
	[NonSerialized]
	private uint _data179;

	[FieldOffset(720)]
	[NonSerialized]
	private uint _data180;

	[FieldOffset(724)]
	[NonSerialized]
	private uint _data181;

	[FieldOffset(728)]
	[NonSerialized]
	private uint _data182;

	[FieldOffset(732)]
	[NonSerialized]
	private uint _data183;

	[FieldOffset(736)]
	[NonSerialized]
	private uint _data184;

	[FieldOffset(740)]
	[NonSerialized]
	private uint _data185;

	[FieldOffset(744)]
	[NonSerialized]
	private uint _data186;

	[FieldOffset(748)]
	[NonSerialized]
	private uint _data187;

	[FieldOffset(752)]
	[NonSerialized]
	private uint _data188;

	[FieldOffset(756)]
	[NonSerialized]
	private uint _data189;

	[FieldOffset(760)]
	[NonSerialized]
	private uint _data190;

	[FieldOffset(764)]
	[NonSerialized]
	private uint _data191;

	[FieldOffset(768)]
	[NonSerialized]
	private uint _data192;

	[FieldOffset(772)]
	[NonSerialized]
	private uint _data193;

	[FieldOffset(776)]
	[NonSerialized]
	private uint _data194;

	[FieldOffset(780)]
	[NonSerialized]
	private uint _data195;

	[FieldOffset(784)]
	[NonSerialized]
	private uint _data196;

	[FieldOffset(788)]
	[NonSerialized]
	private uint _data197;

	[FieldOffset(792)]
	[NonSerialized]
	private uint _data198;

	[FieldOffset(796)]
	[NonSerialized]
	private uint _data199;

	[FieldOffset(800)]
	[NonSerialized]
	private uint _data200;

	[FieldOffset(804)]
	[NonSerialized]
	private uint _data201;

	[FieldOffset(808)]
	[NonSerialized]
	private uint _data202;

	[FieldOffset(812)]
	[NonSerialized]
	private uint _data203;

	[FieldOffset(816)]
	[NonSerialized]
	private uint _data204;

	[FieldOffset(820)]
	[NonSerialized]
	private uint _data205;

	[FieldOffset(824)]
	[NonSerialized]
	private uint _data206;

	[FieldOffset(828)]
	[NonSerialized]
	private uint _data207;

	[FieldOffset(832)]
	[NonSerialized]
	private uint _data208;

	[FieldOffset(836)]
	[NonSerialized]
	private uint _data209;

	[FieldOffset(840)]
	[NonSerialized]
	private uint _data210;

	[FieldOffset(844)]
	[NonSerialized]
	private uint _data211;

	[FieldOffset(848)]
	[NonSerialized]
	private uint _data212;

	[FieldOffset(852)]
	[NonSerialized]
	private uint _data213;

	[FieldOffset(856)]
	[NonSerialized]
	private uint _data214;

	[FieldOffset(860)]
	[NonSerialized]
	private uint _data215;

	[FieldOffset(864)]
	[NonSerialized]
	private uint _data216;

	[FieldOffset(868)]
	[NonSerialized]
	private uint _data217;

	[FieldOffset(872)]
	[NonSerialized]
	private uint _data218;

	[FieldOffset(876)]
	[NonSerialized]
	private uint _data219;

	[FieldOffset(880)]
	[NonSerialized]
	private uint _data220;

	[FieldOffset(884)]
	[NonSerialized]
	private uint _data221;

	[FieldOffset(888)]
	[NonSerialized]
	private uint _data222;

	[FieldOffset(892)]
	[NonSerialized]
	private uint _data223;

	[FieldOffset(896)]
	[NonSerialized]
	private uint _data224;

	[FieldOffset(900)]
	[NonSerialized]
	private uint _data225;

	[FieldOffset(904)]
	[NonSerialized]
	private uint _data226;

	[FieldOffset(908)]
	[NonSerialized]
	private uint _data227;

	[FieldOffset(912)]
	[NonSerialized]
	private uint _data228;

	[FieldOffset(916)]
	[NonSerialized]
	private uint _data229;

	[FieldOffset(920)]
	[NonSerialized]
	private uint _data230;

	[FieldOffset(924)]
	[NonSerialized]
	private uint _data231;

	[FieldOffset(928)]
	[NonSerialized]
	private uint _data232;

	[FieldOffset(932)]
	[NonSerialized]
	private uint _data233;

	[FieldOffset(936)]
	[NonSerialized]
	private uint _data234;

	[FieldOffset(940)]
	[NonSerialized]
	private uint _data235;

	[FieldOffset(944)]
	[NonSerialized]
	private uint _data236;

	[FieldOffset(948)]
	[NonSerialized]
	private uint _data237;

	[FieldOffset(952)]
	[NonSerialized]
	private uint _data238;

	[FieldOffset(956)]
	[NonSerialized]
	private uint _data239;

	[FieldOffset(960)]
	[NonSerialized]
	private uint _data240;

	[FieldOffset(964)]
	[NonSerialized]
	private uint _data241;

	[FieldOffset(968)]
	[NonSerialized]
	private uint _data242;

	[FieldOffset(972)]
	[NonSerialized]
	private uint _data243;

	[FieldOffset(976)]
	[NonSerialized]
	private uint _data244;

	[FieldOffset(980)]
	[NonSerialized]
	private uint _data245;

	[FieldOffset(984)]
	[NonSerialized]
	private uint _data246;

	[FieldOffset(988)]
	[NonSerialized]
	private uint _data247;

	[FieldOffset(992)]
	[NonSerialized]
	private uint _data248;

	[FieldOffset(996)]
	[NonSerialized]
	private uint _data249;

	[FieldOffset(1000)]
	[NonSerialized]
	private uint _data250;

	[FieldOffset(1004)]
	[NonSerialized]
	private uint _data251;

	[FieldOffset(1008)]
	[NonSerialized]
	private uint _data252;

	[FieldOffset(1012)]
	[NonSerialized]
	private uint _data253;

	[FieldOffset(1016)]
	[NonSerialized]
	private uint _data254;

	[FieldOffset(1020)]
	[NonSerialized]
	private uint _data255;
}
