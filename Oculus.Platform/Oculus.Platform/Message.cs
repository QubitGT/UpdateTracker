using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public abstract class Message<T> : Message
{
	public new delegate void Callback(Message<T> message);

	private T data;

	public T Data => data;

	public Message(IntPtr c_message)
		: base(c_message)
	{
		if (!base.IsError)
		{
			data = GetDataFromMessage(c_message);
		}
	}

	protected abstract T GetDataFromMessage(IntPtr c_message);
}
public class Message
{
	public delegate void Callback(Message message);

	public enum MessageType : uint
	{
		Unknown = 0u,
		AbuseReport_ReportRequestHandled = 1267661958u,
		Achievements_AddCount = 65495601u,
		Achievements_AddFields = 346693929u,
		Achievements_GetAllDefinitions = 64177549u,
		Achievements_GetAllProgress = 1335877149u,
		Achievements_GetDefinitionsByName = 1653670332u,
		Achievements_GetNextAchievementDefinitionArrayPage = 712888917u,
		Achievements_GetNextAchievementProgressArrayPage = 792913703u,
		Achievements_GetProgressByName = 354837425u,
		Achievements_Unlock = 1497156573u,
		ApplicationLifecycle_GetRegisteredPIDs = 82169698u,
		ApplicationLifecycle_GetSessionKey = 984570141u,
		ApplicationLifecycle_RegisterSessionKey = 1303818232u,
		Application_CancelAppDownload = 2082496734u,
		Application_CheckAppDownloadProgress = 1429514532u,
		Application_GetVersion = 1751583246u,
		Application_InstallAppUpdateAndRelaunch = 343960453u,
		Application_LaunchOtherApp = 1424151032u,
		Application_StartAppDownload = 1157365870u,
		AssetFile_Delete = 1834842246u,
		AssetFile_DeleteById = 1525206354u,
		AssetFile_DeleteByName = 1108001231u,
		AssetFile_Download = 289710021u,
		AssetFile_DownloadById = 755009938u,
		AssetFile_DownloadByName = 1664536314u,
		AssetFile_DownloadCancel = 134927303u,
		AssetFile_DownloadCancelById = 1365611796u,
		AssetFile_DownloadCancelByName = 1147858170u,
		AssetFile_GetList = 1258057588u,
		AssetFile_Status = 47394656u,
		AssetFile_StatusById = 1570069816u,
		AssetFile_StatusByName = 1104140880u,
		Avatar_LaunchAvatarEditor = 99737939u,
		Challenges_Create = 1750718017u,
		Challenges_DeclineInvite = 1452177088u,
		Challenges_Delete = 642287050u,
		Challenges_Get = 2002276083u,
		Challenges_GetEntries = 303739999u,
		Challenges_GetEntriesAfterRank = 143202943u,
		Challenges_GetEntriesByIds = 828705244u,
		Challenges_GetList = 1126581078u,
		Challenges_GetNextChallenges = 1534894518u,
		Challenges_GetNextEntries = 2135728326u,
		Challenges_GetPreviousChallenges = 246678541u,
		Challenges_GetPreviousEntries = 2026439792u,
		Challenges_Join = 556040297u,
		Challenges_Leave = 694228709u,
		Challenges_UpdateInfo = 292929120u,
		DeviceApplicationIntegrity_GetIntegrityToken = 846310362u,
		Entitlement_GetIsViewerEntitled = 409688241u,
		GroupPresence_Clear = 1839897795u,
		GroupPresence_GetInvitableUsers = 592167921u,
		GroupPresence_GetNextApplicationInviteArrayPage = 83411186u,
		GroupPresence_GetSentInvites = 136710833u,
		GroupPresence_LaunchInvitePanel = 262066079u,
		GroupPresence_LaunchMultiplayerErrorDialog = 693481252u,
		GroupPresence_LaunchRejoinDialog = 360121199u,
		GroupPresence_LaunchRosterPanel = 896698498u,
		GroupPresence_SendInvites = 231461732u,
		GroupPresence_Set = 1734302756u,
		GroupPresence_SetDeeplinkMessageOverride = 1377492749u,
		GroupPresence_SetDestination = 1281042058u,
		GroupPresence_SetIsJoinable = 714018901u,
		GroupPresence_SetLobbySession = 1224693182u,
		GroupPresence_SetMatchSession = 827098296u,
		IAP_ConsumePurchase = 532378329u,
		IAP_GetNextProductArrayPage = 467225263u,
		IAP_GetNextPurchaseArrayPage = 1196886677u,
		IAP_GetProductsBySKU = 2124073717u,
		IAP_GetViewerPurchases = 974095385u,
		IAP_GetViewerPurchasesDurableCache = 1666817579u,
		IAP_LaunchCheckoutFlow = 1067126029u,
		LanguagePack_GetCurrent = 529592533u,
		LanguagePack_SetCurrent = 1531952096u,
		Leaderboard_Get = 1792298744u,
		Leaderboard_GetEntries = 1572030284u,
		Leaderboard_GetEntriesAfterRank = 406293487u,
		Leaderboard_GetEntriesByIds = 962624508u,
		Leaderboard_GetNextEntries = 1310751961u,
		Leaderboard_GetNextLeaderboardArrayPage = 905344667u,
		Leaderboard_GetPreviousEntries = 1224858304u,
		Leaderboard_WriteEntry = 293587198u,
		Leaderboard_WriteEntryWithSupplementaryMetric = 1925616378u,
		Media_ShareToFacebook = 14912239u,
		Notification_MarkAsRead = 1903319523u,
		PushNotification_Register = 1715112799u,
		RichPresence_Clear = 1471632051u,
		RichPresence_GetDestinations = 1483681044u,
		RichPresence_GetNextDestinationArrayPage = 1731624773u,
		RichPresence_Set = 1007973641u,
		UserAgeCategory_Get = 567009472u,
		UserAgeCategory_Report = 776853718u,
		User_Get = 1808768583u,
		User_GetAccessToken = 111696574u,
		User_GetBlockedUsers = 2099254614u,
		User_GetLinkedAccounts = 1469314134u,
		User_GetLoggedInUser = 1131361373u,
		User_GetLoggedInUserFriends = 1484532365u,
		User_GetLoggedInUserManagedInfo = 1891252974u,
		User_GetNextBlockedUserArrayPage = 2083192267u,
		User_GetNextUserArrayPage = 645723971u,
		User_GetNextUserCapabilityArrayPage = 587854745u,
		User_GetOrgScopedID = 418426907u,
		User_GetSdkAccounts = 1733454467u,
		User_GetUserProof = 578880643u,
		User_LaunchBlockFlow = 1876305192u,
		User_LaunchFriendRequestFlow = 151303576u,
		User_LaunchUnblockFlow = 346172055u,
		Voip_GetMicrophoneAvailability = 1951195973u,
		Voip_SetSystemVoipSuppressed = 1161808298u,
		Notification_AbuseReport_ReportButtonPressed = 608644972u,
		Notification_ApplicationLifecycle_LaunchIntentChanged = 78859427u,
		Notification_AssetFile_DownloadUpdate = 803015885u,
		Notification_GroupPresence_InvitationsSent = 1738179766u,
		Notification_GroupPresence_JoinIntentReceived = 2000194038u,
		Notification_GroupPresence_LeaveIntentReceived = 1194846749u,
		Notification_HTTP_Transfer = 2111073839u,
		Notification_Livestreaming_StatusChange = 575101294u,
		Notification_NetSync_ConnectionStatusChanged = 120882378u,
		Notification_NetSync_SessionsChanged = 947814198u,
		Notification_Party_PartyUpdate = 487688882u,
		Notification_Voip_MicrophoneAvailabilityStateUpdate = 1042336599u,
		Notification_Voip_SystemVoipState = 1490179237u,
		Notification_Vrcamera_GetDataChannelMessageUpdate = 1860498236u,
		Notification_Vrcamera_GetSurfaceUpdate = 938610820u,
		Platform_InitializeWithAccessToken = 896085803u,
		Platform_InitializeStandaloneOculus = 1375260172u,
		Platform_InitializeAndroidAsynchronous = 450037684u,
		Platform_InitializeWindowsAsynchronous = 1839708815u,
		Notification_Session_InvitationsSent = 133810304u
	}

