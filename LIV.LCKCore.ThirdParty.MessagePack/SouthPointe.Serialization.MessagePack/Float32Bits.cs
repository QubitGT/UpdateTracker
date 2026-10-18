using System;
using System.Runtime.InteropServices;

namespace SouthPointe.Serialization.MessagePack;

[StructLayout(LayoutKind.Explicit)]
internal struct Float32Bits
{
	[FieldOffset(0)]
	private float value;

	[FieldOffset(0)]
	private byte byte0;

	[FieldOffset(1)]
	private byte byte1;

	[FieldOffset(2)]
	private byte byte2;

	[FieldOffset(3)]
	private byte byte3;

	private Float32Bits(float value)
	{
		this = default(Float32Bits);
		this.value = value;
	}

	internal static void GetBytes(float value, byte[] buffer)
	{
		Float32Bits float32Bits = new Float32Bits(value);
		if (BitConverter.IsLittleEndian)
		{
			buffer[0] = float32Bits.byte3;
			buffer[1] = float32Bits.byte2;
			buffer[2] = float32Bits.byte1;
			buffer[3] = float32Bits.byte0;
		}
		else
		{
			buffer[0] = float32Bits.byte0;
			buffer[1] = float32Bits.byte1;
			buffer[2] = float32Bits.byte2;
			buffer[3] = float32Bits.byte3;
		}
	}

	internal static float ToSingle(byte[] bigEndianBytes)
	{
		Float32Bits float32Bits = default(Float32Bits);
		if (BitConverter.IsLittleEndian)
		{
			float32Bits.byte0 = bigEndianBytes[3];
			float32Bits.byte1 = bigEndianBytes[2];
			float32Bits.byte2 = bigEndianBytes[1];
			float32Bits.byte3 = bigEndianBytes[0];
		}
		else
		{
			float32Bits.byte0 = bigEndianBytes[0];
			float32Bits.byte1 = bigEndianBytes[1];
			float32Bits.byte2 = bigEndianBytes[2];
			float32Bits.byte3 = bigEndianBytes[3];
		}
		return float32Bits.value;
	}
}
