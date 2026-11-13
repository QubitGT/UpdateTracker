using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GorillaTag.Audio;
using UnityEngine;

namespace GorillaTag.Reactions;

public class FireManager : ITickSystemPost
{
	[OnEnterPlay_Clear]
	private static readonly Dictionary<int, FireInstance> _kGObjInstId_to_fire = new Dictionary<int, FireInstance>(256);

	[OnEnterPlay_Clear]
	private static readonly List<FireInstance> _kEnabledReactions = new List<FireInstance>(256);

	[OnEnterPlay_Clear]
	private static readonly List<FireInstance> _kFiresToDespawn = new List<FireInstance>(256);

	[OnEnterPlay_Clear]
	private static readonly Dictionary<Vector3Int, int> _fireSpatialGrid = new Dictionary<Vector3Int, int>(256);

	private const float _kSpatialGridCellSize = 0.2f;

	private const int _kMaxAudioSources = 8;

	[OnEnterPlay_Set(0)]
	private static int _activeAudioSources;

	[field: OnEnterPlay_SetNull]
	internal static FireManager instance { get; private set; }

	[field: OnEnterPlay_Set(false)]
	internal static bool hasInstance { get; private set; }

	bool ITickSystemPost.PostTickRunning { get; set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Initialize()
	{
		if (!ApplicationQuittingState.IsQuitting && !hasInstance)
		{
			instance = new FireManager();
			hasInstance = true;
			TickSystem<object>.AddPostTickCallback(instance);
		}
	}

	internal static void Register(FireInstance f)
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (f == null)
		{
			Debug.LogError("FireManager: You tried to register null!");
		}
		else if (f.GetComponentAndSetFieldIfNullElseLogAndDisable(ref f._thermalVolume, "_thermalVolume", "ThermalSourceVolume", "Disabling.", "Register") & f.GetComponentAndSetFieldIfNullElseLogAndDisable(ref f._loopingAudioSource, "_loopingAudioSource", "AudioSource", "Disabling.", "Register") & f.DisableIfNull(f._extinguishSound.obj, "_extinguishSound", "AudioClip", "Register") & f.DisableIfNull(f._igniteSound.obj, "_igniteSound", "AudioClip", "Register"))
		{
			int instanceID = f.gameObject.GetInstanceID();
			if (!_kGObjInstId_to_fire.TryAdd(instanceID, f))
			{
				Debug.LogError("FireManager: \"" + f.name + "\" was attempted to be registered more than once!", f);
				return;
			}
			f._isRegistered = true;
			f._transform = f.transform;
			f._gameObjectId = instanceID;
			f._defaultTemperature = f._thermalVolume.celsius;
			f._timeSinceExtinguished = f._stayExtinguishedDuration;
			f._deathStateDuration = 0f;
		}
	}

