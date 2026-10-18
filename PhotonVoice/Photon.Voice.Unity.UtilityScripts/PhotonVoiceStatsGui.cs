using ExitGames.Client.Photon;
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts;

public class PhotonVoiceStatsGui : MonoBehaviour
{
	private bool statsWindowOn = true;

	private bool statsOn;

	private bool healthStatsVisible;

	private bool trafficStatsOn;

	private bool buttonsOn;

	private bool voiceStatsOn = true;

	private Rect statsRect = new Rect(0f, 100f, 300f, 50f);

	private int windowId = 200;

	private PhotonPeer peer;

	private VoiceConnection voiceConnection;

	private VoiceClient voiceClient;

	private void OnEnable()
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
		voiceClient = voiceConnection.VoiceClient;
		peer = voiceConnection.Client.LoadBalancingPeer;
		if (statsRect.x <= 0f)
		{
			statsRect.x = (float)Screen.width - statsRect.width;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
		{
			statsWindowOn = !statsWindowOn;
			statsOn = true;
		}
	}

	private void OnGUI()
	{
		if (peer.TrafficStatsEnabled != statsOn)
		{
			peer.TrafficStatsEnabled = statsOn;
		}
		if (statsWindowOn)
		{
			statsRect = GUILayout.Window(windowId, statsRect, TrafficStatsWindow, "Voice Client Messages (shift+tab)");
		}
	}

	private void TrafficStatsWindow(int windowId)
	{
		bool flag = false;
		TrafficStatsGameLevel trafficStatsGameLevel = peer.TrafficStatsGameLevel;
		long num = peer.TrafficStatsElapsedMs / 1000;
		if (num == 0L)
		{
			num = 1L;
		}
		GUILayout.BeginHorizontal();
		buttonsOn = GUILayout.Toggle(buttonsOn, "buttons");
		healthStatsVisible = GUILayout.Toggle(healthStatsVisible, "health");
		trafficStatsOn = GUILayout.Toggle(trafficStatsOn, "traffic");
		voiceStatsOn = GUILayout.Toggle(voiceStatsOn, "voice stats");
		GUILayout.EndHorizontal();
		string text = $"Out {trafficStatsGameLevel.TotalOutgoingMessageCount,4} | In {trafficStatsGameLevel.TotalIncomingMessageCount,4} | Sum {trafficStatsGameLevel.TotalMessageCount,4}";
		string text2 = $"{num}sec average:";
		string text3 = $"Out {trafficStatsGameLevel.TotalOutgoingMessageCount / num,4} | In {trafficStatsGameLevel.TotalIncomingMessageCount / num,4} | Sum {trafficStatsGameLevel.TotalMessageCount / num,4}";
		GUILayout.Label(text);
		GUILayout.Label(text2);
		GUILayout.Label(text3);
		if (buttonsOn)
		{
			GUILayout.BeginHorizontal();
			statsOn = GUILayout.Toggle(statsOn, "stats on");
			if (GUILayout.Button("Reset"))
			{
				peer.TrafficStatsReset();
				peer.TrafficStatsEnabled = true;
			}
			flag = GUILayout.Button("To Log");
			GUILayout.EndHorizontal();
		}
		string text4 = string.Empty;
		string text5 = string.Empty;
		if (trafficStatsOn)
		{
			GUILayout.Box("Voice Client Traffic Stats");
			text4 = "Incoming: \n" + peer.TrafficStatsIncoming;
			text5 = "Outgoing: \n" + peer.TrafficStatsOutgoing;
			GUILayout.Label(text4);
			GUILayout.Label(text5);
		}
		string text6 = string.Empty;
		if (healthStatsVisible)
		{
			GUILayout.Box("Voice Client Health Stats");
			text6 = string.Format("ping: {6}|{9}[+/-{7}|{10}]ms resent:{8} \n\nmax ms between\nsend: {0,4} \ndispatch: {1,4} \n\nlongest dispatch for: \nev({3}):{2,3}ms \nop({5}):{4,3}ms", trafficStatsGameLevel.LongestDeltaBetweenSending, trafficStatsGameLevel.LongestDeltaBetweenDispatching, trafficStatsGameLevel.LongestEventCallback, trafficStatsGameLevel.LongestEventCallbackCode, trafficStatsGameLevel.LongestOpResponseCallback, trafficStatsGameLevel.LongestOpResponseCallbackOpCode, peer.RoundTripTime, peer.RoundTripTimeVariance, peer.ResentReliableCommands, voiceClient.RoundTripTime, voiceClient.RoundTripTimeVariance);
			GUILayout.Label(text6);
		}
		_ = string.Empty;
		if (voiceStatsOn)
		{
			GUILayout.Box("Voice Frames Stats");
			GUILayout.Label($"received: {voiceClient.FramesReceived}, {voiceConnection.FramesReceivedPerSecond:F2}/s \n\nlost: {voiceClient.FramesLost}, {voiceConnection.FramesLostPerSecond:F2}/s ({voiceConnection.FramesLostPercent:F2}%) \n\nsent: {voiceClient.FramesSent} ({voiceClient.FramesSentBytes} bytes)");
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
