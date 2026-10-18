using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public class PhotonStatsGui : MonoBehaviour
{
	public bool statsWindowOn = true;

	public bool statsOn = true;

	public bool healthStatsVisible;

	public bool trafficStatsOn;

	public bool buttonsOn;

	public Rect statsRect = new Rect(0f, 100f, 200f, 50f);

	public int WindowId = 100;

	public bool turnOn;

	public void Start()
	{
		if (statsRect.x <= 0f)
		{
			statsRect.x = (float)Screen.width - statsRect.width;
		}
	}

	public void Update()
	{
		if (turnOn)
		{
			turnOn = false;
			statsWindowOn = !statsWindowOn;
			statsOn = true;
		}
	}

	public void OnGUI()
	{
		if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled != statsOn)
		{
			PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled = statsOn;
		}
		if (statsWindowOn)
		{
			statsRect = GUILayout.Window(WindowId, statsRect, TrafficStatsWindow, "Messages (shift+tab)");
		}
	}

	public void TrafficStatsWindow(int windowID)
	{
		bool flag = false;
		TrafficStatsGameLevel trafficStatsGameLevel = PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsGameLevel;
		long num = PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsElapsedMs / 1000;
		if (num == 0L)
		{
			num = 1L;
		}
		GUILayout.BeginHorizontal();
		buttonsOn = GUILayout.Toggle(buttonsOn, "buttons");
		healthStatsVisible = GUILayout.Toggle(healthStatsVisible, "health");
		trafficStatsOn = GUILayout.Toggle(trafficStatsOn, "traffic");
		GUILayout.EndHorizontal();
		string text = $"Out {trafficStatsGameLevel.TotalOutgoingByteCount,4} | In {trafficStatsGameLevel.TotalIncomingByteCount,4} | Sum {trafficStatsGameLevel.TotalByteCount,4}";
		string text2 = $"{num}sec average:";
		string text3 = $"Out {trafficStatsGameLevel.TotalOutgoingByteCount / num,4} | In {trafficStatsGameLevel.TotalIncomingByteCount / num,4} | Sum {trafficStatsGameLevel.TotalByteCount / num,4}";
		GUILayout.Label(text);
		GUILayout.Label(text2);
		GUILayout.Label(text3);
		if (buttonsOn)
		{
			GUILayout.BeginHorizontal();
			statsOn = GUILayout.Toggle(statsOn, "stats on");
			if (GUILayout.Button("Reset"))
			{
				PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsReset();
				PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled = true;
			}
			flag = GUILayout.Button("To Log");
			GUILayout.EndHorizontal();
		}
		string text4 = string.Empty;
		string text5 = string.Empty;
		if (trafficStatsOn)
		{
			GUILayout.Box("Traffic Stats");
			text4 = "Incoming: \n" + PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsIncoming.ToString();
			text5 = "Outgoing: \n" + PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsOutgoing.ToString();
			GUILayout.Label(text4);
			GUILayout.Label(text5);
		}
		string text6 = string.Empty;
		if (healthStatsVisible)
		{
			GUILayout.Box("Health Stats");
			text6 = string.Format("ping: {6}[+/-{7}]ms resent:{8} \n\nmax ms between\nsend: {0,4} \ndispatch: {1,4} \n\nlongest dispatch for: \nev({3}):{2,3}ms \nop({5}):{4,3}ms", trafficStatsGameLevel.LongestDeltaBetweenSending, trafficStatsGameLevel.LongestDeltaBetweenDispatching, trafficStatsGameLevel.LongestEventCallback, trafficStatsGameLevel.LongestEventCallbackCode, trafficStatsGameLevel.LongestOpResponseCallback, trafficStatsGameLevel.LongestOpResponseCallbackOpCode, PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime, PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTimeVariance, PhotonNetwork.NetworkingClient.LoadBalancingPeer.ResentReliableCommands);
			GUILayout.Label(text6);
		}
		if (flag)
		{
			Debug.Log($"{text}\n{text2}\n{text3}\n{text4}\n{text5}\n{text6}");
		}
		if (GUI.changed)
		{
			statsRect.height = 100f;
		}
		GUI.DragWindow();
	}
}
