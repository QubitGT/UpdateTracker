using System;
using System.Runtime.InteropServices;

public class MothershipServerApiClient : MothershipApiClient
{
	private HandleRef swigCPtr;

	internal MothershipServerApiClient(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.MothershipServerApiClient_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipServerApiClient obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipServerApiClient obj)
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
					MothershipApiPINVOKE.delete_MothershipServerApiClient(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public MothershipServerApiClient(string baseUrl, string titleId, string envId, string apiKey, bool enableRetryQueue)
		: this(MothershipApiPINVOKE.new_MothershipServerApiClient(baseUrl, titleId, envId, apiKey, enableRetryQueue), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void SetVerifyTokenCompleteDelegateWrapper(VerifyTokenCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetVerifyTokenCompleteDelegateWrapper(swigCPtr, VerifyTokenCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool VerifyToken(string mothershipPlayerId, string token, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_VerifyToken(swigCPtr, mothershipPlayerId, token, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetBulkGetAccountLinksCompleteDelegateWrapper(BulkGetAccountLinksCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetBulkGetAccountLinksCompleteDelegateWrapper(swigCPtr, BulkGetAccountLinksCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool BulkGetAccountLinks(AccountLinkLookupVector lookups, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_BulkGetAccountLinks(swigCPtr, AccountLinkLookupVector.getCPtr(lookups), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetBulkGetPlayersCompleteDelegateWrapper(BulkGetPlayersCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetBulkGetPlayersCompleteDelegateWrapper(swigCPtr, BulkGetPlayersCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool BulkGetPlayers(PlayerLookupVector lookups, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_BulkGetPlayers(swigCPtr, PlayerLookupVector.getCPtr(lookups), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateExplicitAccountLinkCompleteDelegateWrapper(ExplicitAccountLinkCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetCreateExplicitAccountLinkCompleteDelegateWrapper(swigCPtr, ExplicitAccountLinkCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateExplicitAccountLink(string titleId, string envId, string playerId, string externalServiceName, string appScopedAccountId, string orgScopedAccountId, string username, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_CreateExplicitAccountLink(swigCPtr, titleId, envId, playerId, externalServiceName, appScopedAccountId, orgScopedAccountId, username, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListAccountAssociationsCompleteDelegateWrapper(ListAccountAssociationsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetListAccountAssociationsCompleteDelegateWrapper(swigCPtr, ListAccountAssociationsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListAccountAssociationsForPlayer(string mothershipPlayerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ListAccountAssociationsForPlayer(swigCPtr, mothershipPlayerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateAccountAssociationsCompleteDelegateWrapper(CreateAccountAssociationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetCreateAccountAssociationsCompleteDelegateWrapper(swigCPtr, CreateAccountAssociationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateAccountAssociation(string mothershipPlayerId, string externalServiceName, string externalServiceOrgScopedId, string externalServiceUserId, string externalServiceUserName, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_CreateAccountAssociation(swigCPtr, mothershipPlayerId, externalServiceName, externalServiceOrgScopedId, externalServiceUserId, externalServiceUserName, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSetUserDataCompleteServerDelegateWrapper(SetUserDataCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetSetUserDataCompleteServerDelegateWrapper(swigCPtr, SetUserDataCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool SetUserData(string userId, string keyName, string value, int generation, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_SetUserData(swigCPtr, userId, keyName, value, generation, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetUserDataCompleteServerDelegateWrapper(GetUserDataCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetGetUserDataCompleteServerDelegateWrapper(swigCPtr, GetUserDataCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetUserData(string userId, string keyName, string metadataId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_GetUserData(swigCPtr, userId, keyName, metadataId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteUserDataCompleteServerDelegateWrapper(DeleteUserDataCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetDeleteUserDataCompleteServerDelegateWrapper(swigCPtr, DeleteUserDataCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteUserData(string userId, string keyName, string metadataId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_DeleteUserData(swigCPtr, userId, keyName, metadataId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListUserDataCompleteServerDelegateWrapper(ListUserDataCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetListUserDataCompleteServerDelegateWrapper(swigCPtr, ListUserDataCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListUserData(string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ListUserData(swigCPtr, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateSharedGroupCompleteDelegateWrapper(CreateSharedGroupCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetCreateSharedGroupCompleteDelegateWrapper(swigCPtr, CreateSharedGroupCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateSharedGroup(string titleId, string envId, string sharedGroupId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_CreateSharedGroup(swigCPtr, titleId, envId, sharedGroupId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetSharedGroupDataCompleteDelegateWrapper(GetSharedGroupDataCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetGetSharedGroupDataCompleteDelegateWrapper(swigCPtr, GetSharedGroupDataCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetSharedGroupData(string titleId, string envId, string sharedGroupId, StringVector keys, bool getMembers, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_GetSharedGroupData(swigCPtr, titleId, envId, sharedGroupId, StringVector.getCPtr(keys), getMembers, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateSharedGroupDataCompleteDelegateWrapper(UpdateSharedGroupDataCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetUpdateSharedGroupDataCompleteDelegateWrapper(swigCPtr, UpdateSharedGroupDataCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateSharedGroupData(string titleId, string envId, string sharedGroupId, StringKeyValueMap data, StringKeyValueMap customTags, StringVector keysToRemove, string permission, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_UpdateSharedGroupData(swigCPtr, titleId, envId, sharedGroupId, StringKeyValueMap.getCPtr(data), StringKeyValueMap.getCPtr(customTags), StringVector.getCPtr(keysToRemove), permission, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetAddSharedGroupMembersCompleteDelegateWrapper(AddSharedGroupMembersCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetAddSharedGroupMembersCompleteDelegateWrapper(swigCPtr, AddSharedGroupMembersCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool AddSharedGroupMembers(string titleId, string envId, string sharedGroupId, StringVector mothershipIds, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_AddSharedGroupMembers(swigCPtr, titleId, envId, sharedGroupId, StringVector.getCPtr(mothershipIds), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRemoveSharedGroupMembersCompleteDelegateWrapper(RemoveSharedGroupMembersCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetRemoveSharedGroupMembersCompleteDelegateWrapper(swigCPtr, RemoveSharedGroupMembersCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RemoveSharedGroupMembers(string titleId, string envId, string sharedGroupId, StringVector mothershipIds, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_RemoveSharedGroupMembers(swigCPtr, titleId, envId, sharedGroupId, StringVector.getCPtr(mothershipIds), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteSharedGroupCompleteDelegateWrapper(DeleteSharedGroupCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetDeleteSharedGroupCompleteDelegateWrapper(swigCPtr, DeleteSharedGroupCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteSharedGroup(string titleId, string envId, string sharedGroupId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_DeleteSharedGroup(swigCPtr, titleId, envId, sharedGroupId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetUserInventoryCompleteServerDelegateWrapper(GetUserInventoryCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetGetUserInventoryCompleteServerDelegateWrapper(swigCPtr, GetUserInventoryCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetUserInventory(string titleId, string envId, string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_GetUserInventory(swigCPtr, titleId, envId, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRunTransactionCompleteServerDelegateWrapper(RunTransactionCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetRunTransactionCompleteServerDelegateWrapper(swigCPtr, RunTransactionCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RunTransaction(string titleId, string envId, string userId, string transactionId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_RunTransaction(swigCPtr, titleId, envId, userId, transactionId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool RunTransactionWithRef(string titleId, string envId, string userId, string transactionId, string refId, string externalServiceName, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_RunTransactionWithRef(swigCPtr, titleId, envId, userId, transactionId, refId, externalServiceName, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetLastTransactionRunCompleteServerDelegateWrapper(GetLastTransactionCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetGetLastTransactionRunCompleteServerDelegateWrapper(swigCPtr, GetLastTransactionCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetLastTransactionRun(string titleId, string envId, string userId, string transactionId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_GetLastTransactionRun(swigCPtr, titleId, envId, userId, transactionId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetConsumeConsumableCompleteServerDelegateWrapper(ServerConsumeConsumableCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetConsumeConsumableCompleteServerDelegateWrapper(swigCPtr, ServerConsumeConsumableCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ConsumeConsumable(string titleId, string envId, string callerId, string entitlementId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ConsumeConsumable(swigCPtr, titleId, envId, callerId, entitlementId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListMothershipTitleDataCompleteServerDelegateWrapper(ListMothershipTitleDataCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetListMothershipTitleDataCompleteServerDelegateWrapper(swigCPtr, ListMothershipTitleDataCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListMothershipTitleData(string titleId, string envId, string deploymentId, StringVector keys, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ListMothershipTitleData(swigCPtr, titleId, envId, deploymentId, StringVector.getCPtr(keys), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetAcceptLanguage(string language)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetAcceptLanguage(swigCPtr, language);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void SetWriteEventsCompleteServerDelegateWrapper(WriteEventsCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetWriteEventsCompleteServerDelegateWrapper(swigCPtr, WriteEventsCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool WriteEvents(MothershipWriteEventsRequest request, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_WriteEvents(swigCPtr, MothershipWriteEventsRequest.getCPtr(request), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListBansBulkCompleteServerDelegateWrapper(ListBansBulkCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetListBansBulkCompleteServerDelegateWrapper(swigCPtr, ListBansBulkCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListBansBulk(StringVector playerIds, int category, bool includeExpired, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ListBansBulk(swigCPtr, StringVector.getCPtr(playerIds), category, includeExpired, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetServerCreateReportCompleteDelegateWrapper(ServerCreateReportCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetServerCreateReportCompleteDelegateWrapper(swigCPtr, ServerCreateReportCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ServerCreateReport(string reportingUserId, string reportedUserId, int category, string platform, bool moddedClient, string metadata, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerCreateReport(swigCPtr, reportingUserId, reportedUserId, category, platform, moddedClient, metadata, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetServerValidateUsernameCompleteDelegateWrapper(ServerValidateUsernameCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetServerValidateUsernameCompleteDelegateWrapper(swigCPtr, ServerValidateUsernameCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ServerValidateUsername(string username, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerValidateUsername(swigCPtr, username, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetServerCreateBanCompleteDelegateWrapper(ServerCreateBanCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetServerCreateBanCompleteDelegateWrapper(swigCPtr, ServerCreateBanCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ServerCreateBan(string playerId, int category, string reason, int durationMinutes, bool orgWide, string metadata, string source, bool isHardwareBan, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerCreateBan(swigCPtr, playerId, category, reason, durationMinutes, orgWide, metadata, source, isHardwareBan, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSendNotificationServerDelegateWrapper(SendNotificationCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetSendNotificationServerDelegateWrapper(swigCPtr, SendNotificationCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool SendNotification(StringVector playerIds, string title, string body, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_SendNotification(swigCPtr, StringVector.getCPtr(playerIds), title, body, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetProgressionTrackValuesForPlayerCompleteServerDelegateWrapper(GetProgressionTrackValuesForPlayerCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetGetProgressionTrackValuesForPlayerCompleteServerDelegateWrapper(swigCPtr, GetProgressionTrackValuesForPlayerCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetProgressionTrackValuesForPlayer(string titleId, string envId, string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_GetProgressionTrackValuesForPlayer(swigCPtr, titleId, envId, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetIncrementProgressionTrackForPlayerCompleteServerDelegateWrapper(IncrementProgressionTrackForPlayerCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetIncrementProgressionTrackForPlayerCompleteServerDelegateWrapper(swigCPtr, IncrementProgressionTrackForPlayerCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool IncrementProgressionTrackForPlayer(string titleId, string envId, string playerId, string trackId, int additionalProgress, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_IncrementProgressionTrackForPlayer(swigCPtr, titleId, envId, playerId, trackId, additionalProgress, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUnlockProgressionTreeNodeCompleteServerDelegateWrapper(UnlockProgressionTreeNodeCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetUnlockProgressionTreeNodeCompleteServerDelegateWrapper(swigCPtr, UnlockProgressionTreeNodeCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UnlockProgressionTreeNode(string treeId, string nodeId, string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_UnlockProgressionTreeNode(swigCPtr, treeId, nodeId, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ForceUnlockProgressionTreeNode(string treeId, string nodeId, string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ForceUnlockProgressionTreeNode(swigCPtr, treeId, nodeId, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetProgressionTreesForPlayerCompleteServerDelegateWrapper(GetProgressionTreesForPlayerCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetGetProgressionTreesForPlayerCompleteServerDelegateWrapper(swigCPtr, GetProgressionTreesForPlayerCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetProgressionTreesForPlayer(string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_GetProgressionTreesForPlayer(swigCPtr, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetLockProgressionTreeNodeCompleteDelegateWrapper(LockProgressionTreeNodeServerCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetLockProgressionTreeNodeCompleteDelegateWrapper(swigCPtr, LockProgressionTreeNodeServerCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool LockProgressionTreeNode(string titleId, string envId, string treeId, string nodeId, string playerId, bool refund_costs, bool rewind_rewards, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_LockProgressionTreeNode(swigCPtr, titleId, envId, treeId, nodeId, playerId, refund_costs, rewind_rewards, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetServerGetPermissionsCompleteDelegateWrapper(ServerGetPermissionsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetServerGetPermissionsCompleteDelegateWrapper(swigCPtr, ServerGetPermissionsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ServerGetPermissions(StringVector playerIds, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerGetPermissions(swigCPtr, StringVector.getCPtr(playerIds), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRefreshSubscriptionsForPlayerCompleteDelegateWrapper(ServerRefreshSubscriptionsForPlayerCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetRefreshSubscriptionsForPlayerCompleteDelegateWrapper(swigCPtr, ServerRefreshSubscriptionsForPlayerCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RefreshSubscriptionsForPlayer(string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_RefreshSubscriptionsForPlayer(swigCPtr, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetServerBulkGetSubscriptionsCompleteDelegateWrapper(ServerGetBulkSubscriptionsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetServerBulkGetSubscriptionsCompleteDelegateWrapper(swigCPtr, ServerGetBulkSubscriptionsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ServerBulkGetSubscriptions(StringVector players, PlatformAndSkuVector platformSkus, StringVector catalogIds, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerBulkGetSubscriptions(swigCPtr, StringVector.getCPtr(players), PlatformAndSkuVector.getCPtr(platformSkus), StringVector.getCPtr(catalogIds), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListGameSessionsCompleteDelegateWrapper(ListGameSessionsCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetListGameSessionsCompleteDelegateWrapper(swigCPtr, ListGameSessionsCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListGameSessions(int page_size, int page_offset, string region, string partition, int min_empty_slots, int max_empty_slots, string session_name_search, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ListGameSessions(swigCPtr, page_size, page_offset, region, partition, min_empty_slots, max_empty_slots, session_name_search, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateGameSessionCompleteDelegateWrapper(UpdateGameSessionCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetUpdateGameSessionCompleteDelegateWrapper(swigCPtr, UpdateGameSessionCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateGameSession(string id, int currentPlayerCount, StringKeyValueMap extraProperties, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_UpdateGameSession(swigCPtr, id, currentPlayerCount, StringKeyValueMap.getCPtr(extraProperties), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRegisterGameSessionCompleteDelegateWrapper(RegisterGameSessionCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetRegisterGameSessionCompleteDelegateWrapper(swigCPtr, RegisterGameSessionCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RegisterGameSession(string gameSessionId, string provider, string gameSessionName, string ip, int port, string requiredTags, int maxPlayerCount, string region, string partition, StringKeyValueMap extraProperties, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_RegisterGameSession(swigCPtr, gameSessionId, provider, gameSessionName, ip, port, requiredTags, maxPlayerCount, region, partition, StringKeyValueMap.getCPtr(extraProperties), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUnregisterGameSessionCompleteDelegateWrapper(UnregisterGameSessionCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetUnregisterGameSessionCompleteDelegateWrapper(swigCPtr, UnregisterGameSessionCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UnregisterGameSession(string id, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_UnregisterGameSession(swigCPtr, id, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetFileCompleteDelegateWrapper(GetFileCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetGetFileCompleteDelegateWrapper(swigCPtr, GetFileCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ServerGetFileById(string fileId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerGetFileById(swigCPtr, fileId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ServerGetFileByNameOrAliasAndVersion(string fileNameOrAlias, string versionOrLatest, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerGetFileByNameOrAliasAndVersion(swigCPtr, fileNameOrAlias, versionOrLatest, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetServerStartMatchBackfillCompleteDelegateWrapper(StartMatchBackfillCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetServerStartMatchBackfillCompleteDelegateWrapper(swigCPtr, StartMatchBackfillCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ServerStartMatchBackfill(string ticketId, string gamemode, MatchmakingPlayerVector players, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerStartMatchBackfill(swigCPtr, ticketId, gamemode, MatchmakingPlayerVector.getCPtr(players), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetServerStopMatchmakingCompleteDelegateWrapper(StopMatchmakingCompleteServerDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipServerApiClient_SetServerStopMatchmakingCompleteDelegateWrapper(swigCPtr, StopMatchmakingCompleteServerDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ServerStopMatchmaking(string ticketId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipServerApiClient_ServerStopMatchmaking(swigCPtr, ticketId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