	internal delegate Message ExtraMessageTypesHandler(IntPtr messageHandle, MessageType message_type);

	private MessageType type;

	private ulong requestID;

	private Error error;

	public MessageType Type => type;

	public bool IsError => error != null;

	public ulong RequestID => requestID;

	internal static ExtraMessageTypesHandler HandleExtraMessageTypes { private get; set; }

	public Message(IntPtr c_message)
	{
		type = CAPI.ovr_Message_GetType(c_message);
		bool num = CAPI.ovr_Message_IsError(c_message);
		requestID = CAPI.ovr_Message_GetRequestID(c_message);
		if (!num)
		{
			IntPtr obj = CAPI.ovr_Message_GetNativeMessage(c_message);
			if (CAPI.ovr_Message_IsError(obj))
			{
				IntPtr obj2 = CAPI.ovr_Message_GetError(obj);
				error = new Error(CAPI.ovr_Error_GetCode(obj2), CAPI.ovr_Error_GetMessage(obj2), CAPI.ovr_Error_GetHttpCode(obj2));
			}
		}
		if (num)
		{
			IntPtr obj3 = CAPI.ovr_Message_GetError(c_message);
			error = new Error(CAPI.ovr_Error_GetCode(obj3), CAPI.ovr_Error_GetMessage(obj3), CAPI.ovr_Error_GetHttpCode(obj3));
		}
		else if (Core.LogMessages)
		{
			string text = CAPI.ovr_Message_GetString(c_message);
			if (text != null)
			{
				Debug.Log(text);
			}
			else
			{
				Debug.Log($"null message string {c_message}");
			}
		}
	}

