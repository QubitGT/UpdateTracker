using System;
using System.Collections.Generic;
using System.Diagnostics;
using Backtrace.Unity.Types;

namespace Backtrace.Unity.Model;

internal class BacktraceStackTrace
{
	public readonly List<BacktraceStackFrame> StackFrames = new List<BacktraceStackFrame>();

	private readonly Exception _exception;

	public BacktraceStackTrace(Exception exception)
	{
		_exception = exception;
		Initialize();
	}

	private void Initialize()
	{
		bool generatedByException = _exception != null;
		if (_exception != null)
		{
			if (_exception is BacktraceUnhandledException)
			{
				BacktraceUnhandledException ex = _exception as BacktraceUnhandledException;
				StackFrames.InsertRange(0, ex.StackFrames);
				return;
			}
			StackFrame[] frames = new StackTrace(_exception, fNeedFileInfo: true).GetFrames();
			if (frames == null || frames.Length == 0)
			{
				frames = new StackTrace(fNeedFileInfo: true).GetFrames();
			}
			SetStacktraceInformation(frames, generatedByException: true);
		}
		else
		{
			StackFrame[] frames2 = new StackTrace(fNeedFileInfo: true).GetFrames();
			SetStacktraceInformation(frames2, generatedByException);
		}
	}

	private void SetStacktraceInformation(StackFrame[] frames, bool generatedByException = false)
	{
		if (frames == null || frames.Length == 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < frames.Length; i++)
		{
			BacktraceStackFrame backtraceStackFrame = new BacktraceStackFrame(frames[i], generatedByException);
			if (!backtraceStackFrame.InvalidFrame)
			{
				backtraceStackFrame.StackFrameType = BacktraceStackFrameType.Dotnet;
				StackFrames.Insert(num, backtraceStackFrame);
				num++;
			}
		}
	}
}
