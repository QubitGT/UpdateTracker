using System;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 2048)]
[NetworkStructWeaved(512)]
public struct _512 : INetworkStruct, IFixedStorage
{
	public const int SIZE = 2048;

	[FieldOffset(0)]
	public unsafe fixed uint Data[512];

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

	[FieldOffset(1024)]
	[NonSerialized]
	private uint _data256;

	[FieldOffset(1028)]
	[NonSerialized]
	private uint _data257;

	[FieldOffset(1032)]
	[NonSerialized]
	private uint _data258;

	[FieldOffset(1036)]
	[NonSerialized]
	private uint _data259;

	[FieldOffset(1040)]
	[NonSerialized]
	private uint _data260;

	[FieldOffset(1044)]
	[NonSerialized]
	private uint _data261;

	[FieldOffset(1048)]
	[NonSerialized]
	private uint _data262;

	[FieldOffset(1052)]
	[NonSerialized]
	private uint _data263;

	[FieldOffset(1056)]
	[NonSerialized]
	private uint _data264;

	[FieldOffset(1060)]
	[NonSerialized]
	private uint _data265;

	[FieldOffset(1064)]
	[NonSerialized]
	private uint _data266;

	[FieldOffset(1068)]
	[NonSerialized]
	private uint _data267;

	[FieldOffset(1072)]
	[NonSerialized]
	private uint _data268;

	[FieldOffset(1076)]
	[NonSerialized]
	private uint _data269;

	[FieldOffset(1080)]
	[NonSerialized]
	private uint _data270;

	[FieldOffset(1084)]
	[NonSerialized]
	private uint _data271;

	[FieldOffset(1088)]
	[NonSerialized]
	private uint _data272;

	[FieldOffset(1092)]
	[NonSerialized]
	private uint _data273;

	[FieldOffset(1096)]
	[NonSerialized]
	private uint _data274;

	[FieldOffset(1100)]
	[NonSerialized]
	private uint _data275;

	[FieldOffset(1104)]
	[NonSerialized]
	private uint _data276;

	[FieldOffset(1108)]
	[NonSerialized]
	private uint _data277;

	[FieldOffset(1112)]
	[NonSerialized]
	private uint _data278;

	[FieldOffset(1116)]
	[NonSerialized]
	private uint _data279;

	[FieldOffset(1120)]
	[NonSerialized]
	private uint _data280;

	[FieldOffset(1124)]
	[NonSerialized]
	private uint _data281;

	[FieldOffset(1128)]
	[NonSerialized]
	private uint _data282;

	[FieldOffset(1132)]
	[NonSerialized]
	private uint _data283;

	[FieldOffset(1136)]
	[NonSerialized]
	private uint _data284;

	[FieldOffset(1140)]
	[NonSerialized]
	private uint _data285;

	[FieldOffset(1144)]
	[NonSerialized]
	private uint _data286;

	[FieldOffset(1148)]
	[NonSerialized]
	private uint _data287;

	[FieldOffset(1152)]
	[NonSerialized]
	private uint _data288;

	[FieldOffset(1156)]
	[NonSerialized]
	private uint _data289;

	[FieldOffset(1160)]
	[NonSerialized]
	private uint _data290;

	[FieldOffset(1164)]
	[NonSerialized]
	private uint _data291;

	[FieldOffset(1168)]
	[NonSerialized]
	private uint _data292;

	[FieldOffset(1172)]
	[NonSerialized]
	private uint _data293;

	[FieldOffset(1176)]
	[NonSerialized]
	private uint _data294;

	[FieldOffset(1180)]
	[NonSerialized]
	private uint _data295;

	[FieldOffset(1184)]
	[NonSerialized]
	private uint _data296;

	[FieldOffset(1188)]
	[NonSerialized]
	private uint _data297;

	[FieldOffset(1192)]
	[NonSerialized]
	private uint _data298;

	[FieldOffset(1196)]
	[NonSerialized]
	private uint _data299;

	[FieldOffset(1200)]
	[NonSerialized]
	private uint _data300;

	[FieldOffset(1204)]
	[NonSerialized]
	private uint _data301;

	[FieldOffset(1208)]
	[NonSerialized]
	private uint _data302;

	[FieldOffset(1212)]
	[NonSerialized]
	private uint _data303;

