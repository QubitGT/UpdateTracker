#define FUSION_LOGLEVEL_TRACE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Fusion;

public class NetworkSceneManagerDefault : Behaviour, INetworkSceneManager
{
	protected struct GetAddressableScenesResult
	{
		public Task<string[]> Task;

		public Action BeforeWaitForCompletion;

		public static implicit operator GetAddressableScenesResult(Task<string[]> task)
		{
			GetAddressableScenesResult result = default(GetAddressableScenesResult);
			result.Task = task;
			return result;
		}
	}

	protected sealed class MultiPeerSceneRoot : MonoBehaviour
	{
		public SceneRef SceneRef;

		public string ScenePath;

		public int SceneHandle;

		public Scene Scene;
	}

	protected struct LoadingScope : IDisposable
	{
		private readonly NetworkSceneManagerDefault _manager;

		public LoadingScope(NetworkSceneManagerDefault manager)
		{
			_manager = manager;
			_manager._isLoading = true;
		}

		public void Dispose()
		{
			_manager._isLoading = false;
		}
	}

	[InlineHelp]
	[ToggleLeft]
	public bool IsSceneTakeOverEnabled = true;

	[InlineHelp]
	[ToggleLeft]
	public bool LogSceneLoadErrors = true;

	[InlineHelp]
	[ToggleLeft]
	public bool DestroySpawnedPrefabsOnSceneUnload = true;

	private static Dictionary<Scene, NetworkSceneManagerDefault> _allOwnedScenes;

	private List<MultiPeerSceneRoot> _multiPeerSceneRoots = new List<MultiPeerSceneRoot>();

	private MultiPeerSceneRoot _multiPeerActiveRoot;

	private List<ICoroutine> _runningCoroutines = new List<ICoroutine>();

	private Scene _tempUnloadScene;

	private bool _isLoading;

	[InlineHelp]
	public string AddressableScenesLabel = "FusionScenes";

	private Lazy<GetAddressableScenesResult> _addressableScenesTask;

	private Dictionary<SceneRef, AsyncOperationHandle<SceneInstance>> _addressableOperations = new Dictionary<SceneRef, AsyncOperationHandle<SceneInstance>>();

	public Scene MultiPeerScene { get; private set; }

	public Transform MultiPeerDontDestroyOnLoadRoot { get; private set; }

	public NetworkRunner Runner { get; private set; }

	private bool IsMultiplePeer => Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple;

	public virtual bool IsBusy
	{
		get
		{
			if (_isLoading)
			{
				return true;
			}
			if (IsMultiplePeer && _multiPeerSceneRoots.Count == 0)
			{
				return true;
			}
			return false;
		}
	}

