using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public abstract class InteractionCasterBase : MonoBehaviour, IInteractionCaster
{
	[SerializeField]
	[Tooltip("Source of origin and direction used when updating sample points.")]
	private Transform m_CastOrigin;

	[Header("Stabilization Parameters")]
	[SerializeField]
	[Tooltip("Determines whether to stabilize the cast origin.")]
	private bool m_EnableStabilization;

	[SerializeField]
	[Tooltip("Factor for stabilizing position. Larger values increase the range of stabilization, making the effect more pronounced over a greater distance.")]
	private float m_PositionStabilization = 0.25f;

	[SerializeField]
	[Tooltip("Factor for stabilizing angle. Larger values increase the range of stabilization, making the effect more pronounced over a greater angle.")]
	private float m_AngleStabilization = 20f;

	[SerializeField]
	[RequireInterface(typeof(IXRRayProvider))]
	[Tooltip("Optional ray provider for calculating stable rotation.")]
	private Object m_AimTargetObject;

	private readonly UnityObjectReferenceCache<IXRRayProvider, Object> m_AimTargetObjectRef = new UnityObjectReferenceCache<IXRRayProvider, Object>();

	private bool m_InitializedStabilizationOrigin;

	private Transform m_StabilizationAnchor;

	private float m_LastStabilizationUpdateTime;

	public bool isInitialized { get; protected set; }

	public Transform castOrigin
	{
		get
		{
			return m_CastOrigin;
		}
		set
		{
			m_CastOrigin = value;
		}
	}

	public Transform effectiveCastOrigin
	{
		get
		{
			if (m_EnableStabilization && m_InitializedStabilizationOrigin)
			{
				return m_StabilizationAnchor;
			}
			return castOrigin;
		}
	}

	public bool enableStabilization
	{
		get
		{
			return m_EnableStabilization;
		}
		set
		{
			m_EnableStabilization = value;
		}
	}

	public float positionStabilization
	{
		get
		{
			return m_PositionStabilization;
		}
		set
		{
			m_PositionStabilization = value;
		}
	}

	public float angleStabilization
	{
		get
		{
			return m_AngleStabilization;
		}
		set
		{
			m_AngleStabilization = value;
		}
	}

	public IXRRayProvider aimTarget
	{
		get
		{
			return m_AimTargetObjectRef.Get(m_AimTargetObject);
		}
		set
		{
			m_AimTargetObjectRef.Set(ref m_AimTargetObject, value);
		}
	}

	protected virtual void OnValidate()
	{
		if (m_CastOrigin == null)
		{
			m_CastOrigin = base.transform;
		}
	}

	protected virtual void Awake()
	{
		if (m_CastOrigin == null)
		{
			m_CastOrigin = base.transform;
		}
		InitializeCaster();
		InitializeStabilization();
	}

	protected virtual void OnDestroy()
	{
		isInitialized = false;
	}

	public virtual bool TryGetColliderTargets(XRInteractionManager interactionManager, List<Collider> targets)
	{
		targets.Clear();
		if (!InitializeCaster() && !InitializeStabilization())
		{
			return false;
		}
		UpdateInternalData();
		return true;
	}

	protected abstract bool InitializeCaster();

	protected virtual void UpdateInternalData()
	{
		if (m_EnableStabilization)
		{
			float deltaTime = Time.unscaledTime - m_LastStabilizationUpdateTime;
			m_LastStabilizationUpdateTime = Time.unscaledTime;
			ref Transform stabilizationAnchor = ref m_StabilizationAnchor;
			ref Transform target = ref m_CastOrigin;
			IXRRayProvider iXRRayProvider = aimTarget;
			XRTransformStabilizer.ApplyStabilization(ref stabilizationAnchor, in target, in iXRRayProvider, m_PositionStabilization, m_AngleStabilization, deltaTime);
		}
	}

	protected virtual bool InitializeStabilization()
	{
		if (!m_EnableStabilization || m_InitializedStabilizationOrigin)
		{
			return true;
		}
		if (m_StabilizationAnchor == null)
		{
			if (!ComponentLocatorUtility<XROrigin>.TryFindComponent(out var component))
			{
				Debug.LogError("Failed to find XROrigin component in scene. Cannot stabilize cast origin for " + GetType().Name + ".", this);
				m_EnableStabilization = false;
				return false;
			}
			string text = "";
			if (TryGetComponent<IXRInteractor>(out var component2))
			{
				text = component2.handedness.ToString();
			}
			m_StabilizationAnchor = new GameObject("[" + text + " " + GetType().Name + "] Stabilization Cast Origin").transform;
			m_StabilizationAnchor.SetParent(component.Origin.transform, worldPositionStays: false);
			m_StabilizationAnchor.SetLocalPose(Pose.identity);
			m_InitializedStabilizationOrigin = true;
		}
		return m_InitializedStabilizationOrigin;
	}
}
