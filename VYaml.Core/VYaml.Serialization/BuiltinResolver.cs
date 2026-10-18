using System;
using System.Collections.Generic;

namespace VYaml.Serialization;

public class BuiltinResolver : IYamlFormatterResolver
{
	private static class FormatterCache<T>
	{
		public static readonly IYamlFormatter<T>? Formatter;

		static FormatterCache()
		{
			if (FormatterMap.TryGetValue(typeof(T), out object value))
			{
				Formatter = (IYamlFormatter<T>)value;
			}
			else if (TryCreateGenericFormatter(typeof(T)) is IYamlFormatter<T> formatter)
			{
				Formatter = formatter;
			}
			else
			{
				Formatter = null;
			}
		}
	}

	public static readonly BuiltinResolver Instance = new BuiltinResolver();

	private static readonly Dictionary<Type, object> FormatterMap = new Dictionary<Type, object>
	{
		{
			typeof(short),
			Int16Formatter.Instance
		},
		{
			typeof(int),
			Int32Formatter.Instance
		},
		{
			typeof(long),
			Int64Formatter.Instance
		},
		{
			typeof(ushort),
			UInt16Formatter.Instance
		},
		{
			typeof(uint),
			UInt32Formatter.Instance
		},
		{
			typeof(ulong),
			UInt64Formatter.Instance
		},
		{
			typeof(float),
			Float32Formatter.Instance
		},
		{
			typeof(double),
			Float64Formatter.Instance
		},
		{
			typeof(bool),
			BooleanFormatter.Instance
		},
		{
			typeof(byte),
			ByteFormatter.Instance
		},
		{
			typeof(sbyte),
			SByteFormatter.Instance
		},
		{
			typeof(DateTime),
			DateTimeFormatter.Instance
		},
		{
			typeof(char),
			CharFormatter.Instance
		},
		{
			typeof(byte[]),
			ByteArrayFormatter.Instance
		},
		{
			typeof(short?),
			NullableInt16Formatter.Instance
		},
		{
			typeof(int?),
			NullableInt32Formatter.Instance
		},
		{
			typeof(long?),
			NullableInt64Formatter.Instance
		},
		{
			typeof(ushort?),
			NullableUInt16Formatter.Instance
		},
		{
			typeof(uint?),
			NullableUInt32Formatter.Instance
		},
		{
			typeof(ulong?),
			NullableUInt64Formatter.Instance
		},
		{
			typeof(float?),
			NullableFloat32Formatter.Instance
		},
		{
			typeof(double?),
			NullableFloat64Formatter.Instance
		},
		{
			typeof(bool?),
			NullableBooleanFormatter.Instance
		},
		{
			typeof(byte?),
			NullableByteFormatter.Instance
		},
		{
			typeof(sbyte?),
			NullableSByteFormatter.Instance
		},
		{
			typeof(DateTime?),
			NullableDateTimeFormatter.Instance
		},
		{
			typeof(char?),
			NullableCharFormatter.Instance
		},
		{
			typeof(string),
			NullableStringFormatter.Instance
		},
		{
			typeof(decimal),
			DecimalFormatter.Instance
		},
		{
			typeof(decimal?),
			new StaticNullableFormatter<decimal>(DecimalFormatter.Instance)
		},
		{
			typeof(TimeSpan),
			TimeSpanFormatter.Instance
		},
		{
			typeof(TimeSpan?),
			new StaticNullableFormatter<TimeSpan>(TimeSpanFormatter.Instance)
		},
		{
			typeof(DateTimeOffset),
			DateTimeOffsetFormatter.Instance
		},
		{
			typeof(DateTimeOffset?),
			new StaticNullableFormatter<DateTimeOffset>(DateTimeOffsetFormatter.Instance)
		},
		{
			typeof(Guid),
			GuidFormatter.Instance
		},
		{
			typeof(Guid?),
			new StaticNullableFormatter<Guid>(GuidFormatter.Instance)
		},
		{
			typeof(Uri),
			UriFormatter.Instance
		},
		{
			typeof(List<short>),
			new ListFormatter<short>()
		},
		{
			typeof(List<int>),
			new ListFormatter<int>()
		},
		{
			typeof(List<long>),
			new ListFormatter<long>()
		},
		{
			typeof(List<ushort>),
			new ListFormatter<ushort>()
		},
		{
			typeof(List<uint>),
			new ListFormatter<uint>()
		},
		{
			typeof(List<ulong>),
			new ListFormatter<ulong>()
		},
		{
			typeof(List<float>),
			new ListFormatter<float>()
		},
		{
			typeof(List<double>),
			new ListFormatter<double>()
		},
		{
			typeof(List<bool>),
			new ListFormatter<bool>()
		},
		{
			typeof(List<byte>),
			new ListFormatter<byte>()
		},
		{
			typeof(List<sbyte>),
			new ListFormatter<sbyte>()
		},
		{
			typeof(List<DateTime>),
			new ListFormatter<DateTime>()
		},
		{
			typeof(List<char>),
			new ListFormatter<char>()
		},
		{
			typeof(List<string>),
			new ListFormatter<string>()
		},
		{
			typeof(object[]),
			new ArrayFormatter<object>()
		},
		{
			typeof(List<object>),
			new ListFormatter<object>()
		}
	};

