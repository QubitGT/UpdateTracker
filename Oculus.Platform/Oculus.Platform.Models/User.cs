using System;

namespace Oculus.Platform.Models;

public class User
{
	public readonly string DisplayName;

	public readonly ulong ID;

	public readonly string ImageURL;

	public readonly ManagedInfo ManagedInfoOptional;

	[Obsolete("Deprecated in favor of ManagedInfoOptional")]
	public readonly ManagedInfo ManagedInfo;

	public readonly string OculusID;

	public readonly string Presence;

	public readonly string PresenceDeeplinkMessage;

	public readonly string PresenceDestinationApiName;

	public readonly string PresenceLobbySessionId;

	public readonly string PresenceMatchSessionId;

	public readonly UserPresenceStatus PresenceStatus;

	public readonly string SmallImageUrl;

	public User(IntPtr o)
	{
		DisplayName = CAPI.ovr_User_GetDisplayName(o);
		ID = CAPI.ovr_User_GetID(o);
		ImageURL = CAPI.ovr_User_GetImageUrl(o);
		IntPtr intPtr = CAPI.ovr_User_GetManagedInfo(o);
		ManagedInfo = new ManagedInfo(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			ManagedInfoOptional = null;
		}
		else
		{
			ManagedInfoOptional = ManagedInfo;
		}
		OculusID = CAPI.ovr_User_GetOculusID(o);
		Presence = CAPI.ovr_User_GetPresence(o);
		PresenceDeeplinkMessage = CAPI.ovr_User_GetPresenceDeeplinkMessage(o);
		PresenceDestinationApiName = CAPI.ovr_User_GetPresenceDestinationApiName(o);
		PresenceLobbySessionId = CAPI.ovr_User_GetPresenceLobbySessionId(o);
		PresenceMatchSessionId = CAPI.ovr_User_GetPresenceMatchSessionId(o);
		PresenceStatus = CAPI.ovr_User_GetPresenceStatus(o);
		SmallImageUrl = CAPI.ovr_User_GetSmallImageUrl(o);
	}
}
