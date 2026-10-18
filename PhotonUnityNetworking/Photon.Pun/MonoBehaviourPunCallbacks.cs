using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun;

public class MonoBehaviourPunCallbacks : MonoBehaviourPun, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IWebRpcCallback, IErrorInfoCallback
{
	public virtual void OnEnable()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	public virtual void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}

	public virtual void OnConnected()
	{
	}

	public virtual void OnLeftRoom()
	{
	}

	public virtual void OnMasterClientSwitched(Player newMasterClient)
	{
	}

	public virtual void OnCreateRoomFailed(short returnCode, string message)
	{
	}

	public virtual void OnJoinRoomFailed(short returnCode, string message)
	{
	}

	public virtual void OnCreatedRoom()
	{
	}

	public virtual void OnJoinedLobby()
	{
	}

	public virtual void OnLeftLobby()
	{
	}

	public virtual void OnDisconnected(DisconnectCause cause)
	{
	}

	public virtual void OnRegionListReceived(RegionHandler regionHandler)
	{
	}

	public virtual void OnRoomListUpdate(List<RoomInfo> roomList)
	{
	}

	public virtual void OnJoinedRoom()
	{
	}

	public virtual void OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	public virtual void OnPlayerLeftRoom(Player otherPlayer)
	{
	}

	public virtual void OnJoinRandomFailed(short returnCode, string message)
	{
	}

	public virtual void OnConnectedToMaster()
	{
	}

	public virtual void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	public virtual void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public virtual void OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
	}

	public virtual void OnCustomAuthenticationFailed(string debugMessage)
	{
	}

	public virtual void OnWebRpcResponse(OperationResponse response)
	{
	}

	public virtual void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
	{
	}

	public virtual void OnErrorInfo(ErrorInfo errorInfo)
	{
	}

	public virtual void OnPreLeavingRoom()
	{
	}
}
