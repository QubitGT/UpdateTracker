using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

[DisallowMultipleComponent]
[AddComponentMenu("XR/Interactors/Sphere Interaction Caster", 22)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Casters.SphereInteractionCaster.html")]
public class SphereInteractionCaster : InteractionCasterBase
{
	private const int k_MaxRaycastHits = 10;

	private readonly RaycastHit[] m_OverlapSphereHits = new RaycastHit[10];

	private readonly Collider[] m_OverlapSphereColliderHits = new Collider[10];

	[Header("Filtering Settings")]
	[SerializeField]
	[Tooltip("Layer mask used for limiting sphere cast and sphere overlap targets.")]
	private LayerMask m_PhysicsLayerMask = -1;

	[SerializeField]
	[Tooltip("Determines whether the cast sphere overlap will hit triggers. Use Global refers to the Queries Hit Triggers setting in Physics Project Settings.")]
	private QueryTriggerInteraction m_PhysicsTriggerInteraction = QueryTriggerInteraction.Ignore;

	[Header("Sphere Casting Settings")]
	[SerializeField]
	[Tooltip("Radius of the sphere cast.")]
	private float m_CastRadius = 0.1f;

	private bool m_FirstFrame = true;

	private Vector3 m_LastSphereCastOrigin = Vector3.zero;

	private PhysicsScene m_LocalPhysicsScene;

	public LayerMask physicsLayerMask
	{
		get
		{
			return m_PhysicsLayerMask;
		}
		set
		{
			m_PhysicsLayerMask = value;
		}
	}

	public QueryTriggerInteraction physicsTriggerInteraction
	{
		get
		{
			return m_PhysicsTriggerInteraction;
		}
		set
		{
			m_PhysicsTriggerInteraction = value;
		}
	}

	public float castRadius
	{
		get
		{
			return m_CastRadius;
		}
		set
		{
			m_CastRadius = value;
		}
	}

	protected virtual void OnEnable()
	{
		m_FirstFrame = true;
		m_LastSphereCastOrigin = Vector3.zero;
	}

	protected virtual void OnDisable()
	{
	}

	public override bool TryGetColliderTargets(XRInteractionManager interactionManager, List<Collider> targets)
	{
		if (!base.TryGetColliderTargets(interactionManager, targets))
		{
			return false;
		}
		Vector3 position = base.effectiveCastOrigin.position;
		Vector3 overlapStart = m_LastSphereCastOrigin;
		Vector3 overlapEnd = position;
		float radius = m_CastRadius * base.transform.lossyScale.x;
		BurstPhysicsUtils.GetSphereOverlapParameters(in overlapStart, in overlapEnd, out var normalizedOverlapVector, out var overlapSqrMagnitude, out var overlapDistance);
		bool result;
		if (m_FirstFrame || overlapSqrMagnitude < 0.001f)
		{
			int num = m_LocalPhysicsScene.OverlapSphere(overlapEnd, radius, m_OverlapSphereColliderHits, m_PhysicsLayerMask, m_PhysicsTriggerInteraction);
			for (int i = 0; i < num; i++)
			{
				targets.Add(m_OverlapSphereColliderHits[i]);
			}
			result = num > 0;
		}
		else
		{
			int num2 = m_LocalPhysicsScene.SphereCast(overlapStart, radius, normalizedOverlapVector, m_OverlapSphereHits, overlapDistance, m_PhysicsLayerMask, m_PhysicsTriggerInteraction);
			for (int j = 0; j < num2; j++)
			{
				targets.Add(m_OverlapSphereHits[j].collider);
			}
			result = num2 > 0;
		}
		m_LastSphereCastOrigin = position;
		m_FirstFrame = false;
		return result;
	}

	protected override bool InitializeCaster()
	{
		if (!base.isInitialized)
		{
			m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
			base.isInitialized = true;
		}
		return base.isInitialized;
	}
}
