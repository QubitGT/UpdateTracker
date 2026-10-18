using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.UtilityScripts;

public static class TurnExtensions
{
	public static readonly string TurnPropKey = "Turn";

	public static readonly string TurnStartPropKey = "TStart";

	public static readonly string FinishedTurnPropKey = "FToA";

	public static void SetTurn(this Room room, int turn, bool setStartTime = false)
	{
		if (room != null && room.CustomProperties != null)
		{
			Hashtable hashtable = new Hashtable();
			hashtable[TurnPropKey] = turn;
			if (setStartTime)
			{
				hashtable[TurnStartPropKey] = PhotonNetwork.ServerTimestamp;
			}
			room.SetCustomProperties(hashtable);
		}
	}

	public static int GetTurn(this RoomInfo room)
	{
		if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnPropKey))
		{
			return 0;
		}
		return (int)room.CustomProperties[TurnPropKey];
	}

	public static int GetTurnStart(this RoomInfo room)
	{
		if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnStartPropKey))
		{
			return 0;
		}
		return (int)room.CustomProperties[TurnStartPropKey];
	}

	public static int GetFinishedTurn(this Player player)
	{
		Room currentRoom = PhotonNetwork.CurrentRoom;
		if (currentRoom == null || currentRoom.CustomProperties == null || !currentRoom.CustomProperties.ContainsKey(TurnPropKey))
		{
			return 0;
		}
		string key = FinishedTurnPropKey + player.ActorNumber;
		return (int)currentRoom.CustomProperties[key];
	}

	public static void SetFinishedTurn(this Player player, int turn)
	{
		Room currentRoom = PhotonNetwork.CurrentRoom;
		if (currentRoom != null && currentRoom.CustomProperties != null)
		{
			string key = FinishedTurnPropKey + player.ActorNumber;
			Hashtable hashtable = new Hashtable();
			hashtable[key] = turn;
			currentRoom.SetCustomProperties(hashtable);
		}
	}
}
