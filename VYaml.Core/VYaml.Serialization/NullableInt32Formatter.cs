using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableInt32Formatter : IYamlFormatter<int?>, IYamlFormatter
{
	public static readonly NullableInt32Formatter Instance = new NullableInt32Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, int? value, YamlSerializationContext context)
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

	public int? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		int scalarAsInt = parser.GetScalarAsInt32();
		parser.Read();
		return scalarAsInt;
	}
}
