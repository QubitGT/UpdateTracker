using System.Collections.Generic;
using UnityEngine;

namespace SouthPointe.Serialization.MessagePack;

public class ColorHandler : ITypeHandler
{
	private readonly SerializationContext context;

	private ITypeHandler floatHandler;

	private ITypeHandler stringHandler;

	private ITypeHandler mapHandler;

	public ColorHandler(SerializationContext context)
	{
		this.context = context;
	}

	public object Read(Format format, FormatReader reader)
	{
		if (format.IsArrayFamily)
		{
			floatHandler = floatHandler ?? context.TypeHandlers.Get<float>();
			int num = reader.ReadArrayLength(format);
			float[] array = new float[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (float)floatHandler.Read(reader.ReadFormat(), reader);
			}
			return new Color(array[0], array[1], array[2], array[3]);
		}
		if (format.IsStringFamily)
		{
			stringHandler = stringHandler ?? context.TypeHandlers.Get<string>();
			ColorUtility.TryParseHtmlString((string)stringHandler.Read(format, reader), out var color);
			return color;
		}
		if (format.IsMapFamily)
		{
			mapHandler = mapHandler ?? context.TypeHandlers.Get<Dictionary<string, float>>();
			Dictionary<string, float> dictionary = (Dictionary<string, float>)mapHandler.Read(format, reader);
			return new Color(dictionary["r"], dictionary["g"], dictionary["b"], dictionary["a"]);
		}
		throw new FormatException(this, format, reader);
	}

	public void Write(object obj, FormatWriter writer)
	{
		Color color = (Color)obj;
		writer.WriteArrayHeader(4);
		writer.Write(color.r);
		writer.Write(color.g);
		writer.Write(color.b);
		writer.Write(color.a);
	}
}
