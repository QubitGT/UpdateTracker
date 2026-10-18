using System;

namespace Fusion;

internal class StartGameException : Exception
{
	public ShutdownReason ShutdownReason { get; internal set; }

	internal StartGameException(ShutdownReason shutdownReason = ShutdownReason.Error, string customMsg = null)
		: base(customMsg ?? shutdownReason.ToString())
	{
		ShutdownReason = shutdownReason;
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}: {2}, {3}: {4}]", "StartGameException", "ShutdownReason", ShutdownReason, "Message", Message);
	}
}
