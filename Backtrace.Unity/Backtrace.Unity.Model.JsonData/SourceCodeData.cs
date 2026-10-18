using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Backtrace.Unity.Model.JsonData;

public class SourceCodeData
{
	public class SourceCode
	{
		public int StartLine { get; set; }

		public int StartColumn { get; set; }

		private string _sourceCodeFullPath { get; set; }

		public string SourceCodeFullPath
		{
			get
			{
				if (!string.IsNullOrEmpty(_sourceCodeFullPath))
				{
					return Path.GetFileName(_sourceCodeFullPath);
				}
				return string.Empty;
			}
			set
			{
				_sourceCodeFullPath = value;
			}
		}

		public static SourceCode FromExceptionStack(BacktraceStackFrame stackFrame)
		{
			return new SourceCode
			{
				StartColumn = stackFrame.Column,
				StartLine = stackFrame.Line,
				SourceCodeFullPath = stackFrame.SourceCodeFullPath
			};
		}
	}

	public Dictionary<string, SourceCode> data = new Dictionary<string, SourceCode>();

	internal SourceCodeData(IEnumerable<BacktraceStackFrame> exceptionStack)
	{
		if (exceptionStack == null || exceptionStack.Count() == 0)
		{
			return;
		}
		foreach (BacktraceStackFrame item in exceptionStack)
		{
			if (!string.IsNullOrEmpty(item.SourceCode))
			{
				string sourceCode = item.SourceCode;
				SourceCode value = SourceCode.FromExceptionStack(item);
				data.Add(sourceCode, value);
			}
		}
	}
}
