using System;
using VYaml.Parser;

namespace VYaml.Serialization;

public class YamlSerializerException : Exception
{
	public static void ThrowInvalidType<T>(T value)
	{
		throw new YamlSerializerException($"Cannot detect a value of enum: {typeof(T)}, {value}");
	}

	public static void ThrowInvalidType<T>()
	{
		throw new YamlSerializerException($"Cannot detect a scalar value of {typeof(T)}");
	}

	public YamlSerializerException(string message)
		: base(message)
	{
	}

	public YamlSerializerException(Marker mark, string message)
		: base($"{message} at {mark}")
	{
	}
}
