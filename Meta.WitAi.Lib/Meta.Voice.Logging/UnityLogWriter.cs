using UnityEngine;

namespace Meta.Voice.Logging;

internal class UnityLogWriter : ILogWriter
{
	public void WriteVerbose(string message)
	{
		Debug.Log(message);
	}

	public void WriteDebug(string message)
	{
		Debug.Log(message);
	}

	public void WriteInfo(string message)
	{
		Debug.Log(message);
	}

	public void WriteWarning(string message)
	{
		Debug.LogWarning(message);
	}

	public void WriteError(string message)
	{
		Debug.LogError(message);
	}
}
