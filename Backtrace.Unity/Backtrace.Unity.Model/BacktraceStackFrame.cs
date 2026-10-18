using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Backtrace.Unity.Json;
using Backtrace.Unity.Types;

namespace Backtrace.Unity.Model;

public class BacktraceStackFrame
{
	private static string[] _frameSeparators = new string[3] { "::", ":", "." };

	public string FunctionName;

	internal BacktraceStackFrameType StackFrameType;

	public int Line;

	public string MemberInfo;

	public string SourceCodeFullPath;

	public int Column;

	public int ILOffset;

	public string SourceCode;

	public string Address;

	public string Assembly;

	public string Library;

	public string FileName
	{
		get
		{
			if (!string.IsNullOrEmpty(Library))
			{
				if (Library.IndexOfAny(Path.GetInvalidPathChars()) != -1 || !Path.HasExtension(Path.GetFileName(Library)))
				{
					return GetFileNameFromFunctionName();
				}
				return GetFileNameFromLibraryName();
			}
			return GetFileNameFromFunctionName();
		}
	}

	public bool InvalidFrame { get; set; }

	public BacktraceJObject ToJson()
	{
		BacktraceJObject backtraceJObject = new BacktraceJObject(new Dictionary<string, string>
		{
			{ "funcName", FunctionName },
			{ "path", FileName },
			{ "metadata_token", MemberInfo },
			{ "assembly", Assembly }
		});
		backtraceJObject.Add("address", ILOffset);
		if (!string.IsNullOrEmpty(Library) && (!Library.StartsWith("<") || !Library.EndsWith(">")))
		{
			backtraceJObject.Add("library", Library);
		}
		if (Line != 0)
		{
			backtraceJObject.Add("line", Line);
		}
		if (Column != 0)
		{
			backtraceJObject.Add("column", Column);
		}
		if (!string.IsNullOrEmpty(SourceCode))
		{
			backtraceJObject.Add("sourceCode", SourceCode);
		}
		return backtraceJObject;
	}

	public BacktraceStackFrame()
	{
	}

	public BacktraceStackFrame(StackFrame frame, bool generatedByException)
	{
		if (frame == null)
		{
			InvalidFrame = true;
			return;
		}
		MethodBase method = frame.GetMethod();
		if (method == null)
		{
			InvalidFrame = true;
			return;
		}
		Type declaringType = method.DeclaringType;
		string assembly = "unknown";
		if (declaringType != null)
		{
			string name = declaringType.Assembly.GetName().Name;
			if (name != null)
			{
				assembly = name;
				if (name == "Backtrace.Unity")
				{
					InvalidFrame = true;
					return;
				}
			}
		}
		FunctionName = GetMethodName(method);
		SourceCodeFullPath = frame.GetFileName();
		Line = frame.GetFileLineNumber();
		ILOffset = frame.GetILOffset();
		Assembly = assembly;
		Library = (string.IsNullOrEmpty(SourceCodeFullPath) ? method.DeclaringType.ToString() : SourceCodeFullPath);
		Column = frame.GetFileColumnNumber();
		try
		{
			MemberInfo = method.MetadataToken.ToString(CultureInfo.InvariantCulture);
		}
		catch (InvalidOperationException)
		{
		}
		InvalidFrame = false;
	}

	private string GetMethodName(MethodBase method)
	{
		string arg = (method.Name.StartsWith(".") ? method.Name.Substring(1, method.Name.Length - 1) : method.Name);
		return $"{((method.DeclaringType == null) ? null : method.DeclaringType.ToString())}.{arg}()";
	}

	private string GetFileNameFromLibraryName()
	{
		string text = Path.GetFileName(Library).Trim();
		int num = text.LastIndexOf(".");
		if (num == -1 || text.IndexOf(".") == num)
		{
			return text;
		}
		text = text.Substring(num + 1);
		return StackFrameType switch
		{
			BacktraceStackFrameType.Dotnet => $"{text}.cs", 
			BacktraceStackFrameType.Android => $"{text}.java", 
			_ => text, 
		};
	}

	private string GetFileNameFromFunctionName()
	{
		if (string.IsNullOrEmpty(FunctionName))
		{
			return string.Empty;
		}
		int num = FunctionName.IndexOf('(');
		if (num == -1)
		{
			num = FunctionName.Length - 1;
		}
		int num2 = -1;
		for (int i = 0; i < _frameSeparators.Length; i++)
		{
			num2 = FunctionName.LastIndexOf(_frameSeparators[i], num);
			if (num2 != -1)
			{
				break;
			}
		}
		if (num2 == -1)
		{
			return string.Empty;
		}
		string[] array = FunctionName.Substring(0, num2).Split(new char[1] { '.' });
		int num3 = array.Length - 1;
		string text = array[num3];
		while (string.IsNullOrEmpty(text) && num3 > 0)
		{
			text = array[num3 - 1];
			num3--;
		}
		if (string.IsNullOrEmpty(text))
		{
			return Library;
		}
		if ((text.IndexOfAny(Path.GetInvalidPathChars()) == -1 && Path.HasExtension(text)) || StackFrameType == BacktraceStackFrameType.Unknown)
		{
			return text;
		}
		return StackFrameType switch
		{
			BacktraceStackFrameType.Dotnet => $"{text}.cs", 
			BacktraceStackFrameType.Android => $"{text}.java", 
			_ => text, 
		};
	}

	public override string ToString()
	{
		return $"{FunctionName} (at {Library}:{Line.ToString(CultureInfo.InvariantCulture)})";
	}
}
