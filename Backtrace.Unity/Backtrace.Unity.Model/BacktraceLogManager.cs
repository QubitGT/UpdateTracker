using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Backtrace.Unity.Model;

internal class BacktraceLogManager
{
	internal readonly Queue<string> LogQueue;

	private readonly object lockObject = new object();

	private readonly uint _limit;

	public int Size => LogQueue.Count;

	public bool Disabled => _limit == 0;

	public BacktraceLogManager(uint numberOfLogs)
	{
		_limit = numberOfLogs;
		LogQueue = new Queue<string>();
	}

	public bool Enqueue(BacktraceReport report)
	{
		return Enqueue(new BacktraceUnityMessage(report));
	}

	public bool Enqueue(string message, string stackTrace, LogType type)
	{
		return Enqueue(new BacktraceUnityMessage(message, stackTrace, type));
	}

	public bool Enqueue(BacktraceUnityMessage unityMessage)
	{
		if (Disabled)
		{
			return false;
		}
		lock (lockObject)
		{
			LogQueue.Enqueue(unityMessage.ToString());
			while (LogQueue.Count > _limit)
			{
				LogQueue.Dequeue();
			}
		}
		return true;
	}

	public string ToSourceCode()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = LogQueue.ToArray();
		foreach (string value in array)
		{
			stringBuilder.AppendLine(value);
		}
		return stringBuilder.ToString();
	}
}
