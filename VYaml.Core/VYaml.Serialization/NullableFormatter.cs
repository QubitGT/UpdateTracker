using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class NullableFormatter<T> : IYamlFormatter<T?>, IYamlFormatter where T : struct
{
	public void Serialize(ref Utf8YamlEmitter emitter, T? value, YamlSerializationContext context)
	{
		if (!value.HasValue)
		{
			emitter.WriteNull();
		}
		else
		{
			context.Resolver.GetFormatterWithVerify<T>().Serialize(ref emitter, value.Value, context);
		}
	}

	public T? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		return context.DeserializeWithAlias<T>(ref parser);
	}
}
