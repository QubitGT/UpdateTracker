using System;

namespace Fusion.Photon.Realtime.Async;

internal class OperationTimeoutException : Exception
{
	public OperationTimeoutException(string message)
		: base(message)
	{
	}
}
