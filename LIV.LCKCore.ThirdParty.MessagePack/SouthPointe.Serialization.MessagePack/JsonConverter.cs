using System;
using System.IO;
using System.Text;

namespace SouthPointe.Serialization.MessagePack;

public class JsonConverter
{
	private readonly SerializationContext context;

	private readonly FormatReader reader;

	private readonly StringBuilder builder;

	private int indentationSize;

	public static string Encode(Stream stream, SerializationContext context = null)
	{
		JsonConverter jsonConverter = new JsonConverter(stream, context);
		try
		{
			return jsonConverter.AppendStream().ToString();
		}
		catch (Exception ex)
		{
			ex.Source = jsonConverter.builder.ToString();
			throw;
		}
	}

	private JsonConverter(Stream stream, SerializationContext context = null)
	{
		this.context = context ?? SerializationContext.Default;
		reader = new FormatReader(stream);
		builder = new StringBuilder();
		indentationSize = 0;
	}

	private JsonConverter AppendStream()
	{
		Format format = reader.ReadFormat();
		if (format.IsNil)
		{
			Append("null");
		}
		else if (format.IsFalse)
		{
			Append("false");
		}
		else if (format.IsTrue)
		{
			Append("true");
		}
		else if (format.IsPositiveFixInt)
		{
			Append(reader.ReadPositiveFixInt(format).ToString());
		}
		else if (format.IsUInt8)
		{
			Append(reader.ReadUInt8().ToString());
		}
		else if (format.IsUInt16)
		{
			Append(reader.ReadUInt16().ToString());
		}
		else if (format.IsUInt32)
		{
			Append(reader.ReadUInt32().ToString());
		}
		else if (format.IsUInt64)
		{
			Append(reader.ReadUInt64().ToString());
		}
		else if (format.IsNegativeFixInt)
		{
			Append(reader.ReadNegativeFixInt(format).ToString());
		}
		else if (format.IsInt8)
		{
			Append(reader.ReadInt8().ToString());
		}
		else if (format.IsInt16)
		{
			Append(reader.ReadInt16().ToString());
		}
		else if (format.IsInt32)
		{
			Append(reader.ReadInt32().ToString());
		}
		else if (format.IsInt64)
		{
			Append(reader.ReadInt64().ToString());
		}
		else if (format.IsFloat32)
		{
			Append(reader.ReadFloat32().ToString());
		}
		else if (format.IsFloat64)
		{
			Append(reader.ReadFloat64().ToString());
		}
		else if (format.IsFixStr)
		{
			AppendQuotedString(reader.ReadFixStr(format));
		}
		else if (format.IsStr8)
		{
			AppendQuotedString(reader.ReadStr8());
		}
		else if (format.IsStr16)
		{
			AppendQuotedString(reader.ReadStr16());
		}
		else if (format.IsStr32)
		{
			AppendQuotedString(reader.ReadStr32());
		}
		else if (format.IsBin8)
		{
			StringifyBinary(reader.ReadBin8());
		}
		else if (format.IsBin16)
		{
			StringifyBinary(reader.ReadBin16());
		}
		else if (format.IsBin32)
		{
			StringifyBinary(reader.ReadBin32());
		}
		else if (format.IsArrayFamily)
		{
			ReadArray(format);
		}
		else if (format.IsMapFamily)
		{
			ReadMap(format);
		}
		else
		{
			if (!format.IsExtFamily)
			{
				throw new FormatException(format, reader);
			}
			ReadExt(format);
		}
		return this;
	}

	public override string ToString()
	{
		return builder.ToString();
	}

	private JsonConverter Indent()
	{
		if (context.JsonOptions.PrettyPrint)
		{
			for (int i = 0; i < indentationSize; i++)
			{
				Append(context.JsonOptions.IndentationString);
			}
		}
		return this;
	}

	private JsonConverter Append(string str)
	{
		builder.Append(str);
		return this;
	}

	private JsonConverter AppendIfPretty(string str)
	{
		if (context.JsonOptions.PrettyPrint)
		{
			Append(str);
		}
		return this;
	}

	private JsonConverter ValueSeparator()
	{
		AppendIfPretty(context.JsonOptions.ValueSeparator);
		return this;
	}

	private JsonConverter AppendQuotedString(string str)
	{
		return Append("\"").Append(str).Append("\"");
	}

	private void StringifyBinary(byte[] bytes)
	{
		Append("[");
		foreach (byte b in bytes)
		{
			Append("0x").Append(b.ToString("X2")).Append(",");
		}
		Append("]");
	}

	private void ReadArray(Format format)
	{
		int num = reader.ReadArrayLength(format);
		if (num == 0)
		{
			Append("[]");
			return;
		}
		Append("[").ValueSeparator();
		indentationSize++;
		for (int i = 0; i < num; i++)
		{
			Indent().AppendStream();
			if (i < num - 1)
			{
				Append(",");
			}
			ValueSeparator();
		}
		indentationSize--;
		Indent().Append("]");
	}

	private void ReadMap(Format format)
	{
		int num = reader.ReadMapLength(format);
		if (num == 0)
		{
			Append("{}");
			return;
		}
		Append("{").ValueSeparator();
		indentationSize++;
		for (int i = 0; i < num; i++)
		{
			Indent().AppendStream().Append(":").AppendIfPretty(" ")
				.AppendStream();
			if (i < num - 1)
			{
				Append(",");
			}
			ValueSeparator();
		}
		indentationSize--;
		Indent().Append("}");
	}

	private void ReadExt(Format format)
	{
		uint length = reader.ReadExtLength(format);
		sbyte extType = reader.ReadExtType(format);
		object obj = context.TypeHandlers.GetExt(extType).ReadExt(length, reader);
		Append(obj.ToString());
	}
}
