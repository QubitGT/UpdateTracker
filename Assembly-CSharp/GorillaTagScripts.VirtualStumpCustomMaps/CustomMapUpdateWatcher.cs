using System.Collections;
using System.Threading.Tasks;
using GorillaNetworking;
using Modio.Mods;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.VirtualStumpCustomMaps;

public class CustomMapUpdateWatcher : MonoBehaviour
{
	private const float PollIntervalSeconds = 10f;

	private const float UnloadTimeoutSeconds = 30f;

	private const float ReloadTimeoutSeconds = 300f;

	private static CustomMapUpdateWatcher instance;

	private ModId watchedModId = ModId.Null;

	private Coroutine pollCoroutine;

	private bool checkInProgress;

	private bool reloadInProgress;

	private bool hmdCallbackHooked;

	private bool hasRoomUpdate;

	private ModId roomUpdateModId = ModId.Null;

	private long roomUpdateModfileId;

	private bool roomUpdateReturnToPosition;

	private Coroutine roomUpdateCoroutine;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Bootstrap()
	{
		if (!(instance != null))
		{
			GameObject obj = new GameObject("CustomMapUpdateWatcher");
			Object.DontDestroyOnLoad(obj);
			instance = obj.AddComponent<CustomMapUpdateWatcher>();
		}
	}

	private void OnEnable()
	{
		CustomMapDevWatchMode.OnWatchStarted.AddListener(OnWatchStarted);
		CustomMapDevWatchMode.OnWatchStopped.AddListener(OnWatchStopped);
		if (CustomMapDevWatchMode.IsWatching)
		{
			OnWatchStarted(CustomMapDevWatchMode.WatchedMapId);
		}
	}

	private void OnDisable()
	{
		CustomMapDevWatchMode.OnWatchStarted.RemoveListener(OnWatchStarted);
		CustomMapDevWatchMode.OnWatchStopped.RemoveListener(OnWatchStopped);
		TeardownPolling();
		roomUpdateCoroutine = null;
		hasRoomUpdate = false;
		reloadInProgress = false;
	}

	private void OnWatchStarted(ModId modId)
	{
		if (!(modId == ModId.Null))
		{
			watchedModId = modId;
			CustomMapManager.OnMapUnloadComplete.RemoveListener(OnMapUnloaded);
			CustomMapManager.OnMapUnloadComplete.AddListener(OnMapUnloaded);
			if (!hmdCallbackHooked)
			{
				OVRManager.HMDMounted += OnHmdMounted;
				hmdCallbackHooked = true;
			}
			if (pollCoroutine != null)
			{
				StopCoroutine(pollCoroutine);
			}
			pollCoroutine = StartCoroutine(PollLoop());
			GTDev.Log($"[CustomMapUpdateWatcher] Watching map {modId} for updates.");
		}
	}

	private void OnWatchStopped(ModId modId)
	{
		TeardownPolling();
		GTDev.Log("[CustomMapUpdateWatcher] Stopped watching.");
	}

	private void TeardownPolling()
	{
		watchedModId = ModId.Null;
		CustomMapManager.OnMapUnloadComplete.RemoveListener(OnMapUnloaded);
		if (hmdCallbackHooked)
		{
			OVRManager.HMDMounted -= OnHmdMounted;
			hmdCallbackHooked = false;
		}
		if (pollCoroutine != null)
		{
			StopCoroutine(pollCoroutine);
			pollCoroutine = null;
		}
	}

	private static void StopWatch()
	{
		CustomMapDevWatchMode.StopWatching();
	}

	private void OnMapUnloaded()
	{
		if (!reloadInProgress)
		{
			GTDev.Log("[CustomMapUpdateWatcher] Watched map unloaded; stopping watch.");
			StopWatch();
		}
	}

	private void OnHmdMounted()
	{
		TriggerCheck();
	}

	private IEnumerator PollLoop()
	{
		WaitForSeconds wait = new WaitForSeconds(10f);
		while (CustomMapDevWatchMode.IsWatching)
		{
			yield return wait;
			if (CustomMapDevWatchMode.IsWatching)
			{
				if (!IsPlayerInStump())
				{
					StopWatch();
					break;
				}
				TriggerCheck();
				continue;
			}
			break;
		}
	}

	private void TriggerCheck()
	{
		if (CustomMapDevWatchMode.IsWatching && !checkInProgress && !reloadInProgress && CanReloadNow() && !CustomMapManager.IsLoading() && !CustomMapManager.IsUnloading())
		{
			CheckForUpdate();
		}
	}

	private async Task CheckForUpdate()
	{
		checkInProgress = true;
		try
		{
			ModId modId = watchedModId;
			long num = await ModIOManager.GetNewerModfileId(modId);
			if (CustomMapDevWatchMode.IsWatchingMap(modId) && !reloadInProgress && num > 0 && CanReloadNow() && IsPlayerInStump() && !CustomMapManager.IsLoading() && !CustomMapManager.IsUnloading())
			{
				GTDev.Log($"[CustomMapUpdateWatcher] Newer version of {modId} found; reloading.");
				bool returnToPosition = CustomMapDevWatchMode.ReturnToPosition;
				if (VirtualStumpSerializer.Instance != null)
				{
					VirtualStumpSerializer.Instance.SendRoomMapUpdated(modId, num, returnToPosition);
				}
				StartCoroutine(ReloadSequence(modId, returnToPosition));
			}
		}
		finally
		{
			checkInProgress = false;
		}
	}

