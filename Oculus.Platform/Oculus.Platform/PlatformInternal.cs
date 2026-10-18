using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform;

public static class PlatformInternal
{
	public enum MessageTypeInternal : uint
	{
		AbuseReport_LaunchAdvancedReportFlow = 1286683246u,
		Application_ExecuteCoordinatedLaunch = 645772532u,
		Application_GetInstalledApplications = 1376744524u,
		Avatar_UpdateMetaData = 2077219214u,
		Cal_FinalizeApplication = 497667029u,
		Cal_GetSuggestedApplications = 1450209301u,
		Cal_ProposeApplication = 1317270237u,
		Colocation_GetCurrentMapUuid = 878018226u,
		Colocation_RequestMap = 840263277u,
		Colocation_ShareMap = 409847005u,
		DeviceApplicationIntegrity_GetAttestationToken = 271557598u,
		GraphAPI_Get = 822018158u,
		GraphAPI_Post = 1990567876u,
		HTTP_Get = 1874211363u,
		HTTP_GetToFile = 1317133401u,
		HTTP_MultiPartPost = 1480774160u,
		HTTP_Post = 1798743375u,
		Livestreaming_IsAllowedForApplication = 191729014u,
		Livestreaming_StartPartyStream = 2066701532u,
		Livestreaming_StartStream = 1343932350u,
		Livestreaming_StopPartyStream = 661065560u,
		Livestreaming_StopStream = 1155796426u,
		Livestreaming_UpdateMicStatus = 475495815u,
		NetSync_Connect = 1684899167u,
		NetSync_Disconnect = 359268021u,
		NetSync_GetSessions = 1859521077u,
		NetSync_GetVoipAttenuation = 288016919u,
		NetSync_GetVoipAttenuationDefault = 1467721888u,
		NetSync_SetVoipAttenuation = 882366454u,
		NetSync_SetVoipAttenuationModel = 1788128654u,
		NetSync_SetVoipChannelCfg = 1553310963u,
		NetSync_SetVoipGroup = 1477614734u,
		NetSync_SetVoipListentoChannels = 1590749746u,
		NetSync_SetVoipMicSource = 855832432u,
		NetSync_SetVoipSessionMuted = 1434844938u,
		NetSync_SetVoipSpeaktoChannels = 766496213u,
		NetSync_SetVoipStreamMode = 1742839095u,
		Party_Create = 450042703u,
		Party_GatherInApplication = 1921499523u,
		Party_Get = 1586058173u,
		Party_GetCurrentForUser = 1489764138u,
		Party_Invite = 901104867u,
		Party_Join = 1744993395u,
		Party_Leave = 848430801u,
		RichPresence_SetDestination = 1328734477u,
		RichPresence_SetIsJoinable = 1050353505u,
		RichPresence_SetLobbySession = 1895893271u,
		RichPresence_SetMatchSession = 1675623566u,
		Room_CreateOrUpdateAndJoinNamed = 2089683601u,
		Room_GetNamedRooms = 125660812u,
		Room_GetSocialRooms = 1636310390u,
		User_CancelRecordingForReportFlow = 65065289u,
		User_GetUserCapabilities = 303837564u,
		User_LaunchReportFlow = 1449304081u,
		User_LaunchReportFlow2 = 2139314275u,
		User_NewEntitledTestUser = 292822787u,
		User_NewTestUser = 921194380u,
		User_NewTestUserFriends = 517416647u,
		User_StartRecordingForReportFlow = 1819161571u,
		User_StopRecordingAndLaunchReportFlow = 1618513035u,
		User_StopRecordingAndLaunchReportFlow2 = 432190251u,
		User_TestUserCreateDeviceManifest = 1701884605u,
		Voip_ReportAppVoipSessions = 408048078u
	}

	public static class HTTP
	{
		public static void SetHttpTransferUpdateCallback(Message<HttpTransferUpdate>.Callback callback)
		{
			Callback.SetNotificationCallback(Message.MessageType.Notification_HTTP_Transfer, callback);
		}
	}

	public static class Users
	{
		public static Request<LinkedAccountList> GetLinkedAccounts(ServiceProvider[] providers)
		{
			if (Core.IsInitialized())
			{
				UserOptions userOptions = new UserOptions();
				foreach (ServiceProvider value in providers)
				{
					userOptions.AddServiceProvider(value);
				}
				return new Request<LinkedAccountList>(CAPI.ovr_User_GetLinkedAccounts((IntPtr)userOptions));
			}
			return null;
		}
	}

	public static void CrashApplication()
	{
		CAPI.ovr_CrashApplication();
	}

