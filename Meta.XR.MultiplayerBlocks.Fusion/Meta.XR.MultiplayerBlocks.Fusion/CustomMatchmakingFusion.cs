using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Meta.XR.BuildingBlocks;
using Meta.XR.MultiplayerBlocks.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meta.XR.MultiplayerBlocks.Fusion;

public class CustomMatchmakingFusion : MonoBehaviour, CustomMatchmaking.ICustomMatchmakingBehaviour
{
	[SerializeField]
	[Tooltip("Indicates the chosen game mode to be used.")]
	private GameMode gameMode = GameMode.Shared;

	[SerializeField]
	[Tooltip("Amount of time in seconds to wait for receiving the session list of a lobby before timing out.")]
	private int getSessionListTimeoutS = 10;

	private NetworkRunner _runnerPrefab;

	private List<SessionInfo> _sessionList;

	public GameMode GameMode
	{
		get
		{
			return gameMode;
		}
		set
		{
			gameMode = value;
		}
	}

	public bool SupportsRoomPassword => false;

	public bool IsConnected => GetActiveNetworkRunner() != null;

	public string ConnectedRoomToken => GetActiveNetworkRunner()?.SessionInfo.Name;

	private void Awake()
	{
		_runnerPrefab = UnityEngine.Object.FindFirstObjectByType<NetworkRunner>();
		if (_runnerPrefab == null)
		{
			throw new InvalidOperationException("Fusion NetworkRunner not found");
		}
	}

	private void OnEnable()
	{
		FusionBBEvents.OnSessionListUpdated += OnSessionListUpdated;
	}

	private void OnDisable()
	{
		FusionBBEvents.OnSessionListUpdated -= OnSessionListUpdated;
	}

	private NetworkRunner InitializeNetworkRunner()
	{
		_runnerPrefab.gameObject.SetActive(value: false);
		NetworkRunner networkRunner = UnityEngine.Object.Instantiate(_runnerPrefab);
		networkRunner.gameObject.SetActive(value: true);
		UnityEngine.Object.DontDestroyOnLoad(networkRunner);
		networkRunner.name = "Temporary Runner Prefab";
		return networkRunner;
	}

	public async Task<CustomMatchmaking.RoomOperationResult> CreateRoom(CustomMatchmaking.RoomCreationOptions options)
	{
		string sessionName = RunTimeUtils.GenerateRandomString(6, includeLowercase: false, includeUppercase: true, includeNumeric: false);
		StartGameResult startGameResult = await InitializeNetworkRunner().StartGame(new StartGameArgs
		{
			GameMode = gameMode,
			Scene = GetSceneInfo(),
			CustomLobbyName = options.LobbyName,
			SessionName = sessionName,
			PlayerCount = options.MaxPlayersPerRoom,
			IsVisible = !options.IsPrivate
		});
		CustomMatchmaking.RoomOperationResult result = default(CustomMatchmaking.RoomOperationResult);
		result.ErrorMessage = (startGameResult.Ok ? null : $"Failed to Start: {startGameResult.ShutdownReason}, Error Message: {startGameResult.ErrorMessage}");
		result.RoomToken = sessionName;
		return result;
	}

	public async Task<CustomMatchmaking.RoomOperationResult> JoinRoom(string roomToken, string roomPassword = null)
	{
		StartGameResult startGameResult = await InitializeNetworkRunner().StartGame(new StartGameArgs
		{
			GameMode = gameMode,
			Scene = GetSceneInfo(),
			SessionName = roomToken
		});
		CustomMatchmaking.RoomOperationResult result = default(CustomMatchmaking.RoomOperationResult);
		result.ErrorMessage = (startGameResult.Ok ? null : $"Failed to Start: {startGameResult.ShutdownReason}, Error Message: {startGameResult.ErrorMessage}");
		result.RoomToken = roomToken;
		result.RoomPassword = roomPassword;
		return result;
	}

