#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Fusion.Photon.Realtime;

[DisallowMultipleComponent]
[AddComponentMenu("")]
internal class SupportLogger : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IErrorInfoCallback
{
	public bool LogTrafficStats = true;

	private LoadBalancingClient client;

	private Stopwatch startStopwatch;

	private bool initialOnApplicationPauseSkipped = false;

	private int pingMax;

	private int pingMin;

	public LoadBalancingClient Client
	{
		get
		{
			return client;
		}
		set
		{
			if (client != value)
			{
				if (client != null)
				{
					client.RemoveCallbackTarget(this);
				}
				client = value;
				if (client != null)
				{
					client.AddCallbackTarget(this);
				}
			}
		}
	}

	protected void Start()
	{
		LogBasics();
		if (startStopwatch == null)
		{
			startStopwatch = new Stopwatch();
			startStopwatch.Start();
		}
	}

	protected void OnDestroy()
	{
		Client = null;
	}

	protected void OnApplicationPause(bool pause)
	{
		if (!initialOnApplicationPauseSkipped)
		{
			initialOnApplicationPauseSkipped = true;
		}
		else
		{
			Debug_.Log(string.Format("{0} SupportLogger OnApplicationPause({1}). Client: {2}.", GetFormattedTimestamp(), pause, (client == null) ? "null" : client.State.ToString()));
		}
	}

	protected void OnApplicationQuit()
	{
		CancelInvoke();
	}

	public void StartLogStats()
	{
		InvokeRepeating("LogStats", 10f, 10f);
	}

	public void StopLogStats()
	{
		CancelInvoke("LogStats");
	}

	private void StartTrackValues()
	{
		InvokeRepeating("TrackValues", 0.5f, 0.5f);
	}

	private void StopTrackValues()
	{
		CancelInvoke("TrackValues");
	}

	private string GetFormattedTimestamp()
	{
		if (startStopwatch == null)
		{
			startStopwatch = new Stopwatch();
			startStopwatch.Start();
		}
		TimeSpan elapsed = startStopwatch.Elapsed;
		if (elapsed.Minutes > 0)
		{
			return string.Format("[{0}:{1}.{1}]", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds);
		}
		return $"[{elapsed.Seconds}.{elapsed.Milliseconds}]";
	}

	private void TrackValues()
	{
		if (client != null)
		{
			int roundTripTime = client.LoadBalancingPeer.RoundTripTime;
			if (roundTripTime > pingMax)
			{
				pingMax = roundTripTime;
			}
			if (roundTripTime < pingMin)
			{
				pingMin = roundTripTime;
			}
		}
	}

	public void LogStats()
	{
		if (client != null && client.State != 0 && LogTrafficStats)
		{
			Debug_.Log($"{GetFormattedTimestamp()} SupportLogger {client.LoadBalancingPeer.VitalStatsToString(all: false)} Ping min/max: {pingMin}/{pingMax}");
		}
	}

