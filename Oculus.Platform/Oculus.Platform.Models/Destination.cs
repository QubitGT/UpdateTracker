using System;

namespace Oculus.Platform.Models;

public class Destination
{
	public readonly string ApiName;

	public readonly string DeeplinkMessage;

	public readonly string DisplayName;

	public readonly string ShareableUri;

	public Destination(IntPtr o)
	{
		ApiName = CAPI.ovr_Destination_GetApiName(o);
		DeeplinkMessage = CAPI.ovr_Destination_GetDeeplinkMessage(o);
		DisplayName = CAPI.ovr_Destination_GetDisplayName(o);
		ShareableUri = CAPI.ovr_Destination_GetShareableUri(o);
	}
}
