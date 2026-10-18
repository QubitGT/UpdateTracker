using System;

namespace Oculus.Platform.Models;

public class ApplicationInvite
{
	public readonly Destination DestinationOptional;

	[Obsolete("Deprecated in favor of DestinationOptional")]
	public readonly Destination Destination;

	public readonly ulong ID;

	public readonly bool IsActive;

	public readonly string LobbySessionId;

	public readonly string MatchSessionId;

	public readonly User RecipientOptional;

	[Obsolete("Deprecated in favor of RecipientOptional")]
	public readonly User Recipient;

	public ApplicationInvite(IntPtr o)
	{
		IntPtr intPtr = CAPI.ovr_ApplicationInvite_GetDestination(o);
		Destination = new Destination(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			DestinationOptional = null;
		}
		else
		{
			DestinationOptional = Destination;
		}
		ID = CAPI.ovr_ApplicationInvite_GetID(o);
		IsActive = CAPI.ovr_ApplicationInvite_GetIsActive(o);
		LobbySessionId = CAPI.ovr_ApplicationInvite_GetLobbySessionId(o);
		MatchSessionId = CAPI.ovr_ApplicationInvite_GetMatchSessionId(o);
		IntPtr intPtr2 = CAPI.ovr_ApplicationInvite_GetRecipient(o);
		Recipient = new User(intPtr2);
		if (intPtr2 == IntPtr.Zero)
		{
			RecipientOptional = null;
		}
		else
		{
			RecipientOptional = Recipient;
		}
	}
}
