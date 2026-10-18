using System;

namespace Photon.Voice;

internal class UnsupportedCodecException : Exception
{
	public UnsupportedCodecException(string info, Codec codec)
		: base("[PV] " + info + ": unsupported codec: " + codec)
	{
	}
}
