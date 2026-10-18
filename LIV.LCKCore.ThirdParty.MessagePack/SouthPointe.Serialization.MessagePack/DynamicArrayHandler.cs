using System;

namespace SouthPointe.Serialization.MessagePack;

public class DynamicArrayHandler : ITypeHandler
{
	private readonly SerializationContext context;

	private readonly Type elementType;

	private readonly ITypeHandler elementTypeHandler;

	public DynamicArrayHandler(SerializationContext context, Type type)
	{
		this.context = context;
		elementType = type.GetElementType();
		elementTypeHandler = context.TypeHandlers.Get(elementType);
	}

	public object Read(Format format, FormatReader reader)
	{
		if (format.IsArrayFamily)
		{
			int num = reader.ReadArrayLength(format);
			Array array = Array.CreateInstance(elementType, num);
			for (int i = 0; i < num; i++)
			{
				object value = elementTypeHandler.Read(reader.ReadFormat(), reader);
				array.SetValue(value, i);
			}
			return array;
		}
		if (format.IsNil)
		{
			if (context.ArrayOptions.NullAsEmptyOnUnpack)
			{
				return Array.CreateInstance(elementType, 0);
			}
			return null;
		}
		throw new FormatException(this, format, reader);
	}

	public void Write(object obj, FormatWriter writer)
	{
		if (obj == null)
		{
			writer.WriteNil();
			return;
		}
		Array array = (Array)obj;
		writer.WriteArrayHeader(array.Length);
		foreach (object item in array)
		{
			elementTypeHandler.Write(item, writer);
		}
	}
}
