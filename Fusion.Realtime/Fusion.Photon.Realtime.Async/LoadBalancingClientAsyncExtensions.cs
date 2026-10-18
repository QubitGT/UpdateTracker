using System;
using System.Threading;
using System.Threading.Tasks;
using Fusion.Async;

namespace Fusion.Photon.Realtime.Async;

internal static class LoadBalancingClientAsyncExtensions
{
	private const int SERVICE_INTERVAL_MS = 10;

	public static Task<RegionHandler> GetRegionsAsync(this LoadBalancingClient client, bool throwOnError = true, bool createServiceTask = true, CancellationToken externalCancelationToken = default(CancellationToken))
	{
		if (!client.ConnectToNameServer())
		{
			return Task.FromException<RegionHandler>(new OperationStartException("Failed to get regions"));
		}
		TaskCompletionSource<RegionHandler> result = new TaskCompletionSource<RegionHandler>(externalCancelationToken);
		OperationHandler handler = client.CreateOpHandler(throwOnError, createServiceTask, externalCancelationToken);
		PhotonConnectionCallbacks connectionCallbacks = handler.ConnectionCallbacks;
		connectionCallbacks.Disconnected = (Action<DisconnectCause>)Delegate.Combine(connectionCallbacks.Disconnected, (Action<DisconnectCause>)delegate(DisconnectCause cause)
		{
			handler.SetResult(0);
			result.SetException(new OperationStartException($"Failed to get regions. Disconnection cause: {cause}"));
		});
		PhotonConnectionCallbacks connectionCallbacks2 = handler.ConnectionCallbacks;
		connectionCallbacks2.RegionListReceived = (Action<RegionHandler>)Delegate.Combine(connectionCallbacks2.RegionListReceived, (Action<RegionHandler>)delegate(RegionHandler regionHandler)
		{
			regionHandler.PingMinimumOfRegions(delegate(RegionHandler regionHandlerWithPing)
			{
				if (externalCancelationToken.IsCancellationRequested)
				{
					result.SetResult(null);
					handler.SetResult(0);
				}
				else
				{
					result.SetResult(regionHandlerWithPing);
					handler.SetResult(0);
				}
			}, string.Empty);
		});
		return result.Task;
	}

	public static Task ConnectUsingSettingsAsync(this LoadBalancingClient client, AppSettings appSettings, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (client.State != ClientState.Disconnected && client.State != 0)
		{
			return Task.FromException(new OperationStartException("Client still connected"));
		}
		if (!client.ConnectUsingSettings(appSettings))
		{
			return Task.FromException(new OperationStartException("Failed to start connecting"));
		}
		return client.CreateOpHandler(throwOnErrors: true, createServiceTask, externalCancellationToken).Task;
	}

	public static Task ReconnectAndRejoinAsync(this LoadBalancingClient client, bool throwOnError = true, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (client.State != ClientState.Disconnected)
		{
			return Task.FromException(new OperationStartException("Client still connected"));
		}
		if (!client.ReconnectAndRejoin())
		{
			return Task.FromException(new OperationStartException("Failed to start reconnecting"));
		}
		return client.CreateOpHandler(throwOnError, createServiceTask, externalCancellationToken).Task;
	}

	public static Task DisconnectAsync(this LoadBalancingClient client, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (client == null)
		{
			return Task.CompletedTask;
		}
		if (client.State == ClientState.Disconnected)
		{
			return Task.CompletedTask;
		}
		OperationHandler handler = client.CreateOpHandler(throwOnErrors: true, createServiceTask, externalCancellationToken);
		PhotonConnectionCallbacks connectionCallbacks = handler.ConnectionCallbacks;
		connectionCallbacks.Disconnected = (Action<DisconnectCause>)Delegate.Combine(connectionCallbacks.Disconnected, (Action<DisconnectCause>)delegate(DisconnectCause cause)
		{
			InternalLogStreams.LogInfo?.Log($"Disconnected: {cause}");
			handler.SetResult(0);
		});
		client.Disconnect();
		return handler.Task;
	}

