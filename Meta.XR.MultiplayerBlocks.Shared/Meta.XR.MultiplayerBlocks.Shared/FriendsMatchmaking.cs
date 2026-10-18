using System;
using System.Threading.Tasks;
using Meta.XR.ImmersiveDebugger;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.MultiplayerBlocks.Shared;

public class FriendsMatchmaking : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Destination's API name obtained from developer.oculus.com under Engagement > Destinations.")]
	private string destinationApi = "destinationApi";

	[SerializeField]
	[Tooltip("Optional message to be sent when inviting friends to join a game room.")]
	private string inviteMessage = "Let's play together!";

	[SerializeField]
	[Tooltip("Maximum number of retries should a Platform SDK request fail.")]
	private uint maxRetries = 3u;

	[SerializeField]
	private UnityEvent<CustomMatchmaking.RoomOperationResult> onMatchRequestFound;

	[SerializeField]
	private UnityEvent<Message<LaunchInvitePanelFlowResult>> onInvitationsSent;

	[SerializeField]
	private UnityEvent<Message<GroupPresenceLeaveIntent>> onLeaveIntentReceived;

	private CustomMatchmaking _customMatchmaking;

	private const string DebugCategory = "Friends Matchmaking";

	public string DestinationApi
	{
		get
		{
			return destinationApi;
		}
		set
		{
			destinationApi = value;
		}
	}

	public string InviteMessage
	{
		get
		{
			return inviteMessage;
		}
		set
		{
			inviteMessage = value;
		}
	}

	public uint MaxRetries
	{
		get
		{
			return maxRetries;
		}
		set
		{
			maxRetries = value;
		}
	}

	private void Awake()
	{
		_customMatchmaking = UnityEngine.Object.FindObjectOfType<CustomMatchmaking>();
		if (_customMatchmaking == null)
		{
			throw new InvalidOperationException("FriendsMatchmaking] No CustomMatchmaking component was found in the scene as is a requirement of FriendsMatchmaking");
		}
		PlatformInit.GetEntitlementInformation(OnEntitlementFinished);
	}

	private void OnEnable()
	{
		_customMatchmaking.onRoomLeaveFinished.AddListener(ClearGroupPresenceCallback);
		_customMatchmaking.onRoomCreationFinished.AddListener(OnRoomOperationResult);
		_customMatchmaking.onRoomJoinFinished.AddListener(OnRoomOperationResult);
	}

	private void OnDisable()
	{
		_customMatchmaking.onRoomLeaveFinished.RemoveListener(ClearGroupPresenceCallback);
		_customMatchmaking.onRoomCreationFinished.RemoveListener(OnRoomOperationResult);
		_customMatchmaking.onRoomJoinFinished.RemoveListener(OnRoomOperationResult);
	}

	[DebugMember(DebugColor.Gray, Category = "Friends Matchmaking")]
	public void LaunchFriendsInvitePanel()
	{
		LaunchFriendsInvitePanelAsync();
	}

	public Task<Message<InvitePanelResultInfo>> LaunchFriendsInvitePanelAsync(InviteOptions inviteOptions = null)
	{
		TaskCompletionSource<Message<InvitePanelResultInfo>> tcs = new TaskCompletionSource<Message<InvitePanelResultInfo>>();
		GroupPresence.LaunchInvitePanel(inviteOptions ?? new InviteOptions()).OnComplete(delegate(Message<InvitePanelResultInfo> message)
		{
			if (message.IsError)
			{
				Debug.LogError("[FriendsMatchmaking] LaunchFriendsInvitePanelAsync failed: " + message.GetError().Message);
			}
			tcs.SetResult(message);
		});
		return tcs.Task;
	}

	[DebugMember(DebugColor.Gray, Category = "Friends Matchmaking")]
	public void LaunchRosterPanel()
	{
		LaunchRosterPanelAsync();
	}

	public Task<Message> LaunchRosterPanelAsync(RosterOptions rosterOptions = null)
	{
		TaskCompletionSource<Message> tcs = new TaskCompletionSource<Message>();
		GroupPresence.LaunchRosterPanel(rosterOptions ?? new RosterOptions()).OnComplete(delegate(Message message)
		{
			if (message.IsError)
			{
				Debug.LogError("[FriendsMatchmaking] LaunchRosterPanelAsync failed: " + message.GetError().Message);
			}
			tcs.SetResult(message);
		});
		return tcs.Task;
	}

	protected virtual async void OnRoomOperationResult(CustomMatchmaking.RoomOperationResult result)
	{
		if (result.IsSuccess)
		{
			await RegisterGameRoom(result.RoomToken, result.RoomPassword);
		}
	}

	protected virtual async Task JoinRoom(string roomId, string roomPassword)
	{
		if (_customMatchmaking.IsConnected && _customMatchmaking.ConnectedRoomToken != roomId)
		{
			_customMatchmaking.LeaveRoom();
		}
		await _customMatchmaking.JoinRoom(roomId, roomPassword);
	}

	protected virtual void ClearGroupPresenceCallback()
	{
		ClearGroupPresence();
	}

	private async Task RegisterGameRoom(string roomId, string roomPassword = null)
	{
		Message message = null;
		for (int i = 0; i < MaxRetries; i++)
		{
			message = await SetGroupPresence(GetGroupPresenceOptions(roomId, roomPassword));
			if (!message.IsError)
			{
				return;
			}
		}
		Debug.LogError("[FriendsMatchmaking] Max retries reached, failed to register game room: " + message?.GetError().Message);
	}

	private static Task<Message> ClearGroupPresence()
	{
		TaskCompletionSource<Message> tcs = new TaskCompletionSource<Message>();
		GroupPresence.Clear().OnComplete(delegate(Message message)
		{
			if (message.IsError)
			{
				Debug.LogError("[FriendsMatchmaking] ClearGroupPresence failed: " + message.GetError().Message);
			}
			tcs.SetResult(message);
		});
		return tcs.Task;
	}

	private static Task<Message> SetGroupPresence(GroupPresenceOptions groupPresenceOptions)
	{
		TaskCompletionSource<Message> tcs = new TaskCompletionSource<Message>();
		GroupPresence.Set(groupPresenceOptions).OnComplete(delegate(Message message)
		{
			tcs.SetResult(message);
		});
		return tcs.Task;
	}

	private void OnEntitlementFinished(PlatformInfo info)
	{
		GroupPresence.SetJoinIntentReceivedNotificationCallback(OnJoinIntentReceived);
		GroupPresence.SetInvitationsSentNotificationCallback(OnInvitationsSent);
		GroupPresence.SetLeaveIntentReceivedNotificationCallback(OnLeaveIntentNotification);
	}

	protected virtual async void OnJoinIntentReceived(Message<GroupPresenceJoinIntent> message)
	{
		var (text, roomPassword) = CustomMatchmakingUtils.ExtractMatchInfoFromSessionId(message.Data.MatchSessionId);
		onMatchRequestFound?.Invoke(new CustomMatchmaking.RoomOperationResult
		{
			RoomToken = text,
			RoomPassword = roomPassword,
			ErrorMessage = (message.IsError ? message.GetError().Message : null)
		});
		await JoinRoom(text, roomPassword);
	}

	private void OnInvitationsSent(Message<LaunchInvitePanelFlowResult> message)
	{
		onInvitationsSent?.Invoke(message);
	}

	private void OnLeaveIntentNotification(Message<GroupPresenceLeaveIntent> message)
	{
		onLeaveIntentReceived?.Invoke(message);
	}

	protected virtual GroupPresenceOptions GetGroupPresenceOptions(string roomId, string roomPassword = null)
	{
		GroupPresenceOptions groupPresenceOptions = new GroupPresenceOptions();
		groupPresenceOptions.SetIsJoinable(value: true);
		groupPresenceOptions.SetDestinationApiName(DestinationApi);
		string text = CustomMatchmakingUtils.EncodeMatchInfoToSessionId(roomId, roomPassword);
		groupPresenceOptions.SetLobbySessionId(text);
		groupPresenceOptions.SetMatchSessionId(text);
		if (!string.IsNullOrEmpty(InviteMessage))
		{
			groupPresenceOptions.SetDeeplinkMessageOverride(InviteMessage);
		}
		return groupPresenceOptions;
	}
}
