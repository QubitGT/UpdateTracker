using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modio.API.Interfaces;
using Modio.API.SchemaDefinitions;
using Modio.Errors;
using Modio.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modio.API.HttpClient;

public class ModioAPIHttpClient : IModioAPIInterface, IDisposable
{
	private readonly System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient();

	private readonly List<string> _defaultParameters = new List<string>();

	private readonly Dictionary<string, string> _pathParameters = new Dictionary<string, string>();

	private string _basePath = string.Empty;

	private CancellationTokenSource _cancellationTokenSource;

	private Error CancelledOrShutDownError(CancellationToken shutdownCancellationToken)
	{
		return new Error((ErrorCode)(shutdownCancellationToken.IsCancellationRequested ? (-2147483588) : (-2147483598)));
	}

	public void SetBasePath(string value)
	{
		_basePath = value;
	}

	public void AddDefaultPathParameter(string key, string value)
	{
		_pathParameters.Add(key, value);
	}

	public void RemoveDefaultPathParameter(string key)
	{
		_pathParameters.Remove(key);
	}

	public void SetDefaultHeader(string name, string value)
	{
		RemoveDefaultHeader(name);
		_client.DefaultRequestHeaders.Add(name, value);
	}

	public void RemoveDefaultHeader(string name)
	{
		_client.DefaultRequestHeaders.Remove(name);
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
		_basePath = string.Empty;
		_client.DefaultRequestHeaders.Clear();
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
		HttpMethod method = MapMethod(modioAPIRequest.Method);
		HttpContent content = MapContent(modioAPIRequest);
		string target = BuildPath(modioAPIRequest);
		HttpRequestMessage httpRequest = new HttpRequestMessage(method, target)
		{
			Content = content
		};
		error = EnforceAuthentication(modioAPIRequest, httpRequest);
		if (!httpRequest.Headers.UserAgent.TryParseAdd(Version.GetCurrent()))
		{
			ModioLog.Error?.Log("Failed to set user agent to " + Version.GetCurrent());
		}
		if ((bool)error)
		{
			return (error, null);
		}
		await LogRequest(httpRequest);
		CancellationToken cachedShutdownToken = _cancellationTokenSource?.Token ?? CancellationToken.None;
		Stream stream;
		try
		{
			if (token == default(CancellationToken))
			{
				token = cachedShutdownToken;
			}
			HttpResponseMessage response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, token);
			stream = await response.Content.ReadAsStreamAsync();
			if (response.StatusCode < HttpStatusCode.OK || response.StatusCode >= HttpStatusCode.MultipleChoices)
			{
				using (StreamReader streamReader = new StreamReader(stream))
				{
					error = await GetErrorAndLogBadResponse(streamReader);
					cachedShutdownToken.ThrowIfCancellationRequested();
					return (error, null);
				}
			}
			cachedShutdownToken.ThrowIfCancellationRequested();
			if (ModioAPI.IsOffline)
			{
				ModioAPI.SetOfflineStatus(isOffline: false);
			}
		}
		catch (HttpRequestException arg)
		{
			ModioLog.Error?.Log($"Can't reach mod.io servers for {target}: {arg}");
			ModioAPI.SetOfflineStatus(isOffline: true);
			return (new HttpError(HttpErrorCode.CANNOT_OPEN_CONNECTION), null);
		}
		catch (TaskCanceledException)
		{
			return (CancelledOrShutDownError(cachedShutdownToken), null);
		}
		catch (OperationCanceledException)
		{
			return (CancelledOrShutDownError(cachedShutdownToken), null);
		}
		catch (Exception ex3)
		{
			if (ex3.InnerException is TaskCanceledException)
			{
				return (CancelledOrShutDownError(cachedShutdownToken), null);
			}
			ModioLog.Error?.Log($"Exception in {target}: {ex3}");
			return (new ErrorException(ex3), null);
		}
		return (Error.None, stream);
	}

	private static Error EnforceAuthentication(ModioAPIRequest downloadRequest, HttpRequestMessage httpRequest)
	{
		if (downloadRequest.Options.RequiresAuthentication && !User.Current.IsAuthenticated)
		{
			return new Error(ErrorCode.USER_NOT_AUTHENTICATED);
		}
		if (User.Current.IsAuthenticated)
		{
			httpRequest.Headers.Add("Authorization", "Bearer " + User.Current?.GetAuthToken());
		}
		return Error.None;
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

	private HttpContent MapContent(ModioAPIRequest request)
	{
		if (request.Method == ModioAPIRequestMethod.Get)
		{
			return null;
		}
		switch (request.ContentType)
		{
		case ModioAPIRequestContentType.None:
		{
			StringContent stringContent = new StringContent(string.Empty);
			stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
			return stringContent;
		}
		case ModioAPIRequestContentType.FormUrlEncoded:
			return new FormUrlEncodedContent(request.Options.FormParameters);
		case ModioAPIRequestContentType.MultipartFormData:
			return PrepareMultipartFormDataContent(request.Options);
		case ModioAPIRequestContentType.ByteArray:
			return PrepareByteArray(request.Options);
		case ModioAPIRequestContentType.String:
			if (request.ContentTypeHint == "application/json")
			{
				ByteArrayContent byteArrayContent = new ByteArrayContent(request.Options.BodyDataBytes);
				byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				return byteArrayContent;
			}
			break;
		}
		throw new NotImplementedException();
	}

	private HttpMethod MapMethod(ModioAPIRequestMethod method)
	{
		return method switch
		{
			ModioAPIRequestMethod.Post => HttpMethod.Post, 
			ModioAPIRequestMethod.Put => HttpMethod.Put, 
			ModioAPIRequestMethod.Get => HttpMethod.Get, 
			ModioAPIRequestMethod.Delete => HttpMethod.Delete, 
			_ => throw new NotImplementedException(), 
		};
	}

	private async Task<(Error error, T)> GetJson<T>(ModioAPIRequest request, Func<JsonTextReader, Task<T>> reader)
	{
		string target = BuildPath(request);
		Error error = await CheckFakeErrorsForTest(target);
		if ((bool)error)
		{
			return (error: error, default(T));
		}
		CancellationToken cachedShutdownToken = _cancellationTokenSource?.Token ?? CancellationToken.None;
		try
		{
			HttpMethod method = MapMethod(request.Method);
			HttpContent content = MapContent(request);
			HttpRequestMessage httpRequest = new HttpRequestMessage(method, target)
			{
				Content = content
			};
			error = EnforceAuthentication(request, httpRequest);
			if ((bool)error)
			{
				return (error: error, default(T));
			}
			if (!httpRequest.Headers.UserAgent.TryParseAdd(Version.GetCurrent()))
			{
				ModioLog.Error?.Log("Failed to set user agent to " + Version.GetCurrent());
			}
			foreach (KeyValuePair<string, string> headerParameter in request.Options.HeaderParameters)
			{
				if (headerParameter.Key == "Content-Range")
				{
					if (httpRequest.Content != null)
					{
						httpRequest.Content.Headers.Add(headerParameter.Key, headerParameter.Value);
					}
				}
				else
				{
					httpRequest.Headers.Add(headerParameter.Key, headerParameter.Value);
				}
			}
			await LogRequest(httpRequest);
			HttpResponseMessage response = await _client.SendAsync(httpRequest, cachedShutdownToken);
			if (response.StatusCode == HttpStatusCode.NoContent)
			{
				cachedShutdownToken.ThrowIfCancellationRequested();
				return (error: Error.None, (T)(object)default(Response204));
			}
			ModioLog.Verbose?.Log(await response.Content.ReadAsStringAsync());
			(Error, T) result2;
			await using (Stream stream = await response.Content.ReadAsStreamAsync())
			{
				using StreamReader streamReader = new StreamReader(stream);
				if (response.StatusCode < HttpStatusCode.OK || response.StatusCode >= HttpStatusCode.MultipleChoices)
				{
					Error item = await GetErrorAndLogBadResponse(streamReader);
					cachedShutdownToken.ThrowIfCancellationRequested();
					result2 = ((response.StatusCode != HttpStatusCode.TooManyRequests || !response.Headers.TryGetValues("retry-after", out var values) || !int.TryParse(values.First(), out var result)) ? (item, default(T)) : (new RateLimitError(RateLimitErrorCode.RATELIMITED, result), default(T)));
				}
				else
				{
					if (ModioAPI.IsOffline)
					{
						ModioAPI.SetOfflineStatus(isOffline: false);
					}
					using JsonTextReader jsonTextReader = new JsonTextReader(streamReader);
					T item2 = await reader(jsonTextReader);
					result2 = (Error.None, item2);
				}
			}
			return result2;
		}
		catch (HttpRequestException arg)
		{
			ModioLog.Error?.Log($"Can't reach mod.io servers for {target}: {arg}");
			ModioAPI.SetOfflineStatus(isOffline: true);
			return (error: new HttpError(HttpErrorCode.CANNOT_OPEN_CONNECTION), default(T));
		}
		catch (TaskCanceledException)
		{
			return (error: CancelledOrShutDownError(cachedShutdownToken), default(T));
		}
		catch (OperationCanceledException)
		{
			return (error: CancelledOrShutDownError(cachedShutdownToken), default(T));
		}
		catch (Exception ex3)
		{
			Exception innerException = ex3.InnerException;
			if (innerException is TaskCanceledException || innerException is OperationCanceledException)
			{
				return (error: CancelledOrShutDownError(cachedShutdownToken), default(T));
			}
			ModioLog.Error?.Log($"Exception in {target}: {ex3}");
			return (error: new ErrorException(ex3), default(T));
		}
	}

	private static async Task<Error> GetErrorAndLogBadResponse(StreamReader streamReader)
	{
		TextReader textReader = null;
		if (streamReader.Peek() != 123)
		{
			string text = await streamReader.ReadToEndAsync();
			int num = text.IndexOf('{');
			if (num > 0)
			{
				string text2 = text.Substring(0, num);
				ModioLog.Error?.Log("Unexpected error from server before JSON: " + text2);
				text = text.Substring(num);
			}
			else if (num == -1)
			{
				if (text == "File Not Found")
				{
					return new Error(ErrorCode.FILE_NOT_FOUND);
				}
				ModioLog.Error?.Log("Unexpected error from server instead of JSON: " + text);
				return new Error(ErrorCode.INVALID_JSON);
			}
			textReader = new StringReader(text);
		}
		ErrorObject errorObject;
		try
		{
			using JsonTextReader reader = new JsonTextReader(textReader ?? streamReader);
			errorObject = new JsonSerializer().Deserialize<ErrorObject>(reader);
		}
		catch (JsonException)
		{
			ModioLog.Error?.Log("There is an error with the json response.");
			return new Error(ErrorCode.INVALID_JSON);
		}
		if (errorObject.Error.ErrorRef == 0L)
		{
			ModioLog.Error?.Log("Invalid error returned from API, please contact mod.io support");
			return new Error(ErrorCode.UNKNOWN);
		}
		textReader?.Dispose();
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

	private string BuildPath(ModioAPIRequest request)
	{
		string text = request.GetUri(_defaultParameters);
		if (!text.StartsWith("https://"))
		{
			text = _basePath + text;
		}
		StringBuilder stringBuilder = new StringBuilder(text);
		foreach (KeyValuePair<string, string> pathParameter in _pathParameters)
		{
			stringBuilder.Replace("{" + pathParameter.Key + "}", pathParameter.Value);
		}
		return stringBuilder.ToString();
	}

	private static HttpContent PrepareMultipartFormDataContent(ModioAPIRequestOptions options)
	{
		MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
		foreach (KeyValuePair<string, string> formParameter in options.FormParameters)
		{
			multipartFormDataContent.Add(new StringContent(formParameter.Value), formParameter.Key);
		}
		foreach (KeyValuePair<string, ModioAPIFileParameter> fileParameter in options.FileParameters)
		{
			if (fileParameter.Value.Unused)
			{
				continue;
			}
			Stream content = fileParameter.Value.GetContent();
			if (content != null)
			{
				StreamContent streamContent = new StreamContent(content);
				if (fileParameter.Value.ContentType != null)
				{
					streamContent.Headers.ContentType = new MediaTypeHeaderValue(fileParameter.Value.ContentType);
				}
				if (fileParameter.Value.Name != null)
				{
					multipartFormDataContent.Add(streamContent, fileParameter.Key, fileParameter.Value.Name);
				}
				else
				{
					multipartFormDataContent.Add(streamContent, fileParameter.Key);
				}
			}
		}
		return multipartFormDataContent;
	}

	private static HttpContent PrepareByteArray(ModioAPIRequestOptions options)
	{
		ByteArrayContent byteArrayContent = new ByteArrayContent(options.BodyDataBytes);
		byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
		return byteArrayContent;
	}

	private async Task LogRequest(HttpRequestMessage request)
	{
		if (ModioLog.Verbose == null || request == null)
		{
			return;
		}
		StringBuilder builder = new StringBuilder();
		builder.AppendLine($"{request.Method} {request.RequestUri} HTTP/{request.Version}");
		foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
		{
			builder.AppendLine(string.Equals(header.Key, "Authorization") ? (header.Key + ": Bearer (omitted)") : (header.Key + ": " + string.Join(", ", header.Value)));
		}
		foreach (KeyValuePair<string, IEnumerable<string>> defaultRequestHeader in _client.DefaultRequestHeaders)
		{
			builder.AppendLine(defaultRequestHeader.Key + ": " + string.Join(", ", defaultRequestHeader.Value));
		}
		if (request.Content != null)
		{
			foreach (KeyValuePair<string, IEnumerable<string>> header2 in request.Content.Headers)
			{
				builder.AppendLine(header2.Key + ": " + string.Join(", ", header2.Value));
			}
			builder.AppendLine();
			StringBuilder stringBuilder = builder;
			stringBuilder.Append(await request.Content.ReadAsStringAsync());
		}
		ModioLog.Verbose.Log(builder.ToString());
	}

	public void Dispose()
	{
		_client.Dispose();
	}
}
