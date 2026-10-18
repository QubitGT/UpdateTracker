using System;

namespace Oculus.Platform.Models;

public class UserCapability
{
	public readonly string Description;

	public readonly bool IsEnabled;

	public readonly string Name;

	public readonly string ReasonCode;

	public UserCapability(IntPtr o)
	{
		Description = CAPI.ovr_UserCapability_GetDescription(o);
		IsEnabled = CAPI.ovr_UserCapability_GetIsEnabled(o);
		Name = CAPI.ovr_UserCapability_GetName(o);
		ReasonCode = CAPI.ovr_UserCapability_GetReasonCode(o);
	}
}
