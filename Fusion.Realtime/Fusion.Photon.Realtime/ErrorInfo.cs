using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class ErrorInfo
{
	public readonly string Info;

	public ErrorInfo(EventData eventData)
	{
		Info = eventData[218] as string;
	}

	public override string ToString()
	{
		return $"ErrorInfo: {Info}";
	}
}