	public virtual Scene MainRunnerScene
	{
		get
		{
			if (IsMultiplePeer)
			{
				return MultiPeerScene;
			}
			return SceneManager.GetActiveScene();
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ClearStatics()
	{
		_allOwnedScenes.Clear();
	}

	static NetworkSceneManagerDefault()
	{
		_allOwnedScenes = new Dictionary<Scene, NetworkSceneManagerDefault>(new FusionUnitySceneManagerUtils.SceneEqualityComparer());
		SceneManager.sceneUnloaded += delegate(Scene s)
		{
			_allOwnedScenes.Remove(s);
		};
	}

	public virtual void Initialize(NetworkRunner runner)
	{
		LoadAddressableScenePathsAsync();
		Runner = runner;
		if (IsMultiplePeer)
		{
			Scene multiPeerScene = SceneManager.CreateScene($"{runner.name}_{runner.LocalPlayer}", new CreateSceneParameters(LocalPhysicsMode.Physics2D | LocalPhysicsMode.Physics3D));
			MultiPeerScene = multiPeerScene;
			MultiPeerDontDestroyOnLoadRoot = new GameObject("[DontDestroyOnLoad]").transform;
			SceneManager.MoveGameObjectToScene(MultiPeerDontDestroyOnLoadRoot.gameObject, MultiPeerScene);
		}
	}

	public virtual void Shutdown()
	{
		Runner = null;
		foreach (Scene item in (from x in _allOwnedScenes
			where x.Value == this
			select x.Key).ToList())
		{
			_allOwnedScenes.Remove(item);
		}
		_multiPeerSceneRoots.Clear();
		_multiPeerActiveRoot = null;
		MultiPeerDontDestroyOnLoadRoot = null;
		Scene multiPeerScene = MultiPeerScene;
		MultiPeerScene = default(Scene);
		if (multiPeerScene.isLoaded)
		{
			if (!multiPeerScene.CanBeUnloaded())
			{
				SceneManager.CreateScene("FusionSceneManager_TempEmptyScene");
			}
			SceneManager.UnloadSceneAsync(multiPeerScene);
		}
	}

	public virtual bool IsRunnerScene(Scene scene)
	{
		if (IsMultiplePeer)
		{
			return scene == MultiPeerScene;
		}
		return true;
	}

	public virtual bool TryGetPhysicsScene2D(out PhysicsScene2D scene2D)
	{
		Scene mainRunnerScene = MainRunnerScene;
		if (mainRunnerScene.IsValid())
		{
			scene2D = mainRunnerScene.GetPhysicsScene2D();
			return true;
		}
		scene2D = default(PhysicsScene2D);
		return false;
	}

	public virtual bool TryGetPhysicsScene3D(out PhysicsScene scene3D)
	{
		Scene mainRunnerScene = MainRunnerScene;
		if (mainRunnerScene.IsValid())
		{
			scene3D = mainRunnerScene.GetPhysicsScene();
			return true;
		}
		scene3D = default(PhysicsScene);
		return false;
	}

	public virtual void MakeDontDestroyOnLoad(GameObject obj)
	{
		if (IsMultiplePeer)
		{
			obj.transform.SetParent(MultiPeerDontDestroyOnLoadRoot, worldPositionStays: true);
		}
		else
		{
			UnityEngine.Object.DontDestroyOnLoad(obj);
		}
	}

	public bool MoveGameObjectToScene(GameObject gameObject, SceneRef sceneRef)
	{
		if (IsMultiplePeer)
		{
			foreach (MultiPeerSceneRoot multiPeerSceneRoot in _multiPeerSceneRoots)
			{
				if ((sceneRef != default(SceneRef) && multiPeerSceneRoot.SceneRef != sceneRef) || (sceneRef == default(SceneRef) && (bool)_multiPeerActiveRoot && multiPeerSceneRoot != _multiPeerActiveRoot))
				{
					continue;
				}
				if (gameObject.scene != MultiPeerScene)
				{
					gameObject.transform.SetParent(null, worldPositionStays: true);
					SceneManager.MoveGameObjectToScene(gameObject, MultiPeerScene);
					if (!Application.isBatchMode)
					{
						Runner.AddVisibilityNodes(gameObject);
					}
				}
				gameObject.transform.SetParent(multiPeerSceneRoot.transform, worldPositionStays: true);
				return true;
			}
			return false;
		}
		if (sceneRef == default(SceneRef))
		{
			return true;
		}
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded && GetSceneRef(sceneAt.path) == sceneRef)
			{
				SceneManager.MoveGameObjectToScene(gameObject, sceneAt);
				return true;
			}
		}
		return false;
	}

	public virtual NetworkSceneAsyncOp LoadScene(SceneRef sceneRef, NetworkLoadSceneParameters parameters)
	{
		return NetworkSceneAsyncOp.FromCoroutine(sceneRef, StartTracedCoroutine(LoadSceneCoroutine(sceneRef, parameters)));
	}

	public virtual NetworkSceneAsyncOp UnloadScene(SceneRef sceneRef)
	{
		return NetworkSceneAsyncOp.FromCoroutine(sceneRef, StartTracedCoroutine(UnloadSceneCoroutine(sceneRef)));
	}

