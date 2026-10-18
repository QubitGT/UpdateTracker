using System;

namespace Fusion.Photon.Realtime.Async;

internal class AuthenticationFailedException : Exception
{
	public AuthenticationFailedException(string message)
		: base(message)
	{
	}
}
