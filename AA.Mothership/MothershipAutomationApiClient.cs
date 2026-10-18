using System;
using System.Runtime.InteropServices;

public class MothershipAutomationApiClient : MothershipApiClient
{
	private HandleRef swigCPtr;

	internal MothershipAutomationApiClient(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.MothershipAutomationApiClient_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipAutomationApiClient obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipAutomationApiClient obj)
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
					MothershipApiPINVOKE.delete_MothershipAutomationApiClient(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public MothershipAutomationApiClient(string baseUrl, string orgId, string apiKey, bool enableRetryQueue, string titleId, string envId)
		: this(MothershipApiPINVOKE.new_MothershipAutomationApiClient__SWIG_0(baseUrl, orgId, apiKey, enableRetryQueue, titleId, envId), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public MothershipAutomationApiClient(string baseUrl, string orgId, string apiKey, bool enableRetryQueue, string titleId)
		: this(MothershipApiPINVOKE.new_MothershipAutomationApiClient__SWIG_1(baseUrl, orgId, apiKey, enableRetryQueue, titleId), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public MothershipAutomationApiClient(string baseUrl, string orgId, string apiKey, bool enableRetryQueue)
		: this(MothershipApiPINVOKE.new_MothershipAutomationApiClient__SWIG_2(baseUrl, orgId, apiKey, enableRetryQueue), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void SetListTitleCompleteDelegateWrapper(ListTitlesCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListTitleCompleteDelegateWrapper(swigCPtr, ListTitlesCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListTitles(IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListTitles(swigCPtr, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetTitleCompleteDelegateWrapper(GetTitleCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetTitleCompleteDelegateWrapper(swigCPtr, GetTitleCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetTitle(string titleId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetTitle(swigCPtr, titleId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateTitleCompleteDelegateWrapper(CreateTitleCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateTitleCompleteDelegateWrapper(swigCPtr, CreateTitleCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateTitle(string titleName, string titleId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateTitle(swigCPtr, titleName, titleId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateEnvironmentCompleteDelegateWrapper(CreateEnvironmentCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateEnvironmentCompleteDelegateWrapper(swigCPtr, CreateEnvironmentCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateEnvironment(string titleId, string envName, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateEnvironment(swigCPtr, titleId, envName, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListEnvironmentsCompleteDelegateWrapper(ListEnvironmentsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListEnvironmentsCompleteDelegateWrapper(swigCPtr, ListEnvironmentsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListEnvironments(string titleId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListEnvironments(swigCPtr, titleId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetEnvironmentCompleteDelegateWrapper(GetEnvironmentCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetEnvironmentCompleteDelegateWrapper(swigCPtr, GetEnvironmentCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetEnvironment(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetEnvironment(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateInsecure1ConfigCompleteDelegateWrapper(UpdateInsecure1ConfigCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateInsecure1ConfigCompleteDelegateWrapper(swigCPtr, UpdateInsecure1ConfigCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateInsecure1Config(string titleId, string envId, bool enabled, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateInsecure1Config(swigCPtr, titleId, envId, enabled, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateInsecure2ConfigCompleteDelegateWrapper(UpdateInsecure2ConfigCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateInsecure2ConfigCompleteDelegateWrapper(swigCPtr, UpdateInsecure2ConfigCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateInsecure2Config(string titleId, string envId, bool enabled, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateInsecure2Config(swigCPtr, titleId, envId, enabled, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateQuestConfigCompleteDelegateWrapper(UpdateQuestConfigCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateQuestConfigCompleteDelegateWrapper(swigCPtr, UpdateQuestConfigCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateQuestConfig(string titleId, string envId, string appId, string appSecret, bool enabled, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateQuestConfig(swigCPtr, titleId, envId, appId, appSecret, enabled, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateRiftConfigCompleteDelegateWrapper(UpdateRiftConfigCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateRiftConfigCompleteDelegateWrapper(swigCPtr, UpdateRiftConfigCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateRiftConfig(string titleId, string envId, string appId, string appSecret, bool enabled, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateRiftConfig(swigCPtr, titleId, envId, appId, appSecret, enabled, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateEnvRequiredTagsCompleteDelegateWrapper(UpdateEnvRequiredTagsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateEnvRequiredTagsCompleteDelegateWrapper(swigCPtr, UpdateEnvRequiredTagsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateEnvRequiredTags(string titleId, string envId, StringVector tags, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateEnvRequiredTags(swigCPtr, titleId, envId, StringVector.getCPtr(tags), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateDeploymentCompleteDelegateWrapper(CreateDeploymentCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateDeploymentCompleteDelegateWrapper(swigCPtr, CreateDeploymentCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateDeployment(string titleId, string envId, string deploymentName, string deploymentId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateDeployment(swigCPtr, titleId, envId, deploymentName, deploymentId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListDeploymentsCompleteDelegateWrapper(ListDeploymentsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListDeploymentsCompleteDelegateWrapper(swigCPtr, ListDeploymentsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListDeployments(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListDeployments(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetDeploymentCompleteDelegateWrapper(GetDeploymentCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetDeploymentCompleteDelegateWrapper(swigCPtr, GetDeploymentCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetDeployment(string titleId, string envId, string deploymentId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetDeployment(swigCPtr, titleId, envId, deploymentId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateDeploymentRequiredTagsCompleteDelegateWrapper(UpdateDeploymentRequiredTagsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateDeploymentRequiredTagsCompleteDelegateWrapper(swigCPtr, UpdateDeploymentRequiredTagsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateDeploymentRequiredTags(string titleId, string envId, string deploymentId, StringVector tags, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateDeploymentRequiredTags(swigCPtr, titleId, envId, deploymentId, StringVector.getCPtr(tags), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListUsersCompleteDelegateWrapper(ListUsersCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListUsersCompleteDelegateWrapper(swigCPtr, ListUsersCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListUsers(string titleId, string envId, string lastSeenMothershipId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListUsers(swigCPtr, titleId, envId, lastSeenMothershipId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdatePlayerTagsCompleteDelegateWrapper(UpdatePlayerTagsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdatePlayerTagsCompleteDelegateWrapper(swigCPtr, UpdatePlayerTagsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdatePlayerTags(string titleId, string envId, PlayerTagsUpdateMap tagUpdates, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdatePlayerTags(swigCPtr, titleId, envId, PlayerTagsUpdateMap.getCPtr(tagUpdates), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateUserDataMetadataCompleteDelegateWrapper(CreateUserDataMetadataCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateUserDataMetadataCompleteDelegateWrapper(swigCPtr, CreateUserDataMetadataCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateUserDataMetadata(string titleId, string envId, string keyName, string keyPerms, string privacyNotes, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateUserDataMetadata(swigCPtr, titleId, envId, keyName, keyPerms, privacyNotes, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateUserDataMetadataCompleteDelegateWrapper(UpdateUserDataMetadataCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateUserDataMetadataCompleteDelegateWrapper(swigCPtr, UpdateUserDataMetadataCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateUserDataMetadata(string titleId, string envId, string keyName, string metadataId, string keyPerms, string privacyNotes, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateUserDataMetadata(swigCPtr, titleId, envId, keyName, metadataId, keyPerms, privacyNotes, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetUserDataMetadataCompleteDelegateWrapper(GetUserDataMetadataCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetUserDataMetadataCompleteDelegateWrapper(swigCPtr, GetUserDataMetadataCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetUserDataMetadata(string titleId, string envId, string keyName, string metadataId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetUserDataMetadata(swigCPtr, titleId, envId, keyName, metadataId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteUserDataMetadataCompleteDelegateWrapper(DeleteUserDataMetadataCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteUserDataMetadataCompleteDelegateWrapper(swigCPtr, DeleteUserDataMetadataCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteUserDataMetadata(string titleId, string envId, string keyName, string metadataId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteUserDataMetadata(swigCPtr, titleId, envId, keyName, metadataId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListUserDataMetadataCompleteDelegateWrapper(ListUserDataMetadataCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListUserDataMetadataCompleteDelegateWrapper(swigCPtr, ListUserDataMetadataCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListUserDataMetadata(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListUserDataMetadata(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSetUserDataCompleteAutomationDelegateWrapper(SetUserDataCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetSetUserDataCompleteAutomationDelegateWrapper(swigCPtr, SetUserDataCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool SetUserData(string titleId, string envId, string userId, string keyName, string value, int generation, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_SetUserData(swigCPtr, titleId, envId, userId, keyName, value, generation, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetUserDataCompleteAutomationDelegateWrapper(GetUserDataCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetUserDataCompleteAutomationDelegateWrapper(swigCPtr, GetUserDataCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetUserData(string titleId, string envId, string userId, string keyName, string metadataId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetUserData(swigCPtr, titleId, envId, userId, keyName, metadataId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteUserDataCompleteAutomationDelegateWrapper(DeleteUserDataCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteUserDataCompleteAutomationDelegateWrapper(swigCPtr, DeleteUserDataCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteUserData(string titleId, string envId, string userId, string keyName, string metadataId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteUserData(swigCPtr, titleId, envId, userId, keyName, metadataId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListUserDataCompleteAutomationDelegateWrapper(ListUserDataCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListUserDataCompleteAutomationDelegateWrapper(swigCPtr, ListUserDataCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListUserData(string titleId, string envId, string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListUserData(swigCPtr, titleId, envId, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateEntitlementCatalogItemCompleteDelegateWrapper(CreateEntitlementCatalogItemCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateEntitlementCatalogItemCompleteDelegateWrapper(swigCPtr, CreateEntitlementCatalogItemCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateEntitlementCatalogItem(string titleId, string envId, string name, string inGameId, string type, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateEntitlementCatalogItem(swigCPtr, titleId, envId, name, inGameId, type, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListEntitlementCatalogItemCompleteDelegateWrapper(ListEntitlementCatalogItemsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListEntitlementCatalogItemCompleteDelegateWrapper(swigCPtr, ListEntitlementCatalogItemsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListEntitlementCatalogItems(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListEntitlementCatalogItems(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetEntitlementCatalogItemCompleteDelegateWrapper(GetEntitlementCatalogItemCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetEntitlementCatalogItemCompleteDelegateWrapper(swigCPtr, GetEntitlementCatalogItemCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetEntitlementCatalogItem(string titleId, string envId, string entitlementId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetEntitlementCatalogItem(swigCPtr, titleId, envId, entitlementId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateEntitlementCatalogItemCompleteDelegateWrapper(UpdateEntitlementCatalogItemCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateEntitlementCatalogItemCompleteDelegateWrapper(swigCPtr, UpdateEntitlementCatalogItemCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateEntitlementCatalogItem(string titleId, string envId, string name, string entitlementId, string inGameId, string type, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateEntitlementCatalogItem(swigCPtr, titleId, envId, name, entitlementId, inGameId, type, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateTransactionCatalogItemCompleteDelegateWrapper(CreateTransactionCatalogItemCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateTransactionCatalogItemCompleteDelegateWrapper(swigCPtr, CreateTransactionCatalogItemCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateTransactionCatalogItem(string titleId, string envId, string name, string externalServiceName, string externalServiceEntitlementId, StringIntMap inventoryChanges, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateTransactionCatalogItem(swigCPtr, titleId, envId, name, externalServiceName, externalServiceEntitlementId, StringIntMap.getCPtr(inventoryChanges), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListTransactionCatalogItemsCompleteDelegateWrapper(ListTransactionCatalogItemsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListTransactionCatalogItemsCompleteDelegateWrapper(swigCPtr, ListTransactionCatalogItemsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListTransactionCatalogItems(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListTransactionCatalogItems(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetTransactionCatalogItemCompleteDelegateWrapper(GetTransactionCatalogItemCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetTransactionCatalogItemCompleteDelegateWrapper(swigCPtr, GetTransactionCatalogItemCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetTransactionCatalogItem(string titleId, string envId, string transactionId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetTransactionCatalogItem(swigCPtr, titleId, envId, transactionId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateTransactionCatalogItemCompleteDelegateWrapper(UpdateTransactionCatalogItemCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateTransactionCatalogItemCompleteDelegateWrapper(swigCPtr, UpdateTransactionCatalogItemCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateTransactionCatalogItem(string titleId, string envId, string transactionId, string name, string externalServiceName, string externalServiceEntitlementId, StringIntMap inventoryChanges, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateTransactionCatalogItem(swigCPtr, titleId, envId, transactionId, name, externalServiceName, externalServiceEntitlementId, StringIntMap.getCPtr(inventoryChanges), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateTransactionCatalogItemSunsetStatusCompleteDelegateWrapper(UpdateTransactionCatalogItemSunsetStatusCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateTransactionCatalogItemSunsetStatusCompleteDelegateWrapper(swigCPtr, UpdateTransactionCatalogItemSunsetStatusCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateTransactionCatalogItemSunsetStatus(string titleId, string envId, string transactionId, bool sunsetStatus, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateTransactionCatalogItemSunsetStatus(swigCPtr, titleId, envId, transactionId, sunsetStatus, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListReportsCompleteDelegateWrapper(ListReportsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListReportsCompleteDelegateWrapper(swigCPtr, ListReportsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListReports(string titleId, string envId, StringVector reportsBy, StringVector reportsAgainst, int category, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListReports(swigCPtr, titleId, envId, StringVector.getCPtr(reportsBy), StringVector.getCPtr(reportsAgainst), category, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetReportCompleteDelegateWrapper(GetReportCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetReportCompleteDelegateWrapper(swigCPtr, GetReportCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetReport(string titleId, string envId, string reportId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetReport(swigCPtr, titleId, envId, reportId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteReportCompleteDelegateWrapper(DeleteReportCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteReportCompleteDelegateWrapper(swigCPtr, DeleteReportCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteReport(string titleId, string envId, string reportId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteReport(swigCPtr, titleId, envId, reportId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetBanCompleteDelegateWrapper(GetBanCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetBanCompleteDelegateWrapper(swigCPtr, GetBanCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetBan(string titleId, string envId, string banId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetBan(swigCPtr, titleId, envId, banId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListBansCompleteDelegateWrapper(ListBansCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListBansCompleteDelegateWrapper(swigCPtr, ListBansCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListBans(string titleId, string envId, string playerId, int category, bool includeExpired, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListBans(swigCPtr, titleId, envId, playerId, category, includeExpired, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListBansBulkCompleteDelegateWrapper(ListBansBulkCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListBansBulkCompleteDelegateWrapper(swigCPtr, ListBansBulkCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListBansBulk(string titleId, string envId, StringVector playerIds, int category, bool includeExpired, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListBansBulk(swigCPtr, titleId, envId, StringVector.getCPtr(playerIds), category, includeExpired, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateBanCompleteDelegateWrapper(CreateBanCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateBanCompleteDelegateWrapper(swigCPtr, CreateBanCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateBan(string titleId, string envId, string playerId, int category, string reason, int durationMinutes, bool orgWide, string metadata, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateBan(swigCPtr, titleId, envId, playerId, category, reason, durationMinutes, orgWide, metadata, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRevokeBanCompleteDelegateWrapper(RevokeBanCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetRevokeBanCompleteDelegateWrapper(swigCPtr, RevokeBanCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RevokeBan(string titleId, string envId, string banId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_RevokeBan(swigCPtr, titleId, envId, banId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateMuteCompleteDelegateWrapper(CreateMuteCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateMuteCompleteDelegateWrapper(swigCPtr, CreateMuteCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateMute(string titleId, string envId, string playerId, int durationMinutes, string source, string reason, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateMute(swigCPtr, titleId, envId, playerId, durationMinutes, source, reason, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetPlayerMutesCompleteDelegateWrapper(GetPlayerMutesCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetPlayerMutesCompleteDelegateWrapper(swigCPtr, GetPlayerMutesCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetPlayerMutes(string titleId, string envId, string playerId, bool includeExpired, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetPlayerMutes(swigCPtr, titleId, envId, playerId, includeExpired, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteMuteCompleteDelegateWrapper(DeleteMuteCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteMuteCompleteDelegateWrapper(swigCPtr, DeleteMuteCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteMute(string titleId, string envId, string playerId, string muteId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteMute(swigCPtr, titleId, envId, playerId, muteId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRevokeMuteCompleteDelegateWrapper(RevokeMuteCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetRevokeMuteCompleteDelegateWrapper(swigCPtr, RevokeMuteCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RevokeMute(string titleId, string envId, string playerId, string muteId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_RevokeMute(swigCPtr, titleId, envId, playerId, muteId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetUserInventoryCompleteAutomationDelegateWrapper(GetUserInventoryCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetUserInventoryCompleteAutomationDelegateWrapper(swigCPtr, GetUserInventoryCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetUserInventory(string titleId, string envId, string userId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetUserInventory(swigCPtr, titleId, envId, userId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRunTransactionCompleteAutomationDelegateWrapper(RunTransactionCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetRunTransactionCompleteAutomationDelegateWrapper(swigCPtr, RunTransactionCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RunTransaction(string titleId, string envId, string userId, string transactionId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_RunTransaction(swigCPtr, titleId, envId, userId, transactionId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetLastTransactionRunCompleteAutomationDelegateWrapper(GetLastTransactionCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetLastTransactionRunCompleteAutomationDelegateWrapper(swigCPtr, GetLastTransactionCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetLastTransactionRun(string titleId, string envId, string userId, string transactionId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetLastTransactionRun(swigCPtr, titleId, envId, userId, transactionId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateServerKeyCompleteDelegateWrapper(CreateServerKeyCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateServerKeyCompleteDelegateWrapper(swigCPtr, CreateServerKeyCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateServerKey(string titleId, string envId, string keyName, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateServerKey(swigCPtr, titleId, envId, keyName, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListMothershipTitleDataCompleteAutomationDelegateWrapper(ListMothershipTitleDataCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListMothershipTitleDataCompleteAutomationDelegateWrapper(swigCPtr, ListMothershipTitleDataCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListMothershipTitleData(string titleId, string envId, string deploymentId, StringVector keys, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListMothershipTitleData(swigCPtr, titleId, envId, deploymentId, StringVector.getCPtr(keys), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSetMothershipTitleDataCompleteDelegateWrapper(SetMothershipTitleDataCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetSetMothershipTitleDataCompleteDelegateWrapper(swigCPtr, SetMothershipTitleDataCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool SetMothershipTitleData(string titleId, string envId, string deploymentId, string key, string data, bool server_only, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_SetMothershipTitleData(swigCPtr, titleId, envId, deploymentId, key, data, server_only, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteMothershipTitleDataCompleteDelegateWrapper(DeleteMothershipTitleDataCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteMothershipTitleDataCompleteDelegateWrapper(swigCPtr, DeleteMothershipTitleDataCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteMothershipTitleData(string titleId, string envId, string deploymentId, string key, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteMothershipTitleData(swigCPtr, titleId, envId, deploymentId, key, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetPlayerAccountLinksCompleteDelegateWrapper(GetPlayerAccountLinksCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetPlayerAccountLinksCompleteDelegateWrapper(swigCPtr, GetPlayerAccountLinksCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetPlayerAccountLinks(string titleId, string envId, string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetPlayerAccountLinks(swigCPtr, titleId, envId, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetAddPlayerAccountLinkCompleteDelegateWrapper(AddPlayerAccountLinkCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetAddPlayerAccountLinkCompleteDelegateWrapper(swigCPtr, AddPlayerAccountLinkCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool AddPlayerAccountLink(string titleId, string envId, string otherToken, string targetPlayer_id, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_AddPlayerAccountLink(swigCPtr, titleId, envId, otherToken, targetPlayer_id, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetSetPrimaryAccountLinkCompleteDelegateWrapper(SetPrimaryAccountLinkCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetSetPrimaryAccountLinkCompleteDelegateWrapper(swigCPtr, SetPrimaryAccountLinkCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool SetPrimaryAccountLink(string titleId, string envId, string linkId, string targetPlayer_id, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_SetPrimaryAccountLink(swigCPtr, titleId, envId, linkId, targetPlayer_id, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteAccountLinkCompleteDelegateWrapper(DeleteAccountLinkCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteAccountLinkCompleteDelegateWrapper(swigCPtr, DeleteAccountLinkCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteAccountLink(string titleId, string envId, string playerId, string linkId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteAccountLink(swigCPtr, titleId, envId, playerId, linkId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateExplicitAccountLinkCompleteDelegateWrapper(ExplicitAccountLinkCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateExplicitAccountLinkCompleteDelegateWrapper(swigCPtr, ExplicitAccountLinkCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateExplicitAccountLink(string titleId, string envId, string playerId, string externalServiceName, string appScopedAccountId, string orgScopedAccountId, string username, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateExplicitAccountLink(swigCPtr, titleId, envId, playerId, externalServiceName, appScopedAccountId, orgScopedAccountId, username, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListAccountAssociationsCompleteDelegateWrapper(ListAccountAssociationsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListAccountAssociationsCompleteDelegateWrapper(swigCPtr, ListAccountAssociationsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListAccountAssociationsForPlayer(string titleId, string envId, string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListAccountAssociationsForPlayer(swigCPtr, titleId, envId, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteAccountAssociationCompleteDelegateWrapper(DeleteAccountAssociationCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteAccountAssociationCompleteDelegateWrapper(swigCPtr, DeleteAccountAssociationCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteAccountAssociation(string titleId, string envId, string playerId, string associationId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteAccountAssociation(swigCPtr, titleId, envId, playerId, associationId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListOfferCatalogItemsCompleteDelegateWrapper(ListOfferCatalogItemsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListOfferCatalogItemsCompleteDelegateWrapper(swigCPtr, ListOfferCatalogItemsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListOfferCatalogItems(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListOfferCatalogItems(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateOfferCatalogItemCompleteDelegateWrapper(CreateOfferCatalogItemCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateOfferCatalogItemCompleteDelegateWrapper(swigCPtr, CreateOfferCatalogItemCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateOfferCatalogItem(string titleId, string envId, string name, string transaction_id, OfferEntitlementMap bundle_pricing, int discount_percent, string subscription_catalog_item_id, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateOfferCatalogItem(swigCPtr, titleId, envId, name, transaction_id, OfferEntitlementMap.getCPtr(bundle_pricing), discount_percent, subscription_catalog_item_id, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListOffersDisplayCompleteDelegateWrapper(ListOffersDisplayCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListOffersDisplayCompleteDelegateWrapper(swigCPtr, ListOffersDisplayCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListOfferDisplays(string titleId, string envid, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListOfferDisplays(swigCPtr, titleId, envid, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateOfferDisplayCompleteDelegateWrapper(CreateOfferDisplayCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateOfferDisplayCompleteDelegateWrapper(swigCPtr, CreateOfferDisplayCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateOfferDisplay(string titleId, string envId, string name, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateOfferDisplay(swigCPtr, titleId, envId, name, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetChangeCommitStatusOfOfferBindingsCompleteDelegateWrapper(ChangeCommitStatusOfOfferBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetChangeCommitStatusOfOfferBindingsCompleteDelegateWrapper(swigCPtr, ChangeCommitStatusOfOfferBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ChangeCommitStatusOfOfferBindings(string titleId, string envId, string deploymentId, string displayId, bool committed, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ChangeCommitStatusOfOfferBindings(swigCPtr, titleId, envId, deploymentId, displayId, committed, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListOfferBindingsCompleteDelegateWrapper(ListOfferBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListOfferBindingsCompleteDelegateWrapper(swigCPtr, ListOfferBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListOfferBindings(string titleId, string envId, string deploymentId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListOfferBindings(swigCPtr, titleId, envId, deploymentId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateOfferBindingCompleteDelegateWrapper(CreateOfferBindingCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateOfferBindingCompleteDelegateWrapper(swigCPtr, CreateOfferBindingCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateOfferBinding(string titleId, string envId, string deploymentId, string offerDisplayId, string offerId, int displayIndex, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateOfferBinding(swigCPtr, titleId, envId, deploymentId, offerDisplayId, offerId, displayIndex, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateProgressionTrackCompleteDelegateWrapper(CreateProgressionTrackCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateProgressionTrackCompleteDelegateWrapper(swigCPtr, CreateProgressionTrackCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateProgressionTrack(string titleId, string envId, string name, int maximum, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateProgressionTrack(swigCPtr, titleId, envId, name, maximum, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateProgressionTrackCompleteDelegateWrapper(UpdateProgressionTrackCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateProgressionTrackCompleteDelegateWrapper(swigCPtr, UpdateProgressionTrackCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateProgressionTrack(string trackId, string titleId, string envId, string name, int maximum, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateProgressionTrack(swigCPtr, trackId, titleId, envId, name, maximum, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListProgressionTracksCompleteDelegateWrapper(ListProgressionTracksCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListProgressionTracksCompleteDelegateWrapper(swigCPtr, ListProgressionTracksCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListProgressionTracks(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListProgressionTracks(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteProgressionTrackCompleteDelegateWrapper(DeleteProgressionTrackCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteProgressionTrackCompleteDelegateWrapper(swigCPtr, DeleteProgressionTrackCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteProgressionTrack(string titleId, string envId, string trackId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteProgressionTrack(swigCPtr, titleId, envId, trackId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateTrackTriggerCompleteDelegateWrapper(CreateTrackTriggerCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateTrackTriggerCompleteDelegateWrapper(swigCPtr, CreateTrackTriggerCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateTrackTrigger(string trackId, string titleId, string envId, string transactionId, string name, int progression, string prerequisiteEntitlementId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateTrackTrigger(swigCPtr, trackId, titleId, envId, transactionId, name, progression, prerequisiteEntitlementId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateTrackTriggerCompleteDelegateWrapper(UpdateTrackTriggerCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateTrackTriggerCompleteDelegateWrapper(swigCPtr, UpdateTrackTriggerCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateTrackTrigger(string trackId, string titleId, string envId, string triggerId, string transactionId, string name, int progression, string prerequisiteEntitlementId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateTrackTrigger(swigCPtr, trackId, titleId, envId, triggerId, transactionId, name, progression, prerequisiteEntitlementId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteTrackTriggerCompleteDelegateWrapper(DeleteTrackTriggerCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteTrackTriggerCompleteDelegateWrapper(swigCPtr, DeleteTrackTriggerCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteTrackTrigger(string trackId, string titleId, string envId, string triggerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteTrackTrigger(swigCPtr, trackId, titleId, envId, triggerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateTrackLevelCompleteDelegateWrapper(CreateTrackLevelCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateTrackLevelCompleteDelegateWrapper(swigCPtr, CreateTrackLevelCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateTrackLevel(string trackId, string titleId, string envId, string name, int progressionAmount, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateTrackLevel(swigCPtr, trackId, titleId, envId, name, progressionAmount, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateTrackLevelCompleteDelegateWrapper(UpdateTrackLevelCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateTrackLevelCompleteDelegateWrapper(swigCPtr, UpdateTrackLevelCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateTrackLevel(string trackId, string titleId, string envId, string levelId, string name, int progressionAmount, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateTrackLevel(swigCPtr, trackId, titleId, envId, levelId, name, progressionAmount, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteTrackLevelCompleteDelegateWrapper(DeleteTrackLevelCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteTrackLevelCompleteDelegateWrapper(swigCPtr, DeleteTrackLevelCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteTrackLevel(string trackId, string titleId, string envId, string levelId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteTrackLevel(swigCPtr, trackId, titleId, envId, levelId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetProgressionTrackValuesForPlayerCompleteAutomationDelegateWrapper(GetProgressionTrackValuesForPlayerCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetProgressionTrackValuesForPlayerCompleteAutomationDelegateWrapper(swigCPtr, GetProgressionTrackValuesForPlayerCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetProgressionTrackValuesForPlayer(string titleId, string envId, string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetProgressionTrackValuesForPlayer(swigCPtr, titleId, envId, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateProgressionTreeCompleteDelegateWrapper(CreateProgressionTreeCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateProgressionTreeCompleteDelegateWrapper(swigCPtr, CreateProgressionTreeCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateProgressionTree(string titleId, string envId, string name, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateProgressionTree(swigCPtr, titleId, envId, name, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateProgressionTreeCompleteDelegateWrapper(UpdateProgressionTreeCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateProgressionTreeCompleteDelegateWrapper(swigCPtr, UpdateProgressionTreeCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateProgressionTree(string titleId, string envId, string treeId, string name, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateProgressionTree(swigCPtr, titleId, envId, treeId, name, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListProgressionTreesCompleteDelegateWrapper(ListProgressionTreesCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListProgressionTreesCompleteDelegateWrapper(swigCPtr, ListProgressionTreesCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListProgressionTrees(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListProgressionTrees(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteProgressionTreeCompleteDelegateWrapper(DeleteProgressionTreeCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteProgressionTreeCompleteDelegateWrapper(swigCPtr, DeleteProgressionTreeCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteProgressionTree(string titleId, string envId, string treeId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteProgressionTree(swigCPtr, titleId, envId, treeId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateProgressionTreeNodeCompleteDelegateWrapper(CreateProgressionTreeNodeCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateProgressionTreeNodeCompleteDelegateWrapper(swigCPtr, CreateProgressionTreeNodeCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateProgressionTreeNode(string titleId, string envId, string treeId, TreeNodeDefinition nodeDefinition, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateProgressionTreeNode(swigCPtr, titleId, envId, treeId, TreeNodeDefinition.getCPtr(nodeDefinition), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateProgressionTreeNodeCompleteDelegateWrapper(UpdateProgressionTreeNodeCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateProgressionTreeNodeCompleteDelegateWrapper(swigCPtr, UpdateProgressionTreeNodeCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateProgressionTreeNode(string titleId, string envId, string treeId, TreeNodeDefinition nodeDefinition, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateProgressionTreeNode(swigCPtr, titleId, envId, treeId, TreeNodeDefinition.getCPtr(nodeDefinition), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteProgressionTreeNodeCompleteDelegateWrapper(DeleteProgressionTreeNodeCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteProgressionTreeNodeCompleteDelegateWrapper(swigCPtr, DeleteProgressionTreeNodeCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteProgressionTreeNode(string titleId, string envId, string treeId, string nodeId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteProgressionTreeNode(swigCPtr, titleId, envId, treeId, nodeId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUnlockProgressionTreeNodeAutomationCompleteDelegateWrapper(UnlockProgressionTreeNodeAutomationCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUnlockProgressionTreeNodeAutomationCompleteDelegateWrapper(swigCPtr, UnlockProgressionTreeNodeAutomationCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UnlockProgressionTreeNode(string titleId, string envId, string treeId, string nodeId, string playerId, bool ignoreCost, bool ignorePrerequisites, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UnlockProgressionTreeNode(swigCPtr, titleId, envId, treeId, nodeId, playerId, ignoreCost, ignorePrerequisites, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetLockProgressionTreeNodeCompleteDelegateWrapper(LockProgressionTreeNodeAutomationCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetLockProgressionTreeNodeCompleteDelegateWrapper(swigCPtr, LockProgressionTreeNodeAutomationCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool LockProgressionTreeNode(string titleId, string envId, string treeId, string nodeId, string playerId, bool refund_costs, bool rewind_rewards, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_LockProgressionTreeNode(swigCPtr, titleId, envId, treeId, nodeId, playerId, refund_costs, rewind_rewards, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateProgressionTreeTrackCompleteDelegateWrapper(CreateProgressionTreeTrackCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateProgressionTreeTrackCompleteDelegateWrapper(swigCPtr, CreateProgressionTreeTrackCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateProgressionTreeTrack(string titleId, string envId, string treeId, string name, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateProgressionTreeTrack(swigCPtr, titleId, envId, treeId, name, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateProgressionTreeBindingsCompleteDelegateWrapper(CreateProgressionTreeBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateProgressionTreeBindingsCompleteDelegateWrapper(swigCPtr, CreateProgressionTreeBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateProgressionTreeBindings(string titleId, string envId, string deploymentId, string treeId, bool visible, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateProgressionTreeBindings(swigCPtr, titleId, envId, deploymentId, treeId, visible, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateProgressionTreeBindingsCompleteDelegateWrapper(UpdateProgressionTreeBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateProgressionTreeBindingsCompleteDelegateWrapper(swigCPtr, UpdateProgressionTreeBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateProgressionTreeBindings(string titleId, string envId, string deploymentId, string treeId, string id, bool visible, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateProgressionTreeBindings(swigCPtr, titleId, envId, deploymentId, treeId, id, visible, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListProgressionTreeBindingsCompleteDelegateWrapper(ListProgressionTreeBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListProgressionTreeBindingsCompleteDelegateWrapper(swigCPtr, ListProgressionTreeBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListProgressionTreeBindings(string titleId, string envId, string deploymentId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListProgressionTreeBindings(swigCPtr, titleId, envId, deploymentId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetDeleteProgressionTreeBindingsCompleteDelegateWrapper(DeleteProgressionTreeBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetDeleteProgressionTreeBindingsCompleteDelegateWrapper(swigCPtr, DeleteProgressionTreeBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool DeleteProgressionTreeBindings(string titleId, string envId, string deploymentId, string treeId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_DeleteProgressionTreeBindings(swigCPtr, titleId, envId, deploymentId, treeId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetProgressionTreeBindingsCompleteDelegateWrapper(GetProgressionTreeBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetProgressionTreeBindingsCompleteDelegateWrapper(swigCPtr, GetProgressionTreeBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetProgressionTreeBindings(string titleId, string envId, string deploymentId, string treeId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetProgressionTreeBindings(swigCPtr, titleId, envId, deploymentId, treeId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateProgressionTrackBindingsCompleteDelegateWrapper(CreateProgressionTrackBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateProgressionTrackBindingsCompleteDelegateWrapper(swigCPtr, CreateProgressionTrackBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateProgressionTrackBindings(string titleId, string envId, string deploymentId, string trackId, bool visible, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateProgressionTrackBindings(swigCPtr, titleId, envId, deploymentId, trackId, visible, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateProgressionTrackBindingsCompleteDelegateWrapper(UpdateProgressionTrackBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateProgressionTrackBindingsCompleteDelegateWrapper(swigCPtr, UpdateProgressionTrackBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateProgressionTrackBindings(string titleId, string envId, string deploymentId, string trackId, string id, bool visible, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateProgressionTrackBindings(swigCPtr, titleId, envId, deploymentId, trackId, id, visible, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListProgressionTrackBindingsCompleteDelegateWrapper(ListProgressionTrackBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListProgressionTrackBindingsCompleteDelegateWrapper(swigCPtr, ListProgressionTrackBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListProgressionTrackBindings(string titleId, string envId, string deploymentId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListProgressionTrackBindings(swigCPtr, titleId, envId, deploymentId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetProgressionTrackBindingsCompleteDelegateWrapper(GetProgressionTrackBindingsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetProgressionTrackBindingsCompleteDelegateWrapper(swigCPtr, GetProgressionTrackBindingsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetProgressionTrackBindings(string titleId, string envId, string deploymentId, string trackId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetProgressionTrackBindings(swigCPtr, titleId, envId, deploymentId, trackId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetGetSubscriptionsForPlayerCompleteDelegateWrapper(AutomationGetPlayerSubscriptionCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetGetSubscriptionsForPlayerCompleteDelegateWrapper(swigCPtr, AutomationGetPlayerSubscriptionCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool GetSubscriptionsForPlayer(string titleId, string envId, string playerId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_GetSubscriptionsForPlayer(swigCPtr, titleId, envId, playerId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetCreateSubscriptionCatalogItemCompleteDelegateWrapper(AutomationCreateSubscriptionCatalogItemCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetCreateSubscriptionCatalogItemCompleteDelegateWrapper(swigCPtr, AutomationCreateSubscriptionCatalogItemCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool CreateSubscriptionCatalogItem(string titleId, string envId, string name, string externalServiceName, SubscriptionPricingVector pricingAndTerms, string sku, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_CreateSubscriptionCatalogItem(swigCPtr, titleId, envId, name, externalServiceName, SubscriptionPricingVector.getCPtr(pricingAndTerms), sku, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListSubscriptionCatalogItemsCompleteDelegateWrapper(AutomationListSubscriptionCatalogItemsCompleteDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListSubscriptionCatalogItemsCompleteDelegateWrapper(swigCPtr, AutomationListSubscriptionCatalogItemsCompleteDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListSubscriptionCatalogItems(string titleId, string envId, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListSubscriptionCatalogItems(swigCPtr, titleId, envId, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetListGameSessionsCompleteDelegateWrapper(ListGameSessionsCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetListGameSessionsCompleteDelegateWrapper(swigCPtr, ListGameSessionsCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ListGameSessions(string titleId, string envId, int pageSize, int offset, string region, string partition, int min_empty_slots, int max_empty_slots, string session_name_search, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_ListGameSessions(swigCPtr, titleId, envId, pageSize, offset, region, partition, min_empty_slots, max_empty_slots, session_name_search, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetRegisterGameSessionCompleteDelegateWrapper(RegisterGameSessionCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetRegisterGameSessionCompleteDelegateWrapper(swigCPtr, RegisterGameSessionCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool RegisterGameSession(string titleId, string envId, string gameSessionId, string provider, string gameSessionName, string requiredTags, string ip, int port, int maxPlayerCount, string region, string partition, StringKeyValueMap extraProperties, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_RegisterGameSession(swigCPtr, titleId, envId, gameSessionId, provider, gameSessionName, requiredTags, ip, port, maxPlayerCount, region, partition, StringKeyValueMap.getCPtr(extraProperties), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUpdateGameSessionCompleteDelegateWrapper(UpdateGameSessionCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUpdateGameSessionCompleteDelegateWrapper(swigCPtr, UpdateGameSessionCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UpdateGameSession(string titleId, string envId, string id, string gameSessionId, string provider, string gameSessionName, string ip, int port, int maxPlayerCount, int currentPlayerCount, string region, StringKeyValueMap extraProperties, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UpdateGameSession(swigCPtr, titleId, envId, id, gameSessionId, provider, gameSessionName, ip, port, maxPlayerCount, currentPlayerCount, region, StringKeyValueMap.getCPtr(extraProperties), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetUnregisterGameSessionCompleteDelegateWrapper(UnregisterGameSessionCompleteAutomationDelegateWrapper wrapper)
	{
		MothershipApiPINVOKE.MothershipAutomationApiClient_SetUnregisterGameSessionCompleteDelegateWrapper(swigCPtr, UnregisterGameSessionCompleteAutomationDelegateWrapper.getCPtr(wrapper));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool UnregisterGameSession(string titleId, string envId, string id, IntPtr userData)
	{
		bool result = MothershipApiPINVOKE.MothershipAutomationApiClient_UnregisterGameSession(swigCPtr, titleId, envId, id, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
