using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Realtime;

[DisallowMultipleComponent]
[AddComponentMenu("")]
public class SupportLogger : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, ILobbyCallbacks, IErrorInfoCallback
{
	public bool LogTrafficStats = true;

	private bool loggedStillOfflineMessage;

	private LoadBalancingClient client;

	private Stopwatch startStopwatch;

	private bool initialOnApplicationPauseSkipped;

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
			UnityEngine.Debug.Log(string.Format("{0} SupportLogger OnApplicationPause({1}). Client: {2}.", GetFormattedTimestamp(), pause, (client == null) ? "null" : client.State.ToString()));
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
			UnityEngine.Debug.Log($"{GetFormattedTimestamp()} SupportLogger {client.LoadBalancingPeer.VitalStatsToString(all: false)} Ping min/max: {pingMin}/{pingMax}");
		}
	}

	private void LogBasics()
	{
		if (client != null)
		{
			List<string> list = new List<string>(10);
			list.Add(Application.unityVersion);
			list.Add(Application.platform.ToString());
			list.Add("ENABLE_MONO");
			list.Add("NET_STANDARD_2_0");
			list.Add("UNITY_64");
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
			UnityEngine.Debug.LogWarning(stringBuilder.ToString());
		}
	}

	public void OnConnected()
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnConnected().");
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
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnConnectedToMaster().");
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnFriendListUpdate(friendList).");
	}

	public void OnJoinedLobby()
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnJoinedLobby(" + client.CurrentLobby?.ToString() + ").");
	}

	public void OnLeftLobby()
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLeftLobby().");
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCreateRoomFailed(" + returnCode + "," + message + ").");
	}

	public void OnJoinedRoom()
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnJoinedRoom(" + client.CurrentRoom?.ToString() + "). " + client.CurrentLobby?.ToString() + " GameServer:" + client.GameServerAddress);
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnJoinRoomFailed(" + returnCode + "," + message + ").");
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnJoinRandomFailed(" + returnCode + "," + message + ").");
	}

	public void OnCreatedRoom()
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCreatedRoom(" + client.CurrentRoom?.ToString() + "). " + client.CurrentLobby?.ToString() + " GameServer:" + client.GameServerAddress);
	}

	public void OnLeftRoom()
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLeftRoom().");
	}

	public void OnPreLeavingRoom()
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnPreLeavingRoom()");
	}

	public void OnDisconnected(DisconnectCause cause)
	{
		StopLogStats();
		StopTrackValues();
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnDisconnected(" + cause.ToString() + ").");
		LogBasics();
		LogStats();
	}

	public void OnRegionListReceived(RegionHandler regionHandler)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRegionListReceived(regionHandler).");
	}

	public void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRoomListUpdate(roomList). roomList.Count: " + roomList.Count);
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerEnteredRoom(" + newPlayer?.ToString() + ").");
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerLeftRoom(" + otherPlayer?.ToString() + ").");
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnRoomPropertiesUpdate(propertiesThatChanged).");
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnPlayerPropertiesUpdate(targetPlayer,changedProps).");
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnMasterClientSwitched(" + newMasterClient?.ToString() + ").");
	}

	public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCustomAuthenticationResponse(" + data.ToStringFull() + ").");
	}

	public void OnCustomAuthenticationFailed(string debugMessage)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnCustomAuthenticationFailed(" + debugMessage + ").");
	}

	public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
	{
		UnityEngine.Debug.Log(GetFormattedTimestamp() + " SupportLogger OnLobbyStatisticsUpdate(lobbyStatistics).");
	}

	public void OnErrorInfo(ErrorInfo errorInfo)
	{
		UnityEngine.Debug.LogError(errorInfo.ToString());
	}
}
