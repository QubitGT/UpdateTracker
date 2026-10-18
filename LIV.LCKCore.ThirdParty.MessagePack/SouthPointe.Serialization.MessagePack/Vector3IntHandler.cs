using UnityEngine;

namespace SouthPointe.Serialization.MessagePack;

public class Vector3IntHandler : ITypeHandler
{
	private readonly SerializationContext context;

	private ITypeHandler intHandler;

	public Vector3IntHandler(SerializationContext context)
	{
		this.context = context;
	}

	public object Read(Format format, FormatReader reader)
	{
		if (format.IsArrayFamily)
		{
			intHandler = intHandler ?? context.TypeHandlers.Get<int>();
			Vector3Int vector3Int = default(Vector3Int);
			vector3Int.x = (int)intHandler.Read(reader.ReadFormat(), reader);
			vector3Int.y = (int)intHandler.Read(reader.ReadFormat(), reader);
			vector3Int.z = (int)intHandler.Read(reader.ReadFormat(), reader);
			return vector3Int;
		}
		throw new FormatException(this, format, reader);
	}

	public void Write(object obj, FormatWriter writer)
	{
		Vector3Int vector3Int = (Vector3Int)obj;
		writer.WriteArrayHeader(2);
		writer.Write(vector3Int.x);
		writer.Write(vector3Int.y);
		writer.Write(vector3Int.z);
	}
}
