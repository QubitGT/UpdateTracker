using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public class PhotonLagSimulationGui : MonoBehaviour
{
	public Rect WindowRect = new Rect(0f, 100f, 120f, 100f);

	public int WindowId = 101;

	public bool Visible = true;

	public PhotonPeer Peer { get; set; }

	public void Start()
	{
		Peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
	}

	public void OnGUI()
	{
		if (Visible)
		{
			if (Peer == null)
			{
				WindowRect = GUILayout.Window(WindowId, WindowRect, NetSimHasNoPeerWindow, "Netw. Sim.");
			}
			else
			{
				WindowRect = GUILayout.Window(WindowId, WindowRect, NetSimWindow, "Netw. Sim.");
			}
		}
	}

	private void NetSimHasNoPeerWindow(int windowId)
	{
		GUILayout.Label("No peer to communicate with. ");
	}

	private void NetSimWindow(int windowId)
	{
		GUILayout.Label($"Rtt:{Peer.RoundTripTime,4} +/-{Peer.RoundTripTimeVariance,3}");
		bool isSimulationEnabled = Peer.IsSimulationEnabled;
		bool flag = GUILayout.Toggle(isSimulationEnabled, "Simulate");
		if (flag != isSimulationEnabled)
		{
			Peer.IsSimulationEnabled = flag;
		}
		float value = Peer.NetworkSimulationSettings.IncomingLag;
		GUILayout.Label("Lag " + value);
		value = GUILayout.HorizontalSlider(value, 0f, 500f);
		Peer.NetworkSimulationSettings.IncomingLag = (int)value;
		Peer.NetworkSimulationSettings.OutgoingLag = (int)value;
		float value2 = Peer.NetworkSimulationSettings.IncomingJitter;
		GUILayout.Label("Jit " + value2);
		value2 = GUILayout.HorizontalSlider(value2, 0f, 100f);
		Peer.NetworkSimulationSettings.IncomingJitter = (int)value2;
		Peer.NetworkSimulationSettings.OutgoingJitter = (int)value2;
		float value3 = Peer.NetworkSimulationSettings.IncomingLossPercentage;
		GUILayout.Label("Loss " + value3);
		value3 = GUILayout.HorizontalSlider(value3, 0f, 10f);
		Peer.NetworkSimulationSettings.IncomingLossPercentage = (int)value3;
		Peer.NetworkSimulationSettings.OutgoingLossPercentage = (int)value3;
		if (GUI.changed)
		{
			WindowRect.height = 100f;
		}
		GUI.DragWindow();
	}
}
