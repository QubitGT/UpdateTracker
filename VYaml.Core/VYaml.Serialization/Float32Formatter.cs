using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class Float32Formatter : IYamlFormatter<float>, IYamlFormatter
{
	public static readonly Float32Formatter Instance = new Float32Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, float value, YamlSerializationContext context)
	{
		emitter.WriteFloat(value);
	}

	public float Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		float scalarAsFloat = parser.GetScalarAsFloat();
		parser.Read();
		return scalarAsFloat;
	}
}
