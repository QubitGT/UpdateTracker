using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

[DisallowMultipleComponent]
public class PhotonTeamsManager : MonoBehaviour, IMatchmakingCallbacks, IInRoomCallbacks
{
	[SerializeField]
	private List<PhotonTeam> teamsList = new List<PhotonTeam>
	{
		new PhotonTeam
		{
			Name = "Blue",
			Code = 1
		},
		new PhotonTeam
		{
			Name = "Red",
			Code = 2
		}
	};

	private Dictionary<byte, PhotonTeam> teamsByCode;

	private Dictionary<string, PhotonTeam> teamsByName;

	private Dictionary<byte, HashSet<Player>> playersPerTeam;

	public const string TeamPlayerProp = "_pt";

	private static PhotonTeamsManager instance;

	public static PhotonTeamsManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindFirstObjectByType<PhotonTeamsManager>();
				if (instance == null)
				{
					instance = new GameObject
					{
						name = "PhotonTeamsManager"
					}.AddComponent<PhotonTeamsManager>();
				}
				instance.Init();
			}
			return instance;
		}
	}

	public static event Action<Player, PhotonTeam> PlayerJoinedTeam;

	public static event Action<Player, PhotonTeam> PlayerLeftTeam;

	private void Awake()
	{
		if (instance == null || (object)this == instance)
		{
			Init();
			instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void OnEnable()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	private void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
		ClearTeams();
	}

	private void Init()
	{
		teamsByCode = new Dictionary<byte, PhotonTeam>(teamsList.Count);
		teamsByName = new Dictionary<string, PhotonTeam>(teamsList.Count);
		playersPerTeam = new Dictionary<byte, HashSet<Player>>(teamsList.Count);
		for (int i = 0; i < teamsList.Count; i++)
		{
			teamsByCode[teamsList[i].Code] = teamsList[i];
			teamsByName[teamsList[i].Name] = teamsList[i];
			playersPerTeam[teamsList[i].Code] = new HashSet<Player>();
		}
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
		UpdateTeams();
	}

	void IMatchmakingCallbacks.OnLeftRoom()
	{
		ClearTeams();
	}

	void IMatchmakingCallbacks.OnPreLeavingRoom()
	{
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if (!changedProps.TryGetValue("_pt", out var value))
		{
			return;
		}
		if (value == null)
		{
			foreach (byte key in playersPerTeam.Keys)
			{
				if (playersPerTeam[key].Remove(targetPlayer))
				{
					if (PhotonTeamsManager.PlayerLeftTeam != null)
					{
						PhotonTeamsManager.PlayerLeftTeam(targetPlayer, teamsByCode[key]);
					}
					break;
				}
			}
			return;
		}
		if (value is byte b)
		{
			foreach (byte key2 in playersPerTeam.Keys)
			{
				if (key2 != b && playersPerTeam[key2].Remove(targetPlayer))
				{
					if (PhotonTeamsManager.PlayerLeftTeam != null)
					{
						PhotonTeamsManager.PlayerLeftTeam(targetPlayer, teamsByCode[key2]);
					}
					break;
				}
			}
			PhotonTeam photonTeam = teamsByCode[b];
			if (!playersPerTeam[b].Add(targetPlayer))
			{
				Debug.LogWarningFormat("Unexpected situation while setting team {0} for player {1}, updating teams for all", photonTeam, targetPlayer);
				UpdateTeams();
			}
			if (PhotonTeamsManager.PlayerJoinedTeam != null)
			{
				PhotonTeamsManager.PlayerJoinedTeam(targetPlayer, photonTeam);
			}
		}
		else
		{
			Debug.LogErrorFormat("Unexpected: custom property key {0} should have of type byte, instead we got {1} of type {2}. Player: {3}", "_pt", value, value.GetType(), targetPlayer);
		}
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
	{
		if (!otherPlayer.IsInactive)
		{
			PhotonTeam photonTeam = otherPlayer.GetPhotonTeam();
			if (photonTeam != null && !playersPerTeam[photonTeam.Code].Remove(otherPlayer))
			{
				Debug.LogWarningFormat("Unexpected situation while removing player {0} who left from team {1}, updating teams for all", otherPlayer, photonTeam);
				UpdateTeams();
			}
		}
	}

	void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
	{
		PhotonTeam photonTeam = newPlayer.GetPhotonTeam();
		if (photonTeam == null || playersPerTeam[photonTeam.Code].Contains(newPlayer))
		{
			return;
		}
		foreach (byte key in teamsByCode.Keys)
		{
			if (playersPerTeam[key].Remove(newPlayer))
			{
				break;
			}
		}
		if (!playersPerTeam[photonTeam.Code].Add(newPlayer))
		{
			Debug.LogWarningFormat("Unexpected situation while adding player {0} who joined to team {1}, updating teams for all", newPlayer, photonTeam);
			UpdateTeams();
		}
	}

	private void UpdateTeams()
	{
		ClearTeams();
		for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
		{
			Player player = PhotonNetwork.PlayerList[i];
			PhotonTeam photonTeam = player.GetPhotonTeam();
			if (photonTeam != null)
			{
				playersPerTeam[photonTeam.Code].Add(player);
			}
		}
	}

	private void ClearTeams()
	{
		foreach (byte key in playersPerTeam.Keys)
		{
			playersPerTeam[key].Clear();
		}
	}

	public bool TryGetTeamByCode(byte code, out PhotonTeam team)
	{
		return teamsByCode.TryGetValue(code, out team);
	}

	public bool TryGetTeamByName(string teamName, out PhotonTeam team)
	{
		return teamsByName.TryGetValue(teamName, out team);
	}

	public PhotonTeam[] GetAvailableTeams()
	{
		if (teamsList != null)
		{
			return teamsList.ToArray();
		}
		return null;
	}

	public bool TryGetTeamMembers(byte code, out Player[] members)
	{
		members = null;
		if (playersPerTeam.TryGetValue(code, out var value))
		{
			members = new Player[value.Count];
			int num = 0;
			foreach (Player item in value)
			{
				members[num] = item;
				num++;
			}
			return true;
		}
		return false;
	}

	public bool TryGetTeamMembers(string teamName, out Player[] members)
	{
		members = null;
		if (TryGetTeamByName(teamName, out var team))
		{
			return TryGetTeamMembers(team.Code, out members);
		}
		return false;
	}

	public bool TryGetTeamMembers(PhotonTeam team, out Player[] members)
	{
		members = null;
		if (team != null)
		{
			return TryGetTeamMembers(team.Code, out members);
		}
		return false;
	}

	public bool TryGetTeamMatesOfPlayer(Player player, out Player[] teamMates)
	{
		teamMates = null;
		if (player == null)
		{
			return false;
		}
		PhotonTeam photonTeam = player.GetPhotonTeam();
		if (photonTeam == null)
		{
			return false;
		}
		if (playersPerTeam.TryGetValue(photonTeam.Code, out var value))
		{
			if (!value.Contains(player))
			{
				Debug.LogWarningFormat("Unexpected situation while getting team mates of player {0} who is joined to team {1}, updating teams for all", player, photonTeam);
				UpdateTeams();
			}
			teamMates = new Player[value.Count - 1];
			int num = 0;
			foreach (Player item in value)
			{
				if (!item.Equals(player))
				{
					teamMates[num] = item;
					num++;
				}
			}
			return true;
		}
		return false;
	}

	public int GetTeamMembersCount(byte code)
	{
		if (TryGetTeamByCode(code, out var team))
		{
			return GetTeamMembersCount(team);
		}
		return 0;
	}

	public int GetTeamMembersCount(string name)
	{
		if (TryGetTeamByName(name, out var team))
		{
			return GetTeamMembersCount(team);
		}
		return 0;
	}

	public int GetTeamMembersCount(PhotonTeam team)
	{
		if (team != null && playersPerTeam.TryGetValue(team.Code, out var value) && value != null)
		{
			return value.Count;
		}
		return 0;
	}

	void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	void IMatchmakingCallbacks.OnCreatedRoom()
	{
	}

	void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
	{
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
	}
}
