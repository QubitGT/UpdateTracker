using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public class PlayerNumbering : MonoBehaviourPunCallbacks
{
	public delegate void PlayerNumberingChanged();

	public static PlayerNumbering instance;

	public static Player[] SortedPlayers;

	public const string RoomPlayerIndexedProp = "pNr";

	public bool dontDestroyOnLoad;

	public static event PlayerNumberingChanged OnPlayerNumberingChanged;

	public void Awake()
	{
		if (instance != null && instance != this && instance.gameObject != null)
		{
			Object.DestroyImmediate(instance.gameObject);
		}
		instance = this;
		if (dontDestroyOnLoad)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
		RefreshData();
	}

	public override void OnJoinedRoom()
	{
		RefreshData();
	}

	public override void OnLeftRoom()
	{
		PhotonNetwork.LocalPlayer.CustomProperties.Remove("pNr");
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		RefreshData();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		RefreshData();
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if (changedProps != null && changedProps.ContainsKey("pNr"))
		{
			RefreshData();
		}
	}

	public void RefreshData()
	{
		if (PhotonNetwork.CurrentRoom == null)
		{
			return;
		}
		if (PhotonNetwork.LocalPlayer.GetPlayerNumber() >= 0)
		{
			SortedPlayers = PhotonNetwork.CurrentRoom.Players.Values.OrderBy((Player p) => p.GetPlayerNumber()).ToArray();
			if (PlayerNumbering.OnPlayerNumberingChanged != null)
			{
				PlayerNumbering.OnPlayerNumberingChanged();
			}
			return;
		}
		HashSet<int> hashSet = new HashSet<int>();
		Player[] array = PhotonNetwork.PlayerList.OrderBy((Player p) => p.ActorNumber).ToArray();
		string text = "all players: ";
		Player[] array2 = array;
		foreach (Player player in array2)
		{
			text = text + player.ActorNumber + "=pNr:" + player.GetPlayerNumber() + ", ";
			int playerNumber = player.GetPlayerNumber();
			if (player.IsLocal)
			{
				Debug.Log("PhotonNetwork.CurrentRoom.PlayerCount = " + PhotonNetwork.CurrentRoom.PlayerCount);
				for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)
				{
					if (!hashSet.Contains(j))
					{
						player.SetPlayerNumber(j);
						break;
					}
				}
				break;
			}
			if (playerNumber < 0)
			{
				break;
			}
			hashSet.Add(playerNumber);
		}
		SortedPlayers = PhotonNetwork.CurrentRoom.Players.Values.OrderBy((Player p) => p.GetPlayerNumber()).ToArray();
		if (PlayerNumbering.OnPlayerNumberingChanged != null)
		{
			PlayerNumbering.OnPlayerNumberingChanged();
		}
	}
}
