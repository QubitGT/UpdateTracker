using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.HttpClient;
using Modio.API.Interfaces;
using Modio.API.SchemaDefinitions;
using Modio.Errors;
using Modio.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace Modio.Unity;

public class ModioAPIUnityClient : IModioAPIInterface, IDisposable
{
	private string _basePath = string.Empty;

	private readonly List<string> _defaultParameters = new List<string>();

	private readonly Dictionary<string, string> _pathParameters = new Dictionary<string, string>();

	private readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();

	private readonly List<UnityWebRequest> _webRequests = new List<UnityWebRequest>();

	private CancellationTokenSource _cancellationTokenSource;

	[ModioDebugMenu(ShowInSettingsMenu = true, ShowInBrowserMenu = false)]
	public static bool UseUnityClient
	{
		get
		{
			return ModioServices.Resolve<IModioAPIInterface>() is ModioAPIUnityClient;
		}
		set
		{
			if (value != UseUnityClient)
			{
				if (value)
				{
					ModioServices.Bind<IModioAPIInterface>().FromNew<ModioAPIUnityClient>();
				}
				else
				{
					ModioServices.Bind<IModioAPIInterface>().FromNew<ModioAPIHttpClient>();
				}
				User.LogOut();
			}
		}
	}

	public void SetBasePath(string value)
	{
		_basePath = value;
	}

	public void AddDefaultPathParameter(string key, string value)
	{
		_pathParameters[key] = value;
	}

	public void RemoveDefaultPathParameter(string key)
	{
		_pathParameters.Remove(key);
	}

	public void AddDefaultParameter(string value)
	{
		_defaultParameters.Add(value);
	}

	public void RemoveDefaultParameter(string value)
	{
		_defaultParameters.Remove(value);
	}

	public void ResetConfiguration()
	{
		_cancellationTokenSource?.Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
		_defaultParameters.Clear();
		_pathParameters.Clear();
		_defaultHeaders.Clear();
		_basePath = string.Empty;
		ModioClient.OnShutdown -= Shutdown;
		ModioClient.OnShutdown += Shutdown;
	}

	private void Shutdown()
	{
		_cancellationTokenSource?.Cancel();
	}

	public async Task<(Error, Stream)> DownloadFile(string url, CancellationToken token = default(CancellationToken))
	{
		Error error = await CheckFakeErrorsForTest(url);
		if ((bool)error)
		{
			return (error, null);
		}
		if (string.IsNullOrEmpty(url))
		{
			ModioLog.Error?.Log("Attempting to download null url");
			return (new HttpError(HttpErrorCode.REQUEST_ERROR), null);
		}
		ModioAPIRequest modioAPIRequest = ModioAPIRequest.New(url);
		CancellationToken cancellationToken = _cancellationTokenSource?.Token ?? CancellationToken.None;
		if (token == default(CancellationToken))
		{
			token = cancellationToken;
		}
		StreamingDownloadHandler handler = new StreamingDownloadHandler(1048576, token);
		UnityWebRequest webRequest = CreateWebRequest(modioAPIRequest, url, handler);
		handler.SetCallingRequest(webRequest);
		Error error2 = EnforceAuthentication(modioAPIRequest, webRequest);
		if ((bool)error2)
		{
			return (error2, null);
		}
		await LogRequest(webRequest);
		Stream stream;
		try
		{
			webRequest.SendWebRequest();
			await handler.ResponseReceived();
			long responseCode = webRequest.responseCode;
			if (responseCode < 200 || responseCode >= 300)
			{
				string text = webRequest.downloadHandler.text;
				if (!IsResponseConnectionFailure(webRequest.responseCode))
				{
					return (GetErrorAndLogBadResponse(text), null);
				}
				ModioLog.Error?.Log($"Unable to reach mod.io servers {webRequest.responseCode}");
				ModioAPI.SetOfflineStatus(isOffline: true);
				webRequest.Abort();
				webRequest.Dispose();
				return (new Error(ErrorCode.CANNOT_OPEN_CONNECTION), null);
			}
			stream = handler.GetStream();
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log($"Exception in {url}: {ex}");
			webRequest.Abort();
			webRequest.Dispose();
			return (new ErrorException(ex), null);
		}
		return (Error.None, stream);
	}

