using System;

namespace Oculus.Platform.Models;

public class PlatformInitialize
{
	public readonly PlatformInitializeResult Result;

	public PlatformInitialize(IntPtr o)
	{
		Result = CAPI.ovr_PlatformInitialize_GetResult(o);
	}
}
