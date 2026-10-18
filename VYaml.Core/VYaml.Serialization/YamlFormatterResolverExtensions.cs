using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace VYaml.Serialization;

public static class YamlFormatterResolverExtensions
{
	private static readonly Dictionary<Type, Func<IYamlFormatterResolver, IYamlFormatter>> FormatterGetters = new Dictionary<Type, Func<IYamlFormatterResolver, IYamlFormatter>>();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IYamlFormatter<T> GetFormatterWithVerify<T>(this IYamlFormatterResolver resolver)
	{
		IYamlFormatter<T> formatter;
		try
		{
			formatter = resolver.GetFormatter<T>();
		}
		catch (TypeInitializationException ex)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
			return null;
		}
		if (formatter != null)
		{
			return formatter;
		}
		Throw(typeof(T), resolver);
		return null;
	}

	private static void Throw(Type t, IYamlFormatterResolver resolver)
	{
		throw new YamlSerializerException(t.FullName + $"{t} is not registered in resolver: {resolver.GetType()}");
	}
}