	internal static void Unregister(FireInstance f)
	{
		if (!ApplicationQuittingState.IsQuitting && f._isRegistered)
		{
			f._isRegistered = false;
			int instanceID = f.gameObject.GetInstanceID();
			if (_kGObjInstId_to_fire.TryGetValue(instanceID, out var value) && value == f)
			{
				_kGObjInstId_to_fire.Remove(instanceID);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector3Int GetSpatialGridPos(Vector3 pos)
	{
		return Vector3Int.FloorToInt(pos / 0.2f);
	}

	private static void ResetFireValues(FireInstance f)
	{
		f._timeSinceExtinguished = Mathf.Min(f._timeSinceExtinguished, f._stayExtinguishedDuration);
		f._timeSinceDyingStart = 0f;
		f._isDespawning = false;
		f._timeAlive = 0f;
		f._thermalVolume.celsius = f._defaultTemperature;
	}

	internal static void SpawnFire(SinglePool pool, Vector3 pos, Vector3 normal, float scale)
	{
		SpawnFire(pool, pos, normal, scale, null);
	}

	internal static void SpawnFire(SinglePool pool, Vector3 pos, Vector3 normal, float scale, Quaternion? rotationOverride)
	{
		if (_fireSpatialGrid.TryGetValue(GetSpatialGridPos(pos), out var value) && _kGObjInstId_to_fire.TryGetValue(value, out var value2))
		{
			ResetFireValues(value2);
			return;
		}
		GameObject gameObject = pool.Instantiate(setActive: false);
		Transform transform = gameObject.transform;
		transform.position = pos;
		if (rotationOverride.HasValue)
		{
			transform.rotation = rotationOverride.Value;
		}
		else
		{
			transform.up = normal;
		}
		transform.localScale = Vector3.one * scale;
		gameObject.SetActive(value: true);
	}

	internal static void OnEnable(FireInstance f)
	{
		if (!ApplicationQuittingState.IsQuitting && f._isRegistered && (!f._despawnOnExtinguish || (!(ObjectPools.instance == null) && ObjectPools.instance.initialized)))
		{
			ResetFireValues(f);
			Vector3 position = f._transform.position;
			UpdateSpatialGrid(f, position);
			f._enabledIndex = _kEnabledReactions.Count;
			_kEnabledReactions.Add(f);
			if (GTAudioOneShot.isInitialized && Time.realtimeSinceStartup > 10f)
			{
				GTAudioOneShot.Play(f._igniteSound, position, f._igniteSoundVolume);
			}
			f._hasAudioSlot = HasLoopingClip(f) && _activeAudioSources < 8;
			if (f._hasAudioSlot)
			{
				_activeAudioSources++;
			}
			f._loopingAudioSource.enabled = f._hasAudioSlot;
		}
	}

	internal static void OnDisable(FireInstance f)
	{
		if (!ApplicationQuittingState.IsQuitting)
		{
			int enabledIndex = f._enabledIndex;
			if (enabledIndex >= 0)
			{
				int index = _kEnabledReactions.Count - 1;
				FireInstance fireInstance = _kEnabledReactions[index];
				_kEnabledReactions[enabledIndex] = fireInstance;
				fireInstance._enabledIndex = enabledIndex;
				_kEnabledReactions.RemoveAt(index);
				f._enabledIndex = -1;
			}
			if (f._isInSpatialGrid)
			{
				_fireSpatialGrid.Remove(f._spatialGridPosition);
				f._isInSpatialGrid = false;
			}
			if (f._hasAudioSlot)
			{
				_activeAudioSources = Mathf.Max(_activeAudioSources - 1, 0);
				f._hasAudioSlot = false;
				GiveFreeAudioSlotToSilentFire();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HasLoopingClip(FireInstance f)
	{
		return f._loopingAudioSource.clip != null;
	}

	private static void GiveFreeAudioSlotToSilentFire()
	{
		if (_activeAudioSources >= 8)
		{
			return;
		}
		for (int i = 0; i < _kEnabledReactions.Count; i++)
		{
			FireInstance fireInstance = _kEnabledReactions[i];
			if (!fireInstance._hasAudioSlot && !fireInstance._isDespawning && HasLoopingClip(fireInstance))
			{
				fireInstance._hasAudioSlot = true;
				_activeAudioSources++;
				fireInstance._loopingAudioSource.enabled = true;
				break;
			}
		}
	}

	private static void UpdateSpatialGrid(FireInstance f, Vector3 pos)
	{
		Vector3Int spatialGridPos = GetSpatialGridPos(pos);
		if (f._isInSpatialGrid)
		{
			if (spatialGridPos == f._spatialGridPosition)
			{
				return;
			}
			_fireSpatialGrid.Remove(f._spatialGridPosition);
			f._isInSpatialGrid = false;
		}
		f._spatialGridPosition = spatialGridPos;
		f._isInSpatialGrid = _fireSpatialGrid.TryAdd(spatialGridPos, f._gameObjectId);
	}

	internal static void OnTriggerEnter(FireInstance f, Collider other)
	{
		if (!f._isDespawning && !ApplicationQuittingState.IsQuitting && other.gameObject.layer == 4)
		{
			Extinguish(f.gameObject, float.MaxValue);
		}
	}

	internal static bool Extinguish(GameObject gObj, float extinguishAmount)
	{
		if (ApplicationQuittingState.IsQuitting || extinguishAmount <= 0f)
		{
			return false;
		}
		if (!_kGObjInstId_to_fire.TryGetValue(gObj.GetInstanceID(), out var value))
		{
			return false;
		}
		if (value._enabledIndex < 0 || value._isDespawning)
		{
			return false;
		}
		if (value._defaultTemperature <= 0f)
		{
			return false;
		}
		float celsius = value._thermalVolume.celsius;
		if (celsius <= 0f)
		{
			value._timeSinceExtinguished = 0f;
			return true;
		}
		float num = celsius - extinguishAmount;
		if (num > 0f)
		{
			value._thermalVolume.celsius = num;
			return true;
		}
		value._thermalVolume.celsius = 0f;
		value._timeSinceExtinguished = 0f;
		GTAudioOneShot.Play(value._extinguishSound, value._transform.position, value._extinguishSoundVolume);
		if (value._despawnOnExtinguish)
		{
			StartDying(value);
		}
		return true;
	}

	private static void StartDying(FireInstance f)
	{
		f._isDespawning = true;
		f._timeSinceDyingStart = 0f;
		f._dyingStartTemperature = f._thermalVolume.celsius;
	}

	void ITickSystemPost.PostTick()
	{
		if (ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		for (int i = 0; i < _kEnabledReactions.Count; i++)
		{
			TickFire(_kEnabledReactions[i], unscaledDeltaTime);
		}
		if (_kFiresToDespawn.Count != 0)
		{
			for (int j = 0; j < _kFiresToDespawn.Count; j++)
			{
				ObjectPools.instance.Destroy(_kFiresToDespawn[j].gameObject);
			}
			_kFiresToDespawn.Clear();
		}
	}

	private static void TickFire(FireInstance f, float deltaTime)
	{
		UpdateSpatialGrid(f, f._transform.position);
		f._timeAlive += deltaTime;
		bool isExtinguished = f.isExtinguished;
		f._timeSinceExtinguished += deltaTime;
		bool isExtinguished2 = f.isExtinguished;
		if (f._isDespawning)
		{
			f._timeSinceDyingStart += deltaTime;
			if (f._timeSinceDyingStart >= f._deathStateDuration)
			{
				_kFiresToDespawn.Add(f);
			}
		}
		else if (f._despawnOnExtinguish && f._timeAlive > f._maxLifetime)
		{
			StartDying(f);
			GTAudioOneShot.Play(f._extinguishSound, f._transform.position, f._extinguishSoundVolume);
		}
		else if (isExtinguished && !isExtinguished2)
		{
			GTAudioOneShot.Play(f._igniteSound, f._transform.position, f._igniteSoundVolume);
		}
		float num = f._thermalVolume.celsius;
		if (f._isDespawning)
		{
			num = ((f._deathStateDuration > 0f) ? Mathf.Lerp(f._dyingStartTemperature, 0f, f._timeSinceDyingStart / f._deathStateDuration) : 0f);
		}
		else if (!isExtinguished2 && num < f._defaultTemperature)
		{
			num = Mathf.Min(num + f._reheatSpeed * deltaTime, f._defaultTemperature);
		}
		f._thermalVolume.celsius = num;
		if (f._hasAudioSlot)
		{
			f._loopingAudioSource.volume = ((f._defaultTemperature > 0f) ? (num / f._defaultTemperature) : 1f);
		}
	}
}