	public static readonly Dictionary<Type, Type> KnownGenericTypes = new Dictionary<Type, Type>
	{
		{
			typeof(Tuple<>),
			typeof(TupleFormatter<>)
		},
		{
			typeof(ValueTuple<>),
			typeof(ValueTupleFormatter<>)
		},
		{
			typeof(Tuple<, >),
			typeof(TupleFormatter<, >)
		},
		{
			typeof(ValueTuple<, >),
			typeof(ValueTupleFormatter<, >)
		},
		{
			typeof(Tuple<, , >),
			typeof(TupleFormatter<, , >)
		},
		{
			typeof(ValueTuple<, , >),
			typeof(ValueTupleFormatter<, , >)
		},
		{
			typeof(Tuple<, , , >),
			typeof(TupleFormatter<, , , >)
		},
		{
			typeof(ValueTuple<, , , >),
			typeof(ValueTupleFormatter<, , , >)
		},
		{
			typeof(Tuple<, , , , >),
			typeof(TupleFormatter<, , , , >)
		},
		{
			typeof(ValueTuple<, , , , >),
			typeof(ValueTupleFormatter<, , , , >)
		},
		{
			typeof(Tuple<, , , , , >),
			typeof(TupleFormatter<, , , , , >)
		},
		{
			typeof(ValueTuple<, , , , , >),
			typeof(ValueTupleFormatter<, , , , , >)
		},
		{
			typeof(Tuple<, , , , , , >),
			typeof(TupleFormatter<, , , , , , >)
		},
		{
			typeof(ValueTuple<, , , , , , >),
			typeof(ValueTupleFormatter<, , , , , , >)
		},
		{
			typeof(Tuple<, , , , , , , >),
			typeof(TupleFormatter<, , , , , , , >)
		},
		{
			typeof(ValueTuple<, , , , , , , >),
			typeof(ValueTupleFormatter<, , , , , , , >)
		},
		{
			typeof(KeyValuePair<, >),
			typeof(KeyValuePairFormatter<, >)
		},
		{
			typeof(Nullable<>),
			typeof(NullableFormatter<>)
		},
		{
			typeof(List<>),
			typeof(ListFormatter<>)
		},
		{
			typeof(Dictionary<, >),
			typeof(DictionaryFormatter<, >)
		},
		{
			typeof(IEnumerable<>),
			typeof(InterfaceEnumerableFormatter<>)
		},
		{
			typeof(ICollection<>),
			typeof(InterfaceCollectionFormatter<>)
		},
		{
			typeof(IReadOnlyCollection<>),
			typeof(InterfaceReadOnlyCollectionFormatter<>)
		},
		{
			typeof(IList<>),
			typeof(InterfaceListFormatter<>)
		},
		{
			typeof(IReadOnlyList<>),
			typeof(InterfaceReadOnlyListFormatter<>)
		},
		{
			typeof(IDictionary<, >),
			typeof(InterfaceDictionaryFormatter<, >)
		},
		{
			typeof(IReadOnlyDictionary<, >),
			typeof(InterfaceReadOnlyDictionaryFormatter<, >)
		}
	};

	public IYamlFormatter<T>? GetFormatter<T>()
	{
		return FormatterCache<T>.Formatter;
	}

	private static object? TryCreateGenericFormatter(Type type)
	{
		Type type2 = null;
		if (!type.IsArray)
		{
			type2 = ((!type.IsEnum) ? TryCreateGenericFormatterType(type, KnownGenericTypes) : typeof(EnumAsStringFormatter<>).MakeGenericType(type));
		}
		else if (type.IsSZArray)
		{
			type2 = typeof(ArrayFormatter<>).MakeGenericType(type.GetElementType());
		}
		if (type2 != null)
		{
			return Activator.CreateInstance(type2);
		}
		return null;
	}

	private static Type? TryCreateGenericFormatterType(Type type, IDictionary<Type, Type> knownTypes)
	{
		if (type.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (knownTypes.TryGetValue(genericTypeDefinition, out Type value))
			{
				return value.MakeGenericType(type.GetGenericArguments());
			}
		}
		return null;
	}
}
