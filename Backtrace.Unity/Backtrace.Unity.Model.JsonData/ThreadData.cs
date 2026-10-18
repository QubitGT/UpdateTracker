using System.Collections.Generic;
using System.Threading;
using Backtrace.Unity.Extensions;
using Backtrace.Unity.Json;

namespace Backtrace.Unity.Model.JsonData;

public class ThreadData
{
	public Dictionary<string, ThreadInformation> ThreadInformations = new Dictionary<string, ThreadInformation>();

	internal string MainThread = string.Empty;

	internal ThreadData(IEnumerable<BacktraceStackFrame> exceptionStack, bool faultingThread)
	{
		Thread currentThread = Thread.CurrentThread;
		string text = currentThread.GenerateValidThreadName().ToLower();
		ThreadInformations[text] = new ThreadInformation(currentThread, exceptionStack, faultingThread);
		MainThread = text;
	}

	public BacktraceJObject ToJson()
	{
		BacktraceJObject backtraceJObject = new BacktraceJObject();
		foreach (KeyValuePair<string, ThreadInformation> threadInformation in ThreadInformations)
		{
			backtraceJObject.Add(threadInformation.Key, threadInformation.Value.ToJson());
		}
		return backtraceJObject;
	}
}
