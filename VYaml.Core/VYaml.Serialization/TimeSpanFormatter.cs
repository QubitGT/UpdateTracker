using System;
using System.Buffers.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class TimeSpanFormatter : IYamlFormatter<TimeSpan>, IYamlFormatter
{
	public static readonly TimeSpanFormatter Instance = new TimeSpanFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, TimeSpan value, YamlSerializationContext context)
	{
		byte[] buffer = context.GetBuffer64();
		if (Utf8Formatter.TryFormat(value, buffer, out var bytesWritten))
		{
			emitter.WriteScalar(buffer[..bytesWritten]);
			return;
		}
		throw new YamlSerializerException($"Cannot serialize a value: {value}");
	}

	public TimeSpan Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.TryGetScalarAsSpan(out var span) && Utf8Parser.TryParse(span, out TimeSpan value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
		{
			parser.Read();
			return value;
		}
		throw new YamlSerializerException($"Cannot detect a scalar value of TimeSpan : {parser.CurrentEventType} {parser.GetScalarAsString()}");
	}
}
