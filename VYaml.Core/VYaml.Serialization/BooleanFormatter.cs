using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class BooleanFormatter : IYamlFormatter<bool>, IYamlFormatter
{
	public static readonly BooleanFormatter Instance = new BooleanFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, bool value, YamlSerializationContext context)
	{
		emitter.WriteBool(value);
	}

	public bool Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		bool scalarAsBool = parser.GetScalarAsBool();
		parser.Read();
		return scalarAsBool;
	}
}
