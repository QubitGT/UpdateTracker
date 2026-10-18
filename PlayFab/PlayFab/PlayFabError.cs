using System;
using System.Collections.Generic;
using System.Text;

namespace PlayFab;

public class PlayFabError
{
	public string ApiEndpoint;

	public int HttpCode;

	public string HttpStatus;

	public PlayFabErrorCode Error;

	public string ErrorMessage;

	public Dictionary<string, List<string>> ErrorDetails;

	public object CustomData;

	[ThreadStatic]
	private static StringBuilder _tempSb;

	public override string ToString()
	{
		return GenerateErrorReport();
	}

	public string GenerateErrorReport()
	{
		if (_tempSb == null)
		{
			_tempSb = new StringBuilder();
		}
		_tempSb.Length = 0;
		if (string.IsNullOrEmpty(ErrorMessage))
		{
			_tempSb.Append(ApiEndpoint).Append(": ").Append("Http Code: ")
				.Append(HttpCode.ToString())
				.Append("\nHttp Status: ")
				.Append(HttpStatus)
				.Append("\nError: ")
				.Append(Error.ToString())
				.Append("\n");
		}
		else
		{
			_tempSb.Append(ApiEndpoint).Append(": ").Append(ErrorMessage);
		}
		if (ErrorDetails != null)
		{
			foreach (KeyValuePair<string, List<string>> errorDetail in ErrorDetails)
			{
				foreach (string item in errorDetail.Value)
				{
					_tempSb.Append("\n").Append(errorDetail.Key).Append(": ")
						.Append(item);
				}
			}
		}
		return _tempSb.ToString();
	}
}
