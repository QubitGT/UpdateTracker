using System;
using System.Collections;
using System.Collections.Generic;

namespace SouthPointe.Serialization.MessagePack;

public class DynamicListHandler : ITypeHandler
{
	private readonly SerializationContext context;

	private readonly Type innerType;

	private readonly ITypeHandler innerTypeHandler;

	public DynamicListHandler(SerializationContext context, Type type)
	{
		this.context = context;
		innerType = type.GetGenericArguments()[0];
		innerTypeHandler = context.TypeHandlers.Get(innerType);
	}

	public object Read(Format format, FormatReader reader)
	{
		Type type = typeof(List<>).MakeGenericType(innerType);
		if (format.IsArrayFamily)
		{
			IList list = (IList)Activator.CreateInstance(type);
			int num = reader.ReadArrayLength(format);
			for (int i = 0; i < num; i++)
			{
				list.Add(innerTypeHandler.Read(reader.ReadFormat(), reader));
			}
			return list;
		}
		if (format.IsNil)
		{
			if (context.ArrayOptions.NullAsEmptyOnUnpack)
			{
				return Activator.CreateInstance(type);
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
		IList list = (IList)obj;
		writer.WriteArrayHeader(list.Count);
		foreach (object item in list)
		{
			innerTypeHandler.Write(item, writer);
		}
	}
}
