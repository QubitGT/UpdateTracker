using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meta.Voice;
using Meta.Voice.Logging;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;

namespace Meta.WitAi;

[LogCategory(LogCategory.Requests)]
public class WitRequest : VoiceServiceRequest, IAudioUploadHandler, IDataUploadHandler
{
	public delegate Dictionary<string, string> OnProvideCustomHeadersEvent();

	public delegate Uri OnCustomizeUriEvent(UriBuilder uriBuilder);

	public delegate void PreSendRequestDelegate(ref Uri src_uri, out Dictionary<string, string> headers);

	private string _path;

	private bool _canSetPath = true;

	public byte[] postData;

	public string postContentType;

	public string forcedHttpMethodType;

	public AudioDurationTracker audioDurationTracker;

	private HttpWebRequest _request;

	private Stream _writeStream;

	private object _streamLock = new object();

	private int _bytesWritten;

	private DateTime _requestStartTime;

	private ConcurrentQueue<byte[]> _writeBuffer = new ConcurrentQueue<byte[]>();

	[Obsolete("Deprecated for Events.OnRawResponse")]
	public Action<string> onRawResponse;

	[Obsolete("Deprecated for WitVRequest.OnProvideCustomUri")]
	public OnCustomizeUriEvent onCustomizeUri;

	public static PreSendRequestDelegate onPreSendRequest;

	private bool _initialized;

	private Thread _requestThread;

	private DateTime _timeoutLastUpdate;

	public WitConfiguration Configuration { get; private set; }

	public int TimeoutMs => base.Options.TimeoutMs;

	public AudioEncoding AudioEncoding { get; set; }

	[Obsolete("Deprecated for AudioEncoding")]
	public AudioEncoding audioEncoding
	{
		get
		{
			return AudioEncoding;
		}
		set
		{
			AudioEncoding = value;
		}
	}

	public string Path
	{
		get
		{
			return _path;
		}
		set
		{
			if (_canSetPath)
			{
				_path = value;
			}
			else
			{
				Logger.Warning("Cannot set WitRequest.Path while after transmission.");
			}
		}
	}

	public string Command { get; private set; }

	public bool IsPost { get; private set; }

	[Obsolete("Deprecated for Options.QueryParams")]
	public VoiceServiceRequestOptions.QueryParam[] queryParams
	{
		get
		{
			List<VoiceServiceRequestOptions.QueryParam> list = new List<VoiceServiceRequestOptions.QueryParam>();
			foreach (string item2 in base.Options?.QueryParams?.Keys)
			{
				VoiceServiceRequestOptions.QueryParam item = new VoiceServiceRequestOptions.QueryParam
				{
					key = item2,
					value = base.Options?.QueryParams[item2]
				};
				list.Add(item);
			}
			return list.ToArray();
		}
	}

	protected override bool DecodeRawResponses => true;

	public bool IsRequestStreamActive
	{
		get
		{
			if (!base.IsActive)
			{
				return IsInputStreamReady;
			}
			return true;
		}
	}

	public bool HasResponseStarted { get; private set; }

	public bool IsInputStreamReady { get; private set; }

	public Action OnInputStreamReady { get; set; }

	public event OnProvideCustomHeadersEvent onProvideCustomHeaders;

	[Obsolete("Use OnInputStreamReady instead")]
	public event Action<WitRequest> onInputStreamReady;

	[Obsolete("Deprecated for Events.OnPartialTranscription")]
	public event Action<string> onPartialTranscription;

	[Obsolete("Deprecated for Events.OnFullTranscription")]
	public event Action<string> onFullTranscription;

	[Obsolete("Deprecated for Events.OnPartialResponse")]
	public event Action<WitRequest> onPartialResponse;

	[Obsolete("Deprecated for Events.OnComplete")]
	public event Action<WitRequest> onResponse;

	public override string ToString()
	{
		return Path;
	}

	public WitRequest(WitConfiguration newConfiguration, string newPath, WitRequestOptions newOptions, VoiceServiceRequestEvents newEvents)
		: base(NLPRequestInputType.Audio, newOptions, newEvents)
	{
		Configuration = newConfiguration;
		Path = newPath;
		_initialized = true;
		SetState(VoiceRequestState.Initialized);
	}

