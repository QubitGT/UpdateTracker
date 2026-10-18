using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.UtilityScripts;

[Obsolete("do not use this or add it to the scene. use PhotonTeamsManager instead")]
public class PunTeams : MonoBehaviourPunCallbacks
{
	[Obsolete("use custom PhotonTeam instead")]
	public enum Team : byte
	{
		none,
		red,
		blue
	}

	[Obsolete("use PhotonTeamsManager.Instance.TryGetTeamMembers instead")]
	public static Dictionary<Team, List<Player>> PlayersPerTeam;

	[Obsolete("do not use this. PhotonTeamsManager.TeamPlayerProp is used internally instead.")]
	public const string TeamPlayerProp = "team";

	public void Start()
	{
		PlayersPerTeam = new Dictionary<Team, List<Player>>();
		foreach (object value in Enum.GetValues(typeof(Team)))
		{
			PlayersPerTeam[(Team)value] = new List<Player>();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Start();
	}

	public override void OnJoinedRoom()
	{
		UpdateTeams();
	}

	public override void OnLeftRoom()
	{
		Start();
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		UpdateTeams();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		UpdateTeams();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		UpdateTeams();
	}

	[Obsolete("do not call this.")]
	public void UpdateTeams()
	{
		foreach (object value in Enum.GetValues(typeof(Team)))
		{
			PlayersPerTeam[(Team)value].Clear();
		}
		for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
		{
			Player player = PhotonNetwork.PlayerList[i];
			Team team = player.GetTeam();
			PlayersPerTeam[team].Add(player);
		}
	}
}
