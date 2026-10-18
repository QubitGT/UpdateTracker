using System;

namespace Meta.XR.MultiplayerBlocks.Colocation;

internal interface INetworkMessenger
{
	event Action<ShareAndLocalizeParams> AnchorShareRequestReceived;

	event Action<ShareAndLocalizeParams> AnchorShareRequestCompleted;

	void SendAnchorShareRequest(ulong targetPlayerId, ShareAndLocalizeParams shareAndLocalizeParams);

	void SendAnchorShareCompleted(ulong targetPlayerId, ShareAndLocalizeParams shareAndLocalizeParams);

	void RegisterLocalPlayer(ulong localPlayerId);
}
