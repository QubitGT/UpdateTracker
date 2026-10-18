using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;

namespace Photon.Voice.PUN.UtilityScripts;

[RequireComponent(typeof(PhotonVoiceView))]
public class VoiceDebugScript : MonoBehaviourPun
{
	private PhotonVoiceView photonVoiceView;

	public bool ForceRecordingAndTransmission;

	public AudioClip TestAudioClip;

	public bool TestUsingAudioClip;

	public bool DisableVad;

	public bool IncreaseLogLevels;

	public bool LocalDebug;

	private void Awake()
	{
		photonVoiceView = GetComponent<PhotonVoiceView>();
	}

	private void Update()
	{
		MaxLogs();
		if (!photonVoiceView.IsRecorder)
		{
			return;
		}
		if (TestUsingAudioClip)
		{
			if ((object)TestAudioClip == null || !TestAudioClip)
			{
				Debug.LogError("Set an AudioClip first");
			}
			else
			{
				photonVoiceView.RecorderInUse.SourceType = Recorder.InputSourceType.AudioClip;
				photonVoiceView.RecorderInUse.AudioClip = TestAudioClip;
				photonVoiceView.RecorderInUse.LoopAudioClip = true;
				if (photonVoiceView.RecorderInUse.RequiresRestart)
				{
					photonVoiceView.RecorderInUse.RestartRecording();
				}
				else
				{
					photonVoiceView.RecorderInUse.StartRecording();
				}
				photonVoiceView.RecorderInUse.TransmitEnabled = true;
			}
		}
		if (ForceRecordingAndTransmission)
		{
			photonVoiceView.RecorderInUse.IsRecording = true;
			photonVoiceView.RecorderInUse.TransmitEnabled = true;
		}
		if (DisableVad)
		{
			photonVoiceView.RecorderInUse.VoiceDetection = false;
		}
	}

	[ContextMenu("CantHearYou")]
	public void CantHearYou()
	{
		if (!PhotonVoiceNetwork.Instance.Client.InRoom)
		{
			Debug.LogError("local voice client is not joined to a voice room");
		}
		else if (!photonVoiceView.IsPhotonViewReady)
		{
			Debug.LogError("PhotonView is not ready yet; maybe PUN client is not joined to a room yet or this PhotonView is not valid");
		}
		else if (!photonVoiceView.IsSpeaker)
		{
			if (base.photonView.IsMine && !photonVoiceView.SetupDebugSpeaker)
			{
				Debug.LogError("local object does not have SetupDebugSpeaker enabled");
				if (LocalDebug)
				{
					Debug.Log("setup debug speaker not enabled, enabling it now (1)");
					photonVoiceView.SetupDebugSpeaker = true;
					photonVoiceView.Setup();
				}
			}
			else
			{
				Debug.LogError("locally not speaker (yet?) (1)");
				photonVoiceView.Setup();
			}
		}
		else
		{
			if (!photonVoiceView.IsSpeakerLinked)
			{
				Debug.LogError("locally speaker not linked, trying late linking & asking anyway");
				PhotonVoiceNetwork.Instance.CheckLateLinking(photonVoiceView.SpeakerInUse, base.photonView.ViewID);
			}
			base.photonView.RPC("CantHearYou", base.photonView.Owner, PhotonVoiceNetwork.Instance.Client.CurrentRoom.Name, PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.ServerIpAddress, PhotonVoiceNetwork.Instance.Client.AppVersion);
		}
	}

