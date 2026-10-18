using UnityEngine;

public class MothershipSharedSettings : ScriptableObject
{
	public string TitleId;

	public string EnvironmentId;

	public string DeploymentId;

	public string BaseUrl = "https://aa-mothership.com";

	public string WebSocketUrl = "wss://r1o4m3joxf.execute-api.us-west-2.amazonaws.com/prod-GT-ws-stage/";

	public string ServerApiKey;

	public bool Enabled;

	public bool RequestLoggingEnabled;
}
