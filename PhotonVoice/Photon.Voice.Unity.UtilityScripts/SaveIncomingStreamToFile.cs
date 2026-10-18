using System;
using System.IO;
using CSCore;
using CSCore.Codecs.WAV;
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts;

[RequireComponent(typeof(VoiceConnection))]
[DisallowMultipleComponent]
public class SaveIncomingStreamToFile : VoiceComponent
{
	private VoiceConnection voiceConnection;

	[SerializeField]
	private bool muteLocalSpeaker;

	protected override void Awake()
	{
		base.Awake();
		voiceConnection = GetComponent<VoiceConnection>();
		voiceConnection.RemoteVoiceAdded += OnRemoteVoiceAdded;
		voiceConnection.SpeakerLinked += OnSpeakerLinked;
	}

	private void OnSpeakerLinked(Speaker speaker)
	{
		if (muteLocalSpeaker && speaker.Actor != null && speaker.Actor.IsLocal)
		{
			AudioSource component = speaker.GetComponent<AudioSource>();
			component.mute = true;
			component.volume = 0f;
		}
	}

	private void OnDestroy()
	{
		voiceConnection.SpeakerLinked -= OnSpeakerLinked;
		voiceConnection.RemoteVoiceAdded -= OnRemoteVoiceAdded;
	}

	private void OnRemoteVoiceAdded(RemoteVoiceLink remoteVoiceLink)
	{
		int bits = 32;
		string filePath = GetFilePath(remoteVoiceLink);
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Incoming stream, output file path: {0}", filePath);
		}
		WaveWriter waveWriter = new WaveWriter(filePath, new WaveFormat(remoteVoiceLink.Info.SamplingRate, bits, remoteVoiceLink.Info.Channels));
		remoteVoiceLink.FloatFrameDecoded += delegate(FrameOut<float> f)
		{
			waveWriter.WriteSamples(f.Buf, 0, f.Buf.Length);
		};
		remoteVoiceLink.RemoteVoiceRemoved += delegate
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Remote voice stream removed: Saving wav file.");
			}
			waveWriter.Dispose();
		};
	}

	private string GetFilePath(RemoteVoiceLink remoteVoiceLink)
	{
		string path = string.Format("in_{0}_{1}_{2}_{3}_{4}.wav", DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-ffff"), UnityEngine.Random.Range(0, 1000), remoteVoiceLink.ChannelId, remoteVoiceLink.PlayerId, remoteVoiceLink.VoiceId);
		return Path.Combine(Application.persistentDataPath, path);
	}
}
