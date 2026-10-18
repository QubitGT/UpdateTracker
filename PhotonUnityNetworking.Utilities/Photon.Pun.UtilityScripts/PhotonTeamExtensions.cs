using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public static class PhotonTeamExtensions
{
	public static PhotonTeam GetPhotonTeam(this Player player)
	{
		if (player.CustomProperties.TryGetValue("_pt", out var value) && PhotonTeamsManager.Instance.TryGetTeamByCode((byte)value, out var team))
		{
			return team;
		}
		return null;
	}

	public static bool JoinTeam(this Player player, PhotonTeam team)
	{
		if (team == null)
		{
			Debug.LogWarning("JoinTeam failed: PhotonTeam provided is null");
			return false;
		}
		if (player.GetPhotonTeam() != null)
		{
			Debug.LogWarningFormat("JoinTeam failed: player ({0}) is already joined to a team ({1}), call SwitchTeam instead", player, team);
			return false;
		}
		return player.SetCustomProperties(new Hashtable { { "_pt", team.Code } });
	}

	public static bool JoinTeam(this Player player, byte teamCode)
	{
		if (PhotonTeamsManager.Instance.TryGetTeamByCode(teamCode, out var team))
		{
			return player.JoinTeam(team);
		}
		return false;
	}

	public static bool JoinTeam(this Player player, string teamName)
	{
		if (PhotonTeamsManager.Instance.TryGetTeamByName(teamName, out var team))
		{
			return player.JoinTeam(team);
		}
		return false;
	}

	public static bool SwitchTeam(this Player player, PhotonTeam team)
	{
		if (team == null)
		{
			Debug.LogWarning("SwitchTeam failed: PhotonTeam provided is null");
			return false;
		}
		PhotonTeam photonTeam = player.GetPhotonTeam();
		if (photonTeam == null)
		{
			Debug.LogWarningFormat("SwitchTeam failed: player ({0}) was not joined to any team, call JoinTeam instead", player);
			return false;
		}
		if (photonTeam.Code == team.Code)
		{
			Debug.LogWarningFormat("SwitchTeam failed: player ({0}) is already joined to the same team {1}", player, team);
			return false;
		}
		return player.SetCustomProperties(new Hashtable { { "_pt", team.Code } }, new Hashtable { { "_pt", photonTeam.Code } });
	}

	public static bool SwitchTeam(this Player player, byte teamCode)
	{
		if (PhotonTeamsManager.Instance.TryGetTeamByCode(teamCode, out var team))
		{
			return player.SwitchTeam(team);
		}
		return false;
	}

	public static bool SwitchTeam(this Player player, string teamName)
	{
		if (PhotonTeamsManager.Instance.TryGetTeamByName(teamName, out var team))
		{
			return player.SwitchTeam(team);
		}
		return false;
	}

	public static bool LeaveCurrentTeam(this Player player)
	{
		PhotonTeam photonTeam = player.GetPhotonTeam();
		if (photonTeam == null)
		{
			Debug.LogWarningFormat("LeaveCurrentTeam failed: player ({0}) was not joined to any team", player);
			return false;
		}
		return player.SetCustomProperties(new Hashtable { { "_pt", null } }, new Hashtable { { "_pt", photonTeam.Code } });
	}

	public static bool TryGetTeamMates(this Player player, out Player[] teamMates)
	{
		return PhotonTeamsManager.Instance.TryGetTeamMatesOfPlayer(player, out teamMates);
	}
}
