using System;
using System.IO;
using CSCore;
using CSCore.Codecs.WAV;
using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts;

[RequireComponent(typeof(Recorder))]
[DisallowMultipleComponent]
public class SaveOutgoingStreamToFile : VoiceComponent
{
	private class OutgoingStreamSaverFloat : IProcessor<float>, IDisposable
	{
		private WaveWriter wavWriter;

		public OutgoingStreamSaverFloat(WaveWriter waveWriter)
		{
			wavWriter = waveWriter;
		}

		public float[] Process(float[] buf)
		{
			wavWriter.WriteSamples(buf, 0, buf.Length);
			return buf;
		}

		public void Dispose()
		{
			if (!wavWriter.IsDisposed && !wavWriter.IsDisposing)
			{
				wavWriter.Dispose();
			}
		}
	}

	private class OutgoingStreamSaverShort : IProcessor<short>, IDisposable
	{
		private WaveWriter wavWriter;

		public OutgoingStreamSaverShort(WaveWriter waveWriter)
		{
			wavWriter = waveWriter;
		}

		public short[] Process(short[] buf)
		{
			for (int i = 0; i < buf.Length; i++)
			{
				wavWriter.Write(buf[i]);
			}
			return buf;
		}

		public void Dispose()
		{
			if (!wavWriter.IsDisposed && !wavWriter.IsDisposing)
			{
				wavWriter.Dispose();
			}
		}
	}

	private WaveWriter wavWriter;

	private void PhotonVoiceCreated(PhotonVoiceCreatedParams photonVoiceCreatedParams)
	{
		VoiceInfo info = photonVoiceCreatedParams.Voice.Info;
		int bits = 32;
		if (photonVoiceCreatedParams.Voice is LocalVoiceAudioShort)
		{
			bits = 16;
		}
		string filePath = GetFilePath();
		wavWriter = new WaveWriter(filePath, new WaveFormat(info.SamplingRate, bits, info.Channels));
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Outgoing stream, output file path: {0}", filePath);
		}
		if (photonVoiceCreatedParams.Voice is LocalVoiceAudioFloat)
		{
			(photonVoiceCreatedParams.Voice as LocalVoiceAudioFloat).AddPreProcessor(new OutgoingStreamSaverFloat(wavWriter));
		}
		else if (photonVoiceCreatedParams.Voice is LocalVoiceAudioShort)
		{
			(photonVoiceCreatedParams.Voice as LocalVoiceAudioShort).AddPreProcessor(new OutgoingStreamSaverShort(wavWriter));
		}
	}

	private string GetFilePath()
	{
		string path = string.Format("out_{0}_{1}.wav", DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-ffff"), UnityEngine.Random.Range(0, 1000));
		return Path.Combine(Application.persistentDataPath, path);
	}

	private void PhotonVoiceRemoved()
	{
		wavWriter.Dispose();
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Recording stopped: Saving wav file.");
		}
	}
}
