using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Audio;
using Meta.Voice.Logging;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Data;
using UnityEngine;

namespace Meta.WitAi.TTS.Debugger;

public class TTSDebugger : MonoBehaviour, ILogSource
{
	private class TTSDebuggerFileStream
	{
		public readonly string FilePath;

		public readonly List<WitResponseNode> EventNodes;

		private readonly FileStream _audioStream;

		public TTSDebuggerFileStream(string filePath)
		{
			FilePath = filePath;
			string path = FilePath + ".raw";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			_audioStream = new FileStream(path, FileMode.Create);
			EventNodes = new List<WitResponseNode>();
		}

		public void AddSamples(float[] samples, int offset, int length)
		{
			for (int i = 0; i < length; i++)
			{
				short value = (short)Mathf.Clamp(samples[offset + i] * 32767f, -32768f, 32767f);
				_audioStream.Write(BitConverter.GetBytes(value));
			}
		}

		public void AddEvent(WitResponseNode ttsEvent)
		{
			EventNodes.Add(ttsEvent);
		}

		public void Dispose()
		{
			EventNodes.Clear();
			_audioStream.Close();
		}
	}

	[Tooltip("The TTS service that will generate tts output files")]
	[SerializeField]
	private TTSService _service;

	[Tooltip("The location within the Assets directory that will output all tts files")]
	[SerializeField]
	private string _outputDirectory = "TtsDebugger";

	private static Regex _fileCleanupRegex;

	private ConcurrentDictionary<string, TTSDebuggerFileStream> _streams = new ConcurrentDictionary<string, TTSDebuggerFileStream>();

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Logging);


	private void Reset()
	{
		if (!_service)
		{
			_service = base.gameObject.GetComponent<TTSService>();
		}
	}

	private static void SetupRegex()
	{
		if (_fileCleanupRegex == null)
		{
			string str = new string(Path.GetInvalidFileNameChars());
			_fileCleanupRegex = new Regex("[" + Regex.Escape(str) + "]");
		}
	}

	private void OnEnable()
	{
		if (!_service)
		{
			_service = base.gameObject.GetComponentInChildren<TTSService>();
		}
		SetListeners(add: true);
	}

	private void OnDisable()
	{
		SetListeners(add: false);
	}

	private void SetListeners(bool add)
	{
		if ((bool)_service)
		{
			_service.Events.Stream.OnStreamBegin.SetListener(OnStreamBegin, add);
			_service.Events.Stream.OnStreamComplete.SetListener(OnStreamComplete, add);
		}
	}

	private string GetClipName(TTSClipData clipData)
	{
		return clipData.clipID + clipData.extension;
	}

	private void OnStreamBegin(TTSClipData clipData)
	{
		string clipName = GetClipName(clipData);
		string text = Application.persistentDataPath + "/" + _outputDirectory;
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		SetupRegex();
		string text2 = _fileCleanupRegex.Replace(clipData.clipID, string.Empty).ToLower();
		DateTime now = DateTime.Now;
		TTSDebuggerFileStream tTSDebuggerFileStream = new TTSDebuggerFileStream($"{text}/{text2}_{clipData.extension.Substring(1)}_{now.Year:0000}{now.Month:00}{now.Day:00}_{now.Hour:00}{now.Minute:00}");
		Logger.Info("TTS Debugger - Begin\nId: {0}\nText: {1}\nVoice: {2}\nFile Type: {3}\nPath: {4}\n{5}{6}", text2, clipData?.textToSpeak ?? "Null", clipData?.voiceSettings?.UniqueId ?? "Null", clipData?.extension ?? "Null", tTSDebuggerFileStream.FilePath, ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Features\\TTS\\Scripts\\Runtime\\Debugger\\TTSDebugger.cs", 142);
		IAudioClipStream clipStream = clipData.clipStream;
		clipStream.OnAddSamples = (AudioClipStreamSampleDelegate)Delegate.Combine(clipStream.OnAddSamples, new AudioClipStreamSampleDelegate(tTSDebuggerFileStream.AddSamples));
		clipData.Events.OnEventJsonAdded += tTSDebuggerFileStream.AddEvent;
		_streams[clipName] = tTSDebuggerFileStream;
	}

	private void OnStreamComplete(TTSClipData clipData)
	{
		string clipName = GetClipName(clipData);
		if (!_streams.TryRemove(clipName, out var value))
		{
			return;
		}
		WitResponseClass witResponseClass = new WitResponseClass();
		witResponseClass["requestId"] = new WitResponseData(clipData?.queryRequestId ?? "Null");
		witResponseClass["fileType"] = new WitResponseData(clipData?.extension?.Substring(1) ?? "Null");
		witResponseClass["clipId"] = new WitResponseData(clipData?.clipID ?? "Null");
		witResponseClass["textToSpeak"] = new WitResponseData(clipData?.textToSpeak ?? "Null");
		witResponseClass["readyDuration"] = new WitResponseData($"{clipData?.readyDuration:0.00} seconds");
		witResponseClass["completeDuration"] = new WitResponseData($"{clipData?.completeDuration:0.00} seconds");
		witResponseClass["length"] = new WitResponseData($"{clipData?.clipStream?.Length:0.00} seconds");
		WitResponseClass witResponseClass2 = new WitResponseClass();
		foreach (KeyValuePair<string, string> item in clipData?.voiceSettings.EncodedValues)
		{
			witResponseClass2[item.Key] = new WitResponseData(item.Value);
		}
		witResponseClass["voiceSettings"] = witResponseClass2;
		WitResponseArray witResponseArray = new WitResponseArray();
		for (int i = 0; i < value.EventNodes.Count; i++)
		{
			witResponseArray[i] = value.EventNodes[i];
		}
		witResponseClass["events"] = witResponseArray;
		string contents = witResponseClass.ToString();
		Logger.Info("TTS Debugger - Complete\nText: {0}\nVoice: {1}\nFile Type: {2}\nPath: {3}", clipData?.textToSpeak ?? "Null", clipData?.voiceSettings?.UniqueId ?? "Null", clipData?.extension ?? "Null", value.FilePath, "OnStreamComplete", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Features\\TTS\\Scripts\\Runtime\\Debugger\\TTSDebugger.cs", 186);
		string path = value.FilePath + ".json";
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		File.WriteAllText(path, contents);
		IAudioClipStream clipStream = clipData.clipStream;
		clipStream.OnAddSamples = (AudioClipStreamSampleDelegate)Delegate.Remove(clipStream.OnAddSamples, new AudioClipStreamSampleDelegate(value.AddSamples));
		clipData.Events.OnEventJsonAdded -= value.AddEvent;
		value.Dispose();
	}
}
