using System;
using System.IO;
using System.Text;

namespace SouthPointe.Serialization.MessagePack;

public class FormatReader
{
	private readonly Stream stream;

	private byte[] buffer = new byte[64];

	internal long Position => stream.Position;

	public FormatReader(Stream stream)
	{
		this.stream = stream;
	}

	public Format ReadFormat()
	{
		int num = stream.ReadByte();
		if (num >= 0)
		{
			return new Format((byte)num);
		}
		throw new FormatException("There is nothing more to read");
	}

	public byte ReadPositiveFixInt(Format format)
	{
		return format & 127;
	}

	public byte ReadUInt8()
	{
		return (byte)stream.ReadByte();
	}

	public ushort ReadUInt16()
	{
		if (stream.Read(buffer, 0, 2) == 2)
		{
			return (ushort)((buffer[0] << 8) | buffer[1]);
		}
		throw new FormatException();
	}

	public uint ReadUInt32()
	{
		if (stream.Read(buffer, 0, 4) == 4)
		{
			return (uint)((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3]);
		}
		throw new FormatException();
	}

	public ulong ReadUInt64()
	{
		if (stream.Read(buffer, 0, 8) == 8)
		{
			return ((ulong)buffer[0] << 56) | ((ulong)buffer[1] << 48) | ((ulong)buffer[2] << 40) | ((ulong)buffer[3] << 32) | ((ulong)buffer[4] << 24) | ((ulong)buffer[5] << 16) | ((ulong)buffer[6] << 8) | buffer[7];
		}
		throw new FormatException();
	}

	public sbyte ReadNegativeFixInt(Format format)
	{
		return (sbyte)((format & 31) - 32);
	}

	public sbyte ReadInt8()
	{
		return (sbyte)stream.ReadByte();
	}

	public short ReadInt16()
	{
		if (stream.Read(buffer, 0, 2) == 2)
		{
			return (short)((buffer[0] << 8) | buffer[1]);
		}
		throw new FormatException();
	}

	public int ReadInt32()
	{
		if (stream.Read(buffer, 0, 4) == 4)
		{
			return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
		}
		throw new FormatException();
	}

	public long ReadInt64()
	{
		if (stream.Read(buffer, 0, 8) == 8)
		{
			return (long)(((ulong)buffer[0] << 56) | ((ulong)buffer[1] << 48) | ((ulong)buffer[2] << 40) | ((ulong)buffer[3] << 32) | ((ulong)buffer[4] << 24) | ((ulong)buffer[5] << 16) | ((ulong)buffer[6] << 8) | buffer[7]);
		}
		throw new FormatException();
	}

	public float ReadFloat32()
	{
		if (stream.Read(buffer, 0, 4) == 4)
		{
			return Float32Bits.ToSingle(buffer);
		}
		throw new FormatException();
	}

	public double ReadFloat64()
	{
		if (stream.Read(buffer, 0, 8) == 8)
		{
			return Float64Bits.ToDouble(buffer);
		}
		throw new FormatException();
	}

	public string ReadFixStr(Format format)
	{
		return ReadStringOfLength(format & 31);
	}

	public string ReadStr8()
	{
		return ReadStringOfLength(ReadUInt8());
	}

	public string ReadStr16()
	{
		return ReadStringOfLength(ReadUInt16());
	}

	public string ReadStr32()
	{
		return ReadStringOfLength(Convert.ToInt32(ReadUInt32()));
	}

	public byte[] ReadBin8()
	{
		return ReadBytesOfLength(ReadUInt8());
	}

	public byte[] ReadBin16()
	{
		return ReadBytesOfLength(ReadUInt16());
	}

	public byte[] ReadBin32()
	{
		return ReadBytesOfLength(Convert.ToInt32(ReadUInt32()));
	}

	public int ReadArrayLength(Format format)
	{
		if (format.IsNil)
		{
			return 0;
		}
		if (format.IsFixArray)
		{
			return format & 15;
		}
		if (format.IsArray16)
		{
			return ReadUInt16();
		}
		if (format.IsArray32)
		{
			return Convert.ToInt32(ReadUInt32());
		}
		throw new FormatException();
	}

	public int ReadMapLength(Format format)
	{
		if (format.IsFixMap)
		{
			return format & 15;
		}
		if (format.IsMap16)
		{
			return ReadUInt16();
		}
		if (format.IsMap32)
		{
			return Convert.ToInt32(ReadUInt32());
		}
		throw new FormatException();
	}

