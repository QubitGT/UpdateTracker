using System;
using UnityEngine;

namespace Oculus.Interaction;

public class AudioPhysics : MonoBehaviour
{
	public class CollisionEvents : MonoBehaviour
	{
		public event Action<Collision> WhenCollisionEnter = delegate
		{
		};

		private void OnCollisionEnter(Collision collision)
		{
			this.WhenCollisionEnter(collision);
		}
	}

	[Tooltip("Add a reference to the rigidbody on this gameobject.")]
	[SerializeField]
	private Rigidbody _rigidbody;

	[Tooltip("Reference an audio trigger instance for soft and hard collisions.")]
	[SerializeField]
	private ImpactAudio _impactAudioEvents;

	[Tooltip("Collisions below this value will play a soft audio event, and collisions above will play a hard audio event.")]
	[Range(0f, 8f)]
	[SerializeField]
	private float _velocitySplit = 1f;

	[Tooltip("Collisions below this value will be ignored and will not play audio.")]
	[Range(0f, 2f)]
	[SerializeField]
	private float _minimumVelocity;

	[Tooltip("The shortest amount of time in seconds between collisions. Used to cull multiple fast collision events.")]
	[Range(0f, 2f)]
	[SerializeField]
	private float _timeBetweenCollisions = 0.2f;

	[Tooltip("By default (false), when two physics objects collide with physics audio components, we only play the one with the higher velocity.Setting this to true will allow both impacts to play.")]
	[SerializeField]
	private bool _allowMultipleCollisions;

	private float _timeAtLastCollision;

	protected bool _started;

	private CollisionEvents _collisionEvents;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_collisionEvents = _rigidbody.gameObject.AddComponent<CollisionEvents>();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_collisionEvents.WhenCollisionEnter += HandleCollisionEnter;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_collisionEvents.WhenCollisionEnter -= HandleCollisionEnter;
		}
	}

	protected virtual void OnDestroy()
	{
		if (_collisionEvents != null)
		{
			UnityEngine.Object.Destroy(_collisionEvents);
		}
	}

	private void HandleCollisionEnter(Collision collision)
	{
		TryPlayCollisionAudio(collision, _rigidbody);
	}

	private void TryPlayCollisionAudio(Collision collision, Rigidbody rigidbody)
	{
		float sqrMagnitude = collision.relativeVelocity.sqrMagnitude;
		if (!(collision.collider.gameObject == null))
		{
			float num = Time.time - _timeAtLastCollision;
			if (!(_timeBetweenCollisions > num) && (_allowMultipleCollisions || !collision.collider.gameObject.TryGetComponent<AudioPhysics>(out var component) || !(GetObjectVelocity(component) > GetObjectVelocity(this))))
			{
				_timeAtLastCollision = Time.time;
				PlayCollisionAudio(_impactAudioEvents, sqrMagnitude);
			}
		}
	}

	private void PlayCollisionAudio(ImpactAudio impactAudio, float magnitude)
	{
		if (!(impactAudio.HardCollisionSound == null) && !(impactAudio.SoftCollisionSound == null) && magnitude > _minimumVelocity)
		{
			if (magnitude > _velocitySplit && impactAudio.HardCollisionSound != null)
			{
				PlaySoundOnAudioTrigger(impactAudio.HardCollisionSound);
			}
			else
			{
				PlaySoundOnAudioTrigger(impactAudio.SoftCollisionSound);
			}
		}
	}

	private static float GetObjectVelocity(AudioPhysics target)
	{
		return target._rigidbody.velocity.sqrMagnitude;
	}

	private void PlaySoundOnAudioTrigger(AudioTrigger audioTrigger)
	{
		if (audioTrigger != null)
		{
			audioTrigger.PlayAudio();
		}
	}
}
