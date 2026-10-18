using System;

namespace SouthPointe.Serialization.MessagePack;

public class DateTimeHandler : IExtTypeHandler, ITypeHandler
{
	private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private readonly SerializationContext context;

	private ITypeHandler stringHandler;

	private ITypeHandler doubleHandler;

	public sbyte ExtType => -1;

	public DateTimeHandler(SerializationContext context)
	{
		this.context = context;
	}

	public object Read(Format format, FormatReader reader)
	{
		if (format.IsExtFamily)
		{
			uint length = reader.ReadExtLength(format);
			if (ExtType == reader.ReadExtType(reader.ReadFormat()))
			{
				return ReadExt(length, reader);
			}
		}
		if (format.IsStringFamily)
		{
			stringHandler = stringHandler ?? context.TypeHandlers.Get<string>();
			return DateTime.Parse((string)stringHandler.Read(format, reader));
		}
		if (format.IsFloatFamily || format.IsIntFamily)
		{
			doubleHandler = doubleHandler ?? context.TypeHandlers.Get<double>();
			double value = (double)doubleHandler.Read(format, reader);
			return epoch.AddSeconds(value).ToLocalTime();
		}
		throw new FormatException(this, format, reader);
	}

	public object ReadExt(uint length, FormatReader reader)
	{
		switch (length)
		{
		case 4u:
			return epoch.AddSeconds(reader.ReadUInt32()).ToLocalTime();
		case 8u:
		{
			byte[] array = reader.ReadBytesOfLength(8);
			uint num = (uint)((array[0] << 22) | (array[1] << 14) | (array[2] << 6)) | ((uint)array[3] >> 2);
			ulong num2 = (ulong)((long)(array[3] & 3) << 32) | ((ulong)array[4] << 24) | ((ulong)array[5] << 16) | ((ulong)array[6] << 8) | array[7];
			return epoch.AddTicks(num / 100).AddSeconds(num2).ToLocalTime();
		}
		case 12u:
			return epoch.AddTicks(reader.ReadUInt32() / 100).AddSeconds(reader.ReadInt64()).ToLocalTime();
		default:
			throw new FormatException();
		}
	}

	public void Write(object obj, FormatWriter writer)
	{
		DateTime dateTime = (DateTime)obj;
		switch (context.DateTimeOptions.PackingFormat)
		{
		case DateTimePackingFormat.Extension:
		{
			TimeSpan timeSpan = dateTime.ToUniversalTime() - epoch;
			writer.WriteExtHeader(12u, ExtType);
			writer.WriteUInt32((uint)((int)(dateTime.Ticks % 10000000) * 100));
			writer.WriteUInt64((ulong)timeSpan.TotalSeconds);
			break;
		}
		case DateTimePackingFormat.String:
			writer.Write(dateTime.ToString("o"));
			break;
		case DateTimePackingFormat.Epoch:
			writer.Write((dateTime.ToUniversalTime() - epoch).TotalSeconds);
			break;
		default:
			throw new FormatException();
		}
	}
}