	~Message()
	{
	}

	public virtual Error GetError()
	{
		return error;
	}

	public virtual HttpTransferUpdate GetHttpTransferUpdate()
	{
		return null;
	}

	public virtual PlatformInitialize GetPlatformInitialize()
	{
		return null;
	}

	public virtual AbuseReportRecording GetAbuseReportRecording()
	{
		return null;
	}

	public virtual AchievementDefinitionList GetAchievementDefinitions()
	{
		return null;
	}

	public virtual AchievementProgressList GetAchievementProgressList()
	{
		return null;
	}

	public virtual AchievementUpdate GetAchievementUpdate()
	{
		return null;
	}

	public virtual AppDownloadProgressResult GetAppDownloadProgressResult()
	{
		return null;
	}

	public virtual AppDownloadResult GetAppDownloadResult()
	{
		return null;
	}

	public virtual ApplicationInviteList GetApplicationInviteList()
	{
		return null;
	}

	public virtual ApplicationVersion GetApplicationVersion()
	{
		return null;
	}

	public virtual AssetDetails GetAssetDetails()
	{
		return null;
	}

	public virtual AssetDetailsList GetAssetDetailsList()
	{
		return null;
	}

	public virtual AssetFileDeleteResult GetAssetFileDeleteResult()
	{
		return null;
	}

	public virtual AssetFileDownloadCancelResult GetAssetFileDownloadCancelResult()
	{
		return null;
	}

	public virtual AssetFileDownloadResult GetAssetFileDownloadResult()
	{
		return null;
	}

	public virtual AssetFileDownloadUpdate GetAssetFileDownloadUpdate()
	{
		return null;
	}

	public virtual AvatarEditorResult GetAvatarEditorResult()
	{
		return null;
	}

	public virtual BlockedUserList GetBlockedUserList()
	{
		return null;
	}

	public virtual Challenge GetChallenge()
	{
		return null;
	}

	public virtual ChallengeEntryList GetChallengeEntryList()
	{
		return null;
	}

	public virtual ChallengeList GetChallengeList()
	{
		return null;
	}

	public virtual DestinationList GetDestinationList()
	{
		return null;
	}

	public virtual GroupPresenceJoinIntent GetGroupPresenceJoinIntent()
	{
		return null;
	}

	public virtual GroupPresenceLeaveIntent GetGroupPresenceLeaveIntent()
	{
		return null;
	}

