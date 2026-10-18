using System;

namespace SouthPointe.Serialization.MessagePack;

public class FloatHandler : ITypeHandler
{
	public object Read(Format format, FormatReader reader)
	{
		if (format.IsFloat32)
		{
			return reader.ReadFloat32();
		}
		if (format.IsFloat64)
		{
			double num = reader.ReadFloat64();
			if (num > 3.4028234663852886E+38)
			{
				throw new InvalidCastException($"{num} is too big for a float");
			}
			if (num < -3.4028234663852886E+38)
			{
				throw new InvalidCastException($"{num} is too small for a float");
			}
			return (float)num;
		}
		if (format.IsPositiveFixInt)
		{
			return (float)(int)reader.ReadPositiveFixInt(format);
		}
		if (format.IsUInt8)
		{
			return (float)(int)reader.ReadUInt8();
		}
		if (format.IsUInt16)
		{
			return (float)(int)reader.ReadUInt16();
		}
		if (format.IsUInt32)
		{
			return (float)reader.ReadUInt32();
		}
		if (format.IsUInt64)
		{
			return (float)reader.ReadUInt64();
		}
		if (format.IsNegativeFixInt)
		{
			return (float)reader.ReadNegativeFixInt(format);
		}
		if (format.IsInt8)
		{
			return (float)reader.ReadInt8();
		}
		if (format.IsInt16)
		{
			return (float)reader.ReadInt16();
		}
		if (format.IsInt32)
		{
			return (float)reader.ReadInt32();
		}
		if (format.IsInt64)
		{
			return (float)reader.ReadInt64();
		}
		if (format.IsNil)
		{
			return 0f;
		}
		throw new FormatException(this, format, reader);
	}

	public void Write(object obj, FormatWriter writer)
	{
		writer.Write((float)obj);
	}
}