	public static void OnRoomMapUpdated(ModId modId, long modfileId, bool returnToPosition)
	{
		if (!(instance == null) && !(modId == ModId.Null) && modfileId > 0)
		{
			instance.hasRoomUpdate = true;
			instance.roomUpdateModId = modId;
			instance.roomUpdateModfileId = modfileId;
			instance.roomUpdateReturnToPosition = returnToPosition;
			if (instance.roomUpdateCoroutine == null)
			{
				instance.roomUpdateCoroutine = instance.StartCoroutine(instance.ApplyRoomUpdates());
			}
		}
	}

	private IEnumerator ApplyRoomUpdates()
	{
		while (hasRoomUpdate)
		{
			while (reloadInProgress || CustomMapManager.IsLoading() || CustomMapManager.IsUnloading())
			{
				yield return null;
			}
			hasRoomUpdate = false;
			ModId modId = roomUpdateModId;
			long modfileId = roomUpdateModfileId;
			bool returnToPosition = roomUpdateReturnToPosition;
			if (!IsInPrivateRoom() || !CustomMapLoader.IsMapLoaded(modId))
			{
				continue;
			}
			Task<bool> waitForModfile = ModIOManager.WaitForModfileToInstall(modId, modfileId);
			while (!waitForModfile.IsCompleted)
			{
				yield return null;
			}
			if (waitForModfile.Status != TaskStatus.RanToCompletion || !waitForModfile.Result)
			{
				GTDev.Log($"[CustomMapUpdateWatcher] Modfile {modfileId} of {modId} is already installed or " + "not available; not reloading.");
			}
			else if (!hasRoomUpdate)
			{
				if (CustomMapManager.IsLoading() || CustomMapManager.IsUnloading())
				{
					hasRoomUpdate = true;
				}
				else if (IsInPrivateRoom() && CustomMapLoader.IsMapLoaded(modId))
				{
					GTDev.Log($"[CustomMapUpdateWatcher] Room watcher updated {modId}; reloading.");
					yield return ReloadSequence(modId, returnToPosition);
				}
			}
		}
		roomUpdateCoroutine = null;
	}

	private IEnumerator ReloadSequence(ModId modId, bool returnToPosition)
	{
		reloadInProgress = true;
		CustomMapDevWatchMode.CapturePlayerPosition(returnToPosition);
		CustomMapManager.ReturnToVirtualStump();
		yield return null;
		bool unloadComplete = false;
		UnityAction onUnloaded = delegate
		{
			unloadComplete = true;
		};
		CustomMapManager.OnMapUnloadComplete.AddListener(onUnloaded);
		if (CustomMapManager.UnloadMap(returnToSinglePlayerIfInPublic: false))
		{
			float unloadDeadline = Time.time + 30f;
			while (!unloadComplete && Time.time < unloadDeadline)
			{
				yield return null;
			}
		}
		CustomMapManager.OnMapUnloadComplete.RemoveListener(onUnloaded);
		bool loadComplete = false;
		bool loadSucceeded = false;
		UnityAction<bool> onLoaded = delegate(bool ok)
		{
			loadComplete = true;
			loadSucceeded = ok;
		};
		CustomMapManager.OnMapLoadComplete.AddListener(onLoaded);
		CustomMapManager.SetRoomMap(modId);
		CustomMapManager.LoadMap(modId, GTMapLoadSource.room_reload);
		float reloadDeadline = Time.time + 300f;
		while (!loadComplete && Time.time < reloadDeadline)
		{
			yield return null;
		}
		CustomMapManager.OnMapLoadComplete.RemoveListener(onLoaded);
		if (loadSucceeded)
		{
			yield return null;
			GTDev.Log(CustomMapDevWatchMode.RestorePlayerPosition(returnToPosition) ? "[CustomMapUpdateWatcher] Reload complete; restored player position." : "[CustomMapUpdateWatcher] Reload complete (no position restore).");
		}
		else
		{
			GTDev.LogWarning("[CustomMapUpdateWatcher] Reload did not complete successfully; leaving the player where they are.");
			CustomMapDevWatchMode.ClearSavedPosition();
		}
		reloadInProgress = false;
		if (CustomMapDevWatchMode.IsWatching && !IsPlayerInStump())
		{
			StopWatch();
		}
	}

	private static bool CanReloadNow()
	{
		NetworkSystem networkSystem = NetworkSystem.Instance;
		if (networkSystem == null || !networkSystem.InRoom)
		{
			return true;
		}
		if (networkSystem.SessionIsPrivate)
		{
			return CustomMapsTerminal.IsDriver;
		}
		return false;
	}

	private static bool IsInPrivateRoom()
	{
		NetworkSystem networkSystem = NetworkSystem.Instance;
		if (networkSystem != null && networkSystem.InRoom)
		{
			return networkSystem.SessionIsPrivate;
		}
		return false;
	}

	private static bool IsPlayerInStump()
	{
		if (GorillaComputer.hasInstance)
		{
			return GorillaComputer.instance.IsPlayerInVirtualStump();
		}
		return false;
	}

	private void OnDestroy()
	{
		CustomMapDevWatchMode.OnWatchStarted.RemoveListener(OnWatchStarted);
		CustomMapDevWatchMode.OnWatchStopped.RemoveListener(OnWatchStopped);
		TeardownPolling();
	}
}