	public virtual SceneRef GetSceneRef(string sceneNameOrPath)
	{
		int sceneBuildIndex = FusionUnitySceneManagerUtils.GetSceneBuildIndex(sceneNameOrPath);
		if (sceneBuildIndex >= 0)
		{
			return SceneRef.FromIndex(sceneBuildIndex);
		}
		if (!TryGetAddressableScenes(out var addressableScenes))
		{
			Log.Error(this, "Failed to resolve addressable scene paths, won't be able to resolve " + sceneNameOrPath + " or any other addressable scene.");
			addressableScenes = Array.Empty<string>();
		}
		int sceneIndex = FusionUnitySceneManagerUtils.GetSceneIndex(addressableScenes, sceneNameOrPath);
		if (sceneIndex >= 0)
		{
			return SceneRef.FromPath(addressableScenes[sceneIndex]);
		}
		return SceneRef.None;
	}

	public SceneRef GetSceneRef(GameObject gameObject)
	{
		if (IsMultiplePeer)
		{
			if (gameObject.scene != MultiPeerScene)
			{
				return default(SceneRef);
			}
			Transform root = gameObject.transform.root;
			foreach (MultiPeerSceneRoot multiPeerSceneRoot in _multiPeerSceneRoots)
			{
				if (multiPeerSceneRoot.transform == root)
				{
					return multiPeerSceneRoot.SceneRef;
				}
			}
			return default(SceneRef);
		}
		return GetSceneRef(gameObject.scene.path);
	}

	public bool OnSceneInfoChanged(NetworkSceneInfo sceneInfo, NetworkSceneInfoChangeSource changeSource)
	{
		return false;
	}

