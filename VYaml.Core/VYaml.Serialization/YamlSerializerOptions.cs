using VYaml.Emitter;

namespace VYaml.Serialization;

public class YamlSerializerOptions
{
	public static YamlSerializerOptions Standard => new YamlSerializerOptions
	{
		Resolver = StandardResolver.Instance
	};

	public IYamlFormatterResolver Resolver { get; set; }

	public YamlEmitOptions EmitOptions { get; set; } = new YamlEmitOptions();


	public bool EnableAliasForDeserialization { get; set; } = true;

}
