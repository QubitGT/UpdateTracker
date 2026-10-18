using System;

namespace VYaml.Parser;

internal class YamlTokenizerException : Exception
{
	public YamlTokenizerException(in Marker marker, string message)
		: base($"{message} at {marker}")
	{
	}
}