	private Task<Error> CheckFakeErrorsForTest(string url)
	{
		ModioAPITestSettings testSettings = ModioClient.Settings.GetPlatformSettings<ModioAPITestSettings>();
		if (testSettings == null)
		{
			return Task.FromResult(Error.None);
		}
		if (testSettings.ShouldFakeDisconnected(url))
		{
			return FakeConnectionError();
		}
		if (testSettings.ShouldFakeRateLimit(url))
		{
			return Task.FromResult((Error)new RateLimitError(RateLimitErrorCode.RATELIMITED, 42));
		}
		return Task.FromResult(Error.None);
		async Task<Error> FakeConnectionError()
		{
			await Task.Delay((int)(testSettings.FakeDisconnectedTimeoutDuration * 1000f));
			return new Error(ErrorCode.CANNOT_OPEN_CONNECTION);
		}
	}

	public void SetDefaultHeader(string name, string value)
	{
		_defaultHeaders[name] = value;
	}

	public void RemoveDefaultHeader(string name)
	{
		_defaultHeaders.Remove(name);
	}

	private UnityWebRequest CreateWebRequest(ModioAPIRequest request, string target, DownloadHandler downloadHandler = null)
	{
		UnityWebRequest unityWebRequest = new UnityWebRequest(target, MapMethod(request.Method))
		{
			downloadHandler = (downloadHandler ?? new DownloadHandlerBuffer())
		};
		foreach (KeyValuePair<string, string> defaultHeader in _defaultHeaders)
		{
			unityWebRequest.SetRequestHeader(defaultHeader.Key, defaultHeader.Value);
		}
		unityWebRequest.SetRequestHeader("User-Agent", Version.GetCurrent());
		unityWebRequest.uploadHandler = MapUploadHandler(request);
		if (unityWebRequest.uploadHandler == null)
		{
			unityWebRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		}
		foreach (KeyValuePair<string, string> headerParameter in request.Options.HeaderParameters)
		{
			unityWebRequest.SetRequestHeader(headerParameter.Key, headerParameter.Value);
		}
		foreach (KeyValuePair<string, string> defaultHeader2 in _defaultHeaders)
		{
			request.Options.HeaderParameters[defaultHeader2.Key] = defaultHeader2.Value;
		}
		return unityWebRequest;
	}

	private static Error EnforceAuthentication(ModioAPIRequest downloadRequest, UnityWebRequest webRequest)
	{
		if (downloadRequest.Options.RequiresAuthentication && !User.Current.IsAuthenticated)
		{
			return new Error(ErrorCode.USER_NOT_AUTHENTICATED);
		}
		if (User.Current.IsAuthenticated)
		{
			webRequest.SetRequestHeader("Authorization", "Bearer " + User.Current?.GetAuthToken());
		}
		return Error.None;
	}

	private async Task<(Error error, T)> GetJson<T>(ModioAPIRequest request, Func<JsonTextReader, Task<T>> reader)
	{
		string target = BuildPath(request);
		Error error = await CheckFakeErrorsForTest(target);
		if ((bool)error)
		{
			return (error: error, default(T));
		}
		using UnityWebRequest webRequest = CreateWebRequest(request, target);
		error = EnforceAuthentication(request, webRequest);
		if ((bool)error)
		{
			return (error: error, default(T));
		}
		_webRequests.Add(webRequest);
		CancellationToken cachedShutdownToken = _cancellationTokenSource?.Token ?? CancellationToken.None;
		try
		{
			await LogRequest(webRequest, request);
			error = await SendRequest(webRequest, cachedShutdownToken);
			if ((bool)error)
			{
				return (error: error, default(T));
			}
			string text = webRequest.downloadHandler.text;
			if (webRequest.responseCode == 204)
			{
				return (error: Error.None, (T)(object)default(Response204));
			}
			ModioLog.Verbose?.Log($"{webRequest.responseCode} | {text}");
			if (webRequest.responseCode < 200 || webRequest.responseCode >= 300)
			{
				if (IsResponseConnectionFailure(webRequest.responseCode))
				{
					ModioLog.Error?.Log($"Unable to reach mod.io servers {webRequest.responseCode}");
					ModioAPI.SetOfflineStatus(isOffline: true);
					return (error: new Error(ErrorCode.CANNOT_OPEN_CONNECTION), default(T));
				}
				if (webRequest.responseCode == 429 && webRequest.GetResponseHeaders().TryGetValue("retry-after", out var value) && !string.IsNullOrEmpty(value) && int.TryParse(value, out var result))
				{
					GetErrorAndLogBadResponse(text);
					return (error: new RateLimitError(RateLimitErrorCode.RATELIMITED, result), default(T));
				}
				return (error: GetErrorAndLogBadResponse(text), default(T));
			}
			if (ModioAPI.IsOffline)
			{
				ModioAPI.SetOfflineStatus(isOffline: false);
			}
			using StringReader stringReader = new StringReader(text);
			using JsonTextReader jsonTextReader = new JsonTextReader(stringReader);
			T item = await reader(jsonTextReader);
			return (error: Error.None, item);
		}
		catch (JsonException arg)
		{
			ModioLog.Verbose?.Log(ErrorCode.HTTP_EXCEPTION.GetMessage($"{target}\n{arg}"));
			return (error: new Error(ErrorCode.INVALID_JSON), default(T));
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log(ex.GetType());
			return (error: new Error(ErrorCode.INVALID_JSON), default(T));
		}
		finally
		{
			_webRequests.Remove(webRequest);
		}
	}

