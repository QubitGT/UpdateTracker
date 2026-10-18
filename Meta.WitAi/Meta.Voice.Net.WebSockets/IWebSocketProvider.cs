using System.Collections.Generic;

namespace Meta.Voice.Net.WebSockets;

public interface IWebSocketProvider
{
	IWebSocket GetWebSocket(string url, Dictionary<string, string> headers);
}
