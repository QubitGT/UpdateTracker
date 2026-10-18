namespace Liv.Lck.Core.Serialization;

public interface ILckSerializer
{
	SerializationType SerializationType { get; }

	byte[] Serialize(object data);

	T Deserialize<T>(byte[] data);
}
