using System;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public static class TeamExtensions
{
	[Obsolete("Use player.GetPhotonTeam")]
	public static PunTeams.Team GetTeam(this Player player)
	{
		if (player.CustomProperties.TryGetValue("team", out var value))
		{
			return (PunTeams.Team)value;
		}
		return PunTeams.Team.none;
	}

	[Obsolete("Use player.JoinTeam")]
	public static void SetTeam(this Player player, PunTeams.Team team)
	{
		if (!PhotonNetwork.IsConnectedAndReady)
		{
			Debug.LogWarning("JoinTeam was called in state: " + PhotonNetwork.NetworkClientState.ToString() + ". Not IsConnectedAndReady.");
		}
		else if (player.GetTeam() != team)
		{
			player.SetCustomProperties(new Hashtable { 
			{
				"team",
				(byte)team
			} });
		}
	}
}