	[FieldOffset(1216)]
	[NonSerialized]
	private uint _data304;

	[FieldOffset(1220)]
	[NonSerialized]
	private uint _data305;

	[FieldOffset(1224)]
	[NonSerialized]
	private uint _data306;

	[FieldOffset(1228)]
	[NonSerialized]
	private uint _data307;

	[FieldOffset(1232)]
	[NonSerialized]
	private uint _data308;

	[FieldOffset(1236)]
	[NonSerialized]
	private uint _data309;

	[FieldOffset(1240)]
	[NonSerialized]
	private uint _data310;

	[FieldOffset(1244)]
	[NonSerialized]
	private uint _data311;

	[FieldOffset(1248)]
	[NonSerialized]
	private uint _data312;

	[FieldOffset(1252)]
	[NonSerialized]
	private uint _data313;

	[FieldOffset(1256)]
	[NonSerialized]
	private uint _data314;

	[FieldOffset(1260)]
	[NonSerialized]
	private uint _data315;

	[FieldOffset(1264)]
	[NonSerialized]
	private uint _data316;

	[FieldOffset(1268)]
	[NonSerialized]
	private uint _data317;

	[FieldOffset(1272)]
	[NonSerialized]
	private uint _data318;

	[FieldOffset(1276)]
	[NonSerialized]
	private uint _data319;

	[FieldOffset(1280)]
	[NonSerialized]
	private uint _data320;

	[FieldOffset(1284)]
	[NonSerialized]
	private uint _data321;

	[FieldOffset(1288)]
	[NonSerialized]
	private uint _data322;

	[FieldOffset(1292)]
	[NonSerialized]
	private uint _data323;

	[FieldOffset(1296)]
	[NonSerialized]
	private uint _data324;

	[FieldOffset(1300)]
	[NonSerialized]
	private uint _data325;

	[FieldOffset(1304)]
	[NonSerialized]
	private uint _data326;

	[FieldOffset(1308)]
	[NonSerialized]
	private uint _data327;

	[FieldOffset(1312)]
	[NonSerialized]
	private uint _data328;

	[FieldOffset(1316)]
	[NonSerialized]
	private uint _data329;

	[FieldOffset(1320)]
	[NonSerialized]
	private uint _data330;

	[FieldOffset(1324)]
	[NonSerialized]
	private uint _data331;

	[FieldOffset(1328)]
	[NonSerialized]
	private uint _data332;

	[FieldOffset(1332)]
	[NonSerialized]
	private uint _data333;

	[FieldOffset(1336)]
	[NonSerialized]
	private uint _data334;

	[FieldOffset(1340)]
	[NonSerialized]
	private uint _data335;

	[FieldOffset(1344)]
	[NonSerialized]
	private uint _data336;

	[FieldOffset(1348)]
	[NonSerialized]
	private uint _data337;

	[FieldOffset(1352)]
	[NonSerialized]
	private uint _data338;

	[FieldOffset(1356)]
	[NonSerialized]
	private uint _data339;

	[FieldOffset(1360)]
	[NonSerialized]
	private uint _data340;

	[FieldOffset(1364)]
	[NonSerialized]
	private uint _data341;

	[FieldOffset(1368)]
	[NonSerialized]
	private uint _data342;

	[FieldOffset(1372)]
	[NonSerialized]
	private uint _data343;

	[FieldOffset(1376)]
	[NonSerialized]
	private uint _data344;

	[FieldOffset(1380)]
	[NonSerialized]
	private uint _data345;

	[FieldOffset(1384)]
	[NonSerialized]
	private uint _data346;

	[FieldOffset(1388)]
	[NonSerialized]
	private uint _data347;

	[FieldOffset(1392)]
	[NonSerialized]
	private uint _data348;

	[FieldOffset(1396)]
	[NonSerialized]
	private uint _data349;

	[FieldOffset(1400)]
	[NonSerialized]
	private uint _data350;

	[FieldOffset(1404)]
	[NonSerialized]
	private uint _data351;

	[FieldOffset(1408)]
	[NonSerialized]
	private uint _data352;

	[FieldOffset(1412)]
	[NonSerialized]
	private uint _data353;

	[FieldOffset(1416)]
	[NonSerialized]
	private uint _data354;

	[FieldOffset(1420)]
	[NonSerialized]
	private uint _data355;

