using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableUInt64Formatter : IYamlFormatter<ulong?>, IYamlFormatter
{
	public static readonly NullableUInt64Formatter Instance = new NullableUInt64Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, ulong? value, YamlSerializationContext context)
	{
		if (value.HasValue)
		{
			emitter.WriteUInt64(value.Value);
		}
		else
		{
			emitter.WriteNull();
		}
	}

	public ulong? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		ulong scalarAsUInt = parser.GetScalarAsUInt64();
		parser.Read();
		return scalarAsUInt;
	}
}