	public async Task<CustomMatchmaking.RoomOperationResult> JoinOpenRoom(string lobbyName)
	{
		ClearSessionList();
		NetworkRunner runner = InitializeNetworkRunner();
		StartGameResult startGameResult = await runner.JoinSessionLobby(SessionLobby.Custom, lobbyName, null, null, false);
		CustomMatchmaking.RoomOperationResult result;
		if (!startGameResult.Ok)
		{
			result = default(CustomMatchmaking.RoomOperationResult);
			result.ErrorMessage = $"Failed to Start: {startGameResult.ShutdownReason}, Error Message: {startGameResult.ErrorMessage}";
			return result;
		}
		List<SessionInfo> list = await GetSessionList(getSessionListTimeoutS);
		if (list == null)
		{
			result = default(CustomMatchmaking.RoomOperationResult);
			result.ErrorMessage = "Failed to fetch the session list from the Lobby " + lobbyName;
			return result;
		}
		if (list.Count == 0)
		{
			result = default(CustomMatchmaking.RoomOperationResult);
			result.ErrorMessage = "No available sessions to join in Lobby " + lobbyName;
			return result;
		}
		SessionInfo session = SelectSessionToJoinFromList(list);
		if (session == null)
		{
			result = default(CustomMatchmaking.RoomOperationResult);
			result.ErrorMessage = "Failed to select a session to join from the session list";
			return result;
		}
		StartGameResult startGameResult2 = await runner.StartGame(new StartGameArgs
		{
			GameMode = gameMode,
			Scene = GetSceneInfo(),
			SessionName = session.Name
		});
		result = default(CustomMatchmaking.RoomOperationResult);
		result.ErrorMessage = (startGameResult2.Ok ? null : $"Failed to Start: {startGameResult2.ShutdownReason}, Error Message: {startGameResult2.ErrorMessage}");
		result.RoomToken = session.Name;
		return result;
	}

	public void LeaveRoom()
	{
		for (int num = NetworkRunner.Instances.Count - 1; num >= 0; num--)
		{
			NetworkRunner networkRunner = NetworkRunner.Instances[num];
			if (!(networkRunner == null) && networkRunner.IsRunning)
			{
				networkRunner.Shutdown();
				UnityEngine.Object.Destroy(networkRunner.gameObject);
			}
		}
	}

	private static NetworkRunner GetActiveNetworkRunner()
	{
		for (int num = NetworkRunner.Instances.Count - 1; num >= 0; num--)
		{
			NetworkRunner networkRunner = NetworkRunner.Instances[num];
			if (networkRunner != null && networkRunner.IsRunning)
			{
				return networkRunner;
			}
		}
		return null;
	}

	private static NetworkSceneInfo GetSceneInfo()
	{
		SceneRef sceneRef = default(SceneRef);
		if (TryGetActiveSceneRef(out var sceneRef2))
		{
			sceneRef = sceneRef2;
		}
		NetworkSceneInfo result = default(NetworkSceneInfo);
		if (sceneRef.IsValid)
		{
			result.AddSceneRef(sceneRef, LoadSceneMode.Additive);
		}
		return result;
	}

	private static bool TryGetActiveSceneRef(out SceneRef sceneRef)
	{
		Scene activeScene = SceneManager.GetActiveScene();
		if (activeScene.buildIndex < 0 || activeScene.buildIndex >= SceneManager.sceneCountInBuildSettings)
		{
			sceneRef = default(SceneRef);
			return false;
		}
		sceneRef = SceneRef.FromIndex(activeScene.buildIndex);
		return true;
	}

	private void ClearSessionList()
	{
		_sessionList = null;
	}

	private async Task<List<SessionInfo>> GetSessionList(float timeoutS)
	{
		TaskCompletionSource<List<SessionInfo>> tcs = new TaskCompletionSource<List<SessionInfo>>();
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(timeoutS));
		cancellationTokenSource.Token.Register(delegate
		{
			tcs.TrySetResult(null);
		});
		while (_sessionList == null && !cancellationTokenSource.Token.IsCancellationRequested)
		{
			await Task.Delay(100);
		}
		if (_sessionList != null)
		{
			tcs.TrySetResult(_sessionList);
		}
		return await tcs.Task;
	}

	protected virtual SessionInfo SelectSessionToJoinFromList(List<SessionInfo> sessionList)
	{
		if (sessionList.Count != 0)
		{
			return sessionList[0];
		}
		return null;
	}

	private void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
		_sessionList = sessionList;
	}
}
