using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace VYaml.Serialization;

public class CompositeResolver : IYamlFormatterResolver
{
	private readonly ConcurrentDictionary<Type, IYamlFormatter> formattersCache = new ConcurrentDictionary<Type, IYamlFormatter>();

	private readonly List<IYamlFormatter> formatters;

	private readonly List<IYamlFormatterResolver> resolvers;

	private readonly object gate = new object();

	public static CompositeResolver Create(IEnumerable<IYamlFormatter> formatters, IEnumerable<IYamlFormatterResolver> resolvers)
	{
		return new CompositeResolver(formatters.ToList(), resolvers.ToList());
	}

	public static CompositeResolver Create(IEnumerable<IYamlFormatter> formatters)
	{
		return new CompositeResolver(formatters.ToList());
	}

	public static CompositeResolver Create(IEnumerable<IYamlFormatterResolver> resolvers)
	{
		return new CompositeResolver(null, resolvers.ToList());
	}

	private CompositeResolver(List<IYamlFormatter>? formatters = null, List<IYamlFormatterResolver>? resolvers = null)
	{
		this.formatters = formatters ?? new List<IYamlFormatter>();
		this.resolvers = resolvers ?? new List<IYamlFormatterResolver>();
	}

	public IYamlFormatter<T>? GetFormatter<T>()
	{
		if (!formattersCache.TryGetValue(typeof(T), out IYamlFormatter value))
		{
			lock (gate)
			{
				foreach (IYamlFormatter formatter2 in formatters)
				{
					if (!(formatter2 is IYamlFormatter<T>))
					{
						continue;
					}
					value = formatter2;
					goto end_IL_0025;
				}
				foreach (IYamlFormatterResolver resolver in resolvers)
				{
					IYamlFormatter<T> formatter = resolver.GetFormatter<T>();
					if (formatter != null)
					{
						value = formatter;
						break;
					}
				}
				end_IL_0025:;
			}
			formattersCache.TryAdd(typeof(T), value);
		}
		return value as IYamlFormatter<T>;
	}

	public void AddFormatter(IYamlFormatter formatter)
	{
		lock (gate)
		{
			formatters.Add(formatter);
		}
	}

	public void AddResolver(IYamlFormatterResolver resolver)
	{
		lock (gate)
		{
			resolvers.Add(resolver);
		}
	}
}
