using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableFloat32Formatter : IYamlFormatter<float?>, IYamlFormatter
{
	public static readonly NullableFloat32Formatter Instance = new NullableFloat32Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, float? value, YamlSerializationContext context)
	{
		if (value.HasValue)
		{
			emitter.WriteFloat(value.Value);
		}
		else
		{
			emitter.WriteNull();
		}
	}

	public float? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		float scalarAsFloat = parser.GetScalarAsFloat();
		parser.Read();
		return scalarAsFloat;
	}
}
