using System;

namespace VYaml.Emitter;

public class YamlEmitterException : Exception
{
	public YamlEmitterException(string message)
		: base(message)
	{
	}
}
