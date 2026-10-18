using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableCharFormatter : IYamlFormatter<char?>, IYamlFormatter
{
	public static readonly NullableCharFormatter Instance = new NullableCharFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, char? value, YamlSerializationContext context)
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

	public char? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		uint scalarAsUInt = parser.GetScalarAsUInt32();
		parser.Read();
		return (char)checked((ushort)scalarAsUInt);
	}
}
