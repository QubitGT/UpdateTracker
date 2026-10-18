using System;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class UriFormatter : IYamlFormatter<Uri>, IYamlFormatter
{
	public static readonly UriFormatter Instance = new UriFormatter();

	public void Serialize(ref Utf8YamlEmitter emitter, Uri value, YamlSerializationContext context)
	{
		emitter.WriteString(value.ToString());
	}

	public Uri Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.TryGetScalarAsString(out string value) && value != null)
		{
			Uri result = new Uri(value, UriKind.RelativeOrAbsolute);
			parser.Read();
			return result;
		}
		throw new YamlSerializerException($"Cannot detect a scalar value of Uri : {parser.CurrentEventType} {parser.GetScalarAsString()}");
	}
}