	private void LogBasics()
	{
		if (client != null)
		{
			List<string> list = new List<string>(10);
			list.Add(Application.unityVersion);
			list.Add(Application.platform.ToString());
			if (RuntimeUnityFlagsSetup.IsENABLE_IL2CPP)
			{
				list.Add("ENABLE_IL2CPP");
			}
			if (RuntimeUnityFlagsSetup.IsENABLE_MONO)
			{
				list.Add("ENABLE_MONO");
			}
			list.Add("DEBUG");
			if (RuntimeUnityFlagsSetup.IsNET_4_6)
			{
				list.Add("NET_4_6");
			}
			if (RuntimeUnityFlagsSetup.IsNET_STANDARD_2_0)
			{
				list.Add("NET_STANDARD_2_0");
			}
			StringBuilder stringBuilder = new StringBuilder();
			string text = ((string.IsNullOrEmpty(client.AppId) || client.AppId.Length < 8) ? client.AppId : (client.AppId.Substring(0, 8) + "***"));
			stringBuilder.AppendFormat("{0} SupportLogger Info: ", GetFormattedTimestamp());
			stringBuilder.AppendFormat("AppID: \"{0}\" AppVersion: \"{1}\" Client: v{2} ({4}) Build: {3} ", text, client.AppVersion, PhotonPeer.Version, string.Join(", ", list.ToArray()), client.LoadBalancingPeer.TargetFramework);
			if (client != null && client.LoadBalancingPeer != null && client.LoadBalancingPeer.SocketImplementation != null)
			{
				stringBuilder.AppendFormat("Socket: {0} ", client.LoadBalancingPeer.SocketImplementation.Name);
			}
			stringBuilder.AppendFormat("UserId: \"{0}\" AuthType: {1} AuthMode: {2} {3} ", client.UserId, (client.AuthValues != null) ? client.AuthValues.AuthType.ToString() : "N/A", client.AuthMode, client.EncryptionMode);
			stringBuilder.AppendFormat("State: {0} ", client.State);
			stringBuilder.AppendFormat("PeerID: {0} ", client.LoadBalancingPeer.PeerID);
			stringBuilder.AppendFormat("NameServer: {0} Current Server: {1} IP: {2} Region: {3} ", client.NameServerHost, client.CurrentServerAddress, client.LoadBalancingPeer.ServerIpAddress, client.CloudRegion);
			stringBuilder.AppendFormat("{0} UTC", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
			Debug_.LogWarning(stringBuilder.ToString());
		}
	}

	public void OnConnected()
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnConnected().");
		pingMax = 0;
		pingMin = client.LoadBalancingPeer.RoundTripTime;
		LogBasics();
		if (LogTrafficStats)
		{
			client.LoadBalancingPeer.TrafficStatsEnabled = false;
			client.LoadBalancingPeer.TrafficStatsEnabled = true;
			StartLogStats();
		}
		StartTrackValues();
	}

	public void OnConnectedToMaster()
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnConnectedToMaster().");
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnFriendListUpdate(friendList).");
	}

	public void OnJoinedLobby()
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnJoinedLobby(" + client.CurrentLobby?.ToString() + ").");
	}

	public void OnLeftLobby()
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnLeftLobby().");
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnCreateRoomFailed(" + returnCode + "," + message + ").");
	}

	public void OnJoinedRoom()
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnJoinedRoom(" + client.CurrentRoom?.ToString() + "). " + client.CurrentLobby?.ToString() + " GameServer:" + client.GameServerAddress);
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnJoinRoomFailed(" + returnCode + "," + message + ").");
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnJoinRandomFailed(" + returnCode + "," + message + ").");
	}

	public void OnCreatedRoom()
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnCreatedRoom(" + client.CurrentRoom?.ToString() + "). " + client.CurrentLobby?.ToString() + " GameServer:" + client.GameServerAddress);
	}

	public void OnLeftRoom()
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnLeftRoom().");
	}

	public void OnDisconnected(DisconnectCause cause)
	{
		StopLogStats();
		StopTrackValues();
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnDisconnected(" + cause.ToString() + ").");
		LogBasics();
		LogStats();
	}

	public void OnRegionListReceived(RegionHandler regionHandler)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnRegionListReceived(regionHandler).");
	}

	public void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnRoomListUpdate(roomList). roomList.Count: " + roomList.Count);
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerEnteredRoom(" + newPlayer?.ToString() + ").");
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerLeftRoom(" + otherPlayer?.ToString() + ").");
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnRoomPropertiesUpdate(propertiesThatChanged).");
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerPropertiesUpdate(targetPlayer,changedProps).");
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnMasterClientSwitched(" + newMasterClient?.ToString() + ").");
	}

	public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnCustomAuthenticationResponse(" + data.ToStringFull() + ").");
	}

	public void OnCustomAuthenticationFailed(string debugMessage)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnCustomAuthenticationFailed(" + debugMessage + ").");
	}

	public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
	{
		Debug_.Log(GetFormattedTimestamp() + " SupportLogger OnLobbyStatisticsUpdate(lobbyStatistics).");
	}

	public void OnErrorInfo(ErrorInfo errorInfo)
	{
		Debug_.LogError(errorInfo.ToString());
	}
}