	private static Error GetErrorAndLogBadResponse(string jsonResponse)
	{
		if (!string.IsNullOrEmpty(jsonResponse) && jsonResponse[0] != '{')
		{
			int num = jsonResponse.IndexOf('{');
			if (num > 0)
			{
				string text = jsonResponse.Substring(0, num);
				ModioLog.Verbose?.Log("Unexpected error from server before JSON: " + text);
				jsonResponse = jsonResponse.Substring(num);
			}
		}
		ErrorObject errorObject;
		try
		{
			using StringReader reader = new StringReader(jsonResponse);
			using JsonTextReader reader2 = new JsonTextReader(reader);
			errorObject = new JsonSerializer().Deserialize<ErrorObject>(reader2);
		}
		catch (JsonException)
		{
			ModioLog.Error?.Log("There is an error with the json response.");
			return new Error(ErrorCode.INVALID_JSON);
		}
		if (errorObject.Error.ErrorRef == 0L)
		{
			ModioLog.Error?.Log("Invalid error returned from API, please contact mod.io support.\n" + $"{errorObject.Error.Code}: {errorObject.Error.Message}");
			return new Error(ErrorCode.UNKNOWN);
		}
		return new Error((ErrorCode)errorObject.Error.ErrorRef);
	}

	public Task<(Error error, T? result)> GetJson<T>(ModioAPIRequest request) where T : struct
	{
		return GetJson(request, (JsonTextReader reader) => Task.FromResult((T?)new JsonSerializer().Deserialize<T>(reader)));
	}

	public Task<(Error error, JToken)> GetJson(ModioAPIRequest request)
	{
		return GetJson(request, (JsonTextReader reader) => JToken.ReadFromAsync(reader));
	}

	private UploadHandler MapUploadHandler(ModioAPIRequest request)
	{
		switch (request.ContentType)
		{
		case ModioAPIRequestContentType.None:
			return null;
		case ModioAPIRequestContentType.FormUrlEncoded:
		{
			string text = CreateFormUrlEncodedContent(request.Options.FormParameters);
			if (!string.IsNullOrEmpty(text))
			{
				return new UploadHandlerRaw(Encoding.UTF8.GetBytes(text))
				{
					contentType = "application/x-www-form-urlencoded"
				};
			}
			return null;
		}
		case ModioAPIRequestContentType.MultipartFormData:
			return CreateMultipartFormDataUploadHandler(request.Options);
		case ModioAPIRequestContentType.ByteArray:
			return PrepareByteArray(request.Options);
		case ModioAPIRequestContentType.String:
			if (request.ContentTypeHint == "application/json")
			{
				return new UploadHandlerRaw(request.Options.BodyDataBytes)
				{
					contentType = request.ContentTypeHint
				};
			}
			break;
		}
		throw new NotImplementedException();
	}

