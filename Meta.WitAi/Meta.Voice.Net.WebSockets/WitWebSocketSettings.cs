using System.Collections.Generic;
using Meta.WitAi;

namespace Meta.Voice.Net.WebSockets;

public class WitWebSocketSettings
{
	public bool VerboseJsonLogging { get; set; }

	public string ServerUrl { get; set; } = "wss://api.wit.ai/composer";


	public int ServerConnectionTimeoutMs { get; set; } = 2000;


	public int ReconnectAttempts { get; set; } = -1;


	public float ReconnectInterval { get; set; } = 1f;


	public bool Debug { get; set; }

	public Dictionary<string, string> AdditionalAuthParameters { get; } = new Dictionary<string, string>();


	public IWitRequestConfiguration Configuration { get; }

	public IWebSocketProvider WebSocketProvider { get; set; }

	public int RequestTimeoutMs => Configuration.RequestTimeoutMs;

	public WitWebSocketSettings(IWitRequestConfiguration configuration)
	{
		Configuration = configuration;
	}
}