	protected virtual IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
	{
		Runner.InvokeSceneLoadStart(sceneRef);
		Scene scene = default(Scene);
		using (MakeLoadingScope())
		{
			LocalPhysicsMode localPhysicsMode = sceneParams.LocalPhysicsMode;
			LoadSceneMode loadSceneMode = sceneParams.LoadSceneMode;
			if (IsMultiplePeer)
			{
				if (localPhysicsMode != 0)
				{
					throw new ArgumentException("Local physics mode is not supported in multiple peer mode", "sceneParams");
				}
				if (loadSceneMode == LoadSceneMode.Single)
				{
					loadSceneMode = LoadSceneMode.Additive;
					try
					{
						foreach (MultiPeerSceneRoot multiPeerSceneRoot in _multiPeerSceneRoots)
						{
							UnityEngine.Object.Destroy(multiPeerSceneRoot.gameObject);
						}
						foreach (MultiPeerSceneRoot root in _multiPeerSceneRoots)
						{
							while (root != null)
							{
								yield return null;
							}
						}
					}
					finally
					{
						_multiPeerSceneRoots.Clear();
					}
				}
			}
			else if (DestroySpawnedPrefabsOnSceneUnload && loadSceneMode == LoadSceneMode.Single)
			{
				for (int j = 0; j < SceneManager.sceneCount; j++)
				{
					Scene sceneAt = SceneManager.GetSceneAt(j);
					SceneRef sceneRef2 = GetSceneRef(sceneAt.path);
					if (sceneRef2 != SceneRef.None)
					{
						DestroyAllRuntimeSpawnedObjectsInScene(sceneAt, sceneRef2);
					}
				}
			}
			if (IsSceneTakeOverEnabled)
			{
				Scene candidate = FindSceneToTakeOver(sceneRef);
				if (candidate.IsValid())
				{
					if (candidate.GetLocalPhysicsMode() != localPhysicsMode)
					{
						throw new InvalidOperationException($"Tried to take over {candidate.Dump()} for {sceneRef}, but physics mode were different: {candidate.GetLocalPhysicsMode()} != {localPhysicsMode}");
					}
					scene = candidate;
					MarkSceneAsOwned(sceneRef, candidate);
					if (loadSceneMode == LoadSceneMode.Single && !IsMultiplePeer)
					{
						for (int i = 0; i < SceneManager.sceneCount; i++)
						{
							Scene sceneAt2 = SceneManager.GetSceneAt(i);
							if (sceneAt2 != candidate)
							{
								yield return SceneManager.UnloadSceneAsync(sceneAt2);
							}
						}
					}
				}
			}
			if (!scene.IsValid())
			{
				if (loadSceneMode == LoadSceneMode.Single)
				{
					_addressableOperations.Clear();
				}
				if (sceneRef.IsIndex)
				{
					AsyncOperation op2 = SceneManager.LoadSceneAsync(sceneRef.AsIndex, new LoadSceneParameters(loadSceneMode, localPhysicsMode));
					if (op2 == null)
					{
						throw new InvalidOperationException($"Scene not found: {sceneRef.AsIndex}");
					}
					scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					MarkSceneAsOwned(sceneRef, scene);
					while (!op2.isDone)
					{
						OnLoadSceneProgress(sceneRef, op2.progress);
						yield return null;
					}
				}
				else
				{
					if (!TryGetAddressableScenes(out var addressableScenes))
					{
						Log.Error(this, $"Failed to resolve addressable scene paths, won't be able to resolve {sceneRef}");
						addressableScenes = Array.Empty<string>();
					}
					string sceneAddress = null;
					string[] array = addressableScenes;
					foreach (string text in array)
					{
						if (sceneRef.IsPath(text))
						{
							sceneAddress = text;
							break;
						}
					}
					if (sceneAddress == null)
					{
						throw new InvalidOperationException($"Unable to find addressable scene path for {sceneRef}");
					}
					LoadSceneParameters loadSceneParameters = new LoadSceneParameters(loadSceneMode, localPhysicsMode);
					AsyncOperationHandle<SceneInstance> op3 = Addressables.LoadSceneAsync(sceneAddress, loadSceneParameters);
					scene = default(Scene);
					op3.Completed += delegate(AsyncOperationHandle<SceneInstance> op)
					{
						if (op.Status == AsyncOperationStatus.Succeeded)
						{
							scene = op.Result.Scene;
							MarkSceneAsOwned(sceneRef, scene);
						}
					};
					op3.Destroyed += delegate
					{
						_addressableOperations.Remove(sceneRef);
					};
					_addressableOperations.Add(sceneRef, op3);
					while (!op3.IsDone)
					{
						OnLoadSceneProgress(sceneRef, op3.PercentComplete);
						yield return null;
					}
					if (!op3.IsValid())
					{
						throw new InvalidOperationException($"Loading operation for {sceneRef} has been destroyed");
					}
					if (op3.Status == AsyncOperationStatus.Failed)
					{
						Addressables.Release(op3);
						throw new InvalidOperationException("Failed to load scene from addressable: " + sceneAddress);
					}
				}
			}
		}
		yield return StartCoroutine(OnSceneLoaded(sceneRef, scene, sceneParams));
	}

