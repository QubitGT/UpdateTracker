using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Fusion;

[DisallowMultipleComponent]
[AddComponentMenu("Fusion/Fusion Bootstrap")]
[ScriptHelp(BackColor = ScriptHeaderBackColor.Steel)]
public class FusionBootstrap : Behaviour
{
	public enum StartModes
	{
		UserInterface,
		Automatic,
		Manual
	}

	public enum Stage
	{
		Disconnected,
		StartingUp,
		UnloadOriginalScene,
		ConnectingServer,
		ConnectingClients,
		AllConnected
	}

	[Serializable]
	private class StartCommand : FusionMppmCommand
	{
		public string RoomName;

		public SceneRef InitialScene;

		public int ClientCount;

		public bool IsShared;

		public static StartCommand Instance;

		public override void Execute()
		{
			Instance = this;
		}
	}

	[InlineHelp]
	[WarnIf("RunnerPrefab", false, "No RunnerPrefab supplied. Will search for a NetworkRunner in the scene at startup.", CompareOperator.Equal)]
	public NetworkRunner RunnerPrefab;

	[InlineHelp]
	[WarnIf("StartMode", 2L, "Start network by calling the methods StartHost(), StartServer(), StartClient(), StartHostPlusClients(), or StartServerPlusClients()", CompareOperator.Equal)]
	public StartModes StartMode;

	[InlineHelp]
	[FormerlySerializedAs("Server")]
	[DrawIf("StartMode", 1L, CompareOperator.Equal, DrawIfMode.ReadOnly, Hide = true)]
	public GameMode AutoStartAs = GameMode.Shared;

	[InlineHelp]
	[DrawIf("StartMode", 0L, CompareOperator.Equal, DrawIfMode.ReadOnly, Hide = true)]
	public bool AutoHideGUI = true;

	[InlineHelp]
	[DrawIf("ShowAutoClients", Hide = true)]
	public int AutoClients = 1;

	[InlineHelp]
	public float ClientStartDelay = 0.1f;

	[InlineHelp]
	public ushort ServerPort;

	[InlineHelp]
	public string DefaultRoomName = string.Empty;

	[NonSerialized]
	private NetworkRunner _server;

	[InlineHelp]
	[ScenePath]
	public string InitialScenePath;

	private static string _initialScenePath;

	[InlineHelp]
	[SerializeField]
	[ReadOnly]
	protected Stage _currentStage;

