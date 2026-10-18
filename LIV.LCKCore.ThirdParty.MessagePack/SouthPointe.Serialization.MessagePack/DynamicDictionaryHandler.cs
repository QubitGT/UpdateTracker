using System;
using System.Collections;

namespace SouthPointe.Serialization.MessagePack;

public class DynamicDictionaryHandler : ITypeHandler
{
	private readonly Type type;

	private readonly ITypeHandler keyHandler;

	private readonly ITypeHandler valueHandler;

	public DynamicDictionaryHandler(SerializationContext context, Type type)
	{
		Type[] genericArguments = type.GetGenericArguments();
		this.type = type;
		keyHandler = context.TypeHandlers.Get(genericArguments[0]);
		valueHandler = context.TypeHandlers.Get(genericArguments[1]);
	}

	public object Read(Format format, FormatReader reader)
	{
		IDictionary dictionary = (IDictionary)Activator.CreateInstance(type);
		if (format.IsNil)
		{
			return dictionary;
		}
		for (int num = reader.ReadMapLength(format); num > 0; num--)
		{
			object key = keyHandler.Read(reader.ReadFormat(), reader);
			object value = valueHandler.Read(reader.ReadFormat(), reader);
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	public void Write(object obj, FormatWriter writer)
	{
		if (obj == null)
		{
			writer.WriteNil();
			return;
		}
		IDictionary dictionary = (IDictionary)obj;
		writer.WriteMapHeader(dictionary.Count);
		foreach (DictionaryEntry item in dictionary)
		{
			keyHandler.Write(item.Key, writer);
			valueHandler.Write(item.Value, writer);
		}
	}
}
