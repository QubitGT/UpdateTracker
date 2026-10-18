using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class Float64Formatter : IYamlFormatter<double>, IYamlFormatter
{
	public static readonly Float64Formatter Instance = new Float64Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, double value, YamlSerializationContext context)
	{
		emitter.WriteDouble(value);
	}

	public double Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		double scalarAsDouble = parser.GetScalarAsDouble();
		parser.Read();
		return scalarAsDouble;
	}
}
