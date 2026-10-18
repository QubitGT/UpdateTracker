namespace VYaml.Serialization;

public class StandardResolver : IYamlFormatterResolver
{
	private static class FormatterCache<T>
	{
		public static readonly IYamlFormatter<T>? Formatter;

		static FormatterCache()
		{
			if (typeof(T) == typeof(object))
			{
				Formatter = PrimitiveObjectResolver.Instance.GetFormatter<T>();
				return;
			}
			IYamlFormatterResolver[] defaultResolvers = DefaultResolvers;
			for (int i = 0; i < defaultResolvers.Length; i++)
			{
				IYamlFormatter<T> formatter = defaultResolvers[i].GetFormatter<T>();
				if (formatter != null)
				{
					Formatter = formatter;
					break;
				}
			}
		}
	}

	public static readonly StandardResolver Instance = new StandardResolver();

	public static readonly IYamlFormatterResolver[] DefaultResolvers = new IYamlFormatterResolver[2]
	{
		BuiltinResolver.Instance,
		GeneratedResolver.Instance
	};

	public IYamlFormatter<T>? GetFormatter<T>()
	{
		return FormatterCache<T>.Formatter;
	}
}
