using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Backtrace.Unity.Common;
using Backtrace.Unity.Extensions;
using Backtrace.Unity.Interfaces;
using Backtrace.Unity.Model;
using Backtrace.Unity.Types;
using UnityEngine;
using UnityEngine.Networking;

namespace Backtrace.Unity.Services;

internal class BacktraceApi : IBacktraceApi
{
	private BacktraceHttpClient _httpClient = new BacktraceHttpClient();

	private bool _shouldDisplayFailureMessage = true;

	private readonly Uri _serverUrl;

	private readonly string _minidumpUrl;

	private readonly BacktraceCredentials _credentials;

	[Obsolete("RequestHandler is obsolete. BacktraceApi won't be able to provide BacktraceData in every situation")]
	public Func<string, BacktraceData, BacktraceResult> RequestHandler { get; set; }

	public Action<Exception> OnServerError { get; set; }

	public Action<BacktraceResult> OnServerResponse { get; set; }

	public bool EnablePerformanceStatistics { get; set; }

	public string ServerUrl => _serverUrl.ToString();

	public BacktraceApi(BacktraceCredentials credentials, bool ignoreSslValidation = false)
	{
		_credentials = credentials;
		if (_credentials == null)
		{
			throw new ArgumentException(string.Format("{0} cannot be null", "BacktraceCredentials"));
		}
		_serverUrl = credentials.GetSubmissionUrl();
		_minidumpUrl = credentials.GetMinidumpSubmissionUrl().ToString();
		_httpClient.IgnoreSslValidation = ignoreSslValidation;
		EnablePerformanceStatistics = false;
	}

	public IEnumerator SendMinidump(string minidumpPath, IEnumerable<string> attachments, IDictionary<string, string> attributes, Action<BacktraceResult> callback = null)
	{
		if (attachments == null)
		{
			attachments = new HashSet<string>();
		}
		Stopwatch stopWatch = (EnablePerformanceStatistics ? Stopwatch.StartNew() : new Stopwatch());
		byte[] array = File.ReadAllBytes(minidumpPath);
		if (array == null || array.Length == 0)
		{
			yield break;
		}
		using UnityWebRequest request = _httpClient.Post(_minidumpUrl, array, attachments, attributes);
		yield return request.SendWebRequest();
		BacktraceResult backtraceResult = (request.ReceivedNetworkError() ? new BacktraceResult
		{
			Message = request.error,
			Status = BacktraceResultStatus.ServerError
		} : BacktraceResult.FromJson(request.downloadHandler.text));
		callback?.Invoke(backtraceResult);
		if (EnablePerformanceStatistics)
		{
			stopWatch.Stop();
			UnityEngine.Debug.Log($"Backtrace - minidump send time: {stopWatch.GetMicroseconds()}μs");
		}
		yield return backtraceResult;
	}

	public IEnumerator Send(BacktraceData data, Action<BacktraceResult> callback = null)
	{
		if (RequestHandler != null)
		{
			yield return RequestHandler(ServerUrl, data);
		}
		else if (data != null)
		{
			string json = data.ToJson();
			yield return Send(json, data.Attachments, data.Deduplication, callback);
		}
	}

	public IEnumerator Send(string json, IEnumerable<string> attachments, int deduplication, Action<BacktraceResult> callback)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (deduplication > 0)
		{
			dictionary["_mod_duplicate"] = deduplication.ToString(CultureInfo.InvariantCulture);
		}
		yield return Send(json, attachments, dictionary, callback);
	}

	public IEnumerator Send(string json, IEnumerable<string> attachments, Dictionary<string, string> attributes, Action<BacktraceResult> callback)
	{
		Stopwatch stopWatch = (EnablePerformanceStatistics ? Stopwatch.StartNew() : new Stopwatch());
		using UnityWebRequest request = _httpClient.Post(ServerUrl, json, attachments, attributes);
		yield return request.SendWebRequest();
		BacktraceResult backtraceResult;
		if (request.responseCode == 429)
		{
			backtraceResult = new BacktraceResult
			{
				Message = "Server report limit reached",
				Status = BacktraceResultStatus.LimitReached
			};
			if (OnServerResponse != null)
			{
				OnServerResponse(backtraceResult);
			}
		}
		else if (request.responseCode == 200 && !request.ReceivedNetworkError())
		{
			backtraceResult = BacktraceResult.FromJson(request.downloadHandler.text);
			_shouldDisplayFailureMessage = true;
			if (OnServerResponse != null)
			{
				OnServerResponse(backtraceResult);
			}
		}
		else
		{
			PrintLog(request);
			Exception ex = new Exception(request.error);
			backtraceResult = BacktraceResult.OnNetworkError(ex);
			if (OnServerError != null)
			{
				OnServerError(ex);
			}
		}
		callback?.Invoke(backtraceResult);
		if (EnablePerformanceStatistics)
		{
			stopWatch.Stop();
		}
		yield return backtraceResult;
	}

	private void PrintLog(UnityWebRequest request)
	{
		if (_shouldDisplayFailureMessage)
		{
			_shouldDisplayFailureMessage = false;
			UnityEngine.Debug.LogWarning(string.Format("{0}{1}", $"[Backtrace]::Reponse code: {request.responseCode}, Response text: {request.error}", "\n Please check provided url to Backtrace service or learn more from our integration guide: https://support.backtrace.io/hc/en-us/articles/360040515991-Unity-Integration-Guide"));
		}
	}
}
