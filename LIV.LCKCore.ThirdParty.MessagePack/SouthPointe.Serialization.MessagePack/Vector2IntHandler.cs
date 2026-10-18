using UnityEngine;

namespace SouthPointe.Serialization.MessagePack;

public class Vector2IntHandler : ITypeHandler
{
	private readonly SerializationContext context;

	private ITypeHandler intHandler;

	public Vector2IntHandler(SerializationContext context)
	{
		this.context = context;
	}

	public object Read(Format format, FormatReader reader)
	{
		if (format.IsArrayFamily)
		{
			intHandler = intHandler ?? context.TypeHandlers.Get<int>();
			Vector2Int vector2Int = default(Vector2Int);
			vector2Int.x = (int)intHandler.Read(reader.ReadFormat(), reader);
			vector2Int.y = (int)intHandler.Read(reader.ReadFormat(), reader);
			return vector2Int;
		}
		throw new FormatException(this, format, reader);
	}

	public void Write(object obj, FormatWriter writer)
	{
		Vector2Int vector2Int = (Vector2Int)obj;
		writer.WriteArrayHeader(2);
		writer.Write(vector2Int.x);
		writer.Write(vector2Int.y);
	}
}
