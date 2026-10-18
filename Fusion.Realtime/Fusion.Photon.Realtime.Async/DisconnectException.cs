using System;

namespace Fusion.Photon.Realtime.Async;

internal class DisconnectException : Exception
{
	public DisconnectCause Cause;

	public DisconnectException(DisconnectCause cause)
		: base(cause.ToString())
	{
		Cause = cause;
	}
}
