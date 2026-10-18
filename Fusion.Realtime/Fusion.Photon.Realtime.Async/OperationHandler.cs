using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Photon.Realtime.Async;

internal class OperationHandler : IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks
{
	public PhotonConnectionCallbacks ConnectionCallbacks = new PhotonConnectionCallbacks();

	public PhotonMatchmakingCallbacks MatchmakingCallbacks = new PhotonMatchmakingCallbacks();

	public PhotonLobbyCallbacks LobbyCallbacks = new PhotonLobbyCallbacks();

	private bool _throwOnErrors;

	private TaskCompletionSource<short> _result;

	private CancellationTokenSource _cancellation;

	private const float OPERATION_TIMEOUT_SEC = 30f;

	public Task<short> Task => _result.Task;

	public TaskCompletionSource<short> CompletionSource => _result;

	public CancellationToken Token => _cancellation.Token;

	public bool IsCancellationRequested => _cancellation.IsCancellationRequested;

	public OperationHandler(bool throwOnErrors = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		_result = new TaskCompletionSource<short>();
		_cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30.0));
		_cancellation.Token.Register(Expire);
		_throwOnErrors = throwOnErrors;
		if (externalCancellationToken != default(CancellationToken))
		{
			externalCancellationToken.Register(Cancel);
		}
	}

	public void SetResult(short result)
	{
		if (_result.TrySetResult(result))
		{
			if (!_cancellation.IsCancellationRequested)
			{
				_cancellation.Cancel();
			}
			_cancellation.Dispose();
		}
	}

	public void SetException(Exception e)
	{
		if (_result.TrySetException(e))
		{
			if (!_cancellation.IsCancellationRequested)
			{
				_cancellation.Cancel();
			}
			_cancellation.Dispose();
		}
	}

	private void Expire()
	{
		SetException(new OperationTimeoutException("Operation timed out"));
	}

	private void Cancel()
	{
		SetException(new OperationCanceledException("Operation cancelled."));
	}

	public void OnConnected()
	{
		ConnectionCallbacks.ConnectedToNameServer?.Invoke();
	}

	public void OnConnectedToMaster()
	{
		if (ConnectionCallbacks.ConnectedToMaster != null)
		{
			ConnectionCallbacks.ConnectedToMaster();
		}
		else
		{
			SetResult(0);
		}
	}

	public void OnCustomAuthenticationFailed(string debugMessage)
	{
		if (ConnectionCallbacks.CustomAuthenticationFailed != null)
		{
			ConnectionCallbacks.CustomAuthenticationFailed(debugMessage);
		}
		else
		{
			SetException(new AuthenticationFailedException(debugMessage));
		}
	}

	public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
		ConnectionCallbacks.CustomAuthenticationResponse?.Invoke(data);
	}

	public void OnDisconnected(DisconnectCause cause)
	{
		if (ConnectionCallbacks.Disconnected != null)
		{
			ConnectionCallbacks.Disconnected(cause);
		}
		else
		{
			SetException(new DisconnectException(cause));
		}
	}

	public void OnRegionListReceived(RegionHandler regionHandler)
	{
		ConnectionCallbacks.RegionListReceived?.Invoke(regionHandler);
	}

	public void OnCreatedRoom()
	{
		MatchmakingCallbacks.CreatedRoom?.Invoke();
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
		if (MatchmakingCallbacks.CreateRoomFailed != null)
		{
			MatchmakingCallbacks.CreateRoomFailed(returnCode, message);
			return;
		}
		if (_throwOnErrors)
		{
			SetException(new OperationException(returnCode, message));
			return;
		}
		InternalLogStreams.LogError?.Log(message);
		SetResult(returnCode);
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		MatchmakingCallbacks.FriendListUpdate?.Invoke(friendList);
	}

	public void OnJoinedRoom()
	{
		if (MatchmakingCallbacks.JoinedRoom != null)
		{
			MatchmakingCallbacks.JoinedRoom();
		}
		else
		{
			SetResult(0);
		}
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
		if (MatchmakingCallbacks.JoinRoomRandomFailed != null)
		{
			MatchmakingCallbacks.JoinRoomRandomFailed(returnCode, message);
			return;
		}
		if (_throwOnErrors)
		{
			SetException(new OperationException(returnCode, message));
			return;
		}
		InternalLogStreams.LogError?.Log(message);
		SetResult(returnCode);
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
		if (MatchmakingCallbacks.JoinRoomFailed != null)
		{
			MatchmakingCallbacks.JoinRoomFailed(returnCode, message);
			return;
		}
		if (_throwOnErrors)
		{
			SetException(new OperationException(returnCode, message));
			return;
		}
		InternalLogStreams.LogError?.Log(message);
		SetResult(returnCode);
	}

	public void OnLeftRoom()
	{
		if (MatchmakingCallbacks.LeftRoom != null)
		{
			MatchmakingCallbacks.LeftRoom();
		}
		else
		{
			SetResult(0);
		}
	}

	public void OnJoinedLobby()
	{
		if (LobbyCallbacks.JoinedLobby != null)
		{
			LobbyCallbacks.JoinedLobby();
		}
		else
		{
			SetResult(0);
		}
	}

	public void OnLeftLobby()
	{
	}

	public void OnRoomListUpdate(List<RoomInfo> roomList)
	{
	}

	public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
	{
	}
}
