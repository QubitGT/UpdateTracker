using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Helpers/Cinemachine Collision Impulse Source")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineCollisionImpulseSource.html")]
public class CinemachineCollisionImpulseSource : CinemachineImpulseSource
{
	[Header("Trigger Object Filter")]
	[Tooltip("Only collisions with objects on these layers will generate Impulse events")]
	[FormerlySerializedAs("m_LayerMask")]
	public LayerMask LayerMask = 1;

	[TagField]
	[Tooltip("No Impulse events will be generated for collisions with objects having these tags")]
	[FormerlySerializedAs("m_IgnoreTag")]
	public string IgnoreTag = string.Empty;

	[Header("How To Generate The Impulse")]
	[Tooltip("If checked, signal direction will be affected by the direction of impact")]
	[FormerlySerializedAs("m_UseImpactDirection")]
	public bool UseImpactDirection;

	[Tooltip("If checked, signal amplitude will be multiplied by the mass of the impacting object")]
	[FormerlySerializedAs("m_ScaleImpactWithMass")]
	public bool ScaleImpactWithMass;

	[Tooltip("If checked, signal amplitude will be multiplied by the speed of the impacting object")]
	[FormerlySerializedAs("m_ScaleImpactWithSpeed")]
	public bool ScaleImpactWithSpeed;

	private Rigidbody m_RigidBody;

	private Rigidbody2D m_RigidBody2D;

	private void Reset()
	{
		LayerMask = 1;
		IgnoreTag = string.Empty;
		UseImpactDirection = false;
		ScaleImpactWithMass = false;
		ScaleImpactWithSpeed = false;
	}

	private void Start()
	{
		TryGetComponent<Rigidbody>(out m_RigidBody);
		TryGetComponent<Rigidbody2D>(out m_RigidBody2D);
	}

	private void OnEnable()
	{
	}

	private void OnCollisionEnter(Collision c)
	{
		GenerateImpactEvent(c.collider, c.relativeVelocity);
	}

	private void OnTriggerEnter(Collider c)
	{
		GenerateImpactEvent(c, Vector3.zero);
	}

	private float GetMassAndVelocity(Collider other, ref Vector3 vel)
	{
		bool flag = vel == Vector3.zero;
		float num = 1f;
		if (ScaleImpactWithMass || ScaleImpactWithSpeed || UseImpactDirection)
		{
			if (m_RigidBody != null)
			{
				if (ScaleImpactWithMass)
				{
					num *= m_RigidBody.mass;
				}
				if (flag)
				{
					vel = -m_RigidBody.linearVelocity;
				}
			}
			Rigidbody rigidbody = ((other != null) ? other.attachedRigidbody : null);
			if (rigidbody != null)
			{
				if (ScaleImpactWithMass)
				{
					num *= rigidbody.mass;
				}
				if (flag)
				{
					vel += rigidbody.linearVelocity;
				}
			}
		}
		return num;
	}

	private void GenerateImpactEvent(Collider other, Vector3 vel)
	{
		if (!base.enabled)
		{
			return;
		}
		if (other != null)
		{
			int layer = other.gameObject.layer;
			if (((1 << layer) & (int)LayerMask) == 0 || (IgnoreTag.Length != 0 && other.CompareTag(IgnoreTag)))
			{
				return;
			}
		}
		float num = GetMassAndVelocity(other, ref vel);
		if (ScaleImpactWithSpeed)
		{
			num *= Mathf.Sqrt(vel.magnitude);
		}
		Vector3 vector = DefaultVelocity;
		if (UseImpactDirection && !vel.AlmostZero())
		{
			vector = -vel.normalized * vector.magnitude;
		}
		GenerateImpulseWithVelocity(vector * num);
	}

	private void OnCollisionEnter2D(Collision2D c)
	{
		GenerateImpactEvent2D(c.collider, c.relativeVelocity);
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		GenerateImpactEvent2D(c, Vector3.zero);
	}

	private float GetMassAndVelocity2D(Collider2D other2d, ref Vector3 vel)
	{
		bool flag = vel == Vector3.zero;
		float num = 1f;
		if (ScaleImpactWithMass || ScaleImpactWithSpeed || UseImpactDirection)
		{
			if (m_RigidBody2D != null)
			{
				if (ScaleImpactWithMass)
				{
					num *= m_RigidBody2D.mass;
				}
				if (flag)
				{
					vel = -m_RigidBody2D.linearVelocity;
				}
			}
			Rigidbody2D rigidbody2D = ((other2d != null) ? other2d.attachedRigidbody : null);
			if (rigidbody2D != null)
			{
				if (ScaleImpactWithMass)
				{
					num *= rigidbody2D.mass;
				}
				if (flag)
				{
					Vector3 vector = rigidbody2D.linearVelocity;
					vel += vector;
				}
			}
		}
		return num;
	}

	private void GenerateImpactEvent2D(Collider2D other2d, Vector3 vel)
	{
		if (!base.enabled)
		{
			return;
		}
		if (other2d != null)
		{
			int layer = other2d.gameObject.layer;
			if (((1 << layer) & (int)LayerMask) == 0 || (IgnoreTag.Length != 0 && other2d.CompareTag(IgnoreTag)))
			{
				return;
			}
		}
		float num = GetMassAndVelocity2D(other2d, ref vel);
		if (ScaleImpactWithSpeed)
		{
			num *= Mathf.Sqrt(vel.magnitude);
		}
		Vector3 vector = DefaultVelocity;
		if (UseImpactDirection && !vel.AlmostZero())
		{
			vector = -vel.normalized * vector.magnitude;
		}
		GenerateImpulseWithVelocity(vector * num);
	}
}
