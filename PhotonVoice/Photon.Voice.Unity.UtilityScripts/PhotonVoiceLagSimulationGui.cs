using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts;

[RequireComponent(typeof(VoiceConnection))]
public class PhotonVoiceLagSimulationGui : MonoBehaviour
{
	private VoiceConnection voiceConnection;

	private Rect windowRect = new Rect(0f, 100f, 200f, 100f);

	private int windowId = 201;

	private bool visible = true;

	private PhotonPeer peer;

	private float debugLostPercent;

	public void OnEnable()
	{
		VoiceConnection[] components = GetComponents<VoiceConnection>();
		if (components == null || components.Length == 0)
		{
			Debug.LogError("No VoiceConnection component found, PhotonVoiceStatsGui disabled", this);
			base.enabled = false;
			return;
		}
		if (components.Length > 1)
		{
			Debug.LogWarningFormat(this, "Multiple VoiceConnection components found, using first occurrence attached to GameObject {0}", components[0].name);
		}
		voiceConnection = components[0];
		peer = voiceConnection.Client.LoadBalancingPeer;
		debugLostPercent = voiceConnection.VoiceClient.DebugLostPercent;
	}

	private void OnGUI()
	{
		if (visible)
		{
			if (peer == null)
			{
				windowRect = GUILayout.Window(windowId, windowRect, NetSimHasNoPeerWindow, "Voice Network Simulation");
			}
			else
			{
				windowRect = GUILayout.Window(windowId, windowRect, NetSimWindow, "Voice Network Simulation");
			}
		}
	}

	private void NetSimHasNoPeerWindow(int windowId)
	{
		GUILayout.Label("No voice peer to communicate with. ");
	}

	private void NetSimWindow(int windowId)
	{
		GUILayout.Label($"Rtt:{peer.RoundTripTime,4} +/-{peer.RoundTripTimeVariance,3}");
		bool isSimulationEnabled = peer.IsSimulationEnabled;
		bool flag = GUILayout.Toggle(isSimulationEnabled, "Simulate");
		if (flag != isSimulationEnabled)
		{
			peer.IsSimulationEnabled = flag;
		}
		float num = peer.NetworkSimulationSettings.IncomingLag;
		GUILayout.Label($"Lag {num}");
		num = GUILayout.HorizontalSlider(num, 0f, 500f);
		peer.NetworkSimulationSettings.IncomingLag = (int)num;
		peer.NetworkSimulationSettings.OutgoingLag = (int)num;
		float num2 = peer.NetworkSimulationSettings.IncomingJitter;
		GUILayout.Label($"Jit {num2}");
		num2 = GUILayout.HorizontalSlider(num2, 0f, 100f);
		peer.NetworkSimulationSettings.IncomingJitter = (int)num2;
		peer.NetworkSimulationSettings.OutgoingJitter = (int)num2;
		float num3 = peer.NetworkSimulationSettings.IncomingLossPercentage;
		GUILayout.Label($"Loss {num3}");
		num3 = GUILayout.HorizontalSlider(num3, 0f, 10f);
		peer.NetworkSimulationSettings.IncomingLossPercentage = (int)num3;
		peer.NetworkSimulationSettings.OutgoingLossPercentage = (int)num3;
		GUILayout.Label($"Lost Audio Frames {(int)debugLostPercent}%");
		debugLostPercent = GUILayout.HorizontalSlider(debugLostPercent, 0f, 100f);
		if (flag)
		{
			voiceConnection.VoiceClient.DebugLostPercent = (int)debugLostPercent;
		}
		else
		{
			voiceConnection.VoiceClient.DebugLostPercent = 0;
		}
		if (GUI.changed)
		{
			windowRect.height = 100f;
		}
		GUI.DragWindow();
	}
}
