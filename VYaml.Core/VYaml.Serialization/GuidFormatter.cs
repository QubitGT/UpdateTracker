using System;
using System.Buffers.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class GuidFormatter : IYamlFormatter<Guid>, IYamlFormatter
{
	public static readonly GuidFormatter Instance = new GuidFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, Guid value, YamlSerializationContext context)
	{
		byte[] buffer = context.GetBuffer64();
		if (Utf8Formatter.TryFormat(value, buffer, out var bytesWritten))
		{
			emitter.WriteScalar(buffer[..bytesWritten]);
			return;
		}
		throw new YamlSerializerException($"Cannot serialize {value}");
	}

	public Guid Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.TryGetScalarAsSpan(out var span) && Utf8Parser.TryParse(span, out Guid value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			parser.Read();
			return value;
		}
		throw new YamlSerializerException($"Cannot detect a scalar value of Guid : {parser.CurrentEventType} {parser.GetScalarAsString()}");
	}
}
