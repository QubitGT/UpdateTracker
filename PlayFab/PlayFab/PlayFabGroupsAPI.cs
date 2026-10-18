using System;
using System.Collections.Generic;
using PlayFab.GroupsModels;
using PlayFab.Internal;

namespace PlayFab;

public static class PlayFabGroupsAPI
{
	static PlayFabGroupsAPI()
	{
	}

	public static bool IsEntityLoggedIn()
	{
		return PlayFabSettings.staticPlayer.IsEntityLoggedIn();
	}

	public static void ForgetAllCredentials()
	{
		PlayFabSettings.staticPlayer.ForgetAllCredentials();
	}

	public static void AcceptGroupApplication(AcceptGroupApplicationRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/AcceptGroupApplication", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void AcceptGroupInvitation(AcceptGroupInvitationRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/AcceptGroupInvitation", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void AddMembers(AddMembersRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/AddMembers", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ApplyToGroup(ApplyToGroupRequest request, Action<ApplyToGroupResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/ApplyToGroup", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void BlockEntity(BlockEntityRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/BlockEntity", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ChangeMemberRole(ChangeMemberRoleRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/ChangeMemberRole", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void CreateGroup(CreateGroupRequest request, Action<CreateGroupResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/CreateGroup", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void CreateRole(CreateGroupRoleRequest request, Action<CreateGroupRoleResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/CreateRole", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void DeleteGroup(DeleteGroupRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/DeleteGroup", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void DeleteRole(DeleteRoleRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/DeleteRole", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void GetGroup(GetGroupRequest request, Action<GetGroupResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/GetGroup", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void InviteToGroup(InviteToGroupRequest request, Action<InviteToGroupResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/InviteToGroup", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void IsMember(IsMemberRequest request, Action<IsMemberResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/IsMember", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ListGroupApplications(ListGroupApplicationsRequest request, Action<ListGroupApplicationsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/ListGroupApplications", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ListGroupBlocks(ListGroupBlocksRequest request, Action<ListGroupBlocksResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/ListGroupBlocks", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ListGroupInvitations(ListGroupInvitationsRequest request, Action<ListGroupInvitationsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/ListGroupInvitations", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ListGroupMembers(ListGroupMembersRequest request, Action<ListGroupMembersResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/ListGroupMembers", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ListMembership(ListMembershipRequest request, Action<ListMembershipResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/ListMembership", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ListMembershipOpportunities(ListMembershipOpportunitiesRequest request, Action<ListMembershipOpportunitiesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/ListMembershipOpportunities", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void RemoveGroupApplication(RemoveGroupApplicationRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/RemoveGroupApplication", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void RemoveGroupInvitation(RemoveGroupInvitationRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/RemoveGroupInvitation", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void RemoveMembers(RemoveMembersRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/RemoveMembers", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void UnblockEntity(UnblockEntityRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/UnblockEntity", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void UpdateGroup(UpdateGroupRequest request, Action<UpdateGroupResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/UpdateGroup", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void UpdateRole(UpdateGroupRoleRequest request, Action<UpdateGroupRoleResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Group/UpdateRole", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}
}
