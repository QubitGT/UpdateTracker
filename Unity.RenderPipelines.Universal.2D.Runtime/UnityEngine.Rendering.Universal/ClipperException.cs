using System;

namespace UnityEngine.Rendering.Universal;

internal class ClipperException : Exception
{
	public ClipperException(string description)
		: base(description)
	{
	}
}
