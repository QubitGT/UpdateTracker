using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Meta.WitAi;

namespace Meta.Voice.Logging;

public class LoggingContext
{
	private readonly StackTrace _stackTrace;

	private readonly string _callSiteMemberName;

	private readonly string _callSiteSourceFilePath;

	private readonly int _callSiteSourceLineNumber;

	private readonly string _workingDirectory = Directory.GetCurrentDirectory();

	internal LoggingContext(StackTrace stackTrace)
	{
		_stackTrace = stackTrace;
		_callSiteSourceLineNumber = -1;
	}

	internal LoggingContext([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		_stackTrace = null;
		_callSiteMemberName = memberName;
		_callSiteSourceFilePath = sourceFilePath;
		_callSiteSourceLineNumber = sourceLineNumber;
	}

	public override string ToString()
	{
		return _stackTrace?.ToString();
	}

	private void AppendSingleFrame(StringBuilder sb, bool colorLogs)
	{
		if (_callSiteSourceLineNumber > 0)
		{
			sb.Append("=== STACK TRACE ===\n");
			string callSiteSourceFilePath = _callSiteSourceFilePath;
			sb.Append($"[{callSiteSourceFilePath}:{_callSiteSourceLineNumber}] ");
			sb.Append(colorLogs ? ("<color=#39CC8F>" + _callSiteMemberName + "</color>(?)\n") : (_callSiteMemberName + "(?)\n"));
		}
	}

	private void AppendFullStack(StringBuilder sb, bool colorLogs, StackFrame[] frames)
	{
		sb.Append("=== STACK TRACE ===\n");
		foreach (StackFrame stackFrame in frames)
		{
			MethodBase method = stackFrame.GetMethod();
			Type declaringType = method.DeclaringType;
			if (!(declaringType == null) && !IsLoggingClass(method.DeclaringType) && !IsSystemClass(method.DeclaringType))
			{
				string fileName = stackFrame.GetFileName();
				int fileLineNumber = stackFrame.GetFileLineNumber();
				string text = string.Join(", ", from p in method.GetParameters()
					select p.ParameterType.Name ?? "");
				if (!string.IsNullOrEmpty(fileName))
				{
					string fileName2 = Path.GetFileName(fileName);
					fileName.Replace(_workingDirectory, "");
					sb.Append($"[{fileName2}:{fileLineNumber}] ");
				}
				string text2 = method.Name ?? "";
				sb.Append(declaringType?.Name);
				sb.Append('.');
				sb.Append(colorLogs ? ("<color=#39CC8F>" + method.Name + "</color>") : (text2 ?? ""));
				sb.Append("(" + text + ")\n");
			}
		}
	}

	public void AppendRelevantContext(StringBuilder sb, bool colorLogs)
	{
		StackFrame[] array = _stackTrace?.GetFrames();
		if (array == null)
		{
			AppendSingleFrame(sb, colorLogs);
		}
		else
		{
			AppendFullStack(sb, colorLogs, array);
		}
	}

	public (string fileName, int lineNumber) GetCallSite()
	{
		if (_callSiteSourceLineNumber > 0)
		{
			return (fileName: _callSiteSourceFilePath, lineNumber: _callSiteSourceLineNumber);
		}
		if (_stackTrace == null)
		{
			return (fileName: string.Empty, lineNumber: 0);
		}
		for (int i = 1; i < _stackTrace.FrameCount; i++)
		{
			StackFrame frame = _stackTrace.GetFrame(i);
			MethodBase method = frame.GetMethod();
			if (!(method.DeclaringType == null) && !IsLoggingClass(method.DeclaringType) && !IsSystemClass(method.DeclaringType))
			{
				string item = frame.GetFileName()?.Replace('\\', '/');
				int fileLineNumber = frame.GetFileLineNumber();
				return (fileName: item, lineNumber: fileLineNumber);
			}
		}
		return (fileName: string.Empty, lineNumber: 0);
	}

	private static bool IsLoggingClass(Type type)
	{
		if (!typeof(ICoreLogger).IsAssignableFrom(type) && !typeof(ILogWriter).IsAssignableFrom(type))
		{
			return type == typeof(VLog);
		}
		return true;
	}

	private static bool IsSystemClass(Type type)
	{
		string @namespace = type.Namespace;
		if (@namespace == null)
		{
			return false;
		}
		if (!@namespace.StartsWith("Unity") && !@namespace.StartsWith("System"))
		{
			return @namespace.StartsWith("Microsoft");
		}
		return true;
	}
}
