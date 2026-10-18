using System.Collections.Generic;

namespace ExitGames.Client.Photon;

public class DisconnectMessage
{
	public short Code;

	public string DebugMessage;

	public Dictionary<byte, object> Parameters;
}
