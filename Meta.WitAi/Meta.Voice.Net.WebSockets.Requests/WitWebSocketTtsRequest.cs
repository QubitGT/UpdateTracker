using System;
using System.Collections.Generic;
using System.IO;
using Meta.Voice.Audio.Decoding;
using Meta.Voice.Net.Encoding.Wit;
using Meta.Voice.TelemetryUtilities;
using Meta.WitAi;
using Meta.WitAi.Json;

namespace Meta.Voice.Net.WebSockets.Requests;

public class WitWebSocketTtsRequest : WitWebSocketJsonRequest
{
	public AudioSampleDecodeDelegate OnSamplesReceived;

	public AudioJsonDecodeDelegate OnEventsReceived;

	private IAudioDecoder _audioDecoder;

	private FileStream _fileStream;

	private readonly List<WitResponseNode> _jsonDecoded = new List<WitResponseNode>();

	private int _sampleCount;

	private int _eventCount;

	public string TextToSpeak { get; }

	public Dictionary<string, string> VoiceSettings { get; }

	public TTSWitAudioType AudioType { get; }

	public bool UseEvents { get; }

	public string DownloadPath { get; }

	public WitWebSocketTtsRequest(string requestId, string textToSpeak, Dictionary<string, string> voiceSettings, TTSWitAudioType audioType, bool useEvents, string downloadPath = null, string opId = null)
		: base(GetTtsNode(textToSpeak, voiceSettings, audioType, useEvents), requestId, null, opId)
	{
		TextToSpeak = textToSpeak;
		VoiceSettings = voiceSettings;
		AudioType = audioType;
		UseEvents = useEvents;
		DownloadPath = downloadPath;
		_audioDecoder = WitRequestSettings.GetTtsAudioDecoder(audioType);
	}

	private static WitResponseNode GetTtsNode(string textToSpeak, Dictionary<string, string> voiceSettings, TTSWitAudioType audioType, bool useEvents)
	{
		WitResponseClass witResponseClass = new WitResponseClass();
		WitResponseClass witResponseClass2 = new WitResponseClass();
		WitResponseClass witResponseClass3 = new WitResponseClass();
		witResponseClass3["q"] = textToSpeak;
		foreach (string key in voiceSettings.Keys)
		{
			witResponseClass3[key] = voiceSettings[key];
		}
		witResponseClass3["accept_header"] = WitRequestSettings.GetAudioMimeType(audioType);
		if (useEvents)
		{
			witResponseClass3["viseme"] = new WitResponseData(aData: true);
		}
		witResponseClass2["synthesize"] = witResponseClass3;
		witResponseClass["data"] = witResponseClass2;
		return witResponseClass;
	}

	public override void HandleDownload(string jsonString, WitResponseNode jsonData, byte[] binaryData)
	{
		if (base.IsComplete || jsonData == null)
		{
			return;
		}
		if (!base.IsDownloading)
		{
			HandleDownloadBegin();
		}
		ReturnRawResponse(jsonString);
		SetResponseData(jsonData);
		if (!string.IsNullOrEmpty(base.Error))
		{
			HandleComplete();
			return;
		}
		try
		{
			WitResponseArray witResponseArray = jsonData["viseme"]?.AsArray;
			if (witResponseArray != null && witResponseArray.Count > 0 && OnEventsReceived != null)
			{
				_jsonDecoded.AddRange(witResponseArray.Childs);
				if (_jsonDecoded.Count > 0)
				{
					_eventCount += _jsonDecoded.Count;
					OnEventsReceived(_jsonDecoded);
					_jsonDecoded.Clear();
				}
			}
			if (binaryData != null && binaryData.Length != 0 && OnSamplesReceived != null && _audioDecoder != null)
			{
				_sampleCount += binaryData.Length;
				_audioDecoder.Decode(binaryData, 0, binaryData.Length, OnSamplesReceived);
			}
			if (_fileStream != null)
			{
				if (witResponseArray != null)
				{
					WitChunk chunkData = default(WitChunk);
					chunkData.jsonData = witResponseArray;
					chunkData.binaryData = binaryData;
					byte[] array = WitChunkConverter.Encode(chunkData);
					_fileStream.Write(array, 0, array.Length);
				}
				else if (binaryData != null)
				{
					_fileStream.Write(binaryData, 0, binaryData.Length);
				}
			}
		}
		catch (Exception ex)
		{
			base.Logger.Error("Decode Response Failed\n{0}\n\n{1}", this, ex);
		}
		WitResponseNode witResponseNode = jsonData["end_stream"];
		if ((object)witResponseNode != null && witResponseNode.AsBool)
		{
			HandleComplete();
		}
	}

	protected override void HandleDownloadBegin()
	{
		base.HandleDownloadBegin();
		if (string.IsNullOrEmpty(DownloadPath))
		{
			return;
		}
		string directoryName = Path.GetDirectoryName(DownloadPath);
		if (!Directory.Exists(directoryName))
		{
			base.Logger.Error("Tts download file directory does not exist\nPath: {0}\n{1}", directoryName, this);
			return;
		}
		try
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(DownloadPath);
			string audioExtension = WitRequestSettings.GetAudioExtension(AudioType, UseEvents);
			string path = Path.Join(directoryName, fileNameWithoutExtension + audioExtension);
			_fileStream = new FileStream(path, FileMode.Create);
		}
		catch (Exception ex)
		{
			base.Logger.Error("Tts download file stream generation failed\n{0}\n{1}", this, ex);
		}
	}

	protected override void HandleComplete()
	{
		if (string.IsNullOrEmpty(base.Error))
		{
			if (_sampleCount == 0)
			{
				base.Error = "No audio samples returned";
				RuntimeTelemetry.Instance.LogPoint(base.OperationId, RuntimeTelemetryPoint.FinalAudioSamplesEmpty);
			}
			else if (_eventCount == 0 && UseEvents)
			{
				base.Error = "No audio events returned";
				RuntimeTelemetry.Instance.LogPoint(base.OperationId, RuntimeTelemetryPoint.FinalAudioEventsEmpty);
			}
		}
		if (_fileStream != null)
		{
			_fileStream.Close();
			_fileStream = null;
		}
		base.HandleComplete();
	}

	public override string ToString()
	{
		return string.Format("Type: {0}\nRequest Id: {1}\nTopic Id: {2}\nText: {3}\nAudio Type: {4}\nUse Events: {5}\nDownload Path: {6}\nError: {7}", GetType().Name, base.RequestId, base.TopicId ?? "Null", TextToSpeak ?? "Null", AudioType, UseEvents, DownloadPath ?? "Null", base.Error ?? "Null");
	}
}
