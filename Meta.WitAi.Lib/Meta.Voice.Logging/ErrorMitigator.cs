using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Lib.Wit.Runtime.Utilities.Logging;
using UnityEngine;

namespace Meta.Voice.Logging;

[LogCategory(LogCategory.Logging, LogCategory.ErrorMitigator)]
public class ErrorMitigator : IErrorMitigator, ILogSource
{
	private readonly Dictionary<ErrorCode, string> _mitigations = new Dictionary<ErrorCode, string>();

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.ErrorMitigator);


	public ErrorMitigator()
	{
		try
		{
			foreach (KnownErrorCode value in Enum.GetValues(typeof(KnownErrorCode)))
			{
				DescriptionAttribute customAttribute = typeof(KnownErrorCode).GetField(value.ToString()).GetCustomAttribute<DescriptionAttribute>();
				if (customAttribute != null)
				{
					_mitigations[value] = customAttribute.Description;
					continue;
				}
				Logger.Error(KnownErrorCode.KnownErrorMissingDescription, "Missing error description for {0}", value);
				_mitigations[value] = "Please file a bug report.";
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to get known error mitigations. Exception: {arg}");
		}
	}

	public string GetMitigation(ErrorCode errorCode)
	{
		if (_mitigations.ContainsKey(errorCode))
		{
			return _mitigations[errorCode];
		}
		return "There are no known mitigations. Please report to the Voice SDK team.";
	}

	public void SetMitigation(ErrorCode errorCode, string mitigation)
	{
		_mitigations[errorCode] = mitigation;
	}
}
