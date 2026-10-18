using System;

namespace Oculus.Platform.Models;

public class RejoinDialogResult
{
	public readonly bool RejoinSelected;

	public RejoinDialogResult(IntPtr o)
	{
		RejoinSelected = CAPI.ovr_RejoinDialogResult_GetRejoinSelected(o);
	}
}
