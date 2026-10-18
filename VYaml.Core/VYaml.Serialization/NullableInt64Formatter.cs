using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableInt64Formatter : IYamlFormatter<long?>, IYamlFormatter
{
	public static readonly NullableInt64Formatter Instance = new NullableInt64Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, long? value, YamlSerializationContext context)
	{
		if (value.HasValue)
		{
			emitter.WriteInt64(value.Value);
		}
		else
		{
			emitter.WriteNull();
		}
	}

	public long? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		long scalarAsInt = parser.GetScalarAsInt64();
		parser.Read();
		return scalarAsInt;
	}
}