	[FieldOffset(1424)]
	[NonSerialized]
	private uint _data356;

	[FieldOffset(1428)]
	[NonSerialized]
	private uint _data357;

	[FieldOffset(1432)]
	[NonSerialized]
	private uint _data358;

	[FieldOffset(1436)]
	[NonSerialized]
	private uint _data359;

	[FieldOffset(1440)]
	[NonSerialized]
	private uint _data360;

	[FieldOffset(1444)]
	[NonSerialized]
	private uint _data361;

	[FieldOffset(1448)]
	[NonSerialized]
	private uint _data362;

	[FieldOffset(1452)]
	[NonSerialized]
	private uint _data363;

	[FieldOffset(1456)]
	[NonSerialized]
	private uint _data364;

	[FieldOffset(1460)]
	[NonSerialized]
	private uint _data365;

	[FieldOffset(1464)]
	[NonSerialized]
	private uint _data366;

	[FieldOffset(1468)]
	[NonSerialized]
	private uint _data367;

	[FieldOffset(1472)]
	[NonSerialized]
	private uint _data368;

	[FieldOffset(1476)]
	[NonSerialized]
	private uint _data369;

	[FieldOffset(1480)]
	[NonSerialized]
	private uint _data370;

	[FieldOffset(1484)]
	[NonSerialized]
	private uint _data371;

	[FieldOffset(1488)]
	[NonSerialized]
	private uint _data372;

	[FieldOffset(1492)]
	[NonSerialized]
	private uint _data373;

	[FieldOffset(1496)]
	[NonSerialized]
	private uint _data374;

	[FieldOffset(1500)]
	[NonSerialized]
	private uint _data375;

	[FieldOffset(1504)]
	[NonSerialized]
	private uint _data376;

	[FieldOffset(1508)]
	[NonSerialized]
	private uint _data377;

	[FieldOffset(1512)]
	[NonSerialized]
	private uint _data378;

	[FieldOffset(1516)]
	[NonSerialized]
	private uint _data379;

	[FieldOffset(1520)]
	[NonSerialized]
	private uint _data380;

	[FieldOffset(1524)]
	[NonSerialized]
	private uint _data381;

	[FieldOffset(1528)]
	[NonSerialized]
	private uint _data382;

	[FieldOffset(1532)]
	[NonSerialized]
	private uint _data383;

	[FieldOffset(1536)]
	[NonSerialized]
	private uint _data384;

	[FieldOffset(1540)]
	[NonSerialized]
	private uint _data385;

	[FieldOffset(1544)]
	[NonSerialized]
	private uint _data386;

	[FieldOffset(1548)]
	[NonSerialized]
	private uint _data387;

	[FieldOffset(1552)]
	[NonSerialized]
	private uint _data388;

	[FieldOffset(1556)]
	[NonSerialized]
	private uint _data389;

	[FieldOffset(1560)]
	[NonSerialized]
	private uint _data390;

	[FieldOffset(1564)]
	[NonSerialized]
	private uint _data391;

	[FieldOffset(1568)]
	[NonSerialized]
	private uint _data392;

	[FieldOffset(1572)]
	[NonSerialized]
	private uint _data393;

	[FieldOffset(1576)]
	[NonSerialized]
	private uint _data394;

	[FieldOffset(1580)]
	[NonSerialized]
	private uint _data395;

	[FieldOffset(1584)]
	[NonSerialized]
	private uint _data396;

	[FieldOffset(1588)]
	[NonSerialized]
	private uint _data397;

	[FieldOffset(1592)]
	[NonSerialized]
	private uint _data398;

	[FieldOffset(1596)]
	[NonSerialized]
	private uint _data399;

	[FieldOffset(1600)]
	[NonSerialized]
	private uint _data400;

	[FieldOffset(1604)]
	[NonSerialized]
	private uint _data401;

	[FieldOffset(1608)]
	[NonSerialized]
	private uint _data402;

	[FieldOffset(1612)]
	[NonSerialized]
	private uint _data403;

	[FieldOffset(1616)]
	[NonSerialized]
	private uint _data404;

	[FieldOffset(1620)]
	[NonSerialized]
	private uint _data405;

	[FieldOffset(1624)]
	[NonSerialized]
	private uint _data406;

	[FieldOffset(1628)]
	[NonSerialized]
	private uint _data407;

