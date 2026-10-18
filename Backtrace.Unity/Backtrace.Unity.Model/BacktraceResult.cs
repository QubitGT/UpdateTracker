using System;
using Backtrace.Unity.Types;
using UnityEngine;

namespace Backtrace.Unity.Model;

public class BacktraceResult
{
	[Serializable]
	private class BacktraceRawResult
	{
		public string response;

		public string _rxid;
	}

	public BacktraceResult InnerExceptionResult;

	public string message;

	public string response;

	public BacktraceResultStatus Status = BacktraceResultStatus.Ok;

	private string @object;

	public string _rxId;

	public string Message
	{
		get
		{
			return message;
		}
		set
		{
			message = value;
		}
	}

	public string Object
	{
		get
		{
			return @object;
		}
		set
		{
			@object = value;
			Status = BacktraceResultStatus.Ok;
		}
	}

	public string RxId
	{
		get
		{
			return _rxId;
		}
		set
		{
			_rxId = value;
			Status = BacktraceResultStatus.Ok;
		}
	}

	internal static BacktraceResult OnLimitReached()
	{
		return new BacktraceResult
		{
			Status = BacktraceResultStatus.LimitReached,
			Message = "Client report limit reached"
		};
	}

	internal static BacktraceResult OnNetworkError(Exception exception)
	{
		return new BacktraceResult
		{
			Message = exception.Message,
			Status = BacktraceResultStatus.NetworkError
		};
	}

	internal void AddInnerResult(BacktraceResult innerResult)
	{
		if (InnerExceptionResult == null)
		{
			InnerExceptionResult = innerResult;
		}
		else
		{
			InnerExceptionResult.AddInnerResult(innerResult);
		}
	}

	public static BacktraceResult FromJson(string json)
	{
		BacktraceResult backtraceResult = new BacktraceResult
		{
			Status = (string.IsNullOrEmpty(json) ? BacktraceResultStatus.Empty : BacktraceResultStatus.Ok)
		};
		if (backtraceResult.Status == BacktraceResultStatus.Empty)
		{
			return backtraceResult;
		}
		try
		{
			BacktraceRawResult backtraceRawResult = JsonUtility.FromJson<BacktraceRawResult>(json);
			backtraceResult.response = backtraceRawResult.response;
			backtraceResult._rxId = backtraceRawResult._rxid;
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"Cannot parse Backtrace JSON response. Error: {json}. Content: {ex.Message}");
		}
		return backtraceResult;
	}
}
