using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableSByteFormatter : IYamlFormatter<sbyte?>, IYamlFormatter
{
	public static readonly NullableSByteFormatter Instance = new NullableSByteFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, sbyte? value, YamlSerializationContext context)
	{
		if (value.HasValue)
		{
			emitter.WriteInt32(value.Value);
		}
		else
		{
			emitter.WriteNull();
		}
	}

	public sbyte? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		int scalarAsInt = parser.GetScalarAsInt32();
		parser.Read();
		return checked((sbyte)scalarAsInt);
	}
}
