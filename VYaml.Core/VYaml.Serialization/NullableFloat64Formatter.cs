using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableFloat64Formatter : IYamlFormatter<double?>, IYamlFormatter
{
	public static readonly NullableFloat64Formatter Instance = new NullableFloat64Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, double? value, YamlSerializationContext context)
	{
		if (value.HasValue)
		{
			emitter.WriteDouble(value.Value);
		}
		else
		{
			emitter.WriteNull();
		}
	}

	public double? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		double scalarAsDouble = parser.GetScalarAsDouble();
		parser.Read();
		return scalarAsDouble;
	}
}
