using System;

namespace Oculus.Platform;

public class AbuseReportOptions
{
	private IntPtr Handle;

	public AbuseReportOptions()
	{
		Handle = CAPI.ovr_AbuseReportOptions_Create();
	}

	public void SetPreventPeopleChooser(bool value)
	{
		CAPI.ovr_AbuseReportOptions_SetPreventPeopleChooser(Handle, value);
	}

	public void SetReportType(AbuseReportType value)
	{
		CAPI.ovr_AbuseReportOptions_SetReportType(Handle, value);
	}

	public static explicit operator IntPtr(AbuseReportOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~AbuseReportOptions()
	{
		CAPI.ovr_AbuseReportOptions_Destroy(Handle);
	}
}
