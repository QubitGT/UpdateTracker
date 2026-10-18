using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class Users
{
	public static ulong GetLoggedInUserID()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetLoggedInUserID", "");
			return CAPI.ovr_GetLoggedInUserID();
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return 0uL;
	}

	public static string GetLoggedInUserLocale()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetLoggedInUserLocale", "");
			return CAPI.ovr_GetLoggedInUserLocale();
		}
		return "";
	}

	public static Request<User> Get(ulong userID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_Get", "");
			return new Request<User>(CAPI.ovr_User_Get(userID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<string> GetAccessToken()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetAccessToken", "");
			return new Request<string>(CAPI.ovr_User_GetAccessToken());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<BlockedUserList> GetBlockedUsers()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetBlockedUsers", "");
			return new Request<BlockedUserList>(CAPI.ovr_User_GetBlockedUsers());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LinkedAccountList> GetLinkedAccounts(UserOptions userOptions)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetLinkedAccounts", "");
			return new Request<LinkedAccountList>(CAPI.ovr_User_GetLinkedAccounts((IntPtr)userOptions));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<User> GetLoggedInUser()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetLoggedInUser", "");
			return new Request<User>(CAPI.ovr_User_GetLoggedInUser());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<UserList> GetLoggedInUserFriends()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetLoggedInUserFriends", "");
			return new Request<UserList>(CAPI.ovr_User_GetLoggedInUserFriends());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<User> GetLoggedInUserManagedInfo()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetLoggedInUserManagedInfo", "");
			return new Request<User>(CAPI.ovr_User_GetLoggedInUserManagedInfo());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<OrgScopedID> GetOrgScopedID(ulong userID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetOrgScopedID", "");
			return new Request<OrgScopedID>(CAPI.ovr_User_GetOrgScopedID(userID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<SdkAccountList> GetSdkAccounts()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetSdkAccounts", "");
			return new Request<SdkAccountList>(CAPI.ovr_User_GetSdkAccounts());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<UserProof> GetUserProof()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetUserProof", "");
			return new Request<UserProof>(CAPI.ovr_User_GetUserProof());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LaunchBlockFlowResult> LaunchBlockFlow(ulong userID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_LaunchBlockFlow", "");
			return new Request<LaunchBlockFlowResult>(CAPI.ovr_User_LaunchBlockFlow(userID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LaunchFriendRequestFlowResult> LaunchFriendRequestFlow(ulong userID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_LaunchFriendRequestFlow", "");
			return new Request<LaunchFriendRequestFlowResult>(CAPI.ovr_User_LaunchFriendRequestFlow(userID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<LaunchUnblockFlowResult> LaunchUnblockFlow(ulong userID)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_LaunchUnblockFlow", "");
			return new Request<LaunchUnblockFlowResult>(CAPI.ovr_User_LaunchUnblockFlow(userID));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<BlockedUserList> GetNextBlockedUserListPage(BlockedUserList list)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetNextBlockedUserListPage", "");
		if (!list.HasNextPage)
		{
			Debug.LogWarning("Oculus.Platform.GetNextBlockedUserListPage: List has no next page");
			return null;
		}
		if (Core.IsInitialized())
		{
			return new Request<BlockedUserList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 2083192267));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<UserList> GetNextUserListPage(UserList list)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetNextUserListPage", "");
		if (!list.HasNextPage)
		{
			Debug.LogWarning("Oculus.Platform.GetNextUserListPage: List has no next page");
			return null;
		}
		if (Core.IsInitialized())
		{
			return new Request<UserList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 645723971));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<UserCapabilityList> GetNextUserCapabilityListPage(UserCapabilityList list)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_Users_GetNextUserCapabilityListPage", "");
		if (!list.HasNextPage)
		{
			Debug.LogWarning("Oculus.Platform.GetNextUserCapabilityListPage: List has no next page");
			return null;
		}
		if (Core.IsInitialized())
		{
			return new Request<UserCapabilityList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 587854745));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
