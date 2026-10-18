using System;

namespace Oculus.Platform.Models;

public class LeaderboardEntry
{
	public readonly string DisplayScore;

	public readonly byte[] ExtraData;

	public readonly ulong ID;

	public readonly int Rank;

	public readonly long Score;

	public readonly SupplementaryMetric SupplementaryMetricOptional;

	[Obsolete("Deprecated in favor of SupplementaryMetricOptional")]
	public readonly SupplementaryMetric SupplementaryMetric;

	public readonly DateTime Timestamp;

	public readonly User User;

	public LeaderboardEntry(IntPtr o)
	{
		DisplayScore = CAPI.ovr_LeaderboardEntry_GetDisplayScore(o);
		ExtraData = CAPI.ovr_LeaderboardEntry_GetExtraData(o);
		ID = CAPI.ovr_LeaderboardEntry_GetID(o);
		Rank = CAPI.ovr_LeaderboardEntry_GetRank(o);
		Score = CAPI.ovr_LeaderboardEntry_GetScore(o);
		IntPtr intPtr = CAPI.ovr_LeaderboardEntry_GetSupplementaryMetric(o);
		SupplementaryMetric = new SupplementaryMetric(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			SupplementaryMetricOptional = null;
		}
		else
		{
			SupplementaryMetricOptional = SupplementaryMetric;
		}
		Timestamp = CAPI.ovr_LeaderboardEntry_GetTimestamp(o);
		User = new User(CAPI.ovr_LeaderboardEntry_GetUser(o));
	}
}
