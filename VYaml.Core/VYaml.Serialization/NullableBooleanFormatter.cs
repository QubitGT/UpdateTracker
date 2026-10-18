using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableBooleanFormatter : IYamlFormatter<bool?>, IYamlFormatter
{
	public static readonly NullableBooleanFormatter Instance = new NullableBooleanFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, bool? value, YamlSerializationContext context)
	{
		if (!value.HasValue)
		{
			emitter.WriteNull();
		}
		else
		{
			emitter.WriteBool(value.Value);
		}
	}

	public bool? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		bool scalarAsBool = parser.GetScalarAsBool();
		parser.Read();
		return scalarAsBool;
	}
}