	[FieldOffset(1632)]
	[NonSerialized]
	private uint _data408;

	[FieldOffset(1636)]
	[NonSerialized]
	private uint _data409;

	[FieldOffset(1640)]
	[NonSerialized]
	private uint _data410;

	[FieldOffset(1644)]
	[NonSerialized]
	private uint _data411;

	[FieldOffset(1648)]
	[NonSerialized]
	private uint _data412;

	[FieldOffset(1652)]
	[NonSerialized]
	private uint _data413;

	[FieldOffset(1656)]
	[NonSerialized]
	private uint _data414;

	[FieldOffset(1660)]
	[NonSerialized]
	private uint _data415;

	[FieldOffset(1664)]
	[NonSerialized]
	private uint _data416;

	[FieldOffset(1668)]
	[NonSerialized]
	private uint _data417;

	[FieldOffset(1672)]
	[NonSerialized]
	private uint _data418;

	[FieldOffset(1676)]
	[NonSerialized]
	private uint _data419;

	[FieldOffset(1680)]
	[NonSerialized]
	private uint _data420;

	[FieldOffset(1684)]
	[NonSerialized]
	private uint _data421;

	[FieldOffset(1688)]
	[NonSerialized]
	private uint _data422;

	[FieldOffset(1692)]
	[NonSerialized]
	private uint _data423;

	[FieldOffset(1696)]
	[NonSerialized]
	private uint _data424;

	[FieldOffset(1700)]
	[NonSerialized]
	private uint _data425;

	[FieldOffset(1704)]
	[NonSerialized]
	private uint _data426;

	[FieldOffset(1708)]
	[NonSerialized]
	private uint _data427;

	[FieldOffset(1712)]
	[NonSerialized]
	private uint _data428;

	[FieldOffset(1716)]
	[NonSerialized]
	private uint _data429;

	[FieldOffset(1720)]
	[NonSerialized]
	private uint _data430;

	[FieldOffset(1724)]
	[NonSerialized]
	private uint _data431;

	[FieldOffset(1728)]
	[NonSerialized]
	private uint _data432;

	[FieldOffset(1732)]
	[NonSerialized]
	private uint _data433;

	[FieldOffset(1736)]
	[NonSerialized]
	private uint _data434;

	[FieldOffset(1740)]
	[NonSerialized]
	private uint _data435;

	[FieldOffset(1744)]
	[NonSerialized]
	private uint _data436;

	[FieldOffset(1748)]
	[NonSerialized]
	private uint _data437;

	[FieldOffset(1752)]
	[NonSerialized]
	private uint _data438;

	[FieldOffset(1756)]
	[NonSerialized]
	private uint _data439;

	[FieldOffset(1760)]
	[NonSerialized]
	private uint _data440;

	[FieldOffset(1764)]
	[NonSerialized]
	private uint _data441;

	[FieldOffset(1768)]
	[NonSerialized]
	private uint _data442;

	[FieldOffset(1772)]
	[NonSerialized]
	private uint _data443;

	[FieldOffset(1776)]
	[NonSerialized]
	private uint _data444;

	[FieldOffset(1780)]
	[NonSerialized]
	private uint _data445;

	[FieldOffset(1784)]
	[NonSerialized]
	private uint _data446;

	[FieldOffset(1788)]
	[NonSerialized]
	private uint _data447;

	[FieldOffset(1792)]
	[NonSerialized]
	private uint _data448;

	[FieldOffset(1796)]
	[NonSerialized]
	private uint _data449;

	[FieldOffset(1800)]
	[NonSerialized]
	private uint _data450;

	[FieldOffset(1804)]
	[NonSerialized]
	private uint _data451;

	[FieldOffset(1808)]
	[NonSerialized]
	private uint _data452;

	[FieldOffset(1812)]
	[NonSerialized]
	private uint _data453;

	[FieldOffset(1816)]
	[NonSerialized]
	private uint _data454;

	[FieldOffset(1820)]
	[NonSerialized]
	private uint _data455;

	[FieldOffset(1824)]
	[NonSerialized]
	private uint _data456;

	[FieldOffset(1828)]
	[NonSerialized]
	private uint _data457;

	[FieldOffset(1832)]
	[NonSerialized]
	private uint _data458;

	[FieldOffset(1836)]
	[NonSerialized]
	private uint _data459;

