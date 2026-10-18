using System;

namespace Fusion.Photon.Realtime.Async;

internal class OperationStartException : Exception
{
	public OperationStartException(string message)
		: base(message)
	{
	}
}
