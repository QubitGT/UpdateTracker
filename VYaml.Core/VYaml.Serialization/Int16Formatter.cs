using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class Int16Formatter : IYamlFormatter<short>, IYamlFormatter
{
	public static readonly Int16Formatter Instance = new Int16Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, short value, YamlSerializationContext context)
	{
		emitter.WriteInt32(value);
	}

	public short Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		int scalarAsInt = parser.GetScalarAsInt32();
		parser.Read();
		return checked((short)scalarAsInt);
	}
}
