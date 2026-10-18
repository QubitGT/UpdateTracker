using System;

namespace Meta.Net.NativeWebSocket;

public class WebSocketException : Exception
{
	public WebSocketException()
	{
	}

	public WebSocketException(string message)
		: base(message)
	{
	}

	public WebSocketException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