	public virtual InstalledApplicationList GetInstalledApplicationList()
	{
		return null;
	}

	public virtual InvitePanelResultInfo GetInvitePanelResultInfo()
	{
		return null;
	}

	public virtual LaunchBlockFlowResult GetLaunchBlockFlowResult()
	{
		return null;
	}

	public virtual LaunchFriendRequestFlowResult GetLaunchFriendRequestFlowResult()
	{
		return null;
	}

	public virtual LaunchInvitePanelFlowResult GetLaunchInvitePanelFlowResult()
	{
		return null;
	}

	public virtual LaunchReportFlowResult GetLaunchReportFlowResult()
	{
		return null;
	}

	public virtual LaunchUnblockFlowResult GetLaunchUnblockFlowResult()
	{
		return null;
	}

	public virtual bool GetLeaderboardDidUpdate()
	{
		return false;
	}

	public virtual LeaderboardEntryList GetLeaderboardEntryList()
	{
		return null;
	}

	public virtual LeaderboardList GetLeaderboardList()
	{
		return null;
	}

	public virtual LinkedAccountList GetLinkedAccountList()
	{
		return null;
	}

	public virtual LivestreamingApplicationStatus GetLivestreamingApplicationStatus()
	{
		return null;
	}

	public virtual LivestreamingStartResult GetLivestreamingStartResult()
	{
		return null;
	}

	public virtual LivestreamingStatus GetLivestreamingStatus()
	{
		return null;
	}

	public virtual LivestreamingVideoStats GetLivestreamingVideoStats()
	{
		return null;
	}

	public virtual MicrophoneAvailabilityState GetMicrophoneAvailabilityState()
	{
		return null;
	}

	public virtual NetSyncConnection GetNetSyncConnection()
	{
		return null;
	}

	public virtual NetSyncSessionList GetNetSyncSessionList()
	{
		return null;
	}

	public virtual NetSyncSessionsChangedNotification GetNetSyncSessionsChangedNotification()
	{
		return null;
	}

	public virtual NetSyncSetSessionPropertyResult GetNetSyncSetSessionPropertyResult()
	{
		return null;
	}

	public virtual NetSyncVoipAttenuationValueList GetNetSyncVoipAttenuationValueList()
	{
		return null;
	}

	public virtual OrgScopedID GetOrgScopedID()
	{
		return null;
	}

	public virtual Party GetParty()
	{
		return null;
	}

	public virtual PartyID GetPartyID()
	{
		return null;
	}

	public virtual PartyUpdateNotification GetPartyUpdateNotification()
	{
		return null;
	}

	public virtual PidList GetPidList()
	{
		return null;
	}

	public virtual ProductList GetProductList()
	{
		return null;
	}

	public virtual Purchase GetPurchase()
	{
		return null;
	}

	public virtual PurchaseList GetPurchaseList()
	{
		return null;
	}

	public virtual PushNotificationResult GetPushNotificationResult()
	{
		return null;
	}

	public virtual RejoinDialogResult GetRejoinDialogResult()
	{
		return null;
	}

	public virtual SdkAccountList GetSdkAccountList()
	{
		return null;
	}

	public virtual SendInvitesResult GetSendInvitesResult()
	{
		return null;
	}

	public virtual ShareMediaResult GetShareMediaResult()
	{
		return null;
	}

	public virtual string GetString()
	{
		return null;
	}

	public virtual SystemVoipState GetSystemVoipState()
	{
		return null;
	}

	public virtual User GetUser()
	{
		return null;
	}

	public virtual UserAccountAgeCategory GetUserAccountAgeCategory()
	{
		return null;
	}

	public virtual UserCapabilityList GetUserCapabilityList()
	{
		return null;
	}

	public virtual UserList GetUserList()
	{
		return null;
	}

	public virtual UserProof GetUserProof()
	{
		return null;
	}

	public virtual UserReportID GetUserReportID()
	{
		return null;
	}

