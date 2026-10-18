using System;
using System.Collections.Generic;
using Backtrace.Unity.Common;
using Backtrace.Unity.Extensions;

namespace Backtrace.Unity.Model;

public class BacktraceReport
{
	private const string ErrorTypeAttributeName = "error.type";

	public readonly Guid Uuid = Guid.NewGuid();

	public readonly long Timestamp = DateTimeHelper.Timestamp();

	public readonly bool ExceptionTypeReport;

	public string Classifier = string.Empty;

	public BacktraceSourceCode SourceCode;

	public string Fingerprint { get; set; }

	public string Factor { get; set; }

	public Dictionary<string, string> Attributes { get; private set; }

	public string Message { get; private set; }

	public Exception Exception { get; private set; }

	public List<string> AttachmentPaths { get; set; }

	public List<BacktraceStackFrame> DiagnosticStack { get; set; }

	public string Symbolication { get; set; }

	public BacktraceReport(string message, Dictionary<string, string> attributes = null, List<string> attachmentPaths = null)
		: this((Exception)null, attributes, attachmentPaths)
	{
		Message = message;
		SetStacktraceInformation();
		SetDefaultAttributes();
	}

	public BacktraceReport(Exception exception, Dictionary<string, string> attributes = null, List<string> attachmentPaths = null)
	{
		Attributes = attributes ?? new Dictionary<string, string>();
		AttachmentPaths = attachmentPaths ?? new List<string>();
		Exception = exception;
		ExceptionTypeReport = exception != null;
		if (ExceptionTypeReport)
		{
			Message = exception.Message;
			SetClassifierInfo();
			SetStacktraceInformation();
		}
		SetDefaultAttributes();
	}

	public void UseSymbolication(string symbolication)
	{
		Symbolication = symbolication;
	}

	private void SetDefaultAttributes()
	{
		Attributes["error.message"] = Message;
		if (!Attributes.ContainsKey("error.type"))
		{
			Attributes["error.type"] = "Message";
		}
	}

	internal void AssignSourceCodeToReport(string text)
	{
		if (DiagnosticStack == null || DiagnosticStack.Count == 0)
		{
			return;
		}
		SourceCode = new BacktraceSourceCode
		{
			Text = text
		};
		foreach (BacktraceStackFrame item in DiagnosticStack)
		{
			item.SourceCode = BacktraceSourceCode.SOURCE_CODE_PROPERTY;
		}
	}

	private void SetClassifierInfo()
	{
		if (!ExceptionTypeReport)
		{
			Classifier = string.Empty;
			Attributes["error.type"] = "Message";
		}
		if (Exception is BacktraceUnhandledException)
		{
			Classifier = (Exception as BacktraceUnhandledException).Classifier;
			string classifier = Classifier;
			if (!(classifier == "ANRException"))
			{
				if (classifier == "OOMException")
				{
					Attributes["error.type"] = "OOMException";
				}
				else
				{
					Attributes["error.type"] = "Unhandled exception";
				}
			}
			else
			{
				Attributes["error.type"] = "Hang";
			}
		}
		else
		{
			Attributes["error.type"] = "Exception";
			Classifier = Exception.GetType().Name;
		}
	}

	internal void SetReportFingerprint(bool generateFingerprint)
	{
		if (generateFingerprint && ((Exception != null && string.IsNullOrEmpty(Exception.StackTrace)) || DiagnosticStack == null || DiagnosticStack.Count == 0))
		{
			string value = (string.IsNullOrEmpty(Message) ? "0000000000000000000000000000000000000000000000000000000000000000" : Message.OnlyLetters().GetSha());
			Attributes["_mod_fingerprint"] = value;
		}
		if (!string.IsNullOrEmpty(Factor))
		{
			Attributes["_mod_factor"] = Factor;
		}
		if (!string.IsNullOrEmpty(Fingerprint))
		{
			Attributes["_mod_fingerprint"] = Fingerprint;
		}
	}

	internal BacktraceData ToBacktraceData(Dictionary<string, string> clientAttributes, int gameObjectDepth)
	{
		return new BacktraceData(this, clientAttributes, gameObjectDepth);
	}

	internal void SetStacktraceInformation()
	{
		BacktraceStackTrace backtraceStackTrace = new BacktraceStackTrace(Exception);
		DiagnosticStack = backtraceStackTrace.StackFrames;
	}

	internal BacktraceReport CreateInnerReport()
	{
		if (!ExceptionTypeReport || Exception.InnerException == null)
		{
			return null;
		}
		BacktraceReport obj = (BacktraceReport)MemberwiseClone();
		obj.Exception = Exception.InnerException;
		obj.SetStacktraceInformation();
		obj.Classifier = obj.Exception.GetType().Name;
		return obj;
	}
}
