using System;
using System.Runtime.InteropServices;

public class MothershipClientApiClient : MothershipApiClient
{
	private HandleRef swigCPtr;

	internal MothershipClientApiClient(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.MothershipClientApiClient_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipClientApiClient obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipClientApiClient obj)
	{
		if (obj != null)
		{
			if (!obj.swigCMemOwn)
			{
				throw new ApplicationException("Cannot release ownership as memory is not owned");
			}
			HandleRef result = obj.swigCPtr;
			obj.swigCMemOwn = false;
			obj.Dispose();
			return result;
		}
		return new HandleRef(null, IntPtr.Zero);
	}

	protected override void Dispose(bool disposing)
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					MothershipApiPINVOKE.delete_MothershipClientApiClient(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public MothershipClientApiClient(string baseUrl, string titleId, string envId, string deploymentId, string websocketUrl, bool enableRetryQueue, string sessionIdUUID)
		: this(MothershipApiPINVOKE.new_MothershipClientApiClient(baseUrl, titleId, envId, deploymentId, websocketUrl, enableRetryQueue, sessionIdUUID), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void Tick(float deltaTimeInSeconds)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_Tick(swigCPtr, deltaTimeInSeconds);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void SetAuthRefreshRequiredDelegateWrapper(AuthRefreshRequiredDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetAuthRefreshRequiredDelegateWrapper(swigCPtr, AuthRefreshRequiredDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void SetLoginCompleteDelegate(LoginCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetLoginCompleteDelegate(swigCPtr, LoginCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool LoginWithInsecure1(string username, string accountId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_LoginWithInsecure1(swigCPtr, username, accountId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool LoginWithInsecure2(string username, string accountId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_LoginWithInsecure2(swigCPtr, username, accountId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool LoginWithQuest(string nonce, string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_LoginWithQuest(swigCPtr, nonce, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool LoginWithRift(string nonce, string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_LoginWithRift(swigCPtr, nonce, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool LoginWithGoogle(string token, string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_LoginWithGoogle(swigCPtr, token, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool LoginWithApple(string signature, string gamePlayerId, string teamPlayerId, string certUri, string salt, string timestamp, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_LoginWithApple(swigCPtr, signature, gamePlayerId, teamPlayerId, certUri, salt, timestamp, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool LoginWithPSN(string authCode, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_LoginWithPSN(swigCPtr, authCode, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool LoginWithSynthesisVR(long deviceId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_LoginWithSynthesisVR(swigCPtr, deviceId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public long GetServerTime(string callerId)
	{
		long result = MothershipApiPINVOKE.MothershipClientApiClient_GetServerTime(swigCPtr, callerId);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSetUserDataCompleteClientDelegateWrapper(SetUserDataCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetSetUserDataCompleteClientDelegateWrapper(swigCPtr, SetUserDataCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool SetUserData(string callerId, string userId, string keyName, string value, int generation, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_SetUserData(swigCPtr, callerId, userId, keyName, value, generation, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetUserDataCompleteClientDelegateWrapper(GetUserDataCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetGetUserDataCompleteClientDelegateWrapper(swigCPtr, GetUserDataCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetUserData(string callerId, string userId, string keyName, string metadataId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_GetUserData(swigCPtr, callerId, userId, keyName, metadataId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteUserDataCompleteClientDelegateWrapper(DeleteUserDataCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetDeleteUserDataCompleteClientDelegateWrapper(swigCPtr, DeleteUserDataCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteUserData(string callerId, string userId, string keyName, string metadataId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_DeleteUserData(swigCPtr, callerId, userId, keyName, metadataId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListUserDataCompleteClientDelegateWrapper(ListUserDataCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetListUserDataCompleteClientDelegateWrapper(swigCPtr, ListUserDataCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListUserData(string callerId, string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ListUserData(swigCPtr, callerId, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateReportCompleteClientDelegateWrapper(CreateReportCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetCreateReportCompleteClientDelegateWrapper(swigCPtr, CreateReportCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateReport(string callerId, string reportedUserId, int category, string platform, bool moddedClient, string metadata, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_CreateReport(swigCPtr, callerId, reportedUserId, category, platform, moddedClient, metadata, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetValidateUsernameCompleteClientDelegateWrapper(ValidateUsernameCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetValidateUsernameCompleteClientDelegateWrapper(swigCPtr, ValidateUsernameCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ValidateUsername(string callerId, string username, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ValidateUsername(swigCPtr, callerId, username, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetUserInventoryCompleteClientDelegateWrapper(GetUserInventoryCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetGetUserInventoryCompleteClientDelegateWrapper(swigCPtr, GetUserInventoryCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetUserInventory(string callerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_GetUserInventory(swigCPtr, callerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetMergedInventoryCompleteClientDelegateWrapper(GetMergedInventoryCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetGetMergedInventoryCompleteClientDelegateWrapper(swigCPtr, GetMergedInventoryCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetMergedInventory(string callerId, string targetId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_GetMergedInventory(swigCPtr, callerId, targetId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetStorefrontCompleteClientDelegateWrapper(GetStorefrontRequestCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetGetStorefrontCompleteClientDelegateWrapper(swigCPtr, GetStorefrontRequestCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetStorefront(string callerId, StringVector offerDisplays, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_GetStorefront(swigCPtr, callerId, StringVector.getCPtr(offerDisplays), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetPurchaseCompleteClientDelegateWrapper(PurchaseOfferRequestCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetPurchaseCompleteClientDelegateWrapper(swigCPtr, PurchaseOfferRequestCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool PurchaseOffer(string callerId, string offerDisplayId, string offerId, int displayIndex, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_PurchaseOffer(swigCPtr, callerId, offerDisplayId, offerId, displayIndex, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetConsumeConsumableCompleteClientDelegateWrapper(ClientConsumeConsumableCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetConsumeConsumableCompleteClientDelegateWrapper(swigCPtr, ClientConsumeConsumableCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ConsumeConsumable(string callerId, string entitlementId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ConsumeConsumable(swigCPtr, callerId, entitlementId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetQuestAuthV2BeginRequestCompleteClientDelegateWrapper(QuestBeginLoginV2RequestCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetQuestAuthV2BeginRequestCompleteClientDelegateWrapper(swigCPtr, QuestBeginLoginV2RequestCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool BeginQuestV2Auth(string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_BeginQuestV2Auth(swigCPtr, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool CompleteQuestV2Auth(string userId, string attestationToken, string metaNonce, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_CompleteQuestV2Auth(swigCPtr, userId, attestationToken, metaNonce, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSteamBeginRequestCompleteClientDelegateWrapper(PlayerSteamBeginLoginResponseCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetSteamBeginRequestCompleteClientDelegateWrapper(swigCPtr, PlayerSteamBeginLoginResponseCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool BeginSteamAuth(IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_BeginSteamAuth(swigCPtr, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool CompleteSteamAuth(string nonce, string ticket, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_CompleteSteamAuth(swigCPtr, nonce, ticket, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListMothershipTitleDataCompleteClientDelegateWrapper(ListMothershipTitleDataCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetListMothershipTitleDataCompleteClientDelegateWrapper(swigCPtr, ListMothershipTitleDataCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListClientMothershipTitleData(string callerId, StringVector keys, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ListClientMothershipTitleData(swigCPtr, callerId, StringVector.getCPtr(keys), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetAcceptLanguage(string language)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetAcceptLanguage(swigCPtr, language);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void SetListGameSessionsCompleteDelegateWrapper(ListGameSessionsCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetListGameSessionsCompleteDelegateWrapper(swigCPtr, ListGameSessionsCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListGameSessions(string callerId, int pageSize, int pageOffset, string region, string partition, int minEmptySlots, int maxEmptySlots, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ListGameSessions(swigCPtr, callerId, pageSize, pageOffset, region, partition, minEmptySlots, maxEmptySlots, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRequestJoinGameSessionCompleteDelegateWrapper(RequestJoinGameSessionCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetRequestJoinGameSessionCompleteDelegateWrapper(swigCPtr, RequestJoinGameSessionCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RequestJoinGameSession(string callerId, string requestSessionId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_RequestJoinGameSession(swigCPtr, callerId, requestSessionId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateSharedGroupCompleteDelegateWrapper(CreateSharedGroupCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetCreateSharedGroupCompleteDelegateWrapper(swigCPtr, CreateSharedGroupCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateSharedGroup(string callerId, string titleId, string envId, string sharedGroupId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_CreateSharedGroup(swigCPtr, callerId, titleId, envId, sharedGroupId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetSharedGroupDataCompleteDelegateWrapper(GetSharedGroupDataCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetGetSharedGroupDataCompleteDelegateWrapper(swigCPtr, GetSharedGroupDataCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetSharedGroupData(string callerId, string titleId, string envId, string sharedGroupId, StringVector keys, bool getMembers, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_GetSharedGroupData(swigCPtr, callerId, titleId, envId, sharedGroupId, StringVector.getCPtr(keys), getMembers, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateSharedGroupDataCompleteDelegateWrapper(UpdateSharedGroupDataCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetUpdateSharedGroupDataCompleteDelegateWrapper(swigCPtr, UpdateSharedGroupDataCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateSharedGroupData(string callerId, string titleId, string envId, string sharedGroupId, StringKeyValueMap data, StringKeyValueMap customTags, StringVector keysToRemove, string permission, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_UpdateSharedGroupData(swigCPtr, callerId, titleId, envId, sharedGroupId, StringKeyValueMap.getCPtr(data), StringKeyValueMap.getCPtr(customTags), StringVector.getCPtr(keysToRemove), permission, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetAddSharedGroupMembersCompleteDelegateWrapper(AddSharedGroupMembersCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetAddSharedGroupMembersCompleteDelegateWrapper(swigCPtr, AddSharedGroupMembersCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool AddSharedGroupMembers(string callerId, string titleId, string envId, string sharedGroupId, StringVector mothershipIds, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_AddSharedGroupMembers(swigCPtr, callerId, titleId, envId, sharedGroupId, StringVector.getCPtr(mothershipIds), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRemoveSharedGroupMembersCompleteDelegateWrapper(RemoveSharedGroupMembersCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetRemoveSharedGroupMembersCompleteDelegateWrapper(swigCPtr, RemoveSharedGroupMembersCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RemoveSharedGroupMembers(string callerId, string titleId, string envId, string sharedGroupId, StringVector mothershipIds, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_RemoveSharedGroupMembers(swigCPtr, callerId, titleId, envId, sharedGroupId, StringVector.getCPtr(mothershipIds), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetMothershipRefreshIAPCompleteDelegateWrapper(MothershipRefreshIAPCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetMothershipRefreshIAPCompleteDelegateWrapper(swigCPtr, MothershipRefreshIAPCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RefreshIAP(string callerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_RefreshIAP(swigCPtr, callerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetFinalizeGooglePurchaseCompleteDelegateWrapper(FinalizeGooglePurchaseCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetFinalizeGooglePurchaseCompleteDelegateWrapper(swigCPtr, FinalizeGooglePurchaseCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool FinalizeGooglePurchase(string callerId, string purchaseToken, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_FinalizeGooglePurchase(swigCPtr, callerId, purchaseToken, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetFinalizeApplePurchaseCompleteDelegateWrapper(FinalizeApplePurchaseCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetFinalizeApplePurchaseCompleteDelegateWrapper(swigCPtr, FinalizeApplePurchaseCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool FinalizeApplePurchase(string callerId, string appleTransactionId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_FinalizeApplePurchase(swigCPtr, callerId, appleTransactionId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetInitSteamPurchaseCompleteDelegateWrapper(InitSteamPurchaseCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetInitSteamPurchaseCompleteDelegateWrapper(swigCPtr, InitSteamPurchaseCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool InitSteamPurchase(string callerId, string offerDisplayId, string offerId, int displayIndex, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_InitSteamPurchase(swigCPtr, callerId, offerDisplayId, offerId, displayIndex, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetFinalizeSteamPurchaseCompleteDelegateWrapper(FinalizeSteamPurchaseCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetFinalizeSteamPurchaseCompleteDelegateWrapper(swigCPtr, FinalizeSteamPurchaseCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool FinalizeSteamPurchase(string callerId, string steamOrderId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_FinalizeSteamPurchase(swigCPtr, callerId, steamOrderId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetWriteEventsCompleteClientDelegateWrapper(WriteEventsCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetWriteEventsCompleteClientDelegateWrapper(swigCPtr, WriteEventsCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool WriteEvents(string callerId, MothershipWriteEventsRequest request, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_WriteEvents(swigCPtr, callerId, MothershipWriteEventsRequest.getCPtr(request), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetNotificationsMessageDelegateWrapper(NotificationsMessageDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetNotificationsMessageDelegateWrapper(swigCPtr, NotificationsMessageDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool OpenNotificationsSocket(string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_OpenNotificationsSocket(swigCPtr, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetProgressionTrackValuesForPlayerCompleteClientDelegateWrapper(GetProgressionTrackValuesForPlayerCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetGetProgressionTrackValuesForPlayerCompleteClientDelegateWrapper(swigCPtr, GetProgressionTrackValuesForPlayerCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetProgressionTrackValuesForPlayer(string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_GetProgressionTrackValuesForPlayer(swigCPtr, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetProgressionTreesForPlayerCompleteClientDelegateWrapper(GetProgressionTreesForPlayerCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetGetProgressionTreesForPlayerCompleteClientDelegateWrapper(swigCPtr, GetProgressionTreesForPlayerCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetProgressionTreesForPlayer(string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_GetProgressionTreesForPlayer(swigCPtr, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetClientGetPermissionsCompleteDelegateWrapper(ClientGetPermissionsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetClientGetPermissionsCompleteDelegateWrapper(swigCPtr, ClientGetPermissionsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientGetPermissions(string callerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientGetPermissions(swigCPtr, callerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetClientGetMySubscriptionsDelegateWrapper(ClientGetMySubscriptionCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetClientGetMySubscriptionsDelegateWrapper(swigCPtr, ClientGetMySubscriptionCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientGetMySubscriptions(string callerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientGetMySubscriptions(swigCPtr, callerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetClientInitSteamSubscriptionPurchaseCompleteDelegateWrapper(ClientInitSteamSubscriptionPurchaseCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetClientInitSteamSubscriptionPurchaseCompleteDelegateWrapper(swigCPtr, ClientInitSteamSubscriptionPurchaseCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientInitSteamSubscriptionPurchase(string callerId, string sku, int priceInUSDCents, int subscriptionBillingFrequency, string subscriptionBillingFrequencyUnit, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientInitSteamSubscriptionPurchase(swigCPtr, callerId, sku, priceInUSDCents, subscriptionBillingFrequency, subscriptionBillingFrequencyUnit, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetClientFinalizeSteamSubscriptionPurchaseCompleteDelegateWrapper(ClientFinalizeSteamSubscriptionPurchaseCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetClientFinalizeSteamSubscriptionPurchaseCompleteDelegateWrapper(swigCPtr, ClientFinalizeSteamSubscriptionPurchaseCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientFinalizeSteamSubscriptionPurchase(string callerId, string steamOrderId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientFinalizeSteamSubscriptionPurchase(swigCPtr, callerId, steamOrderId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetClientBulkGetSubscriptionsDelegateWrapper(ClientGetBulkSubscriptionsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetClientBulkGetSubscriptionsDelegateWrapper(swigCPtr, ClientGetBulkSubscriptionsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientBulkGetSubscriptions(string callerId, StringVector players, PlatformAndSkuVector platformSkus, StringVector catalogIds, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientBulkGetSubscriptions(swigCPtr, callerId, StringVector.getCPtr(players), PlatformAndSkuVector.getCPtr(platformSkus), StringVector.getCPtr(catalogIds), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetFileCompleteDelegateWrapper(GetFileCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetGetFileCompleteDelegateWrapper(swigCPtr, GetFileCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientGetFileById(string callerId, string fileId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientGetFileById(swigCPtr, callerId, fileId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ClientGetFileByNameOrAliasAndVersion(string callerId, string fileNameOrAlias, string versionOrLatest, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientGetFileByNameOrAliasAndVersion(swigCPtr, callerId, fileNameOrAlias, versionOrLatest, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetClientStartMatchmakingCompleteDelegateWrapper(StartMatchmakingCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetClientStartMatchmakingCompleteDelegateWrapper(swigCPtr, StartMatchmakingCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientStartMatchmaking(string callerId, string gamemode, MatchmakingPlayerVector players, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientStartMatchmaking(swigCPtr, callerId, gamemode, MatchmakingPlayerVector.getCPtr(players), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetClientGetMatchmakingStatusCompleteDelegateWrapper(GetMatchmakingStatusCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetClientGetMatchmakingStatusCompleteDelegateWrapper(swigCPtr, GetMatchmakingStatusCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientGetMatchmakingStatus(string callerId, string ticketId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientGetMatchmakingStatus(swigCPtr, callerId, ticketId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetClientStopMatchmakingCompleteDelegateWrapper(StopMatchmakingCompleteClientDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipClientApiClient_SetClientStopMatchmakingCompleteDelegateWrapper(swigCPtr, StopMatchmakingCompleteClientDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ClientStopMatchmaking(string callerId, string ticketId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipClientApiClient_ClientStopMatchmaking(swigCPtr, callerId, ticketId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