	protected virtual IEnumerator UnloadSceneCoroutine(SceneRef sceneRef)
	{
		using (MakeLoadingScope())
		{
			if (IsMultiplePeer)
			{
				for (int i = 0; i < _multiPeerSceneRoots.Count; i++)
				{
					MultiPeerSceneRoot root = _multiPeerSceneRoots[i];
					if (root.SceneRef == sceneRef)
					{
						if (root == _multiPeerActiveRoot)
						{
							_multiPeerActiveRoot = null;
						}
						_multiPeerSceneRoots.RemoveAt(i);
						UnityEngine.Object.Destroy(root.gameObject);
						while (root != null)
						{
							yield return null;
						}
						yield break;
					}
				}
				throw new ArgumentOutOfRangeException($"Did not find a scene to unload: {sceneRef}", "sceneRef");
			}
			Scene scene = default(Scene);
			for (int j = 0; j < SceneManager.sceneCount; j++)
			{
				Scene sceneAt = SceneManager.GetSceneAt(j);
				if (GetSceneRef(sceneAt.path) == sceneRef)
				{
					scene = sceneAt;
					break;
				}
			}
			if (!scene.IsValid())
			{
				throw new ArgumentOutOfRangeException($"Did not find a scene to unload: {sceneRef}", "sceneRef");
			}
			if (DestroySpawnedPrefabsOnSceneUnload)
			{
				DestroyAllRuntimeSpawnedObjectsInScene(scene, sceneRef);
			}
			if (!scene.CanBeUnloaded())
			{
				Log.Warn(Runner, $"Scene {scene.Dump()} can't be unloaded for {sceneRef}, creating a temporary scene to unload it");
				_tempUnloadScene = SceneManager.CreateScene("FusionSceneManager_TempEmptyScene");
			}
			if (_addressableOperations.TryGetValue(sceneRef, out var value))
			{
				yield return Addressables.UnloadSceneAsync(value);
				yield break;
			}
			AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(scene);
			if (asyncOperation == null)
			{
				throw new InvalidOperationException("Failed to unload " + scene.Dump());
			}
			yield return asyncOperation;
		}
	}

