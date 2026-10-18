using System;

namespace SouthPointe.Serialization.MessagePack;

public class FormatException : System.FormatException
{
	public FormatException()
	{
	}

	public FormatException(string message)
		: base(message)
	{
	}

	public FormatException(ITypeHandler handler, Format format, FormatReader reader)
		: base($"{handler.GetType()}: Undefined Format {format} at position {reader.Position}")
	{
	}

	public FormatException(Format format, FormatReader reader)
		: base($"Undefined Format {format} at Position: {reader.Position}")
	{
	}
}
