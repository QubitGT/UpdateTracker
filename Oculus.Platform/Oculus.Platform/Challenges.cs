using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class Challenges
{
	public static Request<ChallengeEntryList> GetNextEntries(ChallengeEntryList list)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_GetNextEntries", "");
			return new Request<ChallengeEntryList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 2135728326));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ChallengeEntryList> GetPreviousEntries(ChallengeEntryList list)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_GetPreviousEntries", "");
			return new Request<ChallengeEntryList>(CAPI.ovr_HTTP_GetWithMessageType(list.PreviousUrl, 2026439792));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ChallengeList> GetNextChallenges(ChallengeList list)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_GetNextChallenges", "");
			return new Request<ChallengeList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 1534894518));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ChallengeList> GetPreviousChallenges(ChallengeList list)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_GetPreviousChallenges", "");
			return new Request<ChallengeList>(CAPI.ovr_HTTP_GetWithMessageType(list.PreviousUrl, 246678541));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<Challenge> Create(string leaderboardName, ChallengeOptions challengeOptions)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_Create", "");
			return new Request<Challenge>(CAPI.ovr_Challenges_Create(leaderboardName, (IntPtr)challengeOptions));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<Challenge> DeclineInvite(ulong challengeID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_DeclineInvite", "");
			return new Request<Challenge>(CAPI.ovr_Challenges_DeclineInvite(challengeID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request Delete(ulong challengeID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_Delete", "");
			return new Request(CAPI.ovr_Challenges_Delete(challengeID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<Challenge> Get(ulong challengeID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_Get", "");
			return new Request<Challenge>(CAPI.ovr_Challenges_Get(challengeID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ChallengeEntryList> GetEntries(ulong challengeID, int limit, LeaderboardFilterType filter, LeaderboardStartAt startAt)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_GetEntries", "");
			return new Request<ChallengeEntryList>(CAPI.ovr_Challenges_GetEntries(challengeID, limit, filter, startAt));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ChallengeEntryList> GetEntriesAfterRank(ulong challengeID, int limit, ulong afterRank)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_GetEntriesAfterRank", "");
			return new Request<ChallengeEntryList>(CAPI.ovr_Challenges_GetEntriesAfterRank(challengeID, limit, afterRank));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ChallengeEntryList> GetEntriesByIds(ulong challengeID, int limit, LeaderboardStartAt startAt, ulong[] userIDs)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_GetEntriesByIds", "");
			return new Request<ChallengeEntryList>(CAPI.ovr_Challenges_GetEntriesByIds(challengeID, limit, startAt, userIDs, (userIDs != null) ? ((uint)userIDs.Length) : 0u));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ChallengeList> GetList(ChallengeOptions challengeOptions, int limit)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_GetList", "");
			return new Request<ChallengeList>(CAPI.ovr_Challenges_GetList((IntPtr)challengeOptions, limit));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<Challenge> Join(ulong challengeID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_Join", "");
			return new Request<Challenge>(CAPI.ovr_Challenges_Join(challengeID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<Challenge> Leave(ulong challengeID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_Leave", "");
			return new Request<Challenge>(CAPI.ovr_Challenges_Leave(challengeID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<Challenge> UpdateInfo(ulong challengeID, ChallengeOptions challengeOptions)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Challenges_UpdateInfo", "");
			return new Request<Challenge>(CAPI.ovr_Challenges_UpdateInfo(challengeID, (IntPtr)challengeOptions));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
