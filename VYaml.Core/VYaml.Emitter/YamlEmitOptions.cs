namespace VYaml.Emitter;

public class YamlEmitOptions
{
	public static readonly YamlEmitOptions Default = new YamlEmitOptions();

	public int IndentWidth { get; set; } = 2;

}
