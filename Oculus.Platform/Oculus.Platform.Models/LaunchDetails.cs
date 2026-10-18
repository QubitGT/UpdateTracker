using System;

namespace Oculus.Platform.Models;

public class LaunchDetails
{
	public readonly string DeeplinkMessage;

	public readonly string DestinationApiName;

	public readonly string LaunchSource;

	public readonly LaunchType LaunchType;

	public readonly string LobbySessionID;

	public readonly string MatchSessionID;

	public readonly string TrackingID;

	public readonly UserList UsersOptional;

	[Obsolete("Deprecated in favor of UsersOptional")]
	public readonly UserList Users;

	public LaunchDetails(IntPtr o)
	{
		DeeplinkMessage = CAPI.ovr_LaunchDetails_GetDeeplinkMessage(o);
		DestinationApiName = CAPI.ovr_LaunchDetails_GetDestinationApiName(o);
		LaunchSource = CAPI.ovr_LaunchDetails_GetLaunchSource(o);
		LaunchType = CAPI.ovr_LaunchDetails_GetLaunchType(o);
		LobbySessionID = CAPI.ovr_LaunchDetails_GetLobbySessionID(o);
		MatchSessionID = CAPI.ovr_LaunchDetails_GetMatchSessionID(o);
		TrackingID = CAPI.ovr_LaunchDetails_GetTrackingID(o);
		IntPtr intPtr = CAPI.ovr_LaunchDetails_GetUsers(o);
		Users = new UserList(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			UsersOptional = null;
		}
		else
		{
			UsersOptional = Users;
		}
	}
}
