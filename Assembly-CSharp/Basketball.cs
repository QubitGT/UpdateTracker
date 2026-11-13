using UnityEngine;

[RequireComponent(typeof(GameEntity))]
[AddComponentMenu("Gorilla Tag/Game Entity/Basketball")]
public class Basketball : MonoBehaviour
{
	[SerializeField]
	private GameEntity entity;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip[] bounceSounds;

	[Tooltip("Impacts slower than this (m/s) are silent")]
	[SerializeField]
	private float minImpactSpeed = 0.6f;

	[Tooltip("Impacts at or above this speed (m/s) play at maxVolume")]
	[SerializeField]
	private float fullVolumeImpactSpeed = 6f;

	[SerializeField]
	private float maxVolume = 0.8f;

	[SerializeField]
	private float minTimeBetweenSounds = 0.08f;

	private float _lastSoundTime;

	private void Reset()
	{
		entity = GetComponent<GameEntity>();
		audioSource = GetComponent<AudioSource>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (bounceSounds != null && bounceSounds.Length != 0 && (bool)audioSource && audioSource.isActiveAndEnabled && !entity.IsHeld() && !(Time.time < _lastSoundTime + minTimeBetweenSounds))
		{
			float magnitude = collision.relativeVelocity.magnitude;
			if (!(magnitude < minImpactSpeed))
			{
				_lastSoundTime = Time.time;
				float a = maxVolume * Mathf.InverseLerp(minImpactSpeed, fullVolumeImpactSpeed, magnitude);
				audioSource.GTPlayOneShot(bounceSounds, Mathf.Max(a, 0.1f));
			}
		}
	}
}
