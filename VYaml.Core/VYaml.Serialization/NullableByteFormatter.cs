using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableByteFormatter : IYamlFormatter<byte?>, IYamlFormatter
{
	public static readonly NullableByteFormatter Instance = new NullableByteFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, byte? value, YamlSerializationContext context)
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

	public byte? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		uint scalarAsUInt = parser.GetScalarAsUInt32();
		parser.Read();
		return checked((byte)scalarAsUInt);
	}
}
