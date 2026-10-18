using System;

namespace SouthPointe.Serialization.MessagePack;

public class UriHandler : ITypeHandler
{
	private readonly SerializationContext context;

	private ITypeHandler stringHandler;

	public UriHandler(SerializationContext context)
	{
		this.context = context;
	}

	private ITypeHandler GetStringHandler()
	{
		ITypeHandler obj = stringHandler ?? context.TypeHandlers.Get<string>();
		ITypeHandler result = obj;
		stringHandler = obj;
		return result;
	}

	public object Read(Format format, FormatReader reader)
	{
		if (format.IsNil)
		{
			return null;
		}
		return new Uri((string)GetStringHandler().Read(format, reader));
	}

	public void Write(object obj, FormatWriter writer)
	{
		if (obj == null)
		{
			writer.WriteNil();
			return;
		}
		string obj2 = ((Uri)obj).ToString();
		GetStringHandler().Write(obj2, writer);
	}
}
