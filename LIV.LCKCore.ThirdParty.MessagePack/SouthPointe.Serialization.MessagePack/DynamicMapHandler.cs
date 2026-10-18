using System;
using System.Collections.Generic;
using System.Reflection;

namespace SouthPointe.Serialization.MessagePack;

public class DynamicMapHandler : ITypeHandler
{
	private readonly SerializationContext context;

	private readonly Lazy<MapDefinition> lazyDefinition;

	private readonly ITypeHandler nameHandler;

	private readonly IMapNamingStrategy nameConverter;

	private static readonly object[] callbackParameters = new object[0];

	public DynamicMapHandler(SerializationContext context, Lazy<MapDefinition> lazyDefinition)
	{
		this.context = context;
		this.lazyDefinition = lazyDefinition;
		nameHandler = context.TypeHandlers.Get<string>();
		nameConverter = context.MapOptions.NamingStrategy;
	}

	public object Read(Format format, FormatReader reader)
	{
		MapDefinition value = lazyDefinition.Value;
		if (format.IsMapFamily)
		{
			object obj = Activator.CreateInstance(value.Type);
			InvokeCallback<OnDeserializingAttribute>(obj, value);
			for (int num = reader.ReadMapLength(format); num > 0; num--)
			{
				string name = (string)nameHandler.Read(reader.ReadFormat(), reader);
				name = nameConverter.OnUnpack(name, value);
				if (value.FieldHandlers.ContainsKey(name))
				{
					object value2 = value.FieldHandlers[name].Read(reader.ReadFormat(), reader);
					value.FieldInfos[name].SetValue(obj, value2);
				}
				else
				{
					if (!context.MapOptions.IgnoreUnknownFieldOnUnpack)
					{
						throw new MissingFieldException(name + " does not exist for type: " + value.Type);
					}
					reader.Skip();
				}
			}
			InvokeCallback<OnDeserializedAttribute>(obj, value);
			return obj;
		}
		if (format.IsEmptyArray && context.MapOptions.AllowEmptyArrayOnUnpack)
		{
			return Activator.CreateInstance(value.Type);
		}
		if (format.IsNil)
		{
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
		MapDefinition value = lazyDefinition.Value;
		InvokeCallback<OnSerializingAttribute>(obj, value);
		writer.WriteMapHeader(DetermineSize(obj, value));
		foreach (KeyValuePair<string, FieldInfo> fieldInfo in value.FieldInfos)
		{
			object value2 = fieldInfo.Value.GetValue(obj);
			if (!context.MapOptions.IgnoreNullOnPack || value2 != null)
			{
				string obj2 = nameConverter.OnPack(fieldInfo.Key, value);
				nameHandler.Write(obj2, writer);
				value.FieldHandlers[fieldInfo.Key].Write(value2, writer);
			}
		}
		InvokeCallback<OnSerializedAttribute>(obj, value);
	}

	private int DetermineSize(object obj, MapDefinition definition)
	{
		if (!context.MapOptions.IgnoreNullOnPack)
		{
			return definition.FieldInfos.Count;
		}
		int num = 0;
		foreach (FieldInfo value in definition.FieldInfos.Values)
		{
			if (value.GetValue(obj) != null)
			{
				num++;
			}
		}
		return num;
	}

	private void InvokeCallback<T>(object obj, MapDefinition definition) where T : Attribute
	{
		Type typeFromHandle = typeof(T);
		if (definition.Callbacks.ContainsKey(typeFromHandle))
		{
			MethodInfo[] array = definition.Callbacks[typeFromHandle];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Invoke(obj, callbackParameters);
			}
		}
	}
}