	protected override void SetState(VoiceRequestState newState)
	{
		if (_initialized)
		{
			base.SetState(newState);
		}
	}

	protected override void OnInit()
	{
		Command = Path.Split('/').First();
		IsPost = WitEndpointConfig.GetEndpointConfig(Configuration).Speech == Command || WitEndpointConfig.GetEndpointConfig(Configuration).Dictation == Command;
		base.OnInit();
	}

	protected override void HandleAudioActivation()
	{
		SetAudioInputState(VoiceAudioInputState.On);
	}

	protected override void HandleAudioDeactivation()
	{
		if (base.State == VoiceRequestState.Transmitting)
		{
			CloseRequestStream();
		}
		SetAudioInputState(VoiceAudioInputState.Off);
	}

	protected override string GetSendError()
	{
		if (Configuration == null)
		{
			return "Configuration is not set. Cannot start request.";
		}
		if (string.IsNullOrEmpty(Configuration.GetClientAccessToken()))
		{
			return "Client access token is not defined. Cannot start request.";
		}
		if (OnInputStreamReady == null)
		{
			return "No input stream delegate found";
		}
		return base.GetSendError();
	}

	private Uri GetUri()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(base.Options.QueryParams);
		Uri uri = WitRequestSettings.GetUri(Configuration, Path, dictionary);
		if (onCustomizeUri != null)
		{
			uri = onCustomizeUri(new UriBuilder(uri));
		}
		return uri;
	}

	private Dictionary<string, string> GetHeaders()
	{
		Dictionary<string, string> headers = WitRequestSettings.GetHeaders(Configuration, base.Options, useServerToken: false);
		if (this.onProvideCustomHeaders != null)
		{
			Delegate[] invocationList = this.onProvideCustomHeaders.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Dictionary<string, string> dictionary = ((OnProvideCustomHeadersEvent)invocationList[i])();
				if (dictionary == null)
				{
					continue;
				}
				foreach (string key in dictionary.Keys)
				{
					headers[key] = dictionary[key];
				}
			}
		}
		return headers;
	}

	protected override void HandleSend()
	{
		HasResponseStarted = false;
		_bytesWritten = 0;
		_requestStartTime = DateTime.UtcNow;
		_requestThread = new Thread((ThreadStart)async delegate
		{
			await StartThreadedRequest(Logger.CorrelationID);
		});
		_requestThread.Start();
	}

	private void SetupSend(out Uri uri, out Dictionary<string, string> headers, CorrelationID correlationID)
	{
		Logger.CorrelationID = correlationID;
		uri = GetUri();
		_canSetPath = false;
		Logger.Verbose(correlationID, "Setup request with URL: {0}", uri);
		headers = GetHeaders();
		onPreSendRequest?.Invoke(ref uri, out headers);
	}

	private async Task StartThreadedRequest(CorrelationID correlationID)
	{
		SetupSend(out var uri, out var headers, correlationID);
		_request = WebRequest.Create(uri.AbsoluteUri) as HttpWebRequest;
		Logger.Verbose("Created web request: {0}", _request?.RequestUri.AbsoluteUri, null, null, null, "StartThreadedRequest", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\WitRequest.cs", 410);
		_request.KeepAlive = false;
		if (forcedHttpMethodType != null)
		{
			_request.Method = forcedHttpMethodType;
		}
		if (postContentType != null)
		{
			if (forcedHttpMethodType == null)
			{
				_request.Method = "POST";
			}
			_request.ContentType = postContentType;
			_request.ContentLength = postData.Length;
		}
		if (IsPost)
		{
			_request.Method = (string.IsNullOrEmpty(forcedHttpMethodType) ? "POST" : forcedHttpMethodType);
			_request.ContentType = AudioEncoding.ToString();
			_request.SendChunked = true;
		}
		if (headers.ContainsKey("User-Agent"))
		{
			_request.UserAgent = headers["User-Agent"];
			headers.Remove("User-Agent");
		}
		foreach (string key in headers.Keys)
		{
			_request.Headers[key] = headers[key];
		}
		ThreadUtility.BackgroundAsync(Logger, WaitForTimeout).WrapErrors();
		_request.Timeout = -1;
		if (_request.Method == "POST" || _request.Method == "PUT")
		{
			await TaskUtility.FromAsyncResult(_request.BeginGetRequestStream(HandleWriteStream, _request));
		}
		if (_request == null)
		{
			MainThreadCallback(delegate
			{
				HandleFailure(-1, "Request canceled prior to start");
			});
		}
		else
		{
			await TaskUtility.FromAsyncResult(_request.BeginGetResponse(HandleResponse, _request));
		}
	}

	private DateTime GetLastUpdate()
	{
		return _timeoutLastUpdate;
	}

	private async Task WaitForTimeout()
	{
		_timeoutLastUpdate = DateTime.UtcNow;
		await TaskUtility.WaitForTimeout(TimeoutMs, GetLastUpdate);
		if (base.IsActive)
		{
			string arg = "";
			if (_request?.RequestUri?.PathAndQuery != null)
			{
				arg = _request.RequestUri.PathAndQuery.Split(new char[1] { '?' })[0].Substring(1);
			}
			string error = $"Request [{arg}] timed out after {(DateTime.UtcNow - _timeoutLastUpdate).TotalMilliseconds:0.0} ms";
			MainThreadCallback(delegate
			{
				HandleFailure(14, error);
			});
			if (_request != null)
			{
				_request.Abort();
			}
			CloseActiveStream();
		}
	}

	private void HandleWriteStream(IAsyncResult ar)
	{
		try
		{
			Stream stream = _request.EndGetRequestStream(ar);
			_bytesWritten = 0;
			if (postData != null && postData.Length != 0)
			{
				_bytesWritten += postData.Length;
				stream.Write(postData, 0, postData.Length);
				stream.Close();
				return;
			}
			IsInputStreamReady = true;
			_writeStream = stream;
			if (OnInputStreamReady != null)
			{
				MainThreadCallback(delegate
				{
					this.onInputStreamReady?.Invoke(this);
					OnInputStreamReady();
				});
			}
		}
		catch (WebException ex)
		{
			WebException ex2 = ex;
			WebException e = ex2;
			if (e.Status != WebExceptionStatus.RequestCanceled && e.Status != WebExceptionStatus.Timeout && base.StatusCode == 0)
			{
				MainThreadCallback(delegate
				{
					HandleFailure((int)e.Status, e.ToString());
				});
			}
		}
		catch (Exception ex3)
		{
			Exception ex4 = ex3;
			Exception e2 = ex4;
			if (base.StatusCode == 0)
			{
				MainThreadCallback(delegate
				{
					HandleFailure(-1, e2.ToString());
				});
			}
		}
	}

	public void Write(byte[] data, int offset, int length)
	{
		if (!IsInputStreamReady || data == null || length == 0)
		{
			return;
		}
		try
		{
			_writeStream.Write(data, offset, length);
			_bytesWritten += length;
			if (audioDurationTracker != null)
			{
				audioDurationTracker.AddBytes(length);
			}
		}
		catch (ObjectDisposedException)
		{
			_writeStream = null;
		}
		catch (Exception)
		{
			return;
		}
		if (WaitingForPost())
		{
			MainThreadCallback(delegate
			{
				Cancel("Stream was closed with no data written.");
			});
		}
	}

	private void HandleResponse(IAsyncResult asyncResult)
	{
		HasResponseStarted = true;
		string text = "";
		int statusCode = 200;
		string error = null;
		try
		{
			using WebResponse webResponse = _request.EndGetResponse(asyncResult);
			HttpWebResponse httpWebResponse = webResponse as HttpWebResponse;
			int statusCode2 = (int)httpWebResponse.StatusCode;
			if (statusCode != statusCode2)
			{
				statusCode = statusCode2;
				error = httpWebResponse.StatusDescription;
			}
			else
			{
				using Stream stream = httpWebResponse.GetResponseStream();
				text = ProcessStreamResponses(stream);
			}
		}
		catch (JSONParseException arg)
		{
			statusCode = -5;
			error = $"Server returned invalid data.\n\n{arg}";
		}
		catch (WebException ex)
		{
			if (ex.Status != WebExceptionStatus.RequestCanceled && ex.Status != WebExceptionStatus.Timeout)
			{
				statusCode = (int)ex.Status;
				error = ex.ToString();
				if (ex.Response is HttpWebResponse httpWebResponse2)
				{
					statusCode = (int)httpWebResponse2.StatusCode;
					try
					{
						using Stream stream2 = httpWebResponse2.GetResponseStream();
						if (stream2 != null)
						{
							using StreamReader streamReader = new StreamReader(stream2);
							text = streamReader.ReadToEnd();
							if (!string.IsNullOrEmpty(text))
							{
								ProcessStringResponses(text);
							}
						}
					}
					catch (JSONParseException)
					{
					}
					catch (Exception)
					{
					}
				}
			}
		}
		catch (Exception ex4)
		{
			statusCode = -1;
			error = ex4.ToString();
		}
		CloseRequestStream();
		HasResponseStarted = false;
		if (!base.IsActive)
		{
			return;
		}
		if (statusCode != 200 && !string.IsNullOrEmpty(text))
		{
			WitResponseNode witResponseNode = JsonConvert.DeserializeToken(text);
			if (witResponseNode != null && witResponseNode.AsObject.HasChild("error"))
			{
				error = witResponseNode["error"].Value;
			}
		}
		MainThreadCallback(delegate
		{
			if (statusCode != 200)
			{
				HandleFailure(statusCode, error);
			}
			else if (base.ResponseData == null && !IsDecoding)
			{
				error = "Server did not return a valid json response.";
				HandleFailure(error);
			}
			else
			{
				MakeLastResponseFinal();
			}
		});
	}

	private string ProcessStreamResponses(Stream stream)
	{
		using StreamReader streamReader = new StreamReader(stream);
		StringBuilder stringBuilder = new StringBuilder();
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append(text);
				if (string.Equals(text, "}"))
				{
					ProcessStringResponse(stringBuilder.ToString());
					stringBuilder.Clear();
				}
			}
		}
		if (stringBuilder.Length > 0)
		{
			ProcessStringResponse(stringBuilder.ToString());
			return stringBuilder.ToString();
		}
		return null;
	}

	private void ProcessStringResponses(string stringResponse)
	{
		string[] array = stringResponse.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string stringResponse2 in array)
		{
			ProcessStringResponse(stringResponse2);
		}
	}

	private void ProcessStringResponse(string stringResponse)
	{
		_timeoutLastUpdate = DateTime.UtcNow;
		HandleRawResponse(stringResponse, final: false);
	}

	protected override void OnRawResponse(string rawResponse)
	{
		MainThreadCallback(delegate
		{
			base.OnRawResponse(rawResponse);
			onRawResponse?.Invoke(rawResponse);
		});
	}

	protected override void OnPartialTranscription()
	{
		base.OnPartialTranscription();
		this.onPartialTranscription?.Invoke(base.Transcription);
	}

	protected override void OnFullTranscription()
	{
		base.OnFullTranscription();
		this.onFullTranscription?.Invoke(base.Transcription);
	}

	protected override void OnPartialResponse(WitResponseNode responseNode)
	{
		base.OnPartialResponse(responseNode);
		this.onPartialResponse?.Invoke(this);
	}

	private bool WaitingForPost()
	{
		if (IsPost && _bytesWritten == 0)
		{
			return base.StatusCode == 0;
		}
		return false;
	}

	protected override bool HasSentAudio()
	{
		if (IsPost)
		{
			return _bytesWritten > 0;
		}
		return false;
	}

	private void CloseRequestStream()
	{
		if (WaitingForPost())
		{
			Cancel("Request was closed with no audio captured.");
		}
		else
		{
			CloseActiveStream();
		}
	}

	private void CloseActiveStream()
	{
		IsInputStreamReady = false;
		lock (_streamLock)
		{
			if (_writeStream != null)
			{
				try
				{
					_writeStream.Close();
				}
				catch (Exception ex)
				{
					Logger.Warning("Write Stream - Close Failed\n{0}", ex);
				}
				_writeStream = null;
			}
		}
		if (_requestThread != null)
		{
			_requestThread.Abort();
			_requestThread = null;
		}
	}

	protected override void HandleCancel()
	{
		CloseActiveStream();
		if (_request != null)
		{
			_request.Abort();
			_request = null;
		}
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (_writeStream != null)
		{
			CloseActiveStream();
		}
		if (_request != null)
		{
			_request.Abort();
			_request = null;
		}
		this.onResponse?.Invoke(this);
		this.onResponse = null;
	}
}
