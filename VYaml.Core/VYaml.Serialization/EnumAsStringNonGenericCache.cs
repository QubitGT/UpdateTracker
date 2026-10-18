using System;
using System.Collections.Concurrent;
using System.Reflection;
using VYaml.Annotations;
using VYaml.Internal;

namespace VYaml.Serialization;

internal class EnumAsStringNonGenericCache
{
	public static readonly EnumAsStringNonGenericCache Instance = new EnumAsStringNonGenericCache();

	private readonly ConcurrentDictionary<object, string> stringValues = new ConcurrentDictionary<object, string>();

	private readonly Func<object, Type, string> valueFactory = CreateValue;

	public string GetStringValue(Type type, object value)
	{
		if (stringValues.TryGetValue(value, out string value2))
		{
			return value2;
		}
		return stringValues.GetOrAdd<Type>(value, valueFactory, type);
	}

	private static string CreateValue(object value, Type type)
	{
		NamingConvention namingConvention = type.GetCustomAttribute<YamlObjectAttribute>()?.NamingConvention ?? NamingConvention.LowerCamelCase;
		return KeyNameMutator.Mutate(Enum.GetName(type, value), namingConvention);
	}
}
