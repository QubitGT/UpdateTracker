using System.Buffers.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class DecimalFormatter : IYamlFormatter<decimal>, IYamlFormatter
{
	public static readonly DecimalFormatter Instance = new DecimalFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, decimal value, YamlSerializationContext context)
	{
		byte[] buffer = context.GetBuffer64();
		if (Utf8Formatter.TryFormat(value, buffer, out var bytesWritten))
		{
			emitter.WriteScalar(buffer[..bytesWritten]);
			return;
		}
		throw new YamlSerializerException($"Cannot serialize a value: {value}");
	}

	public decimal Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.TryGetScalarAsSpan(out var span) && Utf8Parser.TryParse(span, out decimal value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			parser.Read();
			return value;
		}
		throw new YamlSerializerException($"Cannot detect a scalar value of decimal : {parser.CurrentEventType} {parser.GetScalarAsString()}");
	}
}
