using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.WitAi.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Meta.WitAi.Requests;

[LogCategory(LogCategory.Requests)]
internal class VRequest : IVRequest, ILogSource
{
	public static int MaxConcurrentRequests = 3;

	private static List<Task> _activeRequests = new List<Task>();

	private UnityWebRequest _request;

	private TaskCompletionSource<bool> _unityRequestComplete = new TaskCompletionSource<bool>();

	private DateTime _lastResponseReceivedTime;

	protected const string FilePrepend = "file://";

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Requests);


	public string Url { get; set; }

	public Dictionary<string, string> UrlParameters { get; set; }

	public string ContentType { get; set; }

	public VRequestMethod Method { get; set; }

	public DownloadHandler Downloader { get; set; }

	public UploadHandler Uploader { get; set; }

	[Obsolete("Use TimeoutMs instead")]
	public int Timeout
	{
		get
		{
			return Mathf.CeilToInt((float)TimeoutMs / 1000f);
		}
		set
		{
			TimeoutMs = value * 1000;
		}
	}

	public int TimeoutMs { get; set; } = 5000;


	public bool IsQueued { get; private set; }

	public bool IsRunning { get; private set; }

	public bool IsDecoding { get; private set; }

	public bool IsPerforming
	{
		get
		{
			if (!IsQueued && !IsRunning)
			{
				return IsDecoding;
			}
			return true;
		}
	}

	public bool HasFirstResponse { get; private set; }

	public bool IsComplete { get; private set; }

	public TaskCompletionSource<bool> Completion { get; private set; } = new TaskCompletionSource<bool>();


	public int ResponseCode { get; set; }

	public string ResponseError { get; private set; }

	public float UploadProgress { get; private set; }

	public float DownloadProgress { get; private set; }

	public event VRequestProgressDelegate OnUploadProgress;

	public event VRequestProgressDelegate OnDownloadProgress;

	public event VRequestResponseDelegate OnFirstResponse;

	private static async Task WaitForTurn(VRequest request)
	{
		List<Task> list = new List<Task>();
		lock (_activeRequests)
		{
			list.AddRange(_activeRequests);
			_activeRequests.Add(request.Completion.Task);
		}
		if (list.Count >= MaxConcurrentRequests)
		{
			await list.WhenLessThan(MaxConcurrentRequests);
		}
	}

	public virtual void Reset()
	{
		IsComplete = false;
		IsQueued = false;
		IsRunning = false;
		IsDecoding = false;
		HasFirstResponse = false;
		UploadProgress = 0f;
		this.OnUploadProgress?.Invoke(0f);
		DownloadProgress = 0f;
		this.OnDownloadProgress?.Invoke(0f);
		ResponseCode = 200;
		ResponseError = string.Empty;
	}

	public virtual async Task<VRequestResponse<TValue>> Request<TValue>(VRequestDecodeDelegate<TValue> decoder)
	{
		if (IsPerforming)
		{
			return new VRequestResponse<TValue>(-1, $"Cannot make another VRequest while in progress.\nQueued: {IsQueued}\nRunning: {IsRunning}\nDecoding: {IsDecoding}");
		}
		if (decoder == null)
		{
			return new VRequestResponse<TValue>(-1, "Cannot make a VRequest without a decoder.");
		}
		if (string.IsNullOrEmpty(Url))
		{
			return new VRequestResponse<TValue>(-1, "Cannot make a VRequest without a url.");
		}
		string method = GetMethod();
		if (string.IsNullOrEmpty(method))
		{
			return new VRequestResponse<TValue>(-1, "Cannot make a VRequest without a http method.");
		}
		if (IsComplete)
		{
			return new VRequestResponse<TValue>(ResponseCode, ResponseError);
		}
		Reset();
		Uri uri = GetUri();
		string url = uri.AbsoluteUri;
		Dictionary<string, string> headers = GetHeaders();
		string value;
		if (!string.IsNullOrEmpty(ContentType))
		{
			headers["Content-Type"] = ContentType;
		}
		else if (headers.TryGetValue("Content-Type", out value))
		{
			ContentType = value;
		}
		Logger.Verbose("{0} Request\nUrl: {1}\nRequest Id: {2}", method, url, (headers.ContainsKey("X-Wit-Client-Request-Id") ? headers["X-Wit-Client-Request-Id"] : null) ?? "Null", null, "Request", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Requests\\UnityRequests\\VRequest.cs", 349);
		IsQueued = true;
		await WaitForTurn(this);
		IsQueued = false;
		if (IsComplete)
		{
			return new VRequestResponse<TValue>(ResponseCode, ResponseError);
		}
		WaitForTimeout().WrapErrors();
		IsRunning = true;
		await ThreadUtility.CallOnMainThread(delegate
		{
			if (!IsComplete)
			{
				_request = CreateRequest(url, method, headers);
				UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = _request.SendWebRequest();
				if (unityWebRequestAsyncOperation.isDone || _request.isDone)
				{
					MarkRequestComplete(unityWebRequestAsyncOperation);
				}
				else
				{
					unityWebRequestAsyncOperation.completed += MarkRequestComplete;
				}
			}
		});
		if (!IsComplete)
		{
			await WaitWhileRunning();
		}
		IsRunning = false;
		if (IsComplete)
		{
			return new VRequestResponse<TValue>(ResponseCode, ResponseError);
		}
		IsDecoding = true;
		Tuple<int, string> tuple = await GetError(_request);
		ResponseCode = tuple.Item1;
		ResponseError = tuple.Item2;
		if (!string.IsNullOrEmpty(ResponseError))
		{
			IsDecoding = false;
			return new VRequestResponse<TValue>(ResponseCode, ResponseError);
		}
		TValue value2 = await decoder(_request);
		IsDecoding = false;
		if (IsComplete)
		{
			return new VRequestResponse<TValue>(ResponseCode, ResponseError);
		}
		Dispose();
		return new VRequestResponse<TValue>(value2);
	}

	protected virtual Uri GetUri()
	{
		string text = Url;
		if (!HasUriSchema(text))
		{
			text = "file://" + text;
		}
		if (UrlParameters != null)
		{
			bool flag = false;
			if (!text.Contains('?'))
			{
				text += "?";
				flag = true;
			}
			else if (text.EndsWith('?'))
			{
				flag = true;
			}
			foreach (string key in UrlParameters.Keys)
			{
				string text2 = UrlParameters[key];
				if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(text2))
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						text += "&";
					}
					text2 = UnityWebRequest.EscapeURL(text2).Replace("+", "%20");
					text = text + key + "=" + text2;
				}
			}
		}
		return new Uri(text);
	}

	protected virtual string GetMethod()
	{
		return Method switch
		{
			VRequestMethod.HttpGet => "GET", 
			VRequestMethod.HttpPost => "POST", 
			VRequestMethod.HttpPut => "PUT", 
			VRequestMethod.HttpHead => "HEAD", 
			_ => null, 
		};
	}

	protected virtual Dictionary<string, string> GetHeaders()
	{
		return new Dictionary<string, string>();
	}

	private async Task WaitForTimeout()
	{
		UpdateLastResponseTime();
		await TaskUtility.WaitForTimeout(TimeoutMs, GetLastResponseTime, Completion.Task);
		if (!IsComplete)
		{
			ResponseCode = 14;
			ResponseError = "timeout";
			Cancel();
		}
	}

	private void UpdateLastResponseTime()
	{
		_lastResponseReceivedTime = DateTime.UtcNow;
	}

	private DateTime GetLastResponseTime()
	{
		return _lastResponseReceivedTime;
	}

	protected virtual UnityWebRequest CreateRequest(string url, string method, Dictionary<string, string> headers)
	{
		UnityWebRequest unityWebRequest = new UnityWebRequest(url, method);
		if (headers != null)
		{
			foreach (string key in headers.Keys)
			{
				string value = headers[key];
				if (!string.IsNullOrEmpty(value))
				{
					unityWebRequest.SetRequestHeader(key, value);
					continue;
				}
				Logger.Warning("Failed to set null header value for '{0}'", key);
			}
		}
		if (Uploader != null)
		{
			unityWebRequest.uploadHandler = Uploader;
			unityWebRequest.disposeUploadHandlerOnDispose = true;
		}
		if (Downloader != null)
		{
			unityWebRequest.downloadHandler = Downloader;
			unityWebRequest.disposeDownloadHandlerOnDispose = true;
			if (Downloader is IVRequestDownloadDecoder iVRequestDownloadDecoder)
			{
				iVRequestDownloadDecoder.OnFirstResponse += RaiseFirstResponse;
				iVRequestDownloadDecoder.OnResponse += UpdateLastResponseTime;
				iVRequestDownloadDecoder.OnProgress += UpdateDownloadProgress;
			}
		}
		return unityWebRequest;
	}

	private void MarkRequestComplete(AsyncOperation asyncOperation)
	{
		if (_request != null && !IsComplete)
		{
			ResponseCode = (int)_request.responseCode;
			ResponseError = _request.error;
		}
		_unityRequestComplete.TrySetResult(result: true);
	}

	protected virtual async Task WaitWhileRunning()
	{
		await Task.WhenAny<bool>(_unityRequestComplete.Task, Completion.Task);
		if (!IsComplete && _request != null && string.IsNullOrEmpty(ResponseError) && Downloader is IVRequestDownloadDecoder { Completion: not null } iVRequestDownloadDecoder)
		{
			await Task.WhenAny<bool>(iVRequestDownloadDecoder.Completion.Task, Completion.Task);
		}
	}

	protected virtual async Task<Tuple<int, string>> GetError(UnityWebRequest request)
	{
		int code = ResponseCode;
		string error2 = ResponseError;
		if (request == null || !_unityRequestComplete.Task.IsCompleted)
		{
			code = ((code != 200) ? code : (-1));
			error2 = ((!string.IsNullOrEmpty(error2)) ? error2 : "Request disposed prior to completion");
			return new Tuple<int, string>(code, error2);
		}
		if (string.IsNullOrEmpty(error2) || Downloader == null)
		{
			return new Tuple<int, string>(code, error2);
		}
		string text = await GetDownloadedText(request);
		if (string.IsNullOrEmpty(text))
		{
			return new Tuple<int, string>(code, error2);
		}
		error2 = error2 + "\nServer Response: " + text;
		WitResponseNode witResponseNode = JsonConvert.DeserializeToken(text);
		if (witResponseNode == null)
		{
			return new Tuple<int, string>(code, error2);
		}
		WitResponseClass asObject = witResponseNode.AsObject;
		if (!asObject.HasChild("error"))
		{
			return new Tuple<int, string>(code, error2);
		}
		string value = asObject["error"].Value;
		if (!string.IsNullOrEmpty(value))
		{
			error2 = value;
		}
		return new Tuple<int, string>(code, error2);
	}

	private async Task<string> GetDownloadedText(UnityWebRequest request)
	{
		string text = null;
		await ThreadUtility.CallOnMainThread(Logger, delegate
		{
			DownloadHandler downloadHandler = request?.downloadHandler;
			if (downloadHandler == null)
			{
				return;
			}
			try
			{
				if (downloadHandler is DownloadHandlerAudioClip || downloadHandler is DownloadHandlerFile || downloadHandler is DownloadHandlerBuffer)
				{
					byte[] data = downloadHandler.data;
					if (data != null)
					{
						text = Encoding.UTF8.GetString(data);
						return;
					}
				}
				text = downloadHandler?.text;
			}
			catch (Exception exception)
			{
				Logger.Error(exception, "VRequest failed to parse downloaded text via {0}", downloadHandler.GetType().Name);
			}
		});
		return text;
	}

	public virtual void Cancel()
	{
		if (!IsComplete && string.IsNullOrEmpty(ResponseError))
		{
			ResponseCode = -6;
			ResponseError = "Cancelled";
		}
		if (_request != null)
		{
			ThreadUtility.CallOnMainThread(Logger, delegate
			{
				_request?.Abort();
			});
		}
		Dispose();
	}

	protected virtual void Dispose()
	{
		if (_request != null)
		{
			ThreadUtility.CallOnMainThread(Logger, delegate
			{
				if (_request != null)
				{
					_request.uploadHandler?.Dispose();
					_request.downloadHandler?.Dispose();
					_request.Dispose();
					_request = null;
				}
			});
		}
		IsComplete = true;
		lock (_activeRequests)
		{
			_activeRequests.Remove(Completion.Task);
		}
		if (!Completion.Task.IsCompleted)
		{
			Completion.SetResult(result: true);
		}
	}

	protected virtual void RaiseFirstResponse()
	{
		if (!HasFirstResponse)
		{
			HasFirstResponse = true;
			this.OnFirstResponse?.Invoke();
		}
	}

	protected virtual void UpdateDownloadProgress(float progress)
	{
		if (!DownloadProgress.Equals(progress))
		{
			DownloadProgress = progress;
			this.OnDownloadProgress?.Invoke(DownloadProgress);
		}
	}

	public async Task<VRequestResponse<Dictionary<string, string>>> RequestFileHeaders(string url)
	{
		Url = url;
		Method = VRequestMethod.HttpHead;
		return await Request(DecodeFileHeaders);
	}

	private async Task<Dictionary<string, string>> DecodeFileHeaders(UnityWebRequest request)
	{
		Dictionary<string, string> results = null;
		await ThreadUtility.CallOnMainThread(Logger, delegate
		{
			results = request.GetResponseHeaders();
		});
		return results;
	}

	public async Task<VRequestResponse<byte[]>> RequestFile(string url)
	{
		Url = url;
		if (Method == VRequestMethod.Unknown)
		{
			Method = VRequestMethod.HttpGet;
		}
		if (Downloader == null)
		{
			await ThreadUtility.CallOnMainThread(Logger, delegate
			{
				Downloader = new DownloadHandlerBuffer();
			});
		}
		return await Request(DecodeFile);
	}

	private async Task<byte[]> DecodeFile(UnityWebRequest request)
	{
		byte[] data = null;
		await ThreadUtility.CallOnMainThread(Logger, delegate
		{
			data = request.downloadHandler?.data;
		});
		return data;
	}

	public async Task<VRequestResponse<bool>> RequestFileDownload(string url, string downloadPath)
	{
		Url = url;
		if (Method == VRequestMethod.Unknown)
		{
			Method = VRequestMethod.HttpGet;
		}
		string downloadTempPath = GetTmpDownloadPath(downloadPath);
		try
		{
			if (File.Exists(downloadTempPath))
			{
				File.Delete(downloadTempPath);
			}
		}
		catch (Exception arg)
		{
			return new VRequestResponse<bool>(-1, $"Failed to setup download.\nPath: {downloadPath}\n{arg}");
		}
		await ThreadUtility.CallOnMainThread(Logger, delegate
		{
			Downloader = new DownloadHandlerFile(downloadTempPath);
		});
		VRequestResponse<bool> vRequestResponse = await Request(DecodeSuccess);
		if (!string.IsNullOrEmpty(vRequestResponse.Error))
		{
			return new VRequestResponse<bool>(vRequestResponse.Code, vRequestResponse.Error);
		}
		if (!File.Exists(downloadTempPath))
		{
			return new VRequestResponse<bool>(-1, "File not found at download path\nPath: " + downloadTempPath);
		}
		try
		{
			File.Copy(downloadTempPath, downloadPath, overwrite: true);
			return new VRequestResponse<bool>(value: true);
		}
		catch (Exception arg2)
		{
			return new VRequestResponse<bool>(-1, $"Failed to finalize download.\nPath: {downloadPath}\n{arg2}");
		}
	}

	protected Task<bool> DecodeSuccess(UnityWebRequest request)
	{
		return Task.FromResult(result: true);
	}

	public string GetTmpDownloadPath(string downloadPath)
	{
		return downloadPath + ".tmp";
	}

	public async Task<VRequestResponse<bool>> RequestFileExists(string url)
	{
		if (IsWebUrl(url))
		{
			VRequestResponse<Dictionary<string, string>> vRequestResponse = await RequestFileHeaders(url);
			bool value = string.IsNullOrEmpty(vRequestResponse.Error) && vRequestResponse.Value.Keys.Count > 0;
			return new VRequestResponse<bool>(value, vRequestResponse.Code, vRequestResponse.Error);
		}
		if (IsJarPath(url))
		{
			bool exists = false;
			this.OnFirstResponse = delegate
			{
				exists = true;
				Cancel();
			};
			VRequestResponse<byte[]> vRequestResponse2 = await RequestFile(url);
			if (!exists && vRequestResponse2.Code == 200)
			{
				exists = true;
			}
			return new VRequestResponse<bool>(exists);
		}
		try
		{
			Url = url;
			bool value2 = File.Exists(Url);
			return new VRequestResponse<bool>(value2);
		}
		catch (Exception arg)
		{
			return new VRequestResponse<bool>(-1, $"File exists check failed\nUrl: {url}\n{arg}");
		}
	}

	private static bool IsWebUrl(string url)
	{
		return Regex.IsMatch(url, "(http:|https:).*");
	}

	private static bool IsJarPath(string url)
	{
		return Regex.IsMatch(url, "(jar:).*");
	}

	private static bool HasUriSchema(string url)
	{
		return Regex.IsMatch(url, "(http:|https:|jar:|file:).*");
	}

	public async Task<VRequestResponse<string>> RequestText(Action<string> onPartial = null)
	{
		await ThreadUtility.CallOnMainThread(Logger, delegate
		{
			if (onPartial == null)
			{
				Downloader = new DownloadHandlerBuffer();
			}
			else
			{
				Downloader = new TextStreamHandler(onPartial.Invoke);
			}
		});
		return await Request(DecodeText);
	}

	private async Task<string> DecodeText(UnityWebRequest request)
	{
		string text = null;
		await ThreadUtility.CallOnMainThread(Logger, delegate
		{
			text = request.downloadHandler?.text;
		});
		return text;
	}

	public async Task<VRequestResponse<TData>> RequestJson<TData>(Action<TData> onPartial = null)
	{
		ContentType = "application/json";
		bool decoded = false;
		TData lastPartial = default(TData);
		Action<string> onPartial2 = null;
		if (onPartial != null)
		{
			onPartial2 = delegate(string partialText)
			{
				TData val = DecodeJson<TData>(partialText);
				if (val != null)
				{
					decoded = true;
					lastPartial = val;
					onPartial?.Invoke(lastPartial);
				}
			};
		}
		VRequestResponse<string> vRequestResponse = await RequestText(onPartial2);
		if (!string.IsNullOrEmpty(vRequestResponse.Error))
		{
			return new VRequestResponse<TData>(vRequestResponse.Code, vRequestResponse.Error);
		}
		if (!decoded)
		{
			lastPartial = DecodeJson<TData>(vRequestResponse.Value);
		}
		if (lastPartial == null)
		{
			return new VRequestResponse<TData>(-1, "Failed to decode " + typeof(TData).Name + "\n" + vRequestResponse.Value);
		}
		return new VRequestResponse<TData>(lastPartial);
	}

	private TData DecodeJson<TData>(string json)
	{
		if (typeof(TData) == typeof(string))
		{
			return (TData)(object)json;
		}
		return JsonConvert.DeserializeObject<TData>(json, null, suppressWarnings: true);
	}

	public async Task<VRequestResponse<TData>> RequestJsonGet<TData>(Action<TData> onPartial = null)
	{
		Method = VRequestMethod.HttpGet;
		return await RequestJson(onPartial);
	}

	public async Task<VRequestResponse<TData>> RequestJsonPost<TData>(Action<TData> onPartial = null)
	{
		Method = VRequestMethod.HttpPost;
		return await RequestJson(onPartial);
	}

	public async Task<VRequestResponse<TData>> RequestJsonPost<TData>(byte[] postData, Action<TData> onPartial = null)
	{
		await ThreadUtility.CallOnMainThread(Logger, delegate
		{
			Uploader = new UploadHandlerRaw(postData);
		});
		return await RequestJsonPost(onPartial);
	}

	public async Task<VRequestResponse<TData>> RequestJsonPost<TData>(string postText, Action<TData> onPartial = null)
	{
		byte[] postData = EncodeText(postText);
		return await RequestJsonPost(postData, onPartial);
	}

	public async Task<VRequestResponse<TData>> RequestJsonPut<TData>(Action<TData> onPartial = null)
	{
		Method = VRequestMethod.HttpPut;
		return await RequestJson(onPartial);
	}

	public async Task<VRequestResponse<TData>> RequestJsonPut<TData>(byte[] putData, Action<TData> onPartial = null)
	{
		await ThreadUtility.CallOnMainThread(Logger, delegate
		{
			Uploader = new UploadHandlerRaw(putData);
		});
		return await RequestJsonPut(onPartial);
	}

	public async Task<VRequestResponse<TData>> RequestJsonPut<TData>(string putText, Action<TData> onPartial = null)
	{
		byte[] putData = EncodeText(putText);
		return await RequestJsonPut(putData, onPartial);
	}

	private static byte[] EncodeText(string text)
	{
		return Encoding.UTF8.GetBytes(text);
	}
}
