using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class Leaderboards
{
	public static Request<LeaderboardEntryList> GetNextEntries(LeaderboardEntryList list)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_GetNextEntries", "");
			return new Request<LeaderboardEntryList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 1310751961));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LeaderboardEntryList> GetPreviousEntries(LeaderboardEntryList list)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_GetPreviousEntries", "");
			return new Request<LeaderboardEntryList>(CAPI.ovr_HTTP_GetWithMessageType(list.PreviousUrl, 1224858304));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LeaderboardList> Get(string leaderboardName)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_Get", "");
			return new Request<LeaderboardList>(CAPI.ovr_Leaderboard_Get(leaderboardName));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LeaderboardEntryList> GetEntries(string leaderboardName, int limit, LeaderboardFilterType filter, LeaderboardStartAt startAt)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_GetEntries", "");
			return new Request<LeaderboardEntryList>(CAPI.ovr_Leaderboard_GetEntries(leaderboardName, limit, filter, startAt));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LeaderboardEntryList> GetEntriesAfterRank(string leaderboardName, int limit, ulong afterRank)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_GetEntriesAfterRank", "");
			return new Request<LeaderboardEntryList>(CAPI.ovr_Leaderboard_GetEntriesAfterRank(leaderboardName, limit, afterRank));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LeaderboardEntryList> GetEntriesByIds(string leaderboardName, int limit, LeaderboardStartAt startAt, ulong[] userIDs)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_GetEntriesByIds", "");
			return new Request<LeaderboardEntryList>(CAPI.ovr_Leaderboard_GetEntriesByIds(leaderboardName, limit, startAt, userIDs, (userIDs != null) ? ((uint)userIDs.Length) : 0u));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<bool> WriteEntry(string leaderboardName, long score, byte[] extraData = null, bool forceUpdate = false)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_WriteEntry", "");
			return new Request<bool>(CAPI.ovr_Leaderboard_WriteEntry(leaderboardName, score, extraData, (extraData != null) ? ((uint)extraData.Length) : 0u, forceUpdate));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<bool> WriteEntryWithSupplementaryMetric(string leaderboardName, long score, long supplementaryMetric, byte[] extraData = null, bool forceUpdate = false)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_WriteEntryWithSupplementaryMetric", "");
			return new Request<bool>(CAPI.ovr_Leaderboard_WriteEntryWithSupplementaryMetric(leaderboardName, score, supplementaryMetric, extraData, (extraData != null) ? ((uint)extraData.Length) : 0u, forceUpdate));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LeaderboardList> GetNextLeaderboardListPage(LeaderboardList list)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Leaderboards_GetNextLeaderboardListPage", "");
		if (!list.HasNextPage)
		{
			Debug.LogWarning("Oculus.Platform.GetNextLeaderboardListPage: List has no next page");
			return null;
		}
		if (Core.IsInitialized())
		{
			return new Request<LeaderboardList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 905344667));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
