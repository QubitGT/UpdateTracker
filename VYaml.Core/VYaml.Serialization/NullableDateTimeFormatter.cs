using System;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableDateTimeFormatter : IYamlFormatter<DateTime?>, IYamlFormatter
{
	public static readonly NullableDateTimeFormatter Instance = new NullableDateTimeFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, DateTime? value, YamlSerializationContext context)
	{
		if (value.HasValue)
		{
			byte[] buffer = context.GetBuffer64();
			if (!Utf8Formatter.TryFormat(value.Value, buffer, out var bytesWritten, new StandardFormat('O')))
			{
				throw new YamlSerializerException($"Cannot format {value}");
			}
			emitter.WriteScalar(buffer[..bytesWritten]);
		}
		else
		{
			emitter.WriteNull();
		}
	}

	public DateTime? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		if (parser.TryGetScalarAsSpan(out var span) && Utf8Parser.TryParse(span, out DateTime value, out int bytesConsumed, '\0') && bytesConsumed == span.Length)
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
