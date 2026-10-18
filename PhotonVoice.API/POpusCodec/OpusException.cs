using System;
using POpusCodec.Enums;

namespace POpusCodec;

public class OpusException : Exception
{
	private OpusStatusCode _statusCode;

	public OpusStatusCode StatusCode => _statusCode;

	public OpusException(OpusStatusCode statusCode, string message)
		: base(message + " (" + statusCode.ToString() + ")")
	{
		_statusCode = statusCode;
	}
}
