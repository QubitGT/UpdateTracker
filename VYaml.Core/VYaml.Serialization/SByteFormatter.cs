using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class SByteFormatter : IYamlFormatter<sbyte>, IYamlFormatter
{
	public static readonly SByteFormatter Instance = new SByteFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, sbyte value, YamlSerializationContext context)
	{
		emitter.WriteInt32(value);
	}

	public sbyte Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		int scalarAsInt = parser.GetScalarAsInt32();
		parser.Read();
		return checked((sbyte)scalarAsInt);
	}
}
