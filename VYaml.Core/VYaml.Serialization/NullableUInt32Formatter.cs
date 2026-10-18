using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableUInt32Formatter : IYamlFormatter<uint?>, IYamlFormatter
{
	public static readonly NullableUInt32Formatter Instance = new NullableUInt32Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, uint? value, YamlSerializationContext context)
	{
		if (value.HasValue)
		{
			emitter.WriteUInt32(value.Value);
		}
		else
		{
			emitter.WriteNull();
		}
	}

	public uint? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		uint scalarAsUInt = parser.GetScalarAsUInt32();
		parser.Read();
		return scalarAsUInt;
	}
}
