using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public static class PlayerNumberingExtensions
{
	public static int GetPlayerNumber(this Player player)
	{
		if (player == null)
		{
			return -1;
		}
		if (PhotonNetwork.OfflineMode)
		{
			return 0;
		}
		if (!PhotonNetwork.IsConnectedAndReady)
		{
			return -1;
		}
		if (player.CustomProperties.TryGetValue("pNr", out var value))
		{
			return (byte)value;
		}
		return -1;
	}

	public static void SetPlayerNumber(this Player player, int playerNumber)
	{
		if (player != null && !PhotonNetwork.OfflineMode)
		{
			if (playerNumber < 0)
			{
				Debug.LogWarning("Setting invalid playerNumber: " + playerNumber + " for: " + player.ToStringFull());
			}
			if (!PhotonNetwork.IsConnectedAndReady)
			{
				Debug.LogWarning("SetPlayerNumber was called in state: " + PhotonNetwork.NetworkClientState.ToString() + ". Not IsConnectedAndReady.");
			}
			else if (player.GetPlayerNumber() != playerNumber)
			{
				Debug.Log("PlayerNumbering: Set number " + playerNumber);
				player.SetCustomProperties(new Hashtable { 
				{
					"pNr",
					(byte)playerNumber
				} });
			}
		}
	}
}
