using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.Voice.Net.PubSub;
using Meta.WitAi;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;

namespace Meta.Voice.Net.WebSockets.Requests;

[LogCategory(LogCategory.Requests)]
public class WitWebSocketJsonRequest : IWitWebSocketRequest, ILogSource
{
	private UploadChunkDelegate _uploader;

	private DateTime _timeoutStart;

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Requests);


	public string RequestId { get; }

	public string ClientUserId { get; }

	public string OperationId { get; }

	public string TopicId { get; set; }

	public PubSubResponseOptions PublishOptions { get; set; }

	public int TimeoutMs { get; set; }

	public bool IsUploading { get; protected set; }

	public bool IsDownloading { get; private set; }

	public bool IsComplete { get; private set; }

	public TaskCompletionSource<bool> Completion { get; } = new TaskCompletionSource<bool>();


	public int Code { get; protected set; }

	public string Error { get; protected set; }

	public VoiceErrorSimulationType SimulatedErrorType { get; internal set; } = (VoiceErrorSimulationType)(-1);


	public WitResponseNode PostData { get; }

	public WitResponseNode ResponseData { get; protected set; }

	public Action<string> OnRawResponse { get; set; }

	public Action<IWitWebSocketRequest> OnFirstResponse { get; set; }

	public Action<IWitWebSocketRequest> OnComplete { get; set; }

	public WitWebSocketJsonRequest(WitResponseNode postData, string requestId = null, string clientUserId = null, string operationId = null)
	{
		PostData = postData;
		RequestId = (string.IsNullOrEmpty(requestId) ? WitConstants.GetUniqueId() : requestId);
		ClientUserId = (string.IsNullOrEmpty(clientUserId) ? WitRequestSettings.LocalClientUserId : clientUserId);
		OperationId = (string.IsNullOrEmpty(operationId) ? WitConstants.GetUniqueId() : operationId);
	}

	public virtual void HandleUpload(UploadChunkDelegate uploadChunk)
	{
		if (IsUploading)
		{
			return;
		}
		IsUploading = true;
		_uploader = uploadChunk;
		if (!string.IsNullOrEmpty(TopicId) && PostData != null)
		{
			WitResponseClass witResponseClass = new WitResponseClass();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			PubSubSettings.GetTopics(dictionary, TopicId, PublishOptions);
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				witResponseClass[item.Key] = item.Value;
			}
			PostData["publish_topics"] = witResponseClass;
		}
		if (!string.IsNullOrEmpty(ClientUserId) && PostData != null)
		{
			PostData["client_user_id"] = ClientUserId;
		}
		if (!string.IsNullOrEmpty(OperationId) && PostData != null)
		{
			PostData["operation_id"] = OperationId;
		}
		UploadChunk(PostData, null);
		WaitForTimeout().WrapErrors();
	}

	protected void UploadChunk(WitResponseNode uploadJson, byte[] uploadBinary)
	{
		_uploader?.Invoke(RequestId, uploadJson, uploadBinary);
	}

	protected async Task WaitForTimeout()
	{
		UpdateTimeoutStart();
		await TaskUtility.WaitForTimeout(TimeoutMs, GetTimeoutStart, Completion.Task);
		if (!IsComplete)
		{
			SendAbort("timeout");
			Code = 14;
			Error = "timeout";
			HandleComplete();
		}
	}

	private DateTime GetTimeoutStart()
	{
		return _timeoutStart;
	}

	protected void UpdateTimeoutStart()
	{
		_timeoutStart = DateTime.UtcNow;
	}

	public virtual void Cancel()
	{
		if (!IsComplete)
		{
			SendAbort("Cancelled");
			Code = -6;
			Error = "Cancelled";
			HandleComplete();
		}
	}

	protected void SendAbort(string reason)
	{
		if (IsUploading && _uploader != null)
		{
			WitResponseClass witResponseClass = new WitResponseClass();
			WitResponseClass witResponseClass2 = new WitResponseClass();
			witResponseClass2["abort"] = new WitResponseClass();
			witResponseClass2["abort"]["reason"] = reason;
			witResponseClass["data"] = witResponseClass2;
			_uploader?.Invoke(RequestId, witResponseClass, null);
		}
	}

	public virtual void HandleDownload(string jsonString, WitResponseNode jsonData, byte[] binaryData)
	{
		if (!IsDownloading && !IsComplete)
		{
			HandleDownloadBegin();
			ReturnRawResponse(jsonString);
			SetResponseData(jsonData);
			HandleComplete();
		}
	}

	protected virtual void ReturnRawResponse(string jsonString)
	{
		if (OnRawResponse != null)
		{
			ThreadUtility.CallOnMainThread(delegate
			{
				OnRawResponse(jsonString);
			});
		}
	}

	protected virtual void SetResponseData(WitResponseNode newResponseData)
	{
		UpdateTimeoutStart();
		ResponseData = newResponseData;
		string value = ResponseData["code"].Value;
		if (!string.IsNullOrEmpty(value))
		{
			if (int.TryParse(value, out var result))
			{
				Code = result;
			}
			else
			{
				Code = -1;
				Logger.Warning("{0} Response Code is not an integer: {1}\n{2}", GetType().Name, value, this);
			}
		}
		Error = ResponseData["error"].Value;
		string text = ResponseData["topic"]?.Value;
		if (!string.IsNullOrEmpty(text))
		{
			TopicId = text;
		}
	}

	protected virtual void HandleDownloadBegin()
	{
		if (!IsDownloading)
		{
			IsDownloading = true;
			ThreadUtility.CallOnMainThread(RaiseFirstResponse);
		}
	}

	protected virtual void RaiseFirstResponse()
	{
		OnFirstResponse?.Invoke(this);
	}

	protected virtual void HandleComplete()
	{
		if (!IsComplete)
		{
			IsUploading = false;
			_uploader = null;
			IsDownloading = false;
			IsComplete = true;
			if (!Completion.Task.IsCompleted)
			{
				Completion.SetResult(string.IsNullOrEmpty(Error));
			}
			ThreadUtility.CallOnMainThread(RaiseComplete);
		}
	}

	protected virtual void RaiseComplete()
	{
		OnComplete?.Invoke(this);
	}

	public override string ToString()
	{
		return string.Format("Type: {0}\nRequest Id: {1}\nClient User Id: {2}\nTopic Id: {3}\nError: {4}", GetType().Name, RequestId, ClientUserId ?? "Null", TopicId ?? "Null", Error ?? "Null");
	}
}
