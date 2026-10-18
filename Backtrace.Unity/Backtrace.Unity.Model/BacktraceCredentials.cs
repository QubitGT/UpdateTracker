using System;

namespace Backtrace.Unity.Model;

public class BacktraceCredentials
{
	public Uri BacktraceHostUri { get; private set; }

	public Uri GetSubmissionUrl()
	{
		UriBuilder uriBuilder = new UriBuilder(BacktraceHostUri);
		if (!uriBuilder.Scheme.StartsWith("http"))
		{
			uriBuilder.Scheme = $"https://{uriBuilder.Scheme}";
		}
		return uriBuilder.Uri;
	}

	public Uri GetPlCrashReporterSubmissionUrl()
	{
		string text = GetSubmissionUrl().ToString();
		return new UriBuilder((text.IndexOf("submit.backtrace.io") != -1) ? text.Replace("/json", "/plcrash") : text.Replace("format=json", "format=plcrash")).Uri;
	}

	public Uri GetMinidumpSubmissionUrl()
	{
		string text = GetSubmissionUrl().ToString();
		return new UriBuilder((text.IndexOf("submit.backtrace.io") != -1) ? text.Replace("/json", "/minidump") : text.Replace("format=json", "format=minidump")).Uri;
	}

	public Uri GetSymbolsSubmissionUrl(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			throw new ArgumentException("Empty symbols submission token");
		}
		string text = GetSubmissionUrl().ToString();
		if (text.IndexOf("submit.backtrace.io") != -1)
		{
			text = text.Replace("/json", "/symbols");
			int startIndex = text.LastIndexOf("/") - 64;
			string oldValue = text.Substring(startIndex, 64);
			text = text.Replace(oldValue, token);
		}
		else
		{
			text = text.Replace("format=json", "format=symbols");
			int num = text.IndexOf("token=");
			if (num == -1)
			{
				throw new ArgumentException("Missing token in Backtrace url");
			}
			string oldValue2 = text.Substring(num + "token=".Length, 64);
			text = text.Replace(oldValue2, token);
		}
		return new UriBuilder(text).Uri;
	}

	public BacktraceCredentials(string backtraceSubmitUrl)
		: this(new Uri(backtraceSubmitUrl))
	{
	}

	public BacktraceCredentials(Uri backtraceSubmitUrl)
	{
		BacktraceHostUri = backtraceSubmitUrl;
	}

	internal bool IsValid(Uri uri, byte[] token)
	{
		if (token != null && token.Length != 0)
		{
			return uri.IsWellFormedOriginalString();
		}
		return false;
	}
}
