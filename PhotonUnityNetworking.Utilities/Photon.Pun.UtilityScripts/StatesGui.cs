using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public class StatesGui : MonoBehaviour
{
	public Rect GuiOffset = new Rect(250f, 0f, 300f, 300f);

	public bool DontDestroy = true;

	public bool ServerTimestamp;

	public bool DetailedConnection;

	public bool Server;

	public bool AppVersion;

	public bool UserId;

	public bool Room;

	public bool RoomProps;

	public bool EventsIn;

	public bool LocalPlayer;

	public bool PlayerProps;

	public bool Others;

	public bool Buttons;

	public bool ExpectedUsers;

	private Rect GuiRect;

	private static StatesGui Instance;

	private float native_width = 800f;

	private float native_height = 480f;

	private void Awake()
	{
		if (Instance != null)
		{
			Object.DestroyImmediate(base.gameObject);
			return;
		}
		if (DontDestroy)
		{
			Instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		if (EventsIn)
		{
			PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled = true;
		}
	}

	private void OnDisable()
	{
		if (DontDestroy && Instance == this)
		{
			Instance = null;
		}
	}

	private void OnGUI()
	{
		if (PhotonNetwork.NetworkingClient == null || PhotonNetwork.NetworkingClient.LoadBalancingPeer == null || PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsIncoming == null)
		{
			return;
		}
		float x = (float)Screen.width / native_width;
		float y = (float)Screen.height / native_height;
		GUI.matrix = Matrix4x4.TRS(new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(x, y, 1f));
		Rect rect = new Rect(GuiOffset);
		if (rect.x < 0f)
		{
			rect.x = (float)Screen.width - rect.width;
		}
		GuiRect.xMin = rect.x;
		GuiRect.yMin = rect.y;
		GuiRect.xMax = rect.x + rect.width;
		GuiRect.yMax = rect.y + rect.height;
		GUILayout.BeginArea(GuiRect);
		GUILayout.BeginHorizontal();
		if (ServerTimestamp)
		{
			GUILayout.Label(((double)PhotonNetwork.ServerTimestamp / 1000.0).ToString("F3"));
		}
		if (Server)
		{
			GUILayout.Label(PhotonNetwork.ServerAddress + " " + PhotonNetwork.Server);
		}
		if (DetailedConnection)
		{
			GUILayout.Label(PhotonNetwork.NetworkClientState.ToString());
		}
		if (AppVersion)
		{
			GUILayout.Label(PhotonNetwork.NetworkingClient.AppVersion);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (UserId)
		{
			GUILayout.Label("UID: " + ((PhotonNetwork.AuthValues != null) ? PhotonNetwork.AuthValues.UserId : "no UserId"));
			GUILayout.Label("UserId:" + PhotonNetwork.LocalPlayer.UserId);
		}
		GUILayout.EndHorizontal();
		if (Room)
		{
			if (PhotonNetwork.InRoom)
			{
				GUILayout.Label(RoomProps ? PhotonNetwork.CurrentRoom.ToStringFull() : PhotonNetwork.CurrentRoom.ToString());
			}
			else
			{
				GUILayout.Label("not in room");
			}
		}
		if (EventsIn)
		{
			int fragmentCommandCount = PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsIncoming.FragmentCommandCount;
			GUILayout.Label("Events Received: " + PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsGameLevel.EventCount + " Fragments: " + fragmentCommandCount);
		}
		if (LocalPlayer)
		{
			GUILayout.Label(PlayerToString(PhotonNetwork.LocalPlayer));
		}
		if (Others)
		{
			Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
			foreach (Player player in playerListOthers)
			{
				GUILayout.Label(PlayerToString(player));
			}
		}
		if (ExpectedUsers && PhotonNetwork.InRoom)
		{
			GUILayout.Label("Expected: " + ((PhotonNetwork.CurrentRoom.ExpectedUsers != null) ? PhotonNetwork.CurrentRoom.ExpectedUsers.Length : 0) + " " + ((PhotonNetwork.CurrentRoom.ExpectedUsers != null) ? string.Join(",", PhotonNetwork.CurrentRoom.ExpectedUsers) : ""));
		}
		if (Buttons)
		{
			if (!PhotonNetwork.IsConnected && GUILayout.Button("Connect"))
			{
				PhotonNetwork.ConnectUsingSettings();
			}
			GUILayout.BeginHorizontal();
			if (PhotonNetwork.IsConnected && GUILayout.Button("Disconnect"))
			{
				PhotonNetwork.Disconnect();
			}
			if (PhotonNetwork.IsConnected && GUILayout.Button("Close Socket"))
			{
				PhotonNetwork.NetworkingClient.LoadBalancingPeer.StopThread();
			}
			GUILayout.EndHorizontal();
			if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && GUILayout.Button("Leave"))
			{
				PhotonNetwork.LeaveRoom();
			}
			if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerTtl > 0 && GUILayout.Button("Leave(abandon)"))
			{
				PhotonNetwork.LeaveRoom(becomeInactive: false);
			}
			if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom && GUILayout.Button("Join Random"))
			{
				PhotonNetwork.JoinRandomRoom();
			}
			if (PhotonNetwork.IsConnected && !PhotonNetwork.InRoom && GUILayout.Button("Create Room"))
			{
				PhotonNetwork.CreateRoom(null);
			}
		}
		GUILayout.EndArea();
	}

	private string PlayerToString(Player player)
	{
		if (PhotonNetwork.NetworkingClient == null)
		{
			Debug.LogError("nwp is null");
			return "";
		}
		return string.Format("#{0:00} '{1}'{5} {4}{2} {3} {6}", player.ActorNumber + "/userId:<" + player.UserId + ">", player.NickName, player.IsMasterClient ? "(master)" : "", PlayerProps ? player.CustomProperties.ToStringFull() : "", (PhotonNetwork.LocalPlayer.ActorNumber == player.ActorNumber) ? "(you)" : "", player.UserId, player.IsInactive ? " / Is Inactive" : "");
	}
}