	public static Task LeaveRoomAsync(this LoadBalancingClient client, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (client == null)
		{
			return Task.CompletedTask;
		}
		if (client.State == ClientState.Disconnected || !client.InRoom)
		{
			return Task.CompletedTask;
		}
		OperationHandler handler = client.CreateOpHandler(throwOnErrors: true, createServiceTask, externalCancellationToken);
		PhotonMatchmakingCallbacks matchmakingCallbacks = handler.MatchmakingCallbacks;
		matchmakingCallbacks.LeftRoom = (Action)Delegate.Combine(matchmakingCallbacks.LeftRoom, (Action)delegate
		{
			InternalLogStreams.LogInfo?.Log("Left Room");
			handler.SetResult(0);
		});
		client.OpLeaveRoom(becomeInactive: false);
		return handler.Task;
	}

	public static Task<short> CreateRoomAsync(this LoadBalancingClient client, EnterRoomParams enterRoomParams, bool throwOnError = true, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (!client.OpCreateRoom(enterRoomParams))
		{
			return Task.FromException<short>(new OperationStartException("Failed to send CreateRoom operation"));
		}
		return client.CreateOpHandler(throwOnError, createServiceTask, externalCancellationToken).Task;
	}

	public static Task<short> CreateOrJoinRoomAsync(this LoadBalancingClient client, EnterRoomParams enterRoomParams, bool throwOnError = true, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (!client.OpJoinOrCreateRoom(enterRoomParams))
		{
			return Task.FromException<short>(new OperationStartException("Failed to send CreateRoom operation"));
		}
		return client.CreateOpHandler(throwOnError, createServiceTask, externalCancellationToken).Task;
	}

	public static Task<short> JoinRoomAsync(this LoadBalancingClient client, EnterRoomParams enterRoomParams, bool throwOnError = true, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (!client.OpJoinRoom(enterRoomParams))
		{
			return Task.FromException<short>(new OperationStartException("Failed to send JoinRoom operation"));
		}
		return client.CreateOpHandler(throwOnError, createServiceTask, externalCancellationToken).Task;
	}

	public static Task<short> JoinRandomOrCreateRoomAsync(this LoadBalancingClient client, OpJoinRandomRoomParams joinRandomRoomParams, EnterRoomParams enterRoomParams, bool throwOnError = true, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (!client.OpJoinRandomOrCreateRoom(joinRandomRoomParams, enterRoomParams))
		{
			return Task.FromException<short>(new OperationStartException("Failed to send JoinRandomOrCreateRoom operation"));
		}
		return client.CreateOpHandler(throwOnError, createServiceTask, externalCancellationToken).Task;
	}

	public static Task<short> JoinRandomRoomAsync(this LoadBalancingClient client, OpJoinRandomRoomParams joinRandomRoomParams, bool throwOnError = true, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		if (!client.OpJoinRandomRoom(joinRandomRoomParams))
		{
			return Task.FromException<short>(new OperationStartException("Failed to send JoinRandomRoom operation"));
		}
		return client.CreateOpHandler(throwOnError, createServiceTask, externalCancellationToken).Task;
	}

	public static Task<short> JoinLobbyAsync(this LoadBalancingClient client, TypedLobby lobby, bool throwOnError = true, bool createServiceTask = true, CancellationToken externalCancelationToken = default(CancellationToken))
	{
		if (!client.OpJoinLobby(lobby))
		{
			return Task.FromException<short>(new OperationStartException("Failed to send JoinLobby operation"));
		}
		return client.CreateOpHandler(throwOnError, createServiceTask, externalCancelationToken).Task;
	}

	public static OperationHandler CreateOpHandler(this LoadBalancingClient client, bool throwOnErrors = true, bool createServiceTask = true, CancellationToken externalCancellationToken = default(CancellationToken))
	{
		OperationHandler handler = new OperationHandler(throwOnErrors, externalCancellationToken);
		client.AddCallbackTarget(handler);
		TaskManager.ContinueWhenAll(new Task[1] { handler.Task }, delegate
		{
			client.RemoveCallbackTarget(handler);
			return Task.CompletedTask;
		}, handler.Token);
		if (createServiceTask)
		{
			client.Service_ClientUpdate(handler.Token, handler.CompletionSource);
		}
		return handler;
	}

	public static void Service_ClientUpdate(this LoadBalancingClient client, CancellationToken token, TaskCompletionSource<short> completionSource)
	{
		TaskManager.Service(delegate
		{
			try
			{
				if (!token.IsCancellationRequested)
				{
					client.Service();
				}
			}
			catch (Exception exception)
			{
				completionSource.TrySetException(exception);
			}
			return Task.FromResult(client.IsConnected);
		}, token, 10, "AsyncClientUpdate");
	}
}
