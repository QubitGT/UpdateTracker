using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal interface IOnEventCallback
{
	void OnEvent(EventData photonEvent);
}
