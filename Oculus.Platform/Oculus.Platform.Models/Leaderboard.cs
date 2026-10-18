using System;

namespace Oculus.Platform.Models;

public class Leaderboard
{
	public readonly string ApiName;

	public readonly Destination DestinationOptional;

	[Obsolete("Deprecated in favor of DestinationOptional")]
	public readonly Destination Destination;

	public readonly ulong ID;

	public Leaderboard(IntPtr o)
	{
		ApiName = CAPI.ovr_Leaderboard_GetApiName(o);
		IntPtr intPtr = CAPI.ovr_Leaderboard_GetDestination(o);
		Destination = new Destination(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			DestinationOptional = null;
		}
		else
		{
			DestinationOptional = Destination;
		}
		ID = CAPI.ovr_Leaderboard_GetID(o);
	}
}
