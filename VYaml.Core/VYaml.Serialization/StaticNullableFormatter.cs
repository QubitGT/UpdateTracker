using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public sealed class StaticNullableFormatter<T> : IYamlFormatter<T?>, IYamlFormatter where T : struct
{
	private readonly IYamlFormatter<T> underlyingFormatter;

	public StaticNullableFormatter(IYamlFormatter<T> underlyingFormatter)
	{
		this.underlyingFormatter = underlyingFormatter;
	}

	public void Serialize(ref Utf8YamlEmitter emitter, T? value, YamlSerializationContext context)
	{
		if (value.HasValue)
		{
			underlyingFormatter.Serialize(ref emitter, value.Value, context);
		}
		else
		{
			emitter.WriteNull();
		}
	}

	public T? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		return underlyingFormatter.Deserialize(ref parser, context);
	}
}
