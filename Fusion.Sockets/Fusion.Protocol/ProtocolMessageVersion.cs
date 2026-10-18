namespace Fusion.Protocol;

internal enum ProtocolMessageVersion : byte
{
	Invalid = 0,
	V1_0_0 = 1,
	V1_1_0 = 2,
	V1_2_0 = 3,
	V1_2_1 = 4,
	V1_2_2 = 5,
	V1_2_3 = 6,
	V1_3_0 = 7,
	V1_4_0 = 8,
	V1_5_0 = 9,
	V1_6_0 = 10,
	LATEST = 10
}