	private UploadHandler CreateMultipartFormDataUploadHandler(ModioAPIRequestOptions options)
	{
		string text = Guid.NewGuid().ToString().ToUpperInvariant();
		using MemoryStream memoryStream = new MemoryStream();
		using (StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, leaveOpen: true))
		{
			streamWriter.WriteLine("--" + text);
			foreach (KeyValuePair<string, string> formParameter in options.FormParameters)
			{
				streamWriter.WriteLine("--" + text);
				streamWriter.WriteLine("Content-Disposition: form-data; name=" + formParameter.Key);
				streamWriter.WriteLine("Content-Type: text/plain; charset=utf-8");
				streamWriter.WriteLine();
				streamWriter.WriteLine(formParameter.Value);
			}
			foreach (KeyValuePair<string, ModioAPIFileParameter> fileParameter in options.FileParameters)
			{
				if (fileParameter.Value.Unused)
				{
					continue;
				}
				using Stream stream = fileParameter.Value.GetContent();
				if (stream != null)
				{
					streamWriter.WriteLine("--" + text);
					streamWriter.WriteLine("Content-Disposition: form-data; name=\"" + fileParameter.Key + "\"; filename=\"" + fileParameter.Value.Name + "\"; filename*=utf-8''" + fileParameter.Value.Name);
					streamWriter.WriteLine("Content-Type: " + fileParameter.Value.ContentType);
					streamWriter.WriteLine();
					streamWriter.Flush();
					stream.CopyTo(memoryStream);
					streamWriter.WriteLine();
				}
			}
			streamWriter.WriteLine("--" + text + "--");
			streamWriter.Flush();
		}
		return new UploadHandlerRaw(memoryStream.ToArray())
		{
			contentType = "multipart/form-data; boundary=" + text
		};
	}

	private static UploadHandler PrepareByteArray(ModioAPIRequestOptions options)
	{
		string text = Guid.NewGuid().ToString().ToUpperInvariant();
		return new UploadHandlerRaw(options.BodyDataBytes)
		{
			contentType = "multipart/form-data; boundary=" + text
		};
	}

	private string CreateFormUrlEncodedContent(Dictionary<string, string> formParameters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> formParameter in formParameters)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append("&");
			}
			stringBuilder.Append(UnityWebRequest.EscapeURL(formParameter.Key) + "=" + UnityWebRequest.EscapeURL(formParameter.Value));
		}
		return stringBuilder.ToString();
	}

	private string MapMethod(ModioAPIRequestMethod method)
	{
		return method switch
		{
			ModioAPIRequestMethod.Post => "POST", 
			ModioAPIRequestMethod.Put => "PUT", 
			ModioAPIRequestMethod.Get => "GET", 
			ModioAPIRequestMethod.Delete => "DELETE", 
			_ => throw new NotImplementedException(), 
		};
	}

	private string BuildPath(ModioAPIRequest request)
	{
		StringBuilder stringBuilder = new StringBuilder(_basePath + request.GetUri(_defaultParameters));
		foreach (KeyValuePair<string, string> pathParameter in _pathParameters)
		{
			stringBuilder.Replace("{" + pathParameter.Key + "}", pathParameter.Value);
		}
		return stringBuilder.ToString();
	}

	private async Task<Error> SendRequest(UnityWebRequest webRequest, CancellationToken shutdownToken = default(CancellationToken), CancellationToken token = default(CancellationToken))
	{
		UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();
		while (!operation.isDone)
		{
			if (token.IsCancellationRequested || shutdownToken.IsCancellationRequested)
			{
				webRequest.Abort();
				break;
			}
			await Task.Yield();
		}
		if (shutdownToken.IsCancellationRequested)
		{
			return new Error(ErrorCode.SHUTTING_DOWN);
		}
		if (token.IsCancellationRequested)
		{
			return new Error(ErrorCode.OPERATION_CANCELLED);
		}
		return Error.None;
	}

	public void Dispose()
	{
		foreach (UnityWebRequest webRequest in _webRequests)
		{
			webRequest?.Dispose();
		}
	}

	private Task LogRequest(UnityWebRequest request, ModioAPIRequest modioRequest = null)
	{
		if (ModioLog.Verbose == null)
		{
			return Task.CompletedTask;
		}
		if (request == null)
		{
			return Task.CompletedTask;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(request.method + " " + request.uri.PathAndQuery + " HTTP/1.1");
		if (modioRequest != null)
		{
			foreach (var (text3, text4) in modioRequest.Options.HeaderParameters)
			{
				stringBuilder.AppendLine(string.Equals(text3, "Authorization") ? (text3 + ": Bearer (omitted)") : (text3 + ": " + string.Join(", ", text4)));
			}
		}
		foreach (string defaultParameter in _defaultParameters)
		{
			stringBuilder.AppendLine(defaultParameter);
		}
		if (request.uploadHandler != null && request.uploadHandler.data.Length != 0)
		{
			stringBuilder.AppendLine("Content-Type: " + request.uploadHandler.contentType);
			stringBuilder.AppendLine();
			stringBuilder.Append(Encoding.UTF8.GetString(request.uploadHandler.data));
		}
		ModioLog.Verbose?.Log(stringBuilder.ToString());
		return Task.CompletedTask;
	}

	private static bool IsResponseConnectionFailure(long responseCode)
	{
		if (responseCode != 0L && responseCode != 408)
		{
			return responseCode == 503;
		}
		return true;
	}
}