	public uint ReadExtLength(Format format)
	{
		if (format.IsFixExt1)
		{
			return 1u;
		}
		if (format.IsFixExt2)
		{
			return 2u;
		}
		if (format.IsFixExt4)
		{
			return 4u;
		}
		if (format.IsFixExt8)
		{
			return 8u;
		}
		if (format.IsFixExt16)
		{
			return 16u;
		}
		if (format.IsExt8)
		{
			return ReadUInt8();
		}
		if (format.IsExt16)
		{
			return ReadUInt16();
		}
		if (format.IsExt32)
		{
			return ReadUInt32();
		}
		throw new FormatException();
	}

	public sbyte ReadExtType(Format format)
	{
		if (format.IsPositiveFixInt)
		{
			return (sbyte)ReadPositiveFixInt(format);
		}
		if (format.IsUInt8)
		{
			return Convert.ToSByte(ReadUInt8());
		}
		if (format.IsNegativeFixInt)
		{
			return ReadNegativeFixInt(format);
		}
		if (format.IsInt8)
		{
			return ReadInt8();
		}
		throw new FormatException();
	}

	public void Skip()
	{
		Format format = ReadFormat();
		if (format.IsNil || format.IsFalse || format.IsTrue || format.IsPositiveFixInt || format.IsNegativeFixInt)
		{
			return;
		}
		if (format.IsUInt8 || format.IsInt8)
		{
			FastForward(1L);
		}
		else if (format.IsUInt16 || format.IsInt16)
		{
			FastForward(2L);
		}
		else if (format.IsUInt32 || format.IsInt32)
		{
			FastForward(4L);
		}
		else if (format.IsUInt64 || format.IsInt64)
		{
			FastForward(8L);
		}
		else if (format.IsFloat32)
		{
			FastForward(4L);
		}
		else if (format.IsFloat64)
		{
			FastForward(8L);
		}
		else if (format.IsFixStr)
		{
			FastForward(format & 31);
		}
		else if (format.IsStr8)
		{
			FastForward(ReadUInt8());
		}
		else if (format.IsStr16)
		{
			FastForward(ReadUInt16());
		}
		else if (format.IsStr32)
		{
			FastForward(ReadUInt32());
		}
		else if (format.IsBin8)
		{
			FastForward(ReadUInt8());
		}
		else if (format.IsBin16)
		{
			FastForward(ReadUInt16());
		}
		else if (format.IsBin32)
		{
			FastForward(ReadUInt32());
		}
		else if (format.IsArrayFamily)
		{
			for (int num = ReadArrayLength(format); num > 0; num--)
			{
				Skip();
			}
		}
		else if (format.IsMapFamily)
		{
			for (int num2 = ReadMapLength(format); num2 > 0; num2--)
			{
				Skip();
				Skip();
			}
		}
		else if (format.IsFixExt1)
		{
			FastForward(2L);
		}
		else if (format.IsFixExt2)
		{
			FastForward(3L);
		}
		else if (format.IsFixExt4)
		{
			FastForward(5L);
		}
		else if (format.IsFixExt8)
		{
			FastForward(9L);
		}
		else if (format.IsFixExt16)
		{
			FastForward(17L);
		}
		else if (format.IsExt8)
		{
			FastForward(ReadUInt8() + 1);
		}
		else if (format.IsExt16)
		{
			FastForward(ReadUInt16() + 1);
		}
		else if (format.IsExt32)
		{
			FastForward(ReadUInt32() + 1);
		}
	}

	private void FastForward(long offset)
	{
		if (stream.CanSeek)
		{
			stream.Seek(offset, SeekOrigin.Current);
			return;
		}
		while (offset > 0)
		{
			int num = (int)((offset > int.MaxValue) ? int.MaxValue : offset);
			ArrayHelper.AdjustSize(ref buffer, num);
			stream.Read(buffer, 0, num);
			offset -= int.MaxValue;
		}
	}

	private string ReadStringOfLength(int length)
	{
		ArrayHelper.AdjustSize(ref buffer, length);
		stream.Read(buffer, 0, length);
		return Encoding.UTF8.GetString(buffer, 0, length);
	}

	internal byte[] ReadBytesOfLength(int length)
	{
		byte[] result = new byte[length];
		stream.Read(result, 0, length);
		return result;
	}
}
