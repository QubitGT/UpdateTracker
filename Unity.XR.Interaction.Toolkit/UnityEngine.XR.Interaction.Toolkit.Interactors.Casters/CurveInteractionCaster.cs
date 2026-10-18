using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

[DisallowMultipleComponent]
[AddComponentMenu("XR/Interactors/Curve Interaction Caster", 22)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Casters.CurveInteractionCaster.html")]
public class CurveInteractionCaster : InteractionCasterBase, ICurveInteractionCaster, IInteractionCaster, IUIModelUpdater
{
	public enum HitDetectionType
	{
		Raycast,
		SphereCast,
		ConeCast
	}

	public enum QuerySnapVolumeInteraction
	{
		Ignore,
		Collide
	}

	protected sealed class RaycastHitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit a, RaycastHit b)
		{
			return a.distance.CompareTo(b.distance);
		}
	}

	private const int k_MaxRaycastHits = 10;

	private const int k_MinNumCurveSegments = 1;

	private const int k_MaxNumCurveSegments = 100;

	private NativeArray<Vector3> m_SamplePoints;

	[SerializeField]
	private LayerMask m_RaycastMask = -1;

	[SerializeField]
	private QueryTriggerInteraction m_RaycastTriggerInteraction = QueryTriggerInteraction.Ignore;

	[SerializeField]
	private QuerySnapVolumeInteraction m_RaycastSnapVolumeInteraction = QuerySnapVolumeInteraction.Collide;

	[SerializeField]
	private QueryUIDocumentInteraction m_RaycastUIDocumentTriggerInteraction = QueryUIDocumentInteraction.Collide;

	[SerializeField]
	[Range(1f, 100f)]
	private int m_TargetNumCurveSegments = 1;

	[SerializeField]
	private HitDetectionType m_HitDetectionType = HitDetectionType.ConeCast;

	[SerializeField]
	private float m_CastDistance = 10f;

	[SerializeField]
	[Range(0.01f, 0.25f)]
	private float m_SphereCastRadius = 0.1f;

	[SerializeField]
	private float m_ConeCastAngle = 3f;

	private float m_CachedConeCastAngle;

	private float m_CachedConeCastRadius;

	[SerializeField]
	private bool m_LiveConeCastDebugVisuals;

	private PhysicsScene m_LocalPhysicsScene;

	private int m_RaycastHitsCount;

	private readonly RaycastHit[] m_RaycastHits = new RaycastHit[10];

	private readonly RaycastHitComparer m_RaycastHitComparer = new RaycastHitComparer();

	private static readonly RaycastHit[] s_SpherecastScratch = new RaycastHit[10];

	private static readonly HashSet<Collider> s_OptimalHits = new HashSet<Collider>();

	private readonly List<Tuple<Vector3, float>> m_ConeCastDebugInfo = new List<Tuple<Vector3, float>>();

	public NativeArray<Vector3> samplePoints
	{
		get
		{
			return m_SamplePoints;
		}
		protected set
		{
			m_SamplePoints = value;
		}
	}

	public Vector3 lastSamplePoint
	{
		get
		{
			if (base.isInitialized && m_SamplePoints.Length == targetNumCurveSegments + 1)
			{
				return m_SamplePoints[targetNumCurveSegments];
			}
			return base.castOrigin.position;
		}
	}

	public LayerMask raycastMask
	{
		get
		{
			return m_RaycastMask;
		}
		set
		{
			m_RaycastMask = value;
		}
	}

	public QueryTriggerInteraction raycastTriggerInteraction
	{
		get
		{
			return m_RaycastTriggerInteraction;
		}
		set
		{
			m_RaycastTriggerInteraction = value;
		}
	}

	public QuerySnapVolumeInteraction raycastSnapVolumeInteraction
	{
		get
		{
			return m_RaycastSnapVolumeInteraction;
		}
		set
		{
			m_RaycastSnapVolumeInteraction = value;
		}
	}

	public QueryUIDocumentInteraction raycastUIDocumentTriggerInteraction
	{
		get
		{
			return m_RaycastUIDocumentTriggerInteraction;
		}
		set
		{
			m_RaycastUIDocumentTriggerInteraction = value;
		}
	}

	public int targetNumCurveSegments
	{
		get
		{
			return m_TargetNumCurveSegments;
		}
		set
		{
			m_TargetNumCurveSegments = Mathf.Clamp(value, 1, 100);
			base.isInitialized = false;
		}
	}

	public HitDetectionType hitDetectionType
	{
		get
		{
			return m_HitDetectionType;
		}
		set
		{
			m_HitDetectionType = value;
		}
	}

	public float castDistance
	{
		get
		{
			return m_CastDistance;
		}
		set
		{
			m_CastDistance = value;
		}
	}

	public float sphereCastRadius
	{
		get
		{
			return m_SphereCastRadius;
		}
		set
		{
			m_SphereCastRadius = Mathf.Clamp(value, 0.01f, 0.25f);
		}
	}

	public float coneCastAngle
	{
		get
		{
			return m_ConeCastAngle;
		}
		set
		{
			m_ConeCastAngle = value;
		}
	}

	private float coneCastAngleRadius
	{
		get
		{
			if (!Mathf.Approximately(m_CachedConeCastAngle, m_ConeCastAngle))
			{
				m_CachedConeCastAngle = m_ConeCastAngle;
				m_CachedConeCastRadius = math.tan(math.radians(m_CachedConeCastAngle) * 0.5f);
			}
			return m_CachedConeCastRadius;
		}
	}

	public bool liveConeCastDebugVisuals
	{
		get
		{
			return m_LiveConeCastDebugVisuals;
		}
		set
		{
			m_LiveConeCastDebugVisuals = value;
		}
	}

	protected bool isDestroyed { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		if (!Application.isEditor)
		{
			m_LiveConeCastDebugVisuals = false;
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (samplePoints.IsCreated)
		{
			samplePoints.Dispose();
		}
		isDestroyed = true;
	}

	protected override bool InitializeCaster()
	{
		if (!isDestroyed && !base.isInitialized)
		{
			if (samplePoints.IsCreated)
			{
				samplePoints.Dispose();
			}
			m_SamplePoints = new NativeArray<Vector3>(targetNumCurveSegments + 1, Allocator.Persistent);
			m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
			base.isInitialized = true;
		}
		return base.isInitialized;
	}

	public override bool TryGetColliderTargets(XRInteractionManager interactionManager, List<Collider> targets)
	{
		if (!base.TryGetColliderTargets(interactionManager, targets))
		{
			return false;
		}
		if (UpdatePhysicscastHits(in interactionManager))
		{
			for (int i = 0; i < m_RaycastHitsCount; i++)
			{
				targets.Add(m_RaycastHits[i].collider);
			}
			return true;
		}
		return false;
	}

	public bool TryGetColliderTargets(XRInteractionManager interactionManager, List<Collider> targets, List<RaycastHit> raycastHits)
	{
		raycastHits.Clear();
		if (!base.TryGetColliderTargets(interactionManager, targets))
		{
			return false;
		}
		if (UpdatePhysicscastHits(in interactionManager))
		{
			for (int i = 0; i < m_RaycastHitsCount; i++)
			{
				raycastHits.Add(m_RaycastHits[i]);
				targets.Add(m_RaycastHits[i].collider);
			}
			return true;
		}
		return false;
	}

	protected override void UpdateInternalData()
	{
		base.UpdateInternalData();
		UpdateSamplePoints();
	}

	protected virtual void UpdateSamplePoints()
	{
		if (base.isInitialized)
		{
			Transform transform = base.effectiveCastOrigin;
			Vector3 origin = transform.position;
			Vector3 direction = transform.forward;
			UpdateSamplePoints(in origin, in direction, castDistance, m_SamplePoints);
		}
	}

	protected virtual void UpdateSamplePoints(in Vector3 origin, in Vector3 direction, float totalDistance, NativeArray<Vector3> points)
	{
		int length = points.Length;
		if (length < 2)
		{
			return;
		}
		if (length == 2)
		{
			points[0] = origin;
			points[1] = origin + direction * totalDistance;
			return;
		}
		float num = totalDistance / (float)(length - 1);
		Vector3 vector = direction * num;
		points[0] = origin;
		for (int i = 1; i < length; i++)
		{
			points[i] = points[i - 1] + vector;
		}
	}

	protected virtual bool UpdatePhysicscastHits(in XRInteractionManager interactionManager)
	{
		m_RaycastHitsCount = 0;
		m_ConeCastDebugInfo.Clear();
		float num = 0f;
		for (int i = 1; i < samplePoints.Length; i++)
		{
			float3 @float = samplePoints[0];
			float3 float2 = samplePoints[i - 1];
			float3 float3 = samplePoints[i];
			m_RaycastHitsCount = CheckCollidersBetweenPoints(in interactionManager, float2, float3, @float, m_RaycastHits);
			if (m_RaycastHitsCount > 0)
			{
				for (int j = 0; j < m_RaycastHitsCount; j++)
				{
					m_RaycastHits[j].distance += num;
				}
				break;
			}
			float num2 = math.length(float3 - float2);
			num += num2;
		}
		return m_RaycastHitsCount > 0;
	}

	protected virtual int CheckCollidersBetweenPoints(in XRInteractionManager interactionManager, Vector3 from, Vector3 to, Vector3 origin, RaycastHit[] raycastHits)
	{
		int num = 0;
		float3 x = to - from;
		float maxDistance = math.length(x);
		float3 @float = math.normalize(x);
		QueryTriggerInteraction queryTriggerInteraction = ((m_RaycastSnapVolumeInteraction == QuerySnapVolumeInteraction.Collide) ? QueryTriggerInteraction.Collide : m_RaycastTriggerInteraction);
		switch (m_HitDetectionType)
		{
		case HitDetectionType.Raycast:
			num = m_LocalPhysicsScene.Raycast(from, @float, raycastHits, maxDistance, m_RaycastMask, queryTriggerInteraction);
			break;
		case HitDetectionType.SphereCast:
			num = m_LocalPhysicsScene.SphereCast(from, m_SphereCastRadius, @float, raycastHits, maxDistance, m_RaycastMask, queryTriggerInteraction);
			break;
		case HitDetectionType.ConeCast:
		{
			XRInteractionManager interactionManager2 = interactionManager;
			Vector3 direction = @float;
			num = FilteredConecast(interactionManager2, in from, in direction, in origin, raycastHits, maxDistance, m_RaycastMask, queryTriggerInteraction);
			break;
		}
		}
		if (num > 0)
		{
			if (m_HitDetectionType != HitDetectionType.ConeCast)
			{
				num = FilterOutTriggerColliders(interactionManager, raycastHits, num);
			}
			SortingHelpers.Sort(raycastHits, m_RaycastHitComparer, num);
		}
		return num;
	}

	private int FilteredConecast(XRInteractionManager interactionManager, in Vector3 from, in Vector3 direction, in Vector3 origin, RaycastHit[] results, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		s_OptimalHits.Clear();
		float num = math.min(maxDistance, 1000f);
		int num2 = 0;
		int num3 = m_LocalPhysicsScene.Raycast(from, direction, s_SpherecastScratch, maxDistance, layerMask, queryTriggerInteraction);
		if (num3 > 0)
		{
			num3 = FilterOutTriggerColliders(interactionManager, s_SpherecastScratch, num3);
			for (int i = 0; i < num3; i++)
			{
				RaycastHit raycastHit = s_SpherecastScratch[i];
				if (!(raycastHit.distance > num))
				{
					Collider colliderToCheck = raycastHit.collider;
					if (!interactionManager.IsColliderRegisteredToInteractable(in colliderToCheck))
					{
						num = math.min(raycastHit.distance, num);
						raycastHit.distance += 1.5f;
					}
					results[num2] = raycastHit;
					s_OptimalHits.Add(raycastHit.collider);
					num2++;
				}
			}
		}
		float magnitude = (origin - from).magnitude;
		float maxOffset = num;
		float castMax;
		for (float num4 = 0f; num4 < num && !Mathf.Approximately(num4, num); num4 += castMax)
		{
			float offsetFromOrigin = magnitude + num4;
			BurstPhysicsUtils.GetMultiSegmentConecastParameters(coneCastAngleRadius, num4, offsetFromOrigin, maxOffset, in direction, out var originOffset, out var radius, out castMax);
			if (m_LiveConeCastDebugVisuals)
			{
				m_ConeCastDebugInfo.Add(new Tuple<Vector3, float>(from + originOffset, radius));
				m_ConeCastDebugInfo.Add(new Tuple<Vector3, float>(from + originOffset + castMax * direction, radius));
			}
			int num5 = m_LocalPhysicsScene.SphereCast(from + originOffset, radius, direction, s_SpherecastScratch, castMax, layerMask, queryTriggerInteraction);
			if (num5 <= 0)
			{
				continue;
			}
			num5 = FilterOutTriggerColliders(interactionManager, s_SpherecastScratch, num5);
			for (int j = 0; j < num5 && num2 < results.Length; j++)
			{
				RaycastHit raycastHit2 = s_SpherecastScratch[j];
				if (num4 + raycastHit2.distance > num || s_OptimalHits.Contains(raycastHit2.collider))
				{
					continue;
				}
				Collider colliderToCheck = raycastHit2.collider;
				if (!interactionManager.IsColliderRegisteredToInteractable(in colliderToCheck))
				{
					continue;
				}
				if (Mathf.Approximately(raycastHit2.distance, 0f))
				{
					Vector3 a = raycastHit2.point;
					Vector3 b = Vector3.zero;
					if (BurstMathUtility.FastVectorEquals(in a, in b))
					{
						continue;
					}
				}
				float3 origin2 = from;
				float3 conePoint = raycastHit2.point;
				float3 direction2 = direction;
				BurstPhysicsUtils.GetConecastOffset(in origin2, in conePoint, in direction2, out var coneOffset);
				raycastHit2.distance += num4 + 1f + coneOffset;
				results[num2] = raycastHit2;
				num2++;
			}
		}
		s_OptimalHits.Clear();
		Array.Clear(s_SpherecastScratch, 0, 10);
		return num2;
	}

	private int FilterOutTriggerColliders(XRInteractionManager interactionManager, RaycastHit[] raycastHits, int raycastHitCount)
	{
		bool flag = m_RaycastTriggerInteraction == QueryTriggerInteraction.Collide || (m_RaycastTriggerInteraction == QueryTriggerInteraction.UseGlobal && Physics.queriesHitTriggers);
		if (m_RaycastSnapVolumeInteraction == QuerySnapVolumeInteraction.Ignore && flag)
		{
			raycastHitCount = FilterOutSnapTriggerColliders(in interactionManager, raycastHits, raycastHitCount);
		}
		else if (m_RaycastSnapVolumeInteraction == QuerySnapVolumeInteraction.Collide && !flag)
		{
			raycastHitCount = FilterOutNonSnapTriggerColliders(in interactionManager, raycastHits, raycastHitCount);
		}
		return raycastHitCount;
	}

	private static int FilterOutSnapTriggerColliders(in XRInteractionManager interactionManager, RaycastHit[] raycastHits, int count)
	{
		int num = count;
		for (int i = 0; i < num; i++)
		{
			Collider potentialSnapVolumeCollider = raycastHits[i].collider;
			if (potentialSnapVolumeCollider == null || (potentialSnapVolumeCollider.isTrigger && interactionManager.IsColliderRegisteredSnapVolume(in potentialSnapVolumeCollider)))
			{
				raycastHits[i--] = raycastHits[--num];
			}
		}
		return num;
	}

	private static int FilterOutNonSnapTriggerColliders(in XRInteractionManager interactionManager, RaycastHit[] raycastHits, int count)
	{
		int num = count;
		for (int i = 0; i < num; i++)
		{
			Collider potentialSnapVolumeCollider = raycastHits[i].collider;
			if (potentialSnapVolumeCollider == null || (potentialSnapVolumeCollider.isTrigger && !interactionManager.IsColliderRegisteredSnapVolume(in potentialSnapVolumeCollider) && !XRUIToolkitHandler.HasUIDocument(potentialSnapVolumeCollider)))
			{
				raycastHits[i--] = raycastHits[--num];
			}
		}
		return num;
	}

	public bool UpdateUIModel(ref TrackedDeviceModel uiModel, bool isSelectActive, in Vector2 scrollDelta)
	{
		if (!base.isInitialized)
		{
			return false;
		}
		Transform transform = base.effectiveCastOrigin;
		uiModel.position = transform.position;
		uiModel.orientation = transform.rotation;
		uiModel.select = isSelectActive;
		uiModel.scrollDelta = scrollDelta;
		uiModel.raycastLayerMask = m_RaycastMask;
		uiModel.interactionType = UIInteractionType.Ray;
		List<Vector3> raycastPoints = uiModel.raycastPoints;
		raycastPoints.Clear();
		UpdateInternalData();
		int length = m_SamplePoints.Length;
		if (length <= 0)
		{
			return false;
		}
		if (raycastPoints.Capacity < length)
		{
			raycastPoints.Capacity = length;
		}
		for (int i = 0; i < length; i++)
		{
			raycastPoints.Add(m_SamplePoints[i]);
		}
		return true;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Transform transform = ((base.castOrigin != null) ? base.castOrigin : base.transform);
		Vector3 position = transform.position;
		Vector3 vector = position + transform.forward * castDistance;
		Gizmos.color = new Color(0.22745098f, 0.47843137f, 0.972549f, 79f / 85f);
		switch (m_HitDetectionType)
		{
		case HitDetectionType.Raycast:
			Gizmos.DrawLine(position, vector);
			break;
		case HitDetectionType.SphereCast:
		{
			Vector3 vector4 = transform.up * m_SphereCastRadius;
			Vector3 vector5 = transform.right * m_SphereCastRadius;
			Gizmos.DrawWireSphere(position, m_SphereCastRadius);
			Gizmos.DrawLine(position + vector5, vector + vector5);
			Gizmos.DrawLine(position - vector5, vector - vector5);
			Gizmos.DrawLine(position + vector4, vector + vector4);
			Gizmos.DrawLine(position - vector4, vector - vector4);
			Gizmos.DrawWireSphere(vector, m_SphereCastRadius);
			break;
		}
		case HitDetectionType.ConeCast:
		{
			float num = Mathf.Tan(m_ConeCastAngle * (MathF.PI / 180f) * 0.5f) * castDistance;
			vector = position + transform.forward * (castDistance - num);
			Vector3 vector2 = transform.up * num;
			Vector3 vector3 = transform.right * num;
			Gizmos.DrawLine(position, vector);
			Gizmos.DrawLine(position, vector + vector3);
			Gizmos.DrawLine(position, vector - vector3);
			Gizmos.DrawLine(position, vector + vector2);
			Gizmos.DrawLine(position, vector - vector2);
			Gizmos.DrawWireSphere(vector, num);
			break;
		}
		}
		for (int i = 0; i < m_SamplePoints.Length; i++)
		{
			Vector3 vector6 = m_SamplePoints[i];
			float radius = ((m_HitDetectionType == HitDetectionType.SphereCast) ? m_SphereCastRadius : 0.025f);
			Gizmos.color = new Color(0.6392157f, 0.28627452f, 0.6431373f, 0.75f);
			Gizmos.DrawSphere(vector6, radius);
			if (i < m_SamplePoints.Length - 1)
			{
				Vector3 to = m_SamplePoints[i + 1];
				Gizmos.DrawLine(vector6, to);
			}
		}
		if (!m_LiveConeCastDebugVisuals)
		{
			return;
		}
		for (int j = 0; j < m_ConeCastDebugInfo.Count; j += 2)
		{
			Gizmos.color = Color.yellow;
			for (float num2 = 0f; num2 <= 4f; num2 += 1f)
			{
				float num3 = num2 / 4f;
				Gizmos.DrawWireSphere(m_ConeCastDebugInfo[j].Item1 + num3 * (m_ConeCastDebugInfo[j + 1].Item1 - m_ConeCastDebugInfo[j].Item1), m_ConeCastDebugInfo[j].Item2);
			}
		}
	}

	bool IUIModelUpdater.UpdateUIModel(ref TrackedDeviceModel uiModel, bool isSelectActive, in Vector2 scrollDelta)
	{
		return UpdateUIModel(ref uiModel, isSelectActive, in scrollDelta);
	}
}
