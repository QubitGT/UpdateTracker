using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem;

public class HandCollider : MonoBehaviour
{
	[Serializable]
	public class FingerColliders
	{
		[Tooltip("Starting at tip and going down. Max 2.")]
		public Transform[] thumbColliders = new Transform[1];

		[Tooltip("Starting at tip and going down. Max 3.")]
		public Transform[] indexColliders = new Transform[2];

		[Tooltip("Starting at tip and going down. Max 3.")]
		public Transform[] middleColliders = new Transform[2];

		[Tooltip("Starting at tip and going down. Max 3.")]
		public Transform[] ringColliders = new Transform[2];

		[Tooltip("Starting at tip and going down. Max 3.")]
		public Transform[] pinkyColliders = new Transform[2];

		public Transform[] this[int finger]
		{
			get
			{
				return finger switch
				{
					0 => thumbColliders, 
					1 => indexColliders, 
					2 => middleColliders, 
					3 => ringColliders, 
					4 => pinkyColliders, 
					_ => null, 
				};
			}
			set
			{
				switch (finger)
				{
				case 0:
					thumbColliders = value;
					break;
				case 1:
					indexColliders = value;
					break;
				case 2:
					middleColliders = value;
					break;
				case 3:
					ringColliders = value;
					break;
				case 4:
					pinkyColliders = value;
					break;
				}
			}
		}
	}

	private Rigidbody rigidbody;

	[HideInInspector]
	public HandPhysics hand;

	public LayerMask collisionMask;

	private Collider[] colliders;

	public FingerColliders fingerColliders;

	private static PhysicsMaterial physicMaterial_lowfriction;

	private static PhysicsMaterial physicMaterial_highfriction;

	private float scale;

	private Vector3 center;

	private Vector3 targetPosition = Vector3.zero;

	private Quaternion targetRotation = Quaternion.identity;

	protected const float MaxVelocityChange = 10f;

	protected const float VelocityMagic = 6000f;

	protected const float AngularVelocityMagic = 50f;

	protected const float MaxAngularVelocityChange = 20f;

	public bool collidersInRadius;

	private const float minCollisionEnergy = 0.1f;

	private const float maxCollisionEnergy = 1f;

	private const float minCollisionHapticsTime = 0.2f;

	private float lastCollisionHapticsTime;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.maxAngularVelocity = 50f;
	}

	private void Start()
	{
		colliders = GetComponentsInChildren<Collider>();
		if (physicMaterial_lowfriction == null)
		{
			physicMaterial_lowfriction = new PhysicsMaterial("hand_lowFriction");
			physicMaterial_lowfriction.dynamicFriction = 0f;
			physicMaterial_lowfriction.staticFriction = 0f;
			physicMaterial_lowfriction.bounciness = 0f;
			physicMaterial_lowfriction.bounceCombine = PhysicsMaterialCombine.Minimum;
			physicMaterial_lowfriction.frictionCombine = PhysicsMaterialCombine.Minimum;
		}
		if (physicMaterial_highfriction == null)
		{
			physicMaterial_highfriction = new PhysicsMaterial("hand_highFriction");
			physicMaterial_highfriction.dynamicFriction = 1f;
			physicMaterial_highfriction.staticFriction = 1f;
			physicMaterial_highfriction.bounciness = 0f;
			physicMaterial_highfriction.bounceCombine = PhysicsMaterialCombine.Minimum;
			physicMaterial_highfriction.frictionCombine = PhysicsMaterialCombine.Average;
		}
		SetPhysicMaterial(physicMaterial_lowfriction);
		scale = SteamVR_Utils.GetLossyScale(hand.transform);
	}

	private void SetPhysicMaterial(PhysicsMaterial mat)
	{
		if (colliders == null)
		{
			colliders = GetComponentsInChildren<Collider>();
		}
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].sharedMaterial = mat;
		}
	}

	public void SetCollisionDetectionEnabled(bool value)
	{
		rigidbody.detectCollisions = value;
	}

	public void MoveTo(Vector3 position, Quaternion rotation)
	{
		targetPosition = position;
		targetRotation = rotation;
		ExecuteFixedUpdate();
	}

	public void TeleportTo(Vector3 position, Quaternion rotation)
	{
		targetPosition = position;
		targetRotation = rotation;
		MoveTo(position, rotation);
		rigidbody.position = position;
		if (rotation.x != 0f || rotation.y != 0f || rotation.z != 0f || rotation.w != 0f)
		{
			rigidbody.rotation = rotation;
		}
		base.transform.position = position;
		base.transform.rotation = rotation;
	}

	public void Reset()
	{
		TeleportTo(targetPosition, targetRotation);
	}

	public void SetCenterPoint(Vector3 newCenter)
	{
		center = newCenter;
	}

	protected void ExecuteFixedUpdate()
	{
		collidersInRadius = Physics.CheckSphere(center, 0.2f, collisionMask);
		Vector3 velocityTarget;
		Vector3 angularTarget;
		if (!collidersInRadius)
		{
			rigidbody.linearVelocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.MovePosition(targetPosition);
			rigidbody.MoveRotation(targetRotation);
		}
		else if (GetTargetVelocities(out velocityTarget, out angularTarget))
		{
			float maxDistanceDelta = 20f * scale;
			float maxDistanceDelta2 = 10f * scale;
			rigidbody.linearVelocity = Vector3.MoveTowards(rigidbody.linearVelocity, velocityTarget, maxDistanceDelta2);
			rigidbody.angularVelocity = Vector3.MoveTowards(rigidbody.angularVelocity, angularTarget, maxDistanceDelta);
		}
	}

	protected bool GetTargetVelocities(out Vector3 velocityTarget, out Vector3 angularTarget)
	{
		bool flag = false;
		float num = 6000f;
		float num2 = 50f;
		Vector3 vector = targetPosition - rigidbody.position;
		velocityTarget = vector * num * Time.deltaTime;
		if (!float.IsNaN(velocityTarget.x) && !float.IsInfinity(velocityTarget.x))
		{
			flag = true;
		}
		else
		{
			velocityTarget = Vector3.zero;
		}
		(targetRotation * Quaternion.Inverse(rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);
		if (angle > 180f)
		{
			angle -= 360f;
		}
		if (angle != 0f && !float.IsNaN(axis.x) && !float.IsInfinity(axis.x))
		{
			angularTarget = angle * axis * num2 * Time.deltaTime;
			flag = flag;
		}
		else
		{
			angularTarget = Vector3.zero;
		}
		return flag;
	}

	private void OnCollisionEnter(Collision collision)
	{
		bool flag = false;
		if (collision.rigidbody != null && !collision.rigidbody.isKinematic)
		{
			flag = true;
		}
		SetPhysicMaterial(flag ? physicMaterial_highfriction : physicMaterial_lowfriction);
		float magnitude = collision.relativeVelocity.magnitude;
		if (magnitude > 0.1f && Time.time - lastCollisionHapticsTime > 0.2f)
		{
			lastCollisionHapticsTime = Time.time;
			float amplitude = Util.RemapNumber(magnitude, 0.1f, 1f, 0.3f, 1f);
			float duration = Util.RemapNumber(magnitude, 0.1f, 1f, 0f, 0.06f);
			hand.hand.TriggerHapticPulse(duration, 100f, amplitude);
		}
	}
}
