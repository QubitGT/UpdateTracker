using System.IO;
using System.Text;

namespace SouthPointe.Serialization.MessagePack;

public class FormatWriter
{
	private readonly Stream stream;

	private byte[] buffer = new byte[64];

	public FormatWriter(Stream stream)
	{
		this.stream = stream;
	}

	public void WriteFormat(byte formatValue)
	{
		stream.WriteByte(formatValue);
	}

	public void WriteNil()
	{
		stream.WriteByte(192);
	}

	public void Write(bool value)
	{
		stream.WriteByte((byte)(value ? 195 : 194));
	}

	public void Write(byte value)
	{
		if (value <= 127)
		{
			WritePositiveFixInt(value);
			return;
		}
		WriteFormat(204);
		WriteUInt8(value);
	}

	public void Write(ushort value)
	{
		if (value <= 255)
		{
			Write((byte)value);
			return;
		}
		WriteFormat(205);
		WriteUInt16(value);
	}

	public void Write(uint value)
	{
		if (value <= 65535)
		{
			Write((ushort)value);
			return;
		}
		WriteFormat(206);
		WriteUInt32(value);
	}

	public void Write(ulong value)
	{
		if (value <= uint.MaxValue)
		{
			Write((uint)value);
			return;
		}
		WriteFormat(207);
		WriteUInt64(value);
	}

	public void Write(sbyte value)
	{
		if (value >= 0)
		{
			Write((byte)value);
			return;
		}
		if (value >= -32)
		{
			WriteNegativeFixInt(value);
			return;
		}
		WriteFormat(208);
		WriteInt8(value);
	}

	public void Write(short value)
	{
		if (value >= 0)
		{
			Write((ushort)value);
			return;
		}
		if (value >= -128)
		{
			Write((sbyte)value);
			return;
		}
		WriteFormat(209);
		WriteInt16(value);
	}

	public void Write(int value)
	{
		if (value >= 0)
		{
			Write((uint)value);
			return;
		}
		if (value >= -32768)
		{
			Write((short)value);
			return;
		}
		WriteFormat(210);
		WriteInt32(value);
	}

	public void Write(long value)
	{
		if (value >= 0)
		{
			Write((ulong)value);
			return;
		}
		if (value >= int.MinValue)
		{
			Write((int)value);
			return;
		}
		WriteFormat(211);
		WriteInt64(value);
	}

	public void Write(float value)
	{
		WriteFormat(202);
		Float32Bits.GetBytes(value, buffer);
		stream.Write(buffer, 0, 4);
	}

	public void Write(double value)
	{
		WriteFormat(203);
		Float64Bits.GetBytes(value, buffer);
		stream.Write(buffer, 0, 8);
	}

	public void Write(string value)
	{
		if (value == null)
		{
			WriteNil();
			return;
		}
		int byteCount = Encoding.UTF8.GetByteCount(value);
		if (byteCount <= 31)
		{
			WriteFormat((byte)(0xA0u | (byte)byteCount));
		}
		else if (byteCount <= 255)
		{
			WriteFormat(217);
			WriteUInt8((byte)byteCount);
		}
		else if (byteCount <= 65535)
		{
			WriteFormat(218);
			WriteUInt16((ushort)byteCount);
		}
		else
		{
			WriteFormat(219);
			WriteUInt32((uint)byteCount);
		}
		ArrayHelper.AdjustSize(ref buffer, byteCount);
		Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
		stream.Write(buffer, 0, byteCount);
	}

	public void Write(byte[] bytes)
	{
		if (bytes == null)
		{
			WriteNil();
			return;
		}
		if (bytes.Length <= 255)
		{
			WriteFormat(196);
			WriteUInt8((byte)bytes.Length);
		}
		else if (bytes.Length <= 65535)
		{
			WriteFormat(197);
			WriteUInt16((ushort)bytes.Length);
		}
		else
		{
			WriteFormat(198);
			WriteUInt32((uint)bytes.Length);
		}
		stream.Write(bytes, 0, bytes.Length);
	}

	public void WriteArrayHeader(int length)
	{
		if (length <= 15)
		{
			WriteFormat((byte)((uint)length | 0x90u));
		}
		else if (length <= 65535)
		{
			WriteFormat(220);
			WriteUInt16((ushort)length);
		}
		else
		{
			WriteFormat(221);
			WriteUInt32((uint)length);
		}
	}

