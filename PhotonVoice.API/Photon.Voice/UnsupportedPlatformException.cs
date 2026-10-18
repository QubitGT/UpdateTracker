using System;

namespace Photon.Voice;

internal class UnsupportedPlatformException : Exception
{
	public UnsupportedPlatformException(string subject, string platform = null)
		: base("[PV] " + subject + " does not support " + ((platform == null) ? "current" : platform) + " platform")
	{
	}
}