	protected virtual IEnumerator OnSceneLoaded(SceneRef sceneRef, Scene scene, NetworkLoadSceneParameters sceneParams)
	{
		GameObject[] rootObjects;
		NetworkObject[] components = scene.GetComponents<NetworkObject>(includeInactive: true, out rootObjects);
		Array.Sort(components, NetworkObjectSortKeyComparer.Instance);
		if (IsMultiplePeer)
		{
			MultiPeerSceneRoot multiPeerSceneRoot = new GameObject("[" + scene.name + "]").AddComponent<MultiPeerSceneRoot>();
			multiPeerSceneRoot.SceneRef = sceneRef;
			multiPeerSceneRoot.SceneHandle = scene.handle;
			multiPeerSceneRoot.Scene = scene;
			multiPeerSceneRoot.ScenePath = scene.path;
			SceneManager.MoveGameObjectToScene(multiPeerSceneRoot.gameObject, scene);
			GameObject[] array = rootObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].transform.SetParent(multiPeerSceneRoot.transform, worldPositionStays: true);
			}
			_multiPeerSceneRoots.Add(multiPeerSceneRoot);
			SceneManager.MergeScenes(scene, MultiPeerScene);
			if (sceneParams.IsActiveOnLoad)
			{
				_multiPeerActiveRoot = multiPeerSceneRoot;
			}
		}
		else if (sceneParams.IsActiveOnLoad)
		{
			SceneManager.SetActiveScene(scene);
		}
		Runner.RegisterSceneObjects(sceneRef, components, sceneParams.LoadId);
		NetworkRunner runner = Runner;
		SceneLoadDoneArgs info = new SceneLoadDoneArgs(sceneRef, components, scene, rootObjects);
		runner.InvokeSceneLoadDone(in info);
		yield break;
	}

	protected virtual void OnLoadSceneProgress(SceneRef sceneRef, float progress)
	{
	}

	private void DestroyAllRuntimeSpawnedObjectsInScene(Scene scene, SceneRef sceneRef)
	{
		foreach (NetworkObject allNetworkObject in Runner.GetAllNetworkObjects())
		{
			if (allNetworkObject.gameObject.scene == scene && !allNetworkObject.NetworkTypeId.IsSceneObject)
			{
				if (allNetworkObject.HasStateAuthority)
				{
					Runner.Despawn(allNetworkObject);
				}
				else
				{
					UnityEngine.Object.Destroy(allNetworkObject.gameObject);
				}
			}
		}
	}

	private Scene FindSceneToTakeOver(SceneRef sceneRef)
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded && !(GetSceneRef(sceneAt.path) != sceneRef) && !_allOwnedScenes.ContainsKey(sceneAt))
			{
				return sceneAt;
			}
		}
		return default(Scene);
	}

	private ICoroutine StartTracedCoroutine(IEnumerator inner)
	{
		FusionCoroutine fusionCoroutine = new FusionCoroutine(inner);
		_runningCoroutines.Add(fusionCoroutine);
		fusionCoroutine.Completed += delegate(IAsyncOperation x)
		{
			if (LogSceneLoadErrors && x.Error != null)
			{
				Log.Error(Runner, $"Failed async op: {x.Error.SourceException}");
			}
			int num = _runningCoroutines.IndexOf((ICoroutine)x);
			_runningCoroutines.RemoveAt(num);
			if (num < _runningCoroutines.Count)
			{
				StartCoroutine(_runningCoroutines[num]);
			}
		};
		if (_runningCoroutines.Count == 1)
		{
			StartCoroutine(fusionCoroutine);
		}
		return fusionCoroutine;
	}

	protected LoadingScope MakeLoadingScope()
	{
		return new LoadingScope(this);
	}

	protected void MarkSceneAsOwned(SceneRef sceneRef, Scene scene)
	{
		if (_allOwnedScenes.TryGetValue(scene, out var value))
		{
			Log.Warn(Runner, $"Scene {scene.Dump()} (for {sceneRef}) already owned by {value}");
		}
		else
		{
			_allOwnedScenes.Add(scene, this);
		}
	}

	private NetworkSceneAsyncOp FailOp(SceneRef sceneRef, Exception exception)
	{
		if (LogSceneLoadErrors)
		{
			Log.Error(Runner, $"Failed with: {exception}");
		}
		return NetworkSceneAsyncOp.FromError(sceneRef, exception);
	}

	public NetworkSceneManagerDefault()
	{
		_addressableScenesTask = new Lazy<GetAddressableScenesResult>(() => GetAddressableScenes());
	}

	public Task LoadAddressableScenePathsAsync()
	{
		return _addressableScenesTask.Value.Task;
	}

	protected virtual GetAddressableScenesResult GetAddressableScenes()
	{
		TaskCompletionSource<string[]> tcs = new TaskCompletionSource<string[]>();
		AsyncOperationHandle<IList<IResourceLocation>> result = Addressables.LoadResourceLocationsAsync(AddressableScenesLabel, typeof(SceneInstance));
		result.Completed += delegate(AsyncOperationHandle<IList<IResourceLocation>> op)
		{
			try
			{
				if (op.Status == AsyncOperationStatus.Failed)
				{
					tcs.SetException(op.OperationException);
				}
				else
				{
					string[] result3 = op.Result.Select((IResourceLocation x) => x.PrimaryKey).ToArray();
					tcs.SetResult(result3);
				}
			}
			finally
			{
				Addressables.Release(op);
			}
		};
		GetAddressableScenesResult result2 = default(GetAddressableScenesResult);
		result2.Task = tcs.Task;
		result2.BeforeWaitForCompletion = delegate
		{
			if (result.IsValid())
			{
				result.WaitForCompletion();
			}
		};
		return result2;
	}

	protected virtual TimeSpan GetAddressableScenePathsTimeout()
	{
		return TimeSpan.FromSeconds(10.0);
	}

	private bool TryGetAddressableScenes(out string[] addressableScenes)
	{
		if (!_addressableScenesTask.IsValueCreated)
		{
			Log.Warn(Runner, "Going to block the thread in wait for addressable scene paths being resolved, call and await LoadAddressableScenePathsAsync to avoid this.");
		}
		GetAddressableScenesResult value = _addressableScenesTask.Value;
		if (!value.Task.IsCompleted)
		{
			value.BeforeWaitForCompletion?.Invoke();
			if (!value.Task.Wait(GetAddressableScenePathsTimeout()))
			{
				addressableScenes = null;
				return false;
			}
		}
		addressableScenes = value.Task.Result;
		return true;
	}
}
