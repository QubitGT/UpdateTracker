using System;

namespace NativeWebSocket;

public static class WebSocketHelpers
{
	public static WebSocketCloseCode ParseCloseCodeEnum(int closeCode)
	{
		if (Enum.IsDefined(typeof(WebSocketCloseCode), closeCode))
		{
			return (WebSocketCloseCode)closeCode;
		}
		return WebSocketCloseCode.Undefined;
	}

	public static WebSocketException GetErrorMessageFromCode(int errorCode, Exception inner)
	{
		return errorCode switch
		{
			-1 => new WebSocketUnexpectedException("WebSocket instance not found.", inner), 
			-2 => new WebSocketInvalidStateException("WebSocket is already connected or in connecting state.", inner), 
			-3 => new WebSocketInvalidStateException("WebSocket is not connected.", inner), 
			-4 => new WebSocketInvalidStateException("WebSocket is already closing.", inner), 
			-5 => new WebSocketInvalidStateException("WebSocket is already closed.", inner), 
			-6 => new WebSocketInvalidStateException("WebSocket is not in open state.", inner), 
			-7 => new WebSocketInvalidArgumentException("Cannot close WebSocket. An invalid code was specified or reason is too long.", inner), 
			_ => new WebSocketUnexpectedException("Unknown error.", inner), 
		};
	}
}
