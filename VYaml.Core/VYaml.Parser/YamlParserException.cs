using System;

namespace VYaml.Parser;

public class YamlParserException : Exception
{
	public static void Throw(in Marker marker, string message)
	{
		throw new YamlParserException(in marker, message);
	}

	public YamlParserException(in Marker marker, string message)
		: base($"{message} at {marker}")
	{
	}
}
