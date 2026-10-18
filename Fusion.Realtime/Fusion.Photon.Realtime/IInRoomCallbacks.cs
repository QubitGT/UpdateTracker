using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal interface IInRoomCallbacks
{
	void OnPlayerEnteredRoom(Player newPlayer);

	void OnPlayerLeftRoom(Player otherPlayer);

	void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged);

	void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps);

	void OnMasterClientSwitched(Player newMasterClient);
}