	[FieldOffset(1840)]
	[NonSerialized]
	private uint _data460;

	[FieldOffset(1844)]
	[NonSerialized]
	private uint _data461;

	[FieldOffset(1848)]
	[NonSerialized]
	private uint _data462;

	[FieldOffset(1852)]
	[NonSerialized]
	private uint _data463;

	[FieldOffset(1856)]
	[NonSerialized]
	private uint _data464;

	[FieldOffset(1860)]
	[NonSerialized]
	private uint _data465;

	[FieldOffset(1864)]
	[NonSerialized]
	private uint _data466;

	[FieldOffset(1868)]
	[NonSerialized]
	private uint _data467;

	[FieldOffset(1872)]
	[NonSerialized]
	private uint _data468;

	[FieldOffset(1876)]
	[NonSerialized]
	private uint _data469;

	[FieldOffset(1880)]
	[NonSerialized]
	private uint _data470;

	[FieldOffset(1884)]
	[NonSerialized]
	private uint _data471;

	[FieldOffset(1888)]
	[NonSerialized]
	private uint _data472;

	[FieldOffset(1892)]
	[NonSerialized]
	private uint _data473;

	[FieldOffset(1896)]
	[NonSerialized]
	private uint _data474;

	[FieldOffset(1900)]
	[NonSerialized]
	private uint _data475;

	[FieldOffset(1904)]
	[NonSerialized]
	private uint _data476;

	[FieldOffset(1908)]
	[NonSerialized]
	private uint _data477;

	[FieldOffset(1912)]
	[NonSerialized]
	private uint _data478;

	[FieldOffset(1916)]
	[NonSerialized]
	private uint _data479;

	[FieldOffset(1920)]
	[NonSerialized]
	private uint _data480;

	[FieldOffset(1924)]
	[NonSerialized]
	private uint _data481;

	[FieldOffset(1928)]
	[NonSerialized]
	private uint _data482;

	[FieldOffset(1932)]
	[NonSerialized]
	private uint _data483;

	[FieldOffset(1936)]
	[NonSerialized]
	private uint _data484;

	[FieldOffset(1940)]
	[NonSerialized]
	private uint _data485;

	[FieldOffset(1944)]
	[NonSerialized]
	private uint _data486;

	[FieldOffset(1948)]
	[NonSerialized]
	private uint _data487;

	[FieldOffset(1952)]
	[NonSerialized]
	private uint _data488;

	[FieldOffset(1956)]
	[NonSerialized]
	private uint _data489;

	[FieldOffset(1960)]
	[NonSerialized]
	private uint _data490;

	[FieldOffset(1964)]
	[NonSerialized]
	private uint _data491;

	[FieldOffset(1968)]
	[NonSerialized]
	private uint _data492;

	[FieldOffset(1972)]
	[NonSerialized]
	private uint _data493;

	[FieldOffset(1976)]
	[NonSerialized]
	private uint _data494;

	[FieldOffset(1980)]
	[NonSerialized]
	private uint _data495;

	[FieldOffset(1984)]
	[NonSerialized]
	private uint _data496;

	[FieldOffset(1988)]
	[NonSerialized]
	private uint _data497;

	[FieldOffset(1992)]
	[NonSerialized]
	private uint _data498;

	[FieldOffset(1996)]
	[NonSerialized]
	private uint _data499;

	[FieldOffset(2000)]
	[NonSerialized]
	private uint _data500;

	[FieldOffset(2004)]
	[NonSerialized]
	private uint _data501;

	[FieldOffset(2008)]
	[NonSerialized]
	private uint _data502;

	[FieldOffset(2012)]
	[NonSerialized]
	private uint _data503;

	[FieldOffset(2016)]
	[NonSerialized]
	private uint _data504;

	[FieldOffset(2020)]
	[NonSerialized]
	private uint _data505;

	[FieldOffset(2024)]
	[NonSerialized]
	private uint _data506;

	[FieldOffset(2028)]
	[NonSerialized]
	private uint _data507;

	[FieldOffset(2032)]
	[NonSerialized]
	private uint _data508;

	[FieldOffset(2036)]
	[NonSerialized]
	private uint _data509;

	[FieldOffset(2040)]
	[NonSerialized]
	private uint _data510;

	[FieldOffset(2044)]
	[NonSerialized]
	private uint _data511;
}
