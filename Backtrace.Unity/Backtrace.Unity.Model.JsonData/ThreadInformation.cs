using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Backtrace.Unity.Extensions;
using Backtrace.Unity.Json;

namespace Backtrace.Unity.Model.JsonData;

public class ThreadInformation
{
	internal IEnumerable<BacktraceStackFrame> Stack = new List<BacktraceStackFrame>();

	public string Name { get; private set; }

	public bool Fault { get; private set; }

	public BacktraceJObject ToJson()
	{
		List<BacktraceJObject> list = new List<BacktraceJObject>();
		for (int i = 0; i < Stack.Count(); i++)
		{
			list.Add(Stack.ElementAt(i).ToJson());
		}
		BacktraceJObject backtraceJObject = new BacktraceJObject(new Dictionary<string, string> { { "name", Name } });
		backtraceJObject.Add("fault", Fault);
		backtraceJObject.ComplexObjects.Add("stack", list);
		return backtraceJObject;
	}

	public ThreadInformation(string threadName, bool fault, IEnumerable<BacktraceStackFrame> stack)
	{
		Stack = stack ?? new List<BacktraceStackFrame>();
		Name = threadName;
		Fault = fault;
	}

	public ThreadInformation(Thread thread, IEnumerable<BacktraceStackFrame> stack, bool faultingThread = false)
		: this(thread.GenerateValidThreadName().ToLower(), faultingThread, stack)
	{
	}

	private ThreadInformation()
	{
	}
}
