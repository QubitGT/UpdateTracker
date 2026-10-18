using System;
using System.Reflection;
using VYaml.Annotations;

namespace VYaml.Serialization;

public class GeneratedResolver : IYamlFormatterResolver
{
	private static class Check<T>
	{
		internal static bool Registered;
	}

	private static class Cache<T>
	{
		internal static IYamlFormatter<T>? Formatter;

		static Cache()
		{
			if (!Check<T>.Registered)
			{
				TryInvokeRegisterYamlFormatter(typeof(T));
			}
		}
	}

	public static readonly GeneratedResolver Instance = new GeneratedResolver();

	private static bool TryInvokeRegisterYamlFormatter(Type type)
	{
		if (type.GetCustomAttribute<YamlObjectAttribute>() == null)
		{
			return false;
		}
		MethodInfo method = type.GetMethod("__RegisterVYamlFormatter", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			return false;
		}
		method.Invoke(null, null);
		return true;
	}

	[Preserve]
	public static void Register<T>(IYamlFormatter<T> formatter)
	{
		Check<T>.Registered = true;
		Cache<T>.Formatter = formatter;
	}

	public IYamlFormatter<T>? GetFormatter<T>()
	{
		return Cache<T>.Formatter;
	}
}
