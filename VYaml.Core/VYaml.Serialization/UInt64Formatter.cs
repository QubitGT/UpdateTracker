using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class UInt64Formatter : IYamlFormatter<ulong>, IYamlFormatter
{
	public static readonly UInt64Formatter Instance = new UInt64Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, ulong value, YamlSerializationContext context)
	{
		emitter.WriteUInt64(value);
	}

	public ulong Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		ulong scalarAsUInt = parser.GetScalarAsUInt64();
		parser.Read();
		return scalarAsUInt;
	}
}
