using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal interface IWebRpcCallback
{
	void OnWebRpcResponse(OperationResponse response);
}
