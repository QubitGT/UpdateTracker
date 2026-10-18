using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class CharFormatter : IYamlFormatter<char>, IYamlFormatter
{
	public static readonly CharFormatter Instance = new CharFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, char value, YamlSerializationContext context)
	{
		emitter.WriteInt32(value);
	}

	public char Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		uint scalarAsUInt = parser.GetScalarAsUInt32();
		parser.Read();
		return (char)checked((ushort)scalarAsUInt);
	}
}
