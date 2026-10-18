using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Backtrace.Unity.Model;

internal class BacktraceUnityMessage
{
	private readonly string _formattedMessage;

	public readonly string Message;

	public readonly string StackTrace;

	public readonly LogType Type;

	public BacktraceUnityMessage(BacktraceReport report)
	{
		if (report == null)
		{
			throw new ArgumentException("report");
		}
		Message = report.Message;
		if (report.ExceptionTypeReport)
		{
			Type = LogType.Exception;
			StackTrace = GetFormattedStackTrace(report.Exception.StackTrace);
			_formattedMessage = GetFormattedMessage(backtraceFrame: true);
		}
		else
		{
			StackTrace = string.Empty;
			Type = LogType.Warning;
			_formattedMessage = GetFormattedMessage(backtraceFrame: true);
		}
	}

	public BacktraceUnityMessage(string message, string stacktrace, LogType type)
	{
		Message = message;
		StackTrace = GetFormattedStackTrace(stacktrace);
		Type = type;
		_formattedMessage = GetFormattedMessage(backtraceFrame: false);
	}

	private string GetFormattedMessage(bool backtraceFrame)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("[{0}] {1}<{2}>: {3}", DateTime.Now.ToUniversalTime().ToString(CultureInfo.InvariantCulture), backtraceFrame ? "(Backtrace)" : string.Empty, Enum.GetName(typeof(LogType), Type), Message);
		if (IsUnhandledException())
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(string.IsNullOrEmpty(StackTrace) ? "No stack trace available" : StackTrace);
		}
		return stringBuilder.ToString();
	}

	private string GetFormattedStackTrace(string stacktrace)
	{
		if (string.IsNullOrEmpty(stacktrace) || !stacktrace.EndsWith("\n"))
		{
			return stacktrace;
		}
		return stacktrace.Remove(stacktrace.LastIndexOf("\n"));
	}

	public bool IsUnhandledException()
	{
		if (Type == LogType.Exception || Type == LogType.Error)
		{
			return !string.IsNullOrEmpty(Message);
		}
		return false;
	}

	public override string ToString()
	{
		return _formattedMessage;
	}
}