	public void WriteBinHeader(int length)
	{
		if (length <= 255)
		{
			WriteFormat(196);
			WriteUInt8((byte)length);
		}
		else if (length <= 65535)
		{
			WriteFormat(197);
			WriteUInt16((ushort)length);
		}
		else
		{
			WriteFormat(198);
			WriteUInt32((uint)length);
		}
	}

	public void WriteMapHeader(int length)
	{
		if (length <= 15)
		{
			WriteFormat((byte)((uint)length | 0x80u));
		}
		else if (length <= 65535)
		{
			WriteFormat(222);
			WriteUInt16((ushort)length);
		}
		else
		{
			WriteFormat(223);
			WriteUInt32((uint)length);
		}
	}

	public void WriteExtHeader(uint length, sbyte extType)
	{
		if (length == 1)
		{
			WriteFormat(212);
		}
		else if (length == 2)
		{
			WriteFormat(213);
		}
		else if (length == 4)
		{
			WriteFormat(214);
		}
		else if (length == 8)
		{
			WriteFormat(215);
		}
		else if (length == 16)
		{
			WriteFormat(216);
		}
		else if (length <= 255)
		{
			WriteFormat(199);
			WriteUInt8((byte)length);
		}
		else if (length <= 65535)
		{
			WriteFormat(200);
			WriteUInt16((ushort)length);
		}
		else
		{
			if (length > uint.MaxValue)
			{
				throw new FormatException();
			}
			WriteFormat(201);
			WriteUInt32(length);
		}
		stream.WriteByte((byte)extType);
	}

	public void WritePositiveFixInt(byte value)
	{
		if (value >= 0 || value <= 127)
		{
			stream.WriteByte((byte)(value | 0u));
			return;
		}
		throw new FormatException(value + " is out of range for PositiveFixInt");
	}

	public void WriteUInt8(byte value)
	{
		stream.WriteByte(value);
	}

	public void WriteUInt16(ushort value)
	{
		buffer[0] = (byte)(value >> 8);
		buffer[1] = (byte)value;
		stream.Write(buffer, 0, 2);
	}

	public void WriteUInt32(uint value)
	{
		buffer[0] = (byte)(value >> 24);
		buffer[1] = (byte)(value >> 16);
		buffer[2] = (byte)(value >> 8);
		buffer[3] = (byte)value;
		stream.Write(buffer, 0, 4);
	}

	public void WriteUInt64(ulong value)
	{
		buffer[0] = (byte)(value >> 56);
		buffer[1] = (byte)(value >> 48);
		buffer[2] = (byte)(value >> 40);
		buffer[3] = (byte)(value >> 32);
		buffer[4] = (byte)(value >> 24);
		buffer[5] = (byte)(value >> 16);
		buffer[6] = (byte)(value >> 8);
		buffer[7] = (byte)value;
		stream.Write(buffer, 0, 8);
	}

	public void WriteNegativeFixInt(sbyte value)
	{
		if (value >= -32 && value <= -1)
		{
			stream.WriteByte((byte)((byte)value | 0xE0u));
			return;
		}
		throw new FormatException(value + " is out of range for NegativeFixInt");
	}

	public void WriteInt8(sbyte value)
	{
		stream.WriteByte((byte)value);
	}

	public void WriteInt16(short value)
	{
		buffer[0] = (byte)(value >> 8);
		buffer[1] = (byte)value;
		stream.Write(buffer, 0, 2);
	}

	public void WriteInt32(int value)
	{
		buffer[0] = (byte)(value >> 24);
		buffer[1] = (byte)(value >> 16);
		buffer[2] = (byte)(value >> 8);
		buffer[3] = (byte)value;
		stream.Write(buffer, 0, 4);
	}

	public void WriteInt64(long value)
	{
		buffer[0] = (byte)(value >> 56);
		buffer[1] = (byte)(value >> 48);
		buffer[2] = (byte)(value >> 40);
		buffer[3] = (byte)(value >> 32);
		buffer[4] = (byte)(value >> 24);
		buffer[5] = (byte)(value >> 16);
		buffer[6] = (byte)(value >> 8);
		buffer[7] = (byte)value;
		stream.Write(buffer, 0, 8);
	}

	public void WriteRawByte(byte value)
	{
		stream.WriteByte(value);
	}
}
