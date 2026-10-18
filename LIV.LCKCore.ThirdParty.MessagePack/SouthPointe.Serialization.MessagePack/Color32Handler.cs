using System.Collections.Generic;
using UnityEngine;

namespace SouthPointe.Serialization.MessagePack;

public class Color32Handler : ITypeHandler
{
	private readonly SerializationContext context;

	private ITypeHandler byteHandler;

	private ITypeHandler stringHandler;

	private ITypeHandler mapHandler;

	public Color32Handler(SerializationContext context)
	{
		this.context = context;
	}

	public object Read(Format format, FormatReader reader)
	{
		if (format.IsBinaryFamily)
		{
			byte[] array = reader.ReadBin8();
			return new Color32(array[0], array[1], array[2], array[3]);
		}
		if (format.IsArrayFamily)
		{
			byteHandler = byteHandler ?? context.TypeHandlers.Get<byte>();
			int num = reader.ReadArrayLength(format);
			byte[] array2 = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = (byte)byteHandler.Read(reader.ReadFormat(), reader);
			}
			return new Color32(array2[0], array2[1], array2[2], array2[3]);
		}
		if (format.IsStringFamily)
		{
			stringHandler = stringHandler ?? context.TypeHandlers.Get<string>();
			ColorUtility.TryParseHtmlString((string)stringHandler.Read(format, reader), out var color);
			return (Color32)color;
		}
		if (format.IsMapFamily)
		{
			mapHandler = mapHandler ?? context.TypeHandlers.Get<Dictionary<string, byte>>();
			Dictionary<string, byte> dictionary = (Dictionary<string, byte>)mapHandler.Read(format, reader);
			return new Color32(dictionary["r"], dictionary["g"], dictionary["b"], dictionary["a"]);
		}
		throw new FormatException(this, format, reader);
	}

	public void Write(object obj, FormatWriter writer)
	{
		Color32 color = (Color32)obj;
		writer.WriteBinHeader(4);
		writer.WriteRawByte(color.r);
		writer.WriteRawByte(color.g);
		writer.WriteRawByte(color.b);
		writer.WriteRawByte(color.a);
	}
}
