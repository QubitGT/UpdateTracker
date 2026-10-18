using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class Int64Formatter : IYamlFormatter<long>, IYamlFormatter
{
	public static readonly Int64Formatter Instance = new Int64Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, long value, YamlSerializationContext context)
	{
		emitter.WriteInt64(value);
	}

	public long Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		long scalarAsInt = parser.GetScalarAsInt64();
		parser.Read();
		return scalarAsInt;
	}
}
