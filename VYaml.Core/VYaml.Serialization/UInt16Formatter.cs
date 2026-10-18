using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class UInt16Formatter : IYamlFormatter<ushort>, IYamlFormatter
{
	public static readonly UInt16Formatter Instance = new UInt16Formatter();

	public void Serialize(ref Utf8YamlEmitter emitter, ushort value, YamlSerializationContext context)
	{
		emitter.WriteUInt32(value);
	}

	public ushort Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		uint scalarAsUInt = parser.GetScalarAsUInt32();
		parser.Read();
		return checked((ushort)scalarAsUInt);
	}
}