	[DrawIf("IsMPPMEnabled", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	[Header("Multiplayer Play Mode")]
	public bool AutoConnectVirtualInstances = true;

	[DrawIf("IsMPPMEnabled", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	public float VirtualInstanceConnectDelay = 1f;

	public Stage CurrentStage
	{
		get
		{
			return _currentStage;
		}
		internal set
		{
			_currentStage = value;
		}
	}

	public int LastCreatedClientIndex { get; internal set; }

	public GameMode CurrentServerMode { get; internal set; }

	protected bool CanAddClients
	{
		get
		{
			if (CurrentStage == Stage.AllConnected && CurrentServerMode > (GameMode)0 && CurrentServerMode != GameMode.Shared)
			{
				return CurrentServerMode != GameMode.Single;
			}
			return false;
		}
	}

	protected bool CanAddSharedClients
	{
		get
		{
			if (CurrentStage == Stage.AllConnected && CurrentServerMode > (GameMode)0)
			{
				return CurrentServerMode == GameMode.Shared;
			}
			return false;
		}
	}

	protected bool IsShutdown => CurrentStage == Stage.Disconnected;

	protected bool IsShutdownAndMultiPeer
	{
		get
		{
			if (CurrentStage == Stage.Disconnected)
			{
				return UsingMultiPeerMode;
			}
			return false;
		}
	}

	protected bool UsingMultiPeerMode => NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple;

	protected bool ShowAutoClients
	{
		get
		{
			if (UsingMultiPeerMode)
			{
				if (StartMode != 0)
				{
					if (StartMode == StartModes.Automatic)
					{
						return AutoStartAs != GameMode.Single;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	private static bool IsMPPMEnabled => FusionMppm.Status != FusionMppmStatus.Disabled;

	public bool ShouldShowGUI
	{
		get
		{
			if (StartMode == StartModes.UserInterface)
			{
				if (AutoConnectVirtualInstances)
				{
					return FusionMppm.Status != FusionMppmStatus.VirtualInstance;
				}
				return true;
			}
			return false;
		}
	}

	protected virtual void Start()
	{
		if (_initialScenePath == null)
		{
			if (string.IsNullOrEmpty(InitialScenePath))
			{
				Scene activeScene = SceneManager.GetActiveScene();
				if (activeScene.IsValid())
				{
					_initialScenePath = activeScene.path;
				}
				else
				{
					_initialScenePath = SceneManager.GetSceneByBuildIndex(0).path;
				}
				InitialScenePath = _initialScenePath;
			}
			else
			{
				_initialScenePath = InitialScenePath;
			}
		}
		bool flag = NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple;
		NetworkRunner networkRunner = UnityEngine.Object.FindFirstObjectByType<NetworkRunner>();
		if ((bool)networkRunner && networkRunner != RunnerPrefab)
		{
			if (networkRunner.State != NetworkRunner.States.Shutdown)
			{
				base.enabled = false;
				FusionBootstrapDebugGUI component = GetComponent<FusionBootstrapDebugGUI>();
				if ((bool)component)
				{
					UnityEngine.Object.Destroy(component);
				}
				UnityEngine.Object.Destroy(this);
				return;
			}
			if (RunnerPrefab == null)
			{
				RunnerPrefab = networkRunner;
			}
		}
		if (FusionMppm.Status == FusionMppmStatus.VirtualInstance && AutoConnectVirtualInstances)
		{
			StartCoroutine(StartWithMppmVirtualInstance());
			return;
		}
		switch (StartMode)
		{
		case StartModes.Manual:
			break;
		case StartModes.Automatic:
		{
			if (TryGetSceneRef(out var sceneRef))
			{
				StartCoroutine(StartWithClients(clientCount: (AutoStartAs != GameMode.Single) ? (flag ? AutoClients : ((AutoStartAs == GameMode.Client || AutoStartAs == GameMode.Shared || AutoStartAs == GameMode.AutoHostOrClient) ? 1 : 0)) : 0, serverMode: AutoStartAs, sceneRef: sceneRef));
			}
			break;
		}
		default:
			ShowUserInterface();
			break;
		}
	}

	protected void ShowUserInterface()
	{
		if (!TryGetComponent<FusionBootstrapDebugGUI>(out var component))
		{
			component = base.gameObject.AddComponent<FusionBootstrapDebugGUI>();
		}
		component.enabled = true;
	}

	private bool TryGetSceneRef(out SceneRef sceneRef)
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

	[EditorButton(EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("IsShutdown", Hide = true)]
	public virtual void StartSinglePlayer()
	{
		if (TryGetSceneRef(out var sceneRef))
		{
			StartCoroutine(StartWithClients(GameMode.Single, sceneRef, 0));
		}
	}

	[EditorButton(EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("IsShutdown", Hide = true)]
	public virtual void StartServer()
	{
		if (TryGetSceneRef(out var sceneRef))
		{
			StartCoroutine(StartWithClients(GameMode.Server, sceneRef, 0));
		}
	}

	[EditorButton(EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("IsShutdown", Hide = true)]
	public virtual void StartHost()
	{
		if (TryGetSceneRef(out var sceneRef))
		{
			StartCoroutine(StartWithClients(GameMode.Host, sceneRef, 0));
		}
	}

	[EditorButton(EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("IsShutdown", Hide = true)]
	public virtual void StartClient()
	{
		StartCoroutine(StartWithClients(GameMode.Client, default(SceneRef), 1));
	}

	[EditorButton(EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("IsShutdown", Hide = true)]
	public virtual void StartSharedClient()
	{
		if (TryGetSceneRef(out var sceneRef))
		{
			StartCoroutine(StartWithClients(GameMode.Shared, sceneRef, 1));
		}
	}

	[EditorButton("Start Auto Host Or Client", EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("IsShutdown", Hide = true)]
	public virtual void StartAutoClient()
	{
		if (TryGetSceneRef(out var sceneRef))
		{
			StartCoroutine(StartWithClients(GameMode.AutoHostOrClient, sceneRef, 1));
		}
	}

	[EditorButton(EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("IsShutdown", Hide = true)]
	public virtual void StartServerPlusClients()
	{
		StartServerPlusClients(AutoClients);
	}

	[EditorButton(EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("IsShutdown", Hide = true)]
	public void StartHostPlusClients()
	{
		StartHostPlusClients(AutoClients);
	}

	[EditorButton(EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("CurrentStage", Hide = true)]
	public void Shutdown()
	{
		ShutdownAll();
	}

	public virtual void StartServerPlusClients(int clientCount)
	{
		if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
		{
			if (TryGetSceneRef(out var sceneRef))
			{
				StartCoroutine(StartWithClients(GameMode.Server, sceneRef, clientCount));
			}
		}
		else
		{
			Debug.LogWarning("Unable to start multiple NetworkRunners in Unique Instance mode.");
		}
	}

	public void StartHostPlusClients(int clientCount)
	{
		if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
		{
			if (TryGetSceneRef(out var sceneRef))
			{
				StartCoroutine(StartWithClients(GameMode.Host, sceneRef, clientCount));
			}
		}
		else
		{
			Debug.LogWarning("Unable to start multiple NetworkRunners in Unique Instance mode.");
		}
	}

	public void StartMultipleClients(int clientCount)
	{
		if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
		{
			if (TryGetSceneRef(out var sceneRef))
			{
				StartCoroutine(StartWithClients(GameMode.Client, sceneRef, clientCount));
			}
		}
		else
		{
			Debug.LogWarning("Unable to start multiple NetworkRunners in Unique Instance mode.");
		}
	}

	public void StartMultipleSharedClients(int clientCount)
	{
		if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
		{
			if (TryGetSceneRef(out var sceneRef))
			{
				StartCoroutine(StartWithClients(GameMode.Shared, sceneRef, clientCount));
			}
		}
		else
		{
			Debug.LogWarning("Unable to start multiple NetworkRunners in Unique Instance mode.");
		}
	}

	public void StartMultipleAutoClients(int clientCount)
	{
		if (NetworkProjectConfig.Global.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
		{
			if (TryGetSceneRef(out var sceneRef))
			{
				StartCoroutine(StartWithClients(GameMode.AutoHostOrClient, sceneRef, clientCount));
			}
		}
		else
		{
			Debug.LogWarning("Unable to start multiple NetworkRunners in Unique Instance mode.");
		}
	}

	public void ShutdownAll()
	{
		foreach (NetworkRunner item in NetworkRunner.Instances.ToList())
		{
			if (item != null && item.IsRunning)
			{
				item.Shutdown();
			}
		}
		SceneManager.LoadSceneAsync(_initialScenePath);
		UnityEngine.Object.Destroy(RunnerPrefab.gameObject);
		UnityEngine.Object.Destroy(base.gameObject);
		CurrentStage = Stage.Disconnected;
		CurrentServerMode = (GameMode)0;
	}

	protected IEnumerator StartWithClients(GameMode serverMode, SceneRef sceneRef, int clientCount)
	{
		if (CurrentStage != 0)
		{
			yield break;
		}
		bool flag = serverMode != GameMode.Shared && serverMode != GameMode.Client && serverMode != GameMode.AutoHostOrClient;
		if (!flag && clientCount == 0)
		{
			Debug.LogError(string.Format("{0} is set to {1}, and {2} is set to zero. Starting no network runners.", "GameMode", serverMode, "clientCount"));
			yield break;
		}
		CurrentStage = Stage.StartingUp;
		SceneManager.GetActiveScene();
		if (!RunnerPrefab)
		{
			Debug.LogError("RunnerPrefab not set, can't perform debug start.");
			yield break;
		}
		RunnerPrefab = UnityEngine.Object.Instantiate(RunnerPrefab);
		UnityEngine.Object.DontDestroyOnLoad(RunnerPrefab);
		RunnerPrefab.name = "Temporary Runner Prefab";
		NetworkProjectConfig global = NetworkProjectConfig.Global;
		if (global.PeerMode != NetworkProjectConfig.PeerModes.Multiple)
		{
			int num = ((!flag) ? 1 : 0);
			if (clientCount > num)
			{
				Debug.LogWarning(string.Format("Instance mode must be set to {0} to perform a debug start multiple peers. Restricting client count to {1}.", "Multiple", num));
				clientCount = num;
			}
		}
		bool num2 = (serverMode == GameMode.Shared || serverMode == GameMode.AutoHostOrClient || serverMode == GameMode.Server || serverMode == GameMode.Host) && clientCount > 1 && global.PeerMode == NetworkProjectConfig.PeerModes.Multiple;
		bool flag2 = FusionMppm.Status == FusionMppmStatus.MainInstance;
		if ((num2 || flag2) && string.IsNullOrEmpty(DefaultRoomName))
		{
			DefaultRoomName = Guid.NewGuid().ToString();
			Debug.Log("Generated Session Name: " + DefaultRoomName);
		}
		if ((bool)base.gameObject.transform.parent)
		{
			Debug.LogWarning("FusionBootstrap can't be a child game object, un-parenting.");
			base.gameObject.transform.parent = null;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		CurrentServerMode = serverMode;
		if (flag)
		{
			_server = UnityEngine.Object.Instantiate(RunnerPrefab);
			_server.name = serverMode.ToString();
			Task serverTask = InitializeNetworkRunner(_server, serverMode, NetAddress.Any(ServerPort), sceneRef, delegate
			{
			});
			while (!serverTask.IsCompleted)
			{
				yield return new WaitForSeconds(1f);
			}
			if (serverTask.IsFaulted)
			{
				ShutdownAll();
				yield break;
			}
			yield return StartClients(clientCount, serverMode, sceneRef);
		}
		else
		{
			yield return StartClients(clientCount, serverMode, sceneRef);
		}
		if (FusionMppm.Status == FusionMppmStatus.MainInstance && serverMode != GameMode.Single && VirtualInstanceConnectDelay > 0f)
		{
			yield return new WaitForSecondsRealtime(VirtualInstanceConnectDelay);
		}
	}

	protected IEnumerator StartWithMppmVirtualInstance()
	{
		while (StartCommand.Instance == null)
		{
			yield return null;
		}
		StartCommand instance = StartCommand.Instance;
		StartCommand.Instance = null;
		DefaultRoomName = instance.RoomName;
		yield return StartClients(instance.ClientCount, instance.IsShared ? GameMode.Shared : GameMode.Client, instance.InitialScene);
	}

	[EditorButton("Add Additional Client", EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("CanAddClients", Hide = true)]
	public void AddClient()
	{
		if (TryGetSceneRef(out var sceneRef))
		{
			AddClient(GameMode.Client, sceneRef);
		}
	}

	[EditorButton("Add Additional Shared Client", EditorButtonVisibility.PlayMode, 0, false)]
	[DrawIf("CanAddSharedClients", Hide = true)]
	public void AddSharedClient()
	{
		if (TryGetSceneRef(out var sceneRef))
		{
			AddClient(GameMode.Shared, sceneRef);
		}
	}

	public Task AddClient(GameMode serverMode, SceneRef sceneRef)
	{
		NetworkRunner networkRunner = UnityEngine.Object.Instantiate(RunnerPrefab);
		UnityEngine.Object.DontDestroyOnLoad(networkRunner);
		networkRunner.name = $"Client {(char)(65 + LastCreatedClientIndex++)}";
		GameMode gameMode = GameMode.Client;
		if (serverMode == GameMode.Shared || serverMode == GameMode.AutoHostOrClient)
		{
			gameMode = serverMode;
		}
		return InitializeNetworkRunner(networkRunner, gameMode, NetAddress.Any(0), sceneRef, null);
	}

	protected IEnumerator StartClients(int clientCount, GameMode serverMode, SceneRef sceneRef = default(SceneRef))
	{
		CurrentStage = Stage.ConnectingClients;
		List<Task> clientTasks = new List<Task>();
		int i = 0;
		while (i < clientCount)
		{
			clientTasks.Add(AddClient(serverMode, sceneRef));
			yield return new WaitForSeconds(ClientStartDelay);
			int num = i + 1;
			i = num;
		}
		Task clientsStartTask = Task.WhenAll(clientTasks);
		while (!clientsStartTask.IsCompleted)
		{
			yield return new WaitForSeconds(1f);
		}
		if (clientsStartTask.IsFaulted)
		{
			Debug.LogWarning(clientsStartTask.Exception);
		}
		CurrentStage = Stage.AllConnected;
	}

	protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address, SceneRef scene, Action<NetworkRunner> onGameStarted, INetworkRunnerUpdater updater = null)
	{
		INetworkSceneManager networkSceneManager = runner.GetComponent<INetworkSceneManager>();
		if (networkSceneManager == null)
		{
			Debug.Log("NetworkRunner does not have any component implementing INetworkSceneManager interface, adding NetworkSceneManagerDefault.", runner);
			networkSceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
		}
		INetworkObjectProvider networkObjectProvider = runner.GetComponent<INetworkObjectProvider>();
		if (networkObjectProvider == null)
		{
			Debug.Log("NetworkRunner does not have any component implementing INetworkObjectProvider interface, adding NetworkObjectProviderDefault.", runner);
			networkObjectProvider = runner.gameObject.AddComponent<NetworkObjectProviderDefault>();
		}
		NetworkSceneInfo value = default(NetworkSceneInfo);
		if (scene.IsValid)
		{
			value.AddSceneRef(scene, LoadSceneMode.Additive);
		}
		return runner.StartGame(new StartGameArgs
		{
			GameMode = gameMode,
			Address = address,
			Scene = value,
			SessionName = DefaultRoomName,
			OnGameStarted = onGameStarted,
			SceneManager = networkSceneManager,
			Updater = updater,
			ObjectProvider = networkObjectProvider
		});
	}
}