	[PunRPC]
	private void CantHearYou(string roomName, string serverIp, string appVersion, PhotonMessageInfo photonMessageInfo)
	{
		string why;
		if (!PhotonVoiceNetwork.Instance.Client.InRoom)
		{
			why = "voice client not in a room";
		}
		else if (!PhotonVoiceNetwork.Instance.Client.CurrentRoom.Name.Equals(roomName))
		{
			why = $"voice client is on another room {PhotonVoiceNetwork.Instance.Client.CurrentRoom.Name} != {roomName}";
		}
		else if (!PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.ServerIpAddress.Equals(serverIp))
		{
			why = $"voice client is on another server {PhotonVoiceNetwork.Instance.Client.LoadBalancingPeer.ServerIpAddress} != {serverIp}, maybe different Photon Cloud regions";
		}
		else if (!PhotonVoiceNetwork.Instance.Client.AppVersion.Equals(appVersion))
		{
			why = $"voice client uses different AppVersion {PhotonVoiceNetwork.Instance.Client.AppVersion} != {appVersion}";
		}
		else if (!photonVoiceView.IsRecorder)
		{
			why = "recorder not setup (yet?)";
			photonVoiceView.Setup();
		}
		else if (!photonVoiceView.RecorderInUse.IsRecording)
		{
			why = "recorder is not recording";
			photonVoiceView.RecorderInUse.IsRecording = true;
		}
		else if (!photonVoiceView.RecorderInUse.TransmitEnabled)
		{
			why = "recorder is not transmitting";
			photonVoiceView.RecorderInUse.TransmitEnabled = true;
		}
		else if (photonVoiceView.RecorderInUse.InterestGroup != 0)
		{
			why = "recorder.InterestGroup is not zero? is this on purpose? switching it back to zero";
			photonVoiceView.RecorderInUse.InterestGroup = 0;
		}
		else if (!(photonVoiceView.RecorderInUse.UserData is int) || (int)photonVoiceView.RecorderInUse.UserData != base.photonView.ViewID)
		{
			why = $"recorder.UserData ({photonVoiceView.RecorderInUse.UserData}) != photonView.ViewID ({base.photonView.ViewID}), fixing it now";
			photonVoiceView.RecorderInUse.UserData = base.photonView.ViewID;
			photonVoiceView.RecorderInUse.RestartRecording();
		}
		else if (photonVoiceView.RecorderInUse.VoiceDetection && DisableVad)
		{
			why = "recorder vad is enabled, disable it for testing";
			photonVoiceView.RecorderInUse.VoiceDetection = false;
		}
		else if (base.photonView.OwnerActorNr == photonMessageInfo.Sender.ActorNumber)
		{
			if (LocalDebug)
			{
				if (photonVoiceView.IsSpeaker)
				{
					why = "no idea why!, should be working (1)";
					photonVoiceView.RecorderInUse.RestartRecording(force: true);
				}
				else if (!photonVoiceView.SetupDebugSpeaker)
				{
					why = "setup debug speaker not enabled, enabling it now (2)";
					photonVoiceView.SetupDebugSpeaker = true;
					photonVoiceView.Setup();
				}
				else if (!photonVoiceView.RecorderInUse.DebugEchoMode)
				{
					why = "recorder debug echo mode not enabled, enabling it now";
					photonVoiceView.RecorderInUse.DebugEchoMode = true;
				}
				else
				{
					why = "locally not speaker (yet?) (2)";
					photonVoiceView.Setup();
				}
			}
			else
			{
				why = "local object, are you trying to hear yourself? (feedback DebugEcho), LocalDebug is disabled, enable it if you want to diagnose this";
			}
		}
		else
		{
			why = "no idea why!, should be working (2)";
			photonVoiceView.RecorderInUse.RestartRecording(force: true);
		}
		Reply(why, photonMessageInfo.Sender);
	}

	private void Reply(string why, Player player)
	{
		base.photonView.RPC("HeresWhy", player, why);
	}

	[PunRPC]
	private void HeresWhy(string why, PhotonMessageInfo photonMessageInfo)
	{
		Debug.LogErrorFormat("Player {0} replied to my CantHearYou message with {1}", photonMessageInfo.Sender, why);
	}

	private void MaxLogs()
	{
		if (IncreaseLogLevels)
		{
			photonVoiceView.LogLevel = DebugLevel.ALL;
			PhotonVoiceNetwork.Instance.LogLevel = DebugLevel.ALL;
			PhotonVoiceNetwork.Instance.GlobalRecordersLogLevel = DebugLevel.ALL;
			PhotonVoiceNetwork.Instance.GlobalSpeakersLogLevel = DebugLevel.ALL;
			if (photonVoiceView.IsRecorder)
			{
				photonVoiceView.RecorderInUse.LogLevel = DebugLevel.ALL;
			}
			if (photonVoiceView.IsSpeaker)
			{
				photonVoiceView.SpeakerInUse.LogLevel = DebugLevel.ALL;
			}
		}
	}
}
