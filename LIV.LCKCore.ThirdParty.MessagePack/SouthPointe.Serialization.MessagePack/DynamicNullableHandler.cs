using System;

namespace SouthPointe.Serialization.MessagePack;

public class DynamicNullableHandler : ITypeHandler
{
	private readonly ITypeHandler underlyingTypeHandler;

	public DynamicNullableHandler(SerializationContext context, Type type)
	{
		Type underlyingType = Nullable.GetUnderlyingType(type);
		underlyingTypeHandler = context.TypeHandlers.Get(underlyingType);
	}

	public object Read(Format format, FormatReader reader)
	{
		if (format.IsNil)
		{
			return null;
		}
		return underlyingTypeHandler.Read(format, reader);
	}

	public void Write(object obj, FormatWriter writer)
	{
		if (obj == null)
		{
			writer.WriteNil();
		}
		else
		{
			underlyingTypeHandler.Write(obj, writer);
		}
	}
}