	internal static Message ParseMessageHandle(IntPtr messageHandle)
	{
		if (messageHandle.ToInt64() == 0L)
		{
			return null;
		}
		Message message = null;
		MessageType messageType = CAPI.ovr_Message_GetType(messageHandle);
		switch (messageType)
		{
		case MessageType.Achievements_GetAllDefinitions:
		case MessageType.Achievements_GetNextAchievementDefinitionArrayPage:
		case MessageType.Achievements_GetDefinitionsByName:
			message = new MessageWithAchievementDefinitions(messageHandle);
			break;
		case MessageType.Achievements_GetProgressByName:
		case MessageType.Achievements_GetNextAchievementProgressArrayPage:
		case MessageType.Achievements_GetAllProgress:
			message = new MessageWithAchievementProgressList(messageHandle);
			break;
		case MessageType.Achievements_AddCount:
		case MessageType.Achievements_AddFields:
		case MessageType.Achievements_Unlock:
			message = new MessageWithAchievementUpdate(messageHandle);
			break;
		case MessageType.Application_CheckAppDownloadProgress:
			message = new MessageWithAppDownloadProgressResult(messageHandle);
			break;
		case MessageType.Application_InstallAppUpdateAndRelaunch:
		case MessageType.Application_StartAppDownload:
		case MessageType.Application_CancelAppDownload:
			message = new MessageWithAppDownloadResult(messageHandle);
			break;
		case MessageType.GroupPresence_GetNextApplicationInviteArrayPage:
		case MessageType.GroupPresence_GetSentInvites:
			message = new MessageWithApplicationInviteList(messageHandle);
			break;
		case MessageType.Application_GetVersion:
			message = new MessageWithApplicationVersion(messageHandle);
			break;
		case MessageType.AssetFile_Status:
		case MessageType.LanguagePack_GetCurrent:
		case MessageType.AssetFile_StatusByName:
		case MessageType.AssetFile_StatusById:
			message = new MessageWithAssetDetails(messageHandle);
			break;
		case MessageType.AssetFile_GetList:
			message = new MessageWithAssetDetailsList(messageHandle);
			break;
		case MessageType.AssetFile_DeleteByName:
		case MessageType.AssetFile_DeleteById:
		case MessageType.AssetFile_Delete:
			message = new MessageWithAssetFileDeleteResult(messageHandle);
			break;
		case MessageType.AssetFile_DownloadCancel:
		case MessageType.AssetFile_DownloadCancelByName:
		case MessageType.AssetFile_DownloadCancelById:
			message = new MessageWithAssetFileDownloadCancelResult(messageHandle);
			break;
		case MessageType.AssetFile_Download:
		case MessageType.AssetFile_DownloadById:
		case MessageType.LanguagePack_SetCurrent:
		case MessageType.AssetFile_DownloadByName:
			message = new MessageWithAssetFileDownloadResult(messageHandle);
			break;
		case MessageType.Notification_AssetFile_DownloadUpdate:
			message = new MessageWithAssetFileDownloadUpdate(messageHandle);
			break;
		case MessageType.Avatar_LaunchAvatarEditor:
			message = new MessageWithAvatarEditorResult(messageHandle);
			break;
		case MessageType.User_GetNextBlockedUserArrayPage:
		case MessageType.User_GetBlockedUsers:
			message = new MessageWithBlockedUserList(messageHandle);
			break;
		case MessageType.Challenges_UpdateInfo:
		case MessageType.Challenges_Join:
		case MessageType.Challenges_Leave:
		case MessageType.Challenges_DeclineInvite:
		case MessageType.Challenges_Create:
		case MessageType.Challenges_Get:
			message = new MessageWithChallenge(messageHandle);
			break;
		case MessageType.Challenges_GetPreviousChallenges:
		case MessageType.Challenges_GetList:
		case MessageType.Challenges_GetNextChallenges:
			message = new MessageWithChallengeList(messageHandle);
			break;
		case MessageType.Challenges_GetEntriesAfterRank:
		case MessageType.Challenges_GetEntries:
		case MessageType.Challenges_GetEntriesByIds:
		case MessageType.Challenges_GetPreviousEntries:
		case MessageType.Challenges_GetNextEntries:
			message = new MessageWithChallengeEntryList(messageHandle);
			break;
		case MessageType.RichPresence_GetDestinations:
		case MessageType.RichPresence_GetNextDestinationArrayPage:
			message = new MessageWithDestinationList(messageHandle);
			break;
		case MessageType.Entitlement_GetIsViewerEntitled:
		case MessageType.IAP_ConsumePurchase:
		case MessageType.Challenges_Delete:
		case MessageType.GroupPresence_LaunchMultiplayerErrorDialog:
		case MessageType.GroupPresence_SetIsJoinable:
		case MessageType.UserAgeCategory_Report:
		case MessageType.GroupPresence_SetMatchSession:
		case MessageType.GroupPresence_LaunchRosterPanel:
		case MessageType.RichPresence_Set:
		case MessageType.GroupPresence_SetLobbySession:
		case MessageType.AbuseReport_ReportRequestHandled:
		case MessageType.GroupPresence_SetDestination:
		case MessageType.ApplicationLifecycle_RegisterSessionKey:
		case MessageType.GroupPresence_SetDeeplinkMessageOverride:
		case MessageType.RichPresence_Clear:
		case MessageType.GroupPresence_Set:
		case MessageType.GroupPresence_Clear:
		case MessageType.Notification_MarkAsRead:
			message = new Message(messageHandle);
			break;
		case MessageType.Notification_GroupPresence_JoinIntentReceived:
			message = new MessageWithGroupPresenceJoinIntent(messageHandle);
			break;
		case MessageType.Notification_GroupPresence_LeaveIntentReceived:
			message = new MessageWithGroupPresenceLeaveIntent(messageHandle);
			break;
		case MessageType.GroupPresence_LaunchInvitePanel:
			message = new MessageWithInvitePanelResultInfo(messageHandle);
			break;
		case MessageType.User_LaunchBlockFlow:
			message = new MessageWithLaunchBlockFlowResult(messageHandle);
			break;
		case MessageType.User_LaunchFriendRequestFlow:
			message = new MessageWithLaunchFriendRequestFlowResult(messageHandle);
			break;
		case MessageType.Notification_GroupPresence_InvitationsSent:
			message = new MessageWithLaunchInvitePanelFlowResult(messageHandle);
			break;
		case MessageType.User_LaunchUnblockFlow:
			message = new MessageWithLaunchUnblockFlowResult(messageHandle);
			break;
		case MessageType.Leaderboard_GetNextLeaderboardArrayPage:
		case MessageType.Leaderboard_Get:
			message = new MessageWithLeaderboardList(messageHandle);
			break;
		case MessageType.Leaderboard_GetEntriesAfterRank:
		case MessageType.Leaderboard_GetEntriesByIds:
		case MessageType.Leaderboard_GetPreviousEntries:
		case MessageType.Leaderboard_GetNextEntries:
		case MessageType.Leaderboard_GetEntries:
			message = new MessageWithLeaderboardEntryList(messageHandle);
			break;
		case MessageType.Leaderboard_WriteEntry:
		case MessageType.Leaderboard_WriteEntryWithSupplementaryMetric:
			message = new MessageWithLeaderboardDidUpdate(messageHandle);
			break;
		case MessageType.User_GetLinkedAccounts:
			message = new MessageWithLinkedAccountList(messageHandle);
			break;
		case MessageType.Notification_Livestreaming_StatusChange:
			message = new MessageWithLivestreamingStatus(messageHandle);
			break;
		case MessageType.Voip_GetMicrophoneAvailability:
			message = new MessageWithMicrophoneAvailabilityState(messageHandle);
			break;
		case MessageType.Notification_NetSync_ConnectionStatusChanged:
			message = new MessageWithNetSyncConnection(messageHandle);
			break;
		case MessageType.Notification_NetSync_SessionsChanged:
			message = new MessageWithNetSyncSessionsChangedNotification(messageHandle);
			break;
		case MessageType.User_GetOrgScopedID:
			message = new MessageWithOrgScopedID(messageHandle);
			break;
		case MessageType.Notification_Party_PartyUpdate:
			message = new MessageWithPartyUpdateNotification(messageHandle);
			break;
		case MessageType.ApplicationLifecycle_GetRegisteredPIDs:
			message = new MessageWithPidList(messageHandle);
			break;
		case MessageType.IAP_GetNextProductArrayPage:
		case MessageType.IAP_GetProductsBySKU:
			message = new MessageWithProductList(messageHandle);
			break;
		case MessageType.IAP_LaunchCheckoutFlow:
			message = new MessageWithPurchase(messageHandle);
			break;
		case MessageType.IAP_GetViewerPurchases:
		case MessageType.IAP_GetNextPurchaseArrayPage:
		case MessageType.IAP_GetViewerPurchasesDurableCache:
			message = new MessageWithPurchaseList(messageHandle);
			break;
		case MessageType.PushNotification_Register:
			message = new MessageWithPushNotificationResult(messageHandle);
			break;
		case MessageType.GroupPresence_LaunchRejoinDialog:
			message = new MessageWithRejoinDialogResult(messageHandle);
			break;
		case MessageType.User_GetSdkAccounts:
			message = new MessageWithSdkAccountList(messageHandle);
			break;
		case MessageType.GroupPresence_SendInvites:
			message = new MessageWithSendInvitesResult(messageHandle);
			break;
		case MessageType.Media_ShareToFacebook:
			message = new MessageWithShareMediaResult(messageHandle);
			break;
		case MessageType.Notification_ApplicationLifecycle_LaunchIntentChanged:
		case MessageType.User_GetAccessToken:
		case MessageType.Notification_AbuseReport_ReportButtonPressed:
		case MessageType.DeviceApplicationIntegrity_GetIntegrityToken:
		case MessageType.Notification_Vrcamera_GetSurfaceUpdate:
		case MessageType.ApplicationLifecycle_GetSessionKey:
		case MessageType.Notification_Voip_MicrophoneAvailabilityStateUpdate:
		case MessageType.Application_LaunchOtherApp:
		case MessageType.Notification_Vrcamera_GetDataChannelMessageUpdate:
			message = new MessageWithString(messageHandle);
			break;
		case MessageType.Voip_SetSystemVoipSuppressed:
			message = new MessageWithSystemVoipState(messageHandle);
			break;
		case MessageType.User_GetLoggedInUser:
		case MessageType.User_Get:
		case MessageType.User_GetLoggedInUserManagedInfo:
			message = new MessageWithUser(messageHandle);
			break;
		case MessageType.UserAgeCategory_Get:
			message = new MessageWithUserAccountAgeCategory(messageHandle);
			break;
		case MessageType.GroupPresence_GetInvitableUsers:
		case MessageType.User_GetNextUserArrayPage:
		case MessageType.User_GetLoggedInUserFriends:
			message = new MessageWithUserList(messageHandle);
			break;
		case MessageType.User_GetNextUserCapabilityArrayPage:
			message = new MessageWithUserCapabilityList(messageHandle);
			break;
		case MessageType.User_GetUserProof:
			message = new MessageWithUserProof(messageHandle);
			break;
		case MessageType.Notification_Voip_SystemVoipState:
			message = new MessageWithSystemVoipState(messageHandle);
			break;
		case MessageType.Notification_HTTP_Transfer:
			message = new MessageWithHttpTransferUpdate(messageHandle);
			break;
		case MessageType.Platform_InitializeAndroidAsynchronous:
		case MessageType.Platform_InitializeWithAccessToken:
		case MessageType.Platform_InitializeStandaloneOculus:
		case MessageType.Platform_InitializeWindowsAsynchronous:
			message = new MessageWithPlatformInitialize(messageHandle);
			break;
		case MessageType.Notification_Session_InvitationsSent:
			Debug.Log($"Received legacy message type for invites {messageType}, returning no message\n");
			break;
		default:
			message = PlatformInternal.ParseMessageHandle(messageHandle, messageType);
			if (message == null)
			{
				Debug.LogError($"Unrecognized message type {messageType}\n");
			}
			break;
		}
		return message;
	}

	public static Message PopMessage()
	{
		if (!Core.IsInitialized())
		{
			return null;
		}
		IntPtr intPtr = CAPI.ovr_PopMessage();
		Message result = ParseMessageHandle(intPtr);
		CAPI.ovr_FreeMessage(intPtr);
		return result;
	}
}
