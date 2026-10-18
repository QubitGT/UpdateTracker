using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class Int32Formatter : IYamlFormatter<int>, IYamlFormatter
{
	public static readonly Int32Formatter Instance = new Int32Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, int value, YamlSerializationContext context)
	{
		emitter.WriteInt32(value);
	}

	public int Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		int scalarAsInt = parser.GetScalarAsInt32();
		parser.Read();
		return scalarAsInt;
	}
}
