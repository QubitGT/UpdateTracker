using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag.Reactions;

public class FireInstance : MonoBehaviour
{
	[Header("Scene References")]
	[Tooltip("If not assigned it will try to auto assign to a component on the same GameObject.")]
	[SerializeField]
	internal Collider _collider;

	[Tooltip("If not assigned it will try to auto assign to a component on the same GameObject.")]
	[FormerlySerializedAs("_thermalSourceVolume")]
	[SerializeField]
	internal ThermalSourceVolume _thermalVolume;

	[SerializeField]
	internal ParticleSystem _particleSystem;

	[FormerlySerializedAs("_audioSource")]
	[SerializeField]
	internal AudioSource _loopingAudioSource;

	[Tooltip("Not currently applied: intended to darken the emissive color of these renderers as the fire is extinguished.")]
	[SerializeField]
	internal Renderer[] _emissiveRenderers;

	[Header("Asset References")]
	[SerializeField]
	internal GTDirectAssetRef<AudioClip> _extinguishSound;

	[SerializeField]
	internal float _extinguishSoundVolume = 1f;

	[SerializeField]
	internal GTDirectAssetRef<AudioClip> _igniteSound;

	[SerializeField]
	internal float _igniteSoundVolume = 1f;

	[Header("Values")]
	[SerializeField]
	internal bool _despawnOnExtinguish = true;

	[Tooltip("Seconds before the fire burns out and despawns. Only used when Despawn On Extinguish is enabled.")]
	[SerializeField]
	internal float _maxLifetime = 10f;

	[Tooltip("Degrees celsius per second the fire regains, up to its default temperature, after being partially or fully extinguished.")]
	[SerializeField]
	internal float _reheatSpeed = 1f;

	[Tooltip("If you completely extinguish the object, how long should it stay extinguished before it starts reheating?")]
	[SerializeField]
	internal float _stayExtinguishedDuration = 1f;

	internal float _defaultTemperature;

	internal float _timeSinceExtinguished;

	internal float _timeSinceDyingStart;

	internal float _timeAlive;

	internal float _dyingStartTemperature;

	internal bool _isDespawning;

	internal float _deathStateDuration;

	internal bool _isRegistered;

	internal int _enabledIndex = -1;

	internal Transform _transform;

	internal int _gameObjectId;

	internal Vector3Int _spatialGridPosition;

	internal bool _isInSpatialGrid;

	internal bool _hasAudioSlot;

	internal bool isExtinguished => _timeSinceExtinguished < _stayExtinguishedDuration;

	protected void Awake()
	{
		FireManager.Register(this);
	}

	protected void OnDestroy()
	{
		FireManager.Unregister(this);
	}

	protected void OnEnable()
	{
		FireManager.OnEnable(this);
	}

	protected void OnDisable()
	{
		FireManager.OnDisable(this);
	}

	protected void OnTriggerEnter(Collider other)
	{
		FireManager.OnTriggerEnter(this, other);
	}
}
