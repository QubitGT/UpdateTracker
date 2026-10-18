namespace Meta.Net.NativeWebSocket;

public static class WebSocketFactory
{
	public static WebSocket CreateInstance(string url)
	{
		return new WebSocket(url);
	}
}
