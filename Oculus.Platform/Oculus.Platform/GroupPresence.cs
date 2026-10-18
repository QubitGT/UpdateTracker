using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class GroupPresence
{
	public static Request Clear()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_Clear", "");
			return new Request(CAPI.ovr_GroupPresence_Clear());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<UserList> GetInvitableUsers(InviteOptions options)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_GetInvitableUsers", "");
			return new Request<UserList>(CAPI.ovr_GroupPresence_GetInvitableUsers((IntPtr)options));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<ApplicationInviteList> GetSentInvites()
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_GetSentInvites", "");
			return new Request<ApplicationInviteList>(CAPI.ovr_GroupPresence_GetSentInvites());
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<InvitePanelResultInfo> LaunchInvitePanel(InviteOptions options)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_LaunchInvitePanel", "");
			return new Request<InvitePanelResultInfo>(CAPI.ovr_GroupPresence_LaunchInvitePanel((IntPtr)options));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request LaunchMultiplayerErrorDialog(MultiplayerErrorOptions options)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_LaunchMultiplayerErrorDialog", "");
			return new Request(CAPI.ovr_GroupPresence_LaunchMultiplayerErrorDialog((IntPtr)options));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<RejoinDialogResult> LaunchRejoinDialog(string lobby_session_id, string match_session_id, string destination_api_name)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_LaunchRejoinDialog", "");
			return new Request<RejoinDialogResult>(CAPI.ovr_GroupPresence_LaunchRejoinDialog(lobby_session_id, match_session_id, destination_api_name));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request LaunchRosterPanel(RosterOptions options)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_LaunchRosterPanel", "");
			return new Request(CAPI.ovr_GroupPresence_LaunchRosterPanel((IntPtr)options));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request<SendInvitesResult> SendInvites(ulong[] userIDs)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_SendInvites", "");
			return new Request<SendInvitesResult>(CAPI.ovr_GroupPresence_SendInvites(userIDs, (userIDs != null) ? ((uint)userIDs.Length) : 0u));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request Set(GroupPresenceOptions groupPresenceOptions)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_Set", "");
			return new Request(CAPI.ovr_GroupPresence_Set((IntPtr)groupPresenceOptions));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request SetDeeplinkMessageOverride(string deeplink_message)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_SetDeeplinkMessageOverride", "");
			return new Request(CAPI.ovr_GroupPresence_SetDeeplinkMessageOverride(deeplink_message));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request SetDestination(string api_name)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_SetDestination", "");
			return new Request(CAPI.ovr_GroupPresence_SetDestination(api_name));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request SetIsJoinable(bool is_joinable)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_SetIsJoinable", "");
			return new Request(CAPI.ovr_GroupPresence_SetIsJoinable(is_joinable));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request SetLobbySession(string id)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_SetLobbySession", "");
			return new Request(CAPI.ovr_GroupPresence_SetLobbySession(id));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static Request SetMatchSession(string id)
	{
		if (Core.IsInitialized())
		{
			EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_SetMatchSession", "");
			return new Request(CAPI.ovr_GroupPresence_SetMatchSession(id));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}

	public static void SetInvitationsSentNotificationCallback(Message<LaunchInvitePanelFlowResult>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_InvitationsSentNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_GroupPresence_InvitationsSent, callback);
	}

	public static void SetJoinIntentReceivedNotificationCallback(Message<GroupPresenceJoinIntent>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_JoinIntentReceivedNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_GroupPresence_JoinIntentReceived, callback);
	}

	public static void SetLeaveIntentReceivedNotificationCallback(Message<GroupPresenceLeaveIntent>.Callback callback)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_LeaveIntentReceivedNotificationCallback", "");
		Callback.SetNotificationCallback(Message.MessageType.Notification_GroupPresence_LeaveIntentReceived, callback);
	}

	public static Request<ApplicationInviteList> GetNextApplicationInviteListPage(ApplicationInviteList list)
	{
		EventManager.SendUnifiedEvent(isEssential: true, "platform_sdk", "PSDK_GroupPresence_GetNextApplicationInviteListPage", "");
		if (!list.HasNextPage)
		{
			Debug.LogWarning("Oculus.Platform.GetNextApplicationInviteListPage: List has no next page");
			return null;
		}
		if (Core.IsInitialized())
		{
			return new Request<ApplicationInviteList>(CAPI.ovr_HTTP_GetWithMessageType(list.NextUrl, 83411186));
		}
		Debug.LogError(Core.PlatformUninitializedError);
		return null;
	}
}
