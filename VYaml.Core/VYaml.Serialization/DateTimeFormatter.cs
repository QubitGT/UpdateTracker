using System;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class DateTimeFormatter : IYamlFormatter<DateTime>, IYamlFormatter
{
	public static readonly DateTimeFormatter Instance = new DateTimeFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, DateTime value, YamlSerializationContext context)
	{
		byte[] buffer = context.GetBuffer64();
		if (Utf8Formatter.TryFormat(value, buffer, out var bytesWritten, new StandardFormat('O')))
		{
			emitter.WriteScalar(buffer[..bytesWritten]);
			return;
		}
		throw new YamlSerializerException($"Cannot format {value}");
	}

	public DateTime Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.TryGetScalarAsSpan(out var span) && Utf8Parser.TryParse(span, out DateTime value, out int bytesConsumed, 'O') && bytesConsumed == span.Length)
		{
			parser.Read();
			return value;
		}
		if (DateTime.TryParse(parser.GetScalarAsString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
		{
			parser.Read();
			return value;
		}
		throw new YamlSerializerException($"Cannot detect a scalar value of DateTime : {parser.CurrentEventType} {parser.GetScalarAsString()}");
	}
}
