using System;
using System.Threading.Tasks;
using Meta.XR.BuildingBlocks;
using Meta.XR.ImmersiveDebugger;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.MultiplayerBlocks.Shared;

[ExecuteAlways]
public class CustomMatchmaking : MonoBehaviour
{
	public interface ICustomMatchmakingBehaviour
	{
		bool IsConnected { get; }

		string ConnectedRoomToken { get; }

		bool SupportsRoomPassword { get; }

		Task<RoomOperationResult> CreateRoom(RoomCreationOptions options);

		Task<RoomOperationResult> JoinRoom(string roomToken, string roomPassword = null);

		Task<RoomOperationResult> JoinOpenRoom(string lobbyName);

		void LeaveRoom();
	}

	public struct RoomCreationOptions
	{
		public string RoomPassword;

		public int MaxPlayersPerRoom;

		public bool IsPrivate;

		public string LobbyName;
	}

	public struct RoomOperationResult
	{
		public string ErrorMessage;

		public string RoomToken;

		public string RoomPassword;

		public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
	}

	[HideInInspector]
	[Tooltip("Event called when a CreateRoom operation finished")]
	public UnityEvent<RoomOperationResult> onRoomCreationFinished;

	[HideInInspector]
	[Tooltip("Event called when a JoinRoom operation finished")]
	public UnityEvent<RoomOperationResult> onRoomJoinFinished;

	[HideInInspector]
	[Tooltip("Event called when a LeaveRoom operation finished")]
	public UnityEvent onRoomLeaveFinished;

	[SerializeField]
	[HideInInspector]
	[Tooltip("Name of the game lobby the created room belongs to.")]
	private string lobbyName = "myLobby";

	[SerializeField]
	[HideInInspector]
	[Tooltip("Indicates whether this game room is private.")]
	private bool isPrivate;

	[SerializeField]
	[HideInInspector]
	[Tooltip("The maximum number of players allowed in this game room.")]
	private int maxPlayersPerRoom = 4;

	[SerializeField]
	[HideInInspector]
	[Tooltip("Indicates whether a password should be required for other players to be able to join this game room.")]
	private bool isPasswordProtected;

	protected ICustomMatchmakingBehaviour MatchmakingBehaviour;

	private const string DebugCategory = "Custom Matchmaking";

	public string LobbyName
	{
		get
		{
			return lobbyName;
		}
		set
		{
			lobbyName = value;
		}
	}

	public bool IsPrivate
	{
		get
		{
			return isPrivate;
		}
		set
		{
			isPrivate = value;
		}
	}

	public int MaxPlayersPerRoom
	{
		get
		{
			return maxPlayersPerRoom;
		}
		set
		{
			maxPlayersPerRoom = value;
		}
	}

	public bool IsPasswordProtected
	{
		get
		{
			return isPasswordProtected;
		}
		set
		{
			isPasswordProtected = value;
		}
	}

	[DebugMember(DebugColor.Gray, Category = "Custom Matchmaking", Tweakable = false)]
	public bool IsConnected => MatchmakingBehaviour?.IsConnected ?? false;

	[DebugMember(DebugColor.Gray, Category = "Custom Matchmaking", Tweakable = false)]
	public string ConnectedRoomToken => MatchmakingBehaviour?.ConnectedRoomToken ?? string.Empty;

	internal bool SupportsRoomPassword => MatchmakingBehaviour?.SupportsRoomPassword ?? false;

	private void OnEnable()
	{
		MatchmakingBehaviour = this.GetInterfaceComponent<ICustomMatchmakingBehaviour>();
		if (MatchmakingBehaviour == null && Application.isPlaying)
		{
			throw new InvalidOperationException("Using CustomMatchmaking without an ICustomMatchmakingBehaviour present in the game object.");
		}
	}

	[DebugMember(DebugColor.Gray, Category = "Custom Matchmaking")]
	public async Task<RoomOperationResult> CreateRoom()
	{
		return await CreateRoom(new RoomCreationOptions
		{
			RoomPassword = ((IsPasswordProtected && SupportsRoomPassword) ? GenerateRoomPassword() : null),
			MaxPlayersPerRoom = MaxPlayersPerRoom,
			LobbyName = LobbyName,
			IsPrivate = IsPrivate
		});
	}

	public async Task<RoomOperationResult> CreateRoom(RoomCreationOptions options)
	{
		RoomOperationResult roomOperationResult = await MatchmakingBehaviour.CreateRoom(options);
		if (!roomOperationResult.IsSuccess)
		{
			Debug.LogWarning("[CustomMatchmaking] Room creation failed: " + roomOperationResult.ErrorMessage);
		}
		onRoomCreationFinished?.Invoke(roomOperationResult);
		return roomOperationResult;
	}

	public async Task<RoomOperationResult> JoinRoom(string roomToken, string roomPassword)
	{
		RoomOperationResult roomOperationResult = await MatchmakingBehaviour.JoinRoom(roomToken, roomPassword);
		if (!roomOperationResult.IsSuccess)
		{
			Debug.LogWarning("[CustomMatchmaking] Room join failed: " + roomOperationResult.ErrorMessage);
		}
		onRoomJoinFinished?.Invoke(roomOperationResult);
		return roomOperationResult;
	}

	public async Task<RoomOperationResult> JoinOpenRoom(string roomLobby)
	{
		RoomOperationResult roomOperationResult = await MatchmakingBehaviour.JoinOpenRoom(roomLobby);
		if (!roomOperationResult.IsSuccess)
		{
			Debug.LogWarning("[CustomMatchmaking] Join open room failed: " + roomOperationResult.ErrorMessage);
		}
		onRoomJoinFinished?.Invoke(roomOperationResult);
		return roomOperationResult;
	}

	public void LeaveRoom()
	{
		MatchmakingBehaviour.LeaveRoom();
		onRoomLeaveFinished?.Invoke();
	}

	protected virtual string GenerateRoomPassword()
	{
		return RunTimeUtils.GenerateRandomString(16);
	}
}