	internal static Message ParseMessageHandle(IntPtr messageHandle, Message.MessageType messageType)
	{
		Message result = null;
		switch ((MessageTypeInternal)messageType)
		{
		case MessageTypeInternal.User_StartRecordingForReportFlow:
			result = new MessageWithAbuseReportRecording(messageHandle);
			break;
		case MessageTypeInternal.User_CancelRecordingForReportFlow:
		case MessageTypeInternal.Voip_ReportAppVoipSessions:
		case MessageTypeInternal.Colocation_ShareMap:
		case MessageTypeInternal.Livestreaming_UpdateMicStatus:
		case MessageTypeInternal.Application_ExecuteCoordinatedLaunch:
		case MessageTypeInternal.Livestreaming_StopPartyStream:
		case MessageTypeInternal.NetSync_SetVoipSpeaktoChannels:
		case MessageTypeInternal.Colocation_RequestMap:
		case MessageTypeInternal.Party_Leave:
		case MessageTypeInternal.NetSync_SetVoipMicSource:
		case MessageTypeInternal.NetSync_SetVoipAttenuation:
		case MessageTypeInternal.RichPresence_SetIsJoinable:
		case MessageTypeInternal.Cal_ProposeApplication:
		case MessageTypeInternal.RichPresence_SetDestination:
		case MessageTypeInternal.NetSync_SetVoipGroup:
		case MessageTypeInternal.NetSync_SetVoipChannelCfg:
		case MessageTypeInternal.NetSync_SetVoipListentoChannels:
		case MessageTypeInternal.RichPresence_SetMatchSession:
		case MessageTypeInternal.User_TestUserCreateDeviceManifest:
		case MessageTypeInternal.NetSync_SetVoipAttenuationModel:
		case MessageTypeInternal.RichPresence_SetLobbySession:
			result = new Message(messageHandle);
			break;
		case MessageTypeInternal.Application_GetInstalledApplications:
			result = new MessageWithInstalledApplicationList(messageHandle);
			break;
		case MessageTypeInternal.AbuseReport_LaunchAdvancedReportFlow:
		case MessageTypeInternal.User_LaunchReportFlow2:
			result = new MessageWithLaunchReportFlowResult(messageHandle);
			break;
		case MessageTypeInternal.Livestreaming_IsAllowedForApplication:
			result = new MessageWithLivestreamingApplicationStatus(messageHandle);
			break;
		case MessageTypeInternal.Livestreaming_StartStream:
		case MessageTypeInternal.Livestreaming_StartPartyStream:
			result = new MessageWithLivestreamingStartResult(messageHandle);
			break;
		case MessageTypeInternal.Livestreaming_StopStream:
			result = new MessageWithLivestreamingVideoStats(messageHandle);
			break;
		case MessageTypeInternal.NetSync_Disconnect:
		case MessageTypeInternal.NetSync_Connect:
			result = new MessageWithNetSyncConnection(messageHandle);
			break;
		case MessageTypeInternal.NetSync_GetSessions:
			result = new MessageWithNetSyncSessionList(messageHandle);
			break;
		case MessageTypeInternal.NetSync_SetVoipSessionMuted:
		case MessageTypeInternal.NetSync_SetVoipStreamMode:
			result = new MessageWithNetSyncSetSessionPropertyResult(messageHandle);
			break;
		case MessageTypeInternal.NetSync_GetVoipAttenuation:
		case MessageTypeInternal.NetSync_GetVoipAttenuationDefault:
			result = new MessageWithNetSyncVoipAttenuationValueList(messageHandle);
			break;
		case MessageTypeInternal.Party_Get:
			result = new MessageWithParty(messageHandle);
			break;
		case MessageTypeInternal.Party_GetCurrentForUser:
			result = new MessageWithPartyUnderCurrentParty(messageHandle);
			break;
		case MessageTypeInternal.Party_Create:
		case MessageTypeInternal.Party_Invite:
		case MessageTypeInternal.Party_Join:
		case MessageTypeInternal.Party_GatherInApplication:
			result = new MessageWithPartyID(messageHandle);
			break;
		case MessageTypeInternal.DeviceApplicationIntegrity_GetAttestationToken:
		case MessageTypeInternal.User_NewEntitledTestUser:
		case MessageTypeInternal.User_NewTestUserFriends:
		case MessageTypeInternal.GraphAPI_Get:
		case MessageTypeInternal.Colocation_GetCurrentMapUuid:
		case MessageTypeInternal.User_NewTestUser:
		case MessageTypeInternal.HTTP_GetToFile:
		case MessageTypeInternal.HTTP_MultiPartPost:
		case MessageTypeInternal.HTTP_Post:
		case MessageTypeInternal.HTTP_Get:
		case MessageTypeInternal.GraphAPI_Post:
		case MessageTypeInternal.Avatar_UpdateMetaData:
			result = new MessageWithString(messageHandle);
			break;
		case MessageTypeInternal.User_GetUserCapabilities:
			result = new MessageWithUserCapabilityList(messageHandle);
			break;
		case MessageTypeInternal.User_StopRecordingAndLaunchReportFlow2:
		case MessageTypeInternal.User_LaunchReportFlow:
		case MessageTypeInternal.User_StopRecordingAndLaunchReportFlow:
			result = new MessageWithUserReportID(messageHandle);
			break;
		}
		return result;
	}

	public static Request<PlatformInitialize> InitializeStandaloneAsync(ulong appID, string accessToken)
	{
		Request<PlatformInitialize> result = new StandalonePlatform().AsyncInitialize(appID, accessToken) ?? throw new UnityException("Oculus Platform failed to initialize.");
		Core.ForceInitialized();
		new GameObject("Oculus.Platform.CallbackRunner").AddComponent<CallbackRunner>();
		return result;
	}
}
