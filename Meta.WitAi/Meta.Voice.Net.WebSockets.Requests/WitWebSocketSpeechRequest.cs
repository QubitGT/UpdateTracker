using System;
using System.Collections.Generic;
using Meta.WitAi.Json;

namespace Meta.Voice.Net.WebSockets.Requests;

public class WitWebSocketSpeechRequest : WitWebSocketMessageRequest
{
	public bool IsReadyForInput { get; private set; }

	public bool HasSentAudio { get; private set; }

	public event Action OnReadyForInput;

	public WitWebSocketSpeechRequest(string endpoint, Dictionary<string, string> parameters, string requestId = null, string clientUserId = null, string operationId = null, bool endWithFullTranscription = false)
		: base(endpoint, parameters, requestId, clientUserId, operationId, endWithFullTranscription)
	{
	}

	public override void HandleDownload(string jsonString, WitResponseNode jsonData, byte[] binaryData)
	{
		bool flag = false;
		if (!base.IsComplete && !IsReadyForInput)
		{
			string value = jsonData["type"].Value;
			IsReadyForInput = string.Equals(value, "INITIALIZED");
			flag = IsReadyForInput;
		}
		base.HandleDownload(jsonString, jsonData, binaryData);
		if (flag)
		{
			this.OnReadyForInput?.Invoke();
		}
	}

	public void SendAudioData(byte[] buffer, int offset, int length)
	{
		if (base.IsUploading && IsReadyForInput)
		{
			byte[] array = buffer;
			if (offset != 0 || length != buffer.Length)
			{
				array = new byte[length];
				Array.Copy(buffer, offset, array, 0, length);
			}
			UploadChunk(GetAdditionalPostJson(), array);
			if (!HasSentAudio && length > 0)
			{
				HasSentAudio = true;
			}
		}
	}

	public virtual void CloseAudioStream()
	{
		if (base.IsUploading && IsReadyForInput)
		{
			IsReadyForInput = false;
			WitResponseClass asObject = GetAdditionalPostJson().AsObject;
			WitResponseClass witResponseClass = new WitResponseClass();
			witResponseClass["end_stream"] = new WitResponseClass();
			asObject["data"] = witResponseClass;
			UploadChunk(asObject, null);
		}
	}

	private WitResponseNode GetAdditionalPostJson()
	{
		return new WitResponseClass();
	}
}
