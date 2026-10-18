using System;

namespace Oculus.Platform.Models;

public class ChallengeEntry
{
	public readonly string DisplayScore;

	public readonly byte[] ExtraData;

	public readonly ulong ID;

	public readonly int Rank;

	public readonly long Score;

	public readonly DateTime Timestamp;

	public readonly User User;

	public ChallengeEntry(IntPtr o)
	{
		DisplayScore = CAPI.ovr_ChallengeEntry_GetDisplayScore(o);
		ExtraData = CAPI.ovr_ChallengeEntry_GetExtraData(o);
		ID = CAPI.ovr_ChallengeEntry_GetID(o);
		Rank = CAPI.ovr_ChallengeEntry_GetRank(o);
		Score = CAPI.ovr_ChallengeEntry_GetScore(o);
		Timestamp = CAPI.ovr_ChallengeEntry_GetTimestamp(o);
		User = new User(CAPI.ovr_ChallengeEntry_GetUser(o));
	}
}
