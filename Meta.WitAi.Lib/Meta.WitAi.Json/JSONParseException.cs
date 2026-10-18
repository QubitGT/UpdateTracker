using System;

namespace Meta.WitAi.Json;

public class JSONParseException : Exception
{
	public JSONParseException(string message)
		: base(message)
	{
	}
}
