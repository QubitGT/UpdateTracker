using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableUInt16Formatter : IYamlFormatter<ushort?>, IYamlFormatter
{
	public static readonly NullableUInt16Formatter Instance = new NullableUInt16Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, ushort? value, YamlSerializationContext context)
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

	public ushort? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		uint scalarAsUInt = parser.GetScalarAsUInt32();
		parser.Read();
		return checked((ushort)scalarAsUInt);
	}
}
