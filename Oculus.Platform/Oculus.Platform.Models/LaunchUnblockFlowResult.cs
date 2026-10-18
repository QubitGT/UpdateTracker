using System;

namespace Oculus.Platform.Models;

public class LaunchUnblockFlowResult
{
	public readonly bool DidCancel;

	public readonly bool DidUnblock;

	public LaunchUnblockFlowResult(IntPtr o)
	{
		DidCancel = CAPI.ovr_LaunchUnblockFlowResult_GetDidCancel(o);
		DidUnblock = CAPI.ovr_LaunchUnblockFlowResult_GetDidUnblock(o);
	}
}
