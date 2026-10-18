using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class ByteFormatter : IYamlFormatter<byte>, IYamlFormatter
{
	public static readonly ByteFormatter Instance = new ByteFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, byte value, YamlSerializationContext context)
	{
		emitter.WriteInt32(value);
	}

	public byte Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		uint scalarAsUInt = parser.GetScalarAsUInt32();
		parser.Read();
		return checked((byte)scalarAsUInt);
	}
}
