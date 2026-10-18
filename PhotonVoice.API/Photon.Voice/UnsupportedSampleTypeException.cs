using System;

namespace Photon.Voice;

internal class UnsupportedSampleTypeException : Exception
{
	public UnsupportedSampleTypeException(Type t)
		: base("[PV] unsupported sample type: " + t)
	{
	}
}
