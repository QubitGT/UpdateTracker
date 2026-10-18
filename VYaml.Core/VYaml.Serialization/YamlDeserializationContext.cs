using System.Collections.Generic;
using VYaml.Parser;

namespace VYaml.Serialization;

public class YamlDeserializationContext
{
	private readonly Dictionary<Anchor, object?> aliases = new Dictionary<Anchor, object>();

	public YamlSerializerOptions Options { get; set; }

	public IYamlFormatterResolver Resolver { get; set; }

	public YamlDeserializationContext(YamlSerializerOptions options)
	{
		Options = options;
		Resolver = options.Resolver;
	}

	public void Reset()
	{
		aliases.Clear();
	}

	public T DeserializeWithAlias<T>(ref YamlParser parser)
	{
		IYamlFormatter<T> formatterWithVerify = Resolver.GetFormatterWithVerify<T>();
		return DeserializeWithAlias(formatterWithVerify, ref parser);
	}

	public T DeserializeWithAlias<T>(IYamlFormatter<T> innerFormatter, ref YamlParser parser)
	{
		if (TryResolveCurrentAlias<T>(ref parser, out T aliasValue))
		{
			return aliasValue;
		}
		Anchor anchor;
		bool num = parser.TryGetCurrentAnchor(out anchor);
		T val = innerFormatter.Deserialize(ref parser, this);
		if (num)
		{
			RegisterAnchor(anchor, val);
		}
		return val;
	}

	private void RegisterAnchor(Anchor anchor, object? value)
	{
		aliases[anchor] = value;
	}

	private bool TryResolveCurrentAlias<T>(ref YamlParser parser, out T? aliasValue)
	{
		if (parser.CurrentEventType != ParseEventType.Alias)
		{
			aliasValue = default(T);
			return false;
		}
		if (parser.TryGetCurrentAnchor(out Anchor anchor))
		{
			parser.Read();
			if (aliases.TryGetValue(anchor, out object value))
			{
				if (value != null)
				{
					if (value is T val)
					{
						aliasValue = val;
						return true;
					}
					throw new YamlSerializerException("The alias value is not a type of " + typeof(T).Name);
				}
				aliasValue = default(T);
				return true;
			}
			throw new YamlSerializerException($"Could not found an alias value of anchor: {anchor}");
		}
		aliasValue = default(T);
		return false;
	}
}
