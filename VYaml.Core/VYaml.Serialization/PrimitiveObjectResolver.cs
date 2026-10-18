namespace VYaml.Serialization;

public class PrimitiveObjectResolver : IYamlFormatterResolver
{
	private static class FormatterCache<T>
	{
		public static readonly IYamlFormatter<T> Formatter;

		static FormatterCache()
		{
			Formatter = (IYamlFormatter<T>)PrimitiveObjectFormatter.Instance;
		}
	}

	public static readonly PrimitiveObjectResolver Instance = new PrimitiveObjectResolver();

	public IYamlFormatter<T> GetFormatter<T>()
	{
		return FormatterCache<T>.Formatter;
	}
}
