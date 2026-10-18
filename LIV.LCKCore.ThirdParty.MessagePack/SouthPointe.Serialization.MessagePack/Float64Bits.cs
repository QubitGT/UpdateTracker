using System;
using System.Runtime.InteropServices;

namespace SouthPointe.Serialization.MessagePack;

[StructLayout(LayoutKind.Explicit)]
internal struct Float64Bits
{
	[FieldOffset(0)]
	private double value;

	[FieldOffset(0)]
	private byte byte0;

	[FieldOffset(1)]
	private byte byte1;

	[FieldOffset(2)]
	private byte byte2;

	[FieldOffset(3)]
	private byte byte3;

	[FieldOffset(4)]
	private byte byte4;

	[FieldOffset(5)]
	private byte byte5;

	[FieldOffset(6)]
	private byte byte6;

	[FieldOffset(7)]
	private byte byte7;

	private Float64Bits(double value)
	{
		this = default(Float64Bits);
		this.value = value;
	}

	internal static void GetBytes(double value, byte[] buffer)
	{
		Float64Bits float64Bits = new Float64Bits(value);
		if (BitConverter.IsLittleEndian)
		{
			buffer[0] = float64Bits.byte7;
			buffer[1] = float64Bits.byte6;
			buffer[2] = float64Bits.byte5;
			buffer[3] = float64Bits.byte4;
			buffer[4] = float64Bits.byte3;
			buffer[5] = float64Bits.byte2;
			buffer[6] = float64Bits.byte1;
			buffer[7] = float64Bits.byte0;
		}
		else
		{
			buffer[0] = float64Bits.byte0;
			buffer[1] = float64Bits.byte1;
			buffer[2] = float64Bits.byte2;
			buffer[3] = float64Bits.byte3;
			buffer[4] = float64Bits.byte4;
			buffer[5] = float64Bits.byte5;
			buffer[6] = float64Bits.byte6;
			buffer[7] = float64Bits.byte7;
		}
	}

	internal static double ToDouble(byte[] bigEndianBytes)
	{
		Float64Bits float64Bits = default(Float64Bits);
		if (BitConverter.IsLittleEndian)
		{
			float64Bits.byte0 = bigEndianBytes[7];
			float64Bits.byte1 = bigEndianBytes[6];
			float64Bits.byte2 = bigEndianBytes[5];
			float64Bits.byte3 = bigEndianBytes[4];
			float64Bits.byte4 = bigEndianBytes[3];
			float64Bits.byte5 = bigEndianBytes[2];
			float64Bits.byte6 = bigEndianBytes[1];
			float64Bits.byte7 = bigEndianBytes[0];
		}
		else
		{
			float64Bits.byte0 = bigEndianBytes[0];
			float64Bits.byte1 = bigEndianBytes[1];
			float64Bits.byte2 = bigEndianBytes[2];
			float64Bits.byte3 = bigEndianBytes[3];
			float64Bits.byte4 = bigEndianBytes[4];
			float64Bits.byte5 = bigEndianBytes[5];
			float64Bits.byte6 = bigEndianBytes[6];
			float64Bits.byte7 = bigEndianBytes[7];
		}
		return float64Bits.value;
	}
}
