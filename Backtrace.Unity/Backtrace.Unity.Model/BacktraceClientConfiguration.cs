using System;
using Backtrace.Unity.Types;
using UnityEngine;

namespace Backtrace.Unity.Model;

[Serializable]
public class BacktraceClientConfiguration : ScriptableObject
{
	public string ServerUrl;

	public int ReportPerMin;

	public bool HandleUnhandledExceptions = true;

	public bool IgnoreSslValidation;

	public bool DestroyOnLoad = true;

	public bool HandleANR = true;

	public bool OomReports;

	public int GameObjectDepth;

	public MiniDumpType MinidumpType = MiniDumpType.None;

	public void UpdateServerUrl()
	{
		if (string.IsNullOrEmpty(ServerUrl) || !Uri.TryCreate(ServerUrl, UriKind.RelativeOrAbsolute, out var _))
		{
			return;
		}
		try
		{
			ServerUrl = new UriBuilder(ServerUrl)
			{
				Scheme = Uri.UriSchemeHttps,
				Port = 6098
			}.Uri.ToString();
		}
		catch (Exception)
		{
			Debug.LogWarning("Invalid Backtrace URL");
		}
	}

	public bool ValidateServerUrl()
	{
		if (!ServerUrl.Contains("backtrace.io") && !ServerUrl.Contains("submit.backtrace.io"))
		{
			return false;
		}
		Uri result;
		bool result2 = Uri.TryCreate(ServerUrl, UriKind.RelativeOrAbsolute, out result);
		try
		{
			UriBuilder uriBuilder = new UriBuilder(ServerUrl);
			uriBuilder.Scheme = Uri.UriSchemeHttps;
			uriBuilder.Port = 6098;
			uriBuilder.Uri.ToString();
			return result2;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
