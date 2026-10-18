using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class Achievements
{
	public static Request<AchievementUpdate> AddCount(string name, ulong count)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_AddCount", "");
			return new Request<AchievementUpdate>(CAPI.ovr_Achievements_AddCount(name, count));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AchievementUpdate> AddFields(string name, string fields)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_AddFields", "");
			return new Request<AchievementUpdate>(CAPI.ovr_Achievements_AddFields(name, fields));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AchievementDefinitionList> GetAllDefinitions()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_GetAllDefinitions", "");
			return new Request<AchievementDefinitionList>(CAPI.ovr_Achievements_GetAllDefinitions());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AchievementProgressList> GetAllProgress()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_GetAllProgress", "");
			return new Request<AchievementProgressList>(CAPI.ovr_Achievements_GetAllProgress());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AchievementDefinitionList> GetDefinitionsByName(string[] names)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_GetDefinitionsByName", "");
			return new Request<AchievementDefinitionList>(CAPI.ovr_Achievements_GetDefinitionsByName(names, (names != null) ? names.Length : 0));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AchievementProgressList> GetProgressByName(string[] names)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_GetProgressByName", "");
			return new Request<AchievementProgressList>(CAPI.ovr_Achievements_GetProgressByName(names, (names != null) ? names.Length : 0));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AchievementUpdate> Unlock(string name)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_Unlock", "");
			return new Request<AchievementUpdate>(CAPI.ovr_Achievements_Unlock(name));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AchievementDefinitionList> GetNextAchievementDefinitionListPage(AchievementDefinitionList list)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_GetNextAchievementDefinitionListPage", "");
		if (!list.HasNextPage)
		{
			Debug.LogWarning("Oculus.Platform.GetNextAchievementDefinitionListPage: List has no next page");
			return null;
		}
		if (Core.IsInitialized())
		{
			return new Request<AchievementDefinitionList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 712888917));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<AchievementProgressList> GetNextAchievementProgressListPage(AchievementProgressList list)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Achievements_GetNextAchievementProgressListPage", "");
		if (!list.HasNextPage)
		{
			Debug.LogWarning("Oculus.Platform.GetNextAchievementProgressListPage: List has no next page");
			return null;
		}
		if (Core.IsInitialized())
		{
			return new Request<AchievementProgressList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 792913703));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
