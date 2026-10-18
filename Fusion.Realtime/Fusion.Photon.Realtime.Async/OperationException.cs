using System;

namespace Fusion.Photon.Realtime.Async;

internal class OperationException : Exception
{
	public short ErrorCode;

	public OperationException(short errorCode, string message)
		: base($"{message} (ErrorCode: {errorCode})")
	{
		ErrorCode = errorCode;
	}
}
