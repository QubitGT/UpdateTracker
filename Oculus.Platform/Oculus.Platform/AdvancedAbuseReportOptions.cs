using System;

namespace Oculus.Platform;

public class AdvancedAbuseReportOptions
{
	private IntPtr Handle;

	public AdvancedAbuseReportOptions()
	{
		Handle = CAPI.ovr_AdvancedAbuseReportOptions_Create();
	}

	public void SetDeveloperDefinedContext(string key, string value)
	{
		CAPI.ovr_AdvancedAbuseReportOptions_SetDeveloperDefinedContextString(Handle, key, value);
	}

	public void ClearDeveloperDefinedContext()
	{
		CAPI.ovr_AdvancedAbuseReportOptions_ClearDeveloperDefinedContext(Handle);
	}

	public void SetObjectType(string value)
	{
		CAPI.ovr_AdvancedAbuseReportOptions_SetObjectType(Handle, value);
	}

	public void SetReportType(AbuseReportType value)
	{
		CAPI.ovr_AdvancedAbuseReportOptions_SetReportType(Handle, value);
	}

	public void AddSuggestedUser(ulong userID)
	{
		CAPI.ovr_AdvancedAbuseReportOptions_AddSuggestedUser(Handle, userID);
	}

	public void ClearSuggestedUsers()
	{
		CAPI.ovr_AdvancedAbuseReportOptions_ClearSuggestedUsers(Handle);
	}

	public void SetVideoMode(AbuseReportVideoMode value)
	{
		CAPI.ovr_AdvancedAbuseReportOptions_SetVideoMode(Handle, value);
	}

	public static explicit operator IntPtr(AdvancedAbuseReportOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~AdvancedAbuseReportOptions()
	{
		CAPI.ovr_AdvancedAbuseReportOptions_Destroy(Handle);
	}
}
