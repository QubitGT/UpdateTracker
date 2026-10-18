using System;

namespace Unity.Cinemachine;

internal class ClipperLibException : Exception
{
	public ClipperLibException(string description)
		: base(description)
	{
	}
}
