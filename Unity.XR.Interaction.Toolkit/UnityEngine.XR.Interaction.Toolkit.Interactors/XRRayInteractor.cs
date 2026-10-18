using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Curves;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[DisallowMultipleComponent]
[AddComponentMenu("XR/Interactors/XR Ray Interactor", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor.html")]
public class XRRayInteractor : XRBaseInputInteractor, IAdvancedLineRenderable, ILineRenderable, IUIHoverInteractor, IUIInteractor, IXRRayProvider, IXRScaleValueProvider
{
	protected sealed class RaycastHitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit a, RaycastHit b)
		{
			float num = ((a.collider != null) ? a.distance : float.MaxValue);
			float value = ((b.collider != null) ? b.distance : float.MaxValue);
			return num.CompareTo(value);
		}
	}

	public enum LineType
	{
		StraightLine,
		ProjectileCurve,
		BezierCurve
	}

	public enum QuerySnapVolumeInteraction
	{
		Ignore,
		Collide
	}

	public enum HitDetectionType
	{
		Raycast,
		SphereCast,
		ConeCast
	}

	public enum RotateMode
	{
		RotateOverTime,
		MatchDirection
	}

	private struct SamplePoint
	{
		public float3 position { get; set; }

		public float parameter { get; set; }
	}

	[Obsolete("AnchorRotationMode has been deprecated in version 3.0.0. Use RotateMode instead.")]
	public enum AnchorRotationMode
	{
		RotateOverTime,
		MatchDirection
	}

	private const int k_MaxRaycastHits = 10;

	private const int k_MaxSpherecastHits = 10;

	private const int k_MinSampleFrequency = 2;

	private const int k_MaxSampleFrequency = 100;

	private static readonly List<IXRInteractable> s_Results = new List<IXRInteractable>();

	private static readonly RaycastHit[] s_SpherecastScratch = new RaycastHit[10];

	private static readonly HashSet<Collider> s_OptimalHits = new HashSet<Collider>();

	private readonly List<Tuple<Vector3, float>> m_ConeCastDebugInfo = new List<Tuple<Vector3, float>>();

	[SerializeField]
	private LineType m_LineType;

	[SerializeField]
	private bool m_BlendVisualLinePoints = true;

	[SerializeField]
	private float m_MaxRaycastDistance = 30f;

	[SerializeField]
	private Transform m_RayOriginTransform;

	[SerializeField]
	private Transform m_ReferenceFrame;

	[SerializeField]
	private float m_Velocity = 16f;

	[SerializeField]
	private float m_Acceleration = 9.8f;

	[SerializeField]
	private float m_AdditionalGroundHeight = 0.1f;

	[SerializeField]
	private float m_AdditionalFlightTime = 0.5f;

	[SerializeField]
	private float m_EndPointDistance = 30f;

	[SerializeField]
	private float m_EndPointHeight = -10f;

	[SerializeField]
	private float m_ControlPointDistance = 10f;

	[SerializeField]
	private float m_ControlPointHeight = 5f;

	[SerializeField]
	[Range(2f, 100f)]
	private int m_SampleFrequency = 20;

	[SerializeField]
	private HitDetectionType m_HitDetectionType;

	[SerializeField]
	[Range(0.01f, 0.25f)]
	private float m_SphereCastRadius = 0.1f;

	[SerializeField]
	[Range(0f, 180f)]
	private float m_ConeCastAngle = 6f;

	private float m_CachedConeCastAngle;

	private float m_CachedConeCastRadius;

	[SerializeField]
	private bool m_LiveConeCastDebugVisuals;

	[SerializeField]
	private LayerMask m_RaycastMask = -1;

	[SerializeField]
	private QueryTriggerInteraction m_RaycastTriggerInteraction = QueryTriggerInteraction.Ignore;

	[SerializeField]
	private QuerySnapVolumeInteraction m_RaycastSnapVolumeInteraction = QuerySnapVolumeInteraction.Collide;

	[SerializeField]
	private bool m_HitClosestOnly;

	[SerializeField]
	private bool m_HoverToSelect;

	[SerializeField]
	private float m_HoverTimeToSelect = 0.5f;

	[SerializeField]
	private bool m_AutoDeselect;

	[SerializeField]
	private float m_TimeToAutoDeselect = 3f;

	[SerializeField]
	private bool m_EnableUIInteraction = true;

	[SerializeField]
	private bool m_BlockInteractionsWithScreenSpaceUI;

	[SerializeField]
	private bool m_BlockUIOnInteractableSelection = true;

	[FormerlySerializedAs("m_AllowAnchorControl")]
	[SerializeField]
	private bool m_ManipulateAttachTransform = true;

	[SerializeField]
	private bool m_UseForceGrab;

	[SerializeField]
	private float m_RotateSpeed = 180f;

	[SerializeField]
	private float m_TranslateSpeed = 1f;

	[FormerlySerializedAs("m_AnchorRotateReferenceFrame")]
	[SerializeField]
	private Transform m_RotateReferenceFrame;

	[FormerlySerializedAs("m_AnchorRotationMode")]
	[SerializeField]
	private RotateMode m_RotateMode;

	[SerializeField]
	private UIHoverEnterEvent m_UIHoverEntered = new UIHoverEnterEvent();

	[SerializeField]
	private UIHoverExitEvent m_UIHoverExited = new UIHoverExitEvent();

	[SerializeField]
	private bool m_EnableARRaycasting;

	[SerializeField]
	private bool m_OccludeARHitsWith3DObjects;

	[SerializeField]
	private bool m_OccludeARHitsWith2DObjects;

	[SerializeField]
	private ScaleMode m_ScaleMode;

	[SerializeField]
	private XRInputButtonReader m_UIPressInput = new XRInputButtonReader("UI Press");

	[SerializeField]
	private XRInputValueReader<Vector2> m_UIScrollInput = new XRInputValueReader<Vector2>("UI Scroll");

	[SerializeField]
	private XRInputValueReader<Vector2> m_TranslateManipulationInput = new XRInputValueReader<Vector2>("Translate Manipulation");

	[SerializeField]
	private XRInputValueReader<Vector2> m_RotateManipulationInput = new XRInputValueReader<Vector2>("Rotate Manipulation");

	[SerializeField]
	private XRInputValueReader<Vector2> m_DirectionalManipulationInput = new XRInputValueReader<Vector2>("Directional Manipulation");

	[SerializeField]
	private XRInputButtonReader m_ScaleToggleInput = new XRInputButtonReader("Scale Toggle");

	[SerializeField]
	private XRInputValueReader<Vector2> m_ScaleOverTimeInput = new XRInputValueReader<Vector2>("Scale Over Time");

	[SerializeField]
	private XRInputValueReader<float> m_ScaleDistanceDeltaInput = new XRInputValueReader<float>("Scale Distance Delta", XRInputValueReader.InputSourceMode.Unused);

	private bool m_HasRayOriginTransform;

	private bool m_HasReferenceFrame;

	private bool m_ScaleInputActive;

	private readonly List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();

	private readonly Dictionary<IXRInteractable, RaycastHit> m_InteractableRaycastHits = new Dictionary<IXRInteractable, RaycastHit>();

	private float m_LastTimeHoveredObjectChanged;

	private bool m_PassedHoverTimeToSelect;

	private float m_LastTimeAutoSelected;

	private bool m_PassedTimeToAutoDeselect;

	private GameObject m_LastUIObject;

	private float m_LastTimeHoveredUIChanged;

	private bool m_HoverUISelectActive;

	private bool m_BlockUIAutoDeselect;

	private readonly RaycastHit[] m_RaycastHits = new RaycastHit[10];

	private int m_RaycastHitsCount;

	private readonly RaycastHitComparer m_RaycastHitComparer = new RaycastHitComparer();

	private List<SamplePoint> m_SamplePoints;

	private int m_SamplePointsFrameUpdated = -1;

	private int m_RaycastHitEndpointIndex;

	private int m_UIRaycastHitEndpointIndex;

	private readonly float3[] m_ControlPoints = new float3[3];

	private readonly float3[] m_HitChordControlPoints = new float3[3];

	private static List<SamplePoint> s_ScratchSamplePoints;

	private static readonly float3[] s_ScratchControlPoints = new float3[3];

	private PhysicsScene m_LocalPhysicsScene;

	private RegisteredUIInteractorCache m_RegisteredUIInteractorCache;

	private bool m_RaycastHitOccurred;

	private RaycastHit m_RaycastHit;

	private RaycastResult m_UIRaycastHit;

	private bool m_IsUIHitClosest;

	private IXRInteractable m_RaycastInteractable;

	[Obsolete("m_ActionBasedController has been deprecated in version 3.0.0.")]
	private ActionBasedController m_ActionBasedController;

	[Obsolete("m_DeviceBasedController has been deprecated in version 3.0.0.")]
	private XRController m_DeviceBasedController;

	[Obsolete("m_ScreenSpaceController has been deprecated in version 3.0.0.")]
	private XRScreenSpaceController m_ScreenSpaceController;

	[Obsolete("m_IsActionBasedController has been deprecated in version 3.0.0.")]
	private bool m_IsActionBasedController;

	[Obsolete("m_IsDeviceBasedController has been deprecated in version 3.0.0.")]
	private bool m_IsDeviceBasedController;

	[Obsolete("m_IsScreenSpaceController has been deprecated in version 3.0.0.")]
	private bool m_IsScreenSpaceController;

	public LineType lineType
	{
		get
		{
			return m_LineType;
		}
		set
		{
			m_LineType = value;
		}
	}

	public bool blendVisualLinePoints
	{
		get
		{
			return m_BlendVisualLinePoints;
		}
		set
		{
			m_BlendVisualLinePoints = value;
		}
	}

	public float maxRaycastDistance
	{
		get
		{
			return m_MaxRaycastDistance;
		}
		set
		{
			m_MaxRaycastDistance = value;
		}
	}

	public Transform rayOriginTransform
	{
		get
		{
			return m_RayOriginTransform;
		}
		set
		{
			m_RayOriginTransform = value;
			m_HasRayOriginTransform = m_RayOriginTransform != null;
		}
	}

	public Transform referenceFrame
	{
		get
		{
			return m_ReferenceFrame;
		}
		set
		{
			m_ReferenceFrame = value;
			m_HasReferenceFrame = m_ReferenceFrame != null;
		}
	}

	public float velocity
	{
		get
		{
			return m_Velocity;
		}
		set
		{
			m_Velocity = value;
		}
	}

	public float acceleration
	{
		get
		{
			return m_Acceleration;
		}
		set
		{
			m_Acceleration = value;
		}
	}

	public float additionalGroundHeight
	{
		get
		{
			return m_AdditionalGroundHeight;
		}
		set
		{
			m_AdditionalGroundHeight = value;
		}
	}

	public float additionalFlightTime
	{
		get
		{
			return m_AdditionalFlightTime;
		}
		set
		{
			m_AdditionalFlightTime = value;
		}
	}

	public float endPointDistance
	{
		get
		{
			return m_EndPointDistance;
		}
		set
		{
			m_EndPointDistance = value;
		}
	}

	public float endPointHeight
	{
		get
		{
			return m_EndPointHeight;
		}
		set
		{
			m_EndPointHeight = value;
		}
	}

	public float controlPointDistance
	{
		get
		{
			return m_ControlPointDistance;
		}
		set
		{
			m_ControlPointDistance = value;
		}
	}

	public float controlPointHeight
	{
		get
		{
			return m_ControlPointHeight;
		}
		set
		{
			m_ControlPointHeight = value;
		}
	}

	public int sampleFrequency
	{
		get
		{
			return m_SampleFrequency;
		}
		set
		{
			m_SampleFrequency = SanitizeSampleFrequency(value);
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

	public float sphereCastRadius
	{
		get
		{
			return m_SphereCastRadius;
		}
		set
		{
			m_SphereCastRadius = value;
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

	public bool hitClosestOnly
	{
		get
		{
			return m_HitClosestOnly;
		}
		set
		{
			m_HitClosestOnly = value;
		}
	}

	public bool hoverToSelect
	{
		get
		{
			return m_HoverToSelect;
		}
		set
		{
			m_HoverToSelect = value;
		}
	}

	public float hoverTimeToSelect
	{
		get
		{
			return m_HoverTimeToSelect;
		}
		set
		{
			m_HoverTimeToSelect = value;
		}
	}

	public bool autoDeselect
	{
		get
		{
			return m_AutoDeselect;
		}
		set
		{
			m_AutoDeselect = value;
		}
	}

	public float timeToAutoDeselect
	{
		get
		{
			return m_TimeToAutoDeselect;
		}
		set
		{
			m_TimeToAutoDeselect = value;
		}
	}

	public bool enableUIInteraction
	{
		get
		{
			return m_EnableUIInteraction;
		}
		set
		{
			if (m_EnableUIInteraction != value)
			{
				m_EnableUIInteraction = value;
				m_RegisteredUIInteractorCache?.RegisterOrUnregisterXRUIInputModule(m_EnableUIInteraction);
			}
		}
	}

	public bool blockInteractionsWithScreenSpaceUI
	{
		get
		{
			return m_BlockInteractionsWithScreenSpaceUI;
		}
		set
		{
			m_BlockInteractionsWithScreenSpaceUI = value;
		}
	}

	public bool blockUIOnInteractableSelection
	{
		get
		{
			return m_BlockUIOnInteractableSelection;
		}
		set
		{
			m_BlockUIOnInteractableSelection = value;
		}
	}

	public bool manipulateAttachTransform
	{
		get
		{
			return m_ManipulateAttachTransform;
		}
		set
		{
			m_ManipulateAttachTransform = value;
		}
	}

	public bool useForceGrab
	{
		get
		{
			return m_UseForceGrab;
		}
		set
		{
			m_UseForceGrab = value;
		}
	}

	public float rotateSpeed
	{
		get
		{
			return m_RotateSpeed;
		}
		set
		{
			m_RotateSpeed = value;
		}
	}

	public float translateSpeed
	{
		get
		{
			return m_TranslateSpeed;
		}
		set
		{
			m_TranslateSpeed = value;
		}
	}

	public Transform rotateReferenceFrame
	{
		get
		{
			return m_RotateReferenceFrame;
		}
		set
		{
			m_RotateReferenceFrame = value;
		}
	}

	public RotateMode rotateMode
	{
		get
		{
			return m_RotateMode;
		}
		set
		{
			m_RotateMode = value;
		}
	}

	public UIHoverEnterEvent uiHoverEntered
	{
		get
		{
			return m_UIHoverEntered;
		}
		set
		{
			m_UIHoverEntered = value;
		}
	}

	public UIHoverExitEvent uiHoverExited
	{
		get
		{
			return m_UIHoverExited;
		}
		set
		{
			m_UIHoverExited = value;
		}
	}

	public bool enableARRaycasting
	{
		get
		{
			return m_EnableARRaycasting;
		}
		set
		{
			m_EnableARRaycasting = value;
		}
	}

	public bool occludeARHitsWith3DObjects
	{
		get
		{
			return m_OccludeARHitsWith3DObjects;
		}
		set
		{
			m_OccludeARHitsWith3DObjects = value;
		}
	}

	public bool occludeARHitsWith2DObjects
	{
		get
		{
			return m_OccludeARHitsWith2DObjects;
		}
		set
		{
			m_OccludeARHitsWith2DObjects = value;
		}
	}

	public ScaleMode scaleMode
	{
		get
		{
			return m_ScaleMode;
		}
		set
		{
			m_ScaleMode = value;
		}
	}

	public XRInputButtonReader uiPressInput
	{
		get
		{
			return m_UIPressInput;
		}
		set
		{
			SetInputProperty(ref m_UIPressInput, value);
		}
	}

	public XRInputValueReader<Vector2> uiScrollInput
	{
		get
		{
			return m_UIScrollInput;
		}
		set
		{
			SetInputProperty(ref m_UIScrollInput, value);
		}
	}

	public XRInputValueReader<Vector2> translateManipulationInput
	{
		get
		{
			return m_TranslateManipulationInput;
		}
		set
		{
			SetInputProperty(ref m_TranslateManipulationInput, value);
		}
	}

	public XRInputValueReader<Vector2> rotateManipulationInput
	{
		get
		{
			return m_RotateManipulationInput;
		}
		set
		{
			SetInputProperty(ref m_RotateManipulationInput, value);
		}
	}

	public XRInputValueReader<Vector2> directionalManipulationInput
	{
		get
		{
			return m_DirectionalManipulationInput;
		}
		set
		{
			SetInputProperty(ref m_DirectionalManipulationInput, value);
		}
	}

	public XRInputButtonReader scaleToggleInput
	{
		get
		{
			return m_ScaleToggleInput;
		}
		set
		{
			SetInputProperty(ref m_ScaleToggleInput, value);
		}
	}

	public XRInputValueReader<Vector2> scaleOverTimeInput
	{
		get
		{
			return m_ScaleOverTimeInput;
		}
		set
		{
			SetInputProperty(ref m_ScaleOverTimeInput, value);
		}
	}

	public XRInputValueReader<float> scaleDistanceDeltaInput
	{
		get
		{
			return m_ScaleDistanceDeltaInput;
		}
		set
		{
			SetInputProperty(ref m_ScaleDistanceDeltaInput, value);
		}
	}

	public float angle
	{
		get
		{
			GetLineOriginAndDirection(out var _, out var direction);
			return GetProjectileAngle(direction);
		}
	}

	protected IXRInteractable currentNearestValidTarget { get; private set; }

	public Vector3 rayEndPoint { get; private set; }

	public Transform rayEndTransform { get; private set; }

	public float scaleValue { get; protected set; }

	private Transform effectiveRayOrigin
	{
		get
		{
			if (!m_HasRayOriginTransform)
			{
				return base.transform;
			}
			return m_RayOriginTransform;
		}
	}

	private Vector3 referenceUp
	{
		get
		{
			if (!m_HasReferenceFrame)
			{
				return Vector3.up;
			}
			return m_ReferenceFrame.up;
		}
	}

	private Vector3 referencePosition
	{
		get
		{
			if (!m_HasReferenceFrame)
			{
				return Vector3.zero;
			}
			return m_ReferenceFrame.position;
		}
	}

	private int closestAnyHitIndex
	{
		get
		{
			if (m_RaycastHitEndpointIndex <= 0 || m_UIRaycastHitEndpointIndex <= 0)
			{
				if (m_RaycastHitEndpointIndex <= 0)
				{
					return m_UIRaycastHitEndpointIndex;
				}
				return m_RaycastHitEndpointIndex;
			}
			return Mathf.Min(m_RaycastHitEndpointIndex, m_UIRaycastHitEndpointIndex);
		}
	}

	public override bool isSelectActive
	{
		get
		{
			if (m_BlockInteractionsWithScreenSpaceUI && !base.hasSelection && IsOverScreenSpaceCanvas())
			{
				return false;
			}
			if (m_HoverToSelect && m_PassedHoverTimeToSelect)
			{
				return base.allowSelect;
			}
			return base.isSelectActive;
		}
	}

	[Obsolete("Velocity has been deprecated. Use velocity instead. (UnityUpgradable) -> velocity", true)]
	public float Velocity
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	[Obsolete("Acceleration has been deprecated. Use acceleration instead. (UnityUpgradable) -> acceleration", true)]
	public float Acceleration
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	[Obsolete("AdditionalFlightTime has been deprecated. Use additionalFlightTime instead. (UnityUpgradable) -> additionalFlightTime", true)]
	public float AdditionalFlightTime
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	[Obsolete("Angle has been deprecated. Use angle instead. (UnityUpgradable) -> angle", true)]
	public float Angle => 0f;

	[Obsolete("originalAttachTransform has been deprecated. Use rayOriginTransform instead. (UnityUpgradable) -> rayOriginTransform", true)]
	protected Transform originalAttachTransform
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("allowAnchorControl has been renamed in version 3.0.0. Use manipulateAttachTransform instead. (UnityUpgradable) -> manipulateAttachTransform")]
	public bool allowAnchorControl
	{
		get
		{
			return manipulateAttachTransform;
		}
		set
		{
			manipulateAttachTransform = value;
		}
	}

	[Obsolete("anchorRotateReferenceFrame has been renamed in version 3.0.0. Use rotateReferenceFrame instead. (UnityUpgradable) -> rotateReferenceFrame")]
	public Transform anchorRotateReferenceFrame
	{
		get
		{
			return rotateReferenceFrame;
		}
		set
		{
			rotateReferenceFrame = value;
		}
	}

	[Obsolete("anchorRotationMode has been deprecated in version 3.0.0. Use rotateMode instead.")]
	public AnchorRotationMode anchorRotationMode
	{
		get
		{
			return (AnchorRotationMode)rotateMode;
		}
		set
		{
			rotateMode = (RotateMode)value;
		}
	}

	[Obsolete("isUISelectActive has been deprecated in version 3.0.0. Use uiPressInput to read button input instead.")]
	protected override bool isUISelectActive
	{
		get
		{
			if (m_HoverToSelect && m_HoverUISelectActive)
			{
				return base.allowSelect;
			}
			return base.isUISelectActive;
		}
	}

	protected void OnValidate()
	{
		m_HasRayOriginTransform = m_RayOriginTransform != null;
		m_HasReferenceFrame = m_ReferenceFrame != null;
		m_SampleFrequency = SanitizeSampleFrequency(m_SampleFrequency);
		m_RegisteredUIInteractorCache?.RegisterOrUnregisterXRUIInputModule(m_EnableUIInteraction);
	}

	protected override void Awake()
	{
		base.Awake();
		base.buttonReaders.Add(m_UIPressInput);
		base.valueReaders.Add(m_UIScrollInput);
		base.valueReaders.Add(m_TranslateManipulationInput);
		base.valueReaders.Add(m_RotateManipulationInput);
		base.valueReaders.Add(m_DirectionalManipulationInput);
		base.buttonReaders.Add(m_ScaleToggleInput);
		base.valueReaders.Add(m_ScaleOverTimeInput);
		base.valueReaders.Add(m_ScaleDistanceDeltaInput);
		m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
		m_RegisteredUIInteractorCache = new RegisteredUIInteractorCache(this);
		CreateSamplePointsListsIfNecessary();
		FindReferenceFrame();
		CreateRayOrigin();
		if (!Application.isEditor)
		{
			m_LiveConeCastDebugVisuals = false;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_EnableUIInteraction)
		{
			m_RegisteredUIInteractorCache?.RegisterWithXRUIInputModule();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_SamplePoints?.Clear();
		m_RegisteredUIInteractorCache?.UnregisterFromXRUIInputModule();
	}

	protected virtual void OnDrawGizmosSelected()
	{
		if (m_LineType == LineType.StraightLine)
		{
			Transform transform = ((m_RayOriginTransform != null) ? m_RayOriginTransform : base.transform);
			Vector3 position = transform.position;
			Vector3 vector = position + transform.forward * m_MaxRaycastDistance;
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
				float num = Mathf.Tan(m_ConeCastAngle * (MathF.PI / 180f) * 0.5f) * m_MaxRaycastDistance;
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
		}
		if (!Application.isPlaying || m_SamplePoints == null || m_SamplePoints.Count < 2)
		{
			return;
		}
		if (TryGetCurrent3DRaycastHit(out var raycastHit))
		{
			Gizmos.color = new Color(0.22745098f, 0.47843137f, 0.972549f, 79f / 85f);
			Gizmos.DrawLine(raycastHit.point, raycastHit.point + raycastHit.normal.normalized * 0.075f);
		}
		if (TryGetCurrentUIRaycastResult(out var raycastResult))
		{
			Gizmos.color = new Color(0.22745098f, 0.47843137f, 0.972549f, 79f / 85f);
			Gizmos.DrawLine(raycastResult.worldPosition, raycastResult.worldPosition + raycastResult.worldNormal.normalized * 0.075f);
		}
		int num2 = closestAnyHitIndex;
		for (int i = 0; i < m_SamplePoints.Count; i++)
		{
			SamplePoint samplePoint = m_SamplePoints[i];
			float radius = ((m_HitDetectionType == HitDetectionType.SphereCast) ? m_SphereCastRadius : 0.025f);
			Gizmos.color = ((num2 == 0 || i < num2) ? new Color(0.6392157f, 0.28627452f, 0.6431373f, 0.75f) : new Color(41f / 51f, 0.56078434f, 41f / 51f, 0.5f));
			Gizmos.DrawSphere(samplePoint.position, radius);
			if (i < m_SamplePoints.Count - 1)
			{
				SamplePoint samplePoint2 = m_SamplePoints[i + 1];
				Gizmos.DrawLine(samplePoint.position, samplePoint2.position);
			}
		}
		switch (m_LineType)
		{
		case LineType.ProjectileCurve:
			DrawQuadraticBezierGizmo(m_HitChordControlPoints[0], m_HitChordControlPoints[1], m_HitChordControlPoints[2]);
			break;
		case LineType.BezierCurve:
			DrawQuadraticBezierGizmo(m_ControlPoints[0], m_ControlPoints[1], m_ControlPoints[2]);
			break;
		}
		if (!m_LiveConeCastDebugVisuals)
		{
			return;
		}
		for (int j = 0; j < m_ConeCastDebugInfo.Count; j += 2)
		{
			Gizmos.color = Color.yellow;
			for (float num3 = 0f; num3 <= 4f; num3 += 1f)
			{
				float num4 = num3 / 4f;
				Gizmos.DrawWireSphere(m_ConeCastDebugInfo[j].Item1 + num4 * (m_ConeCastDebugInfo[j + 1].Item1 - m_ConeCastDebugInfo[j].Item1), m_ConeCastDebugInfo[j].Item2);
			}
		}
	}

	private static void DrawQuadraticBezierGizmo(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Gizmos.color = new Color(1f, 0f, 0f, 0.75f);
		Gizmos.DrawSphere(p0, 0.025f);
		Gizmos.DrawSphere(p1, 0.025f);
		Gizmos.DrawSphere(p2, 0.025f);
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.75f);
		Gizmos.DrawLine(p0, p1);
		Gizmos.DrawLine(p1, p2);
		Gizmos.color = new Color(0f, 0f, 8.2f, 0.75f);
		for (float num = 0.1f; num <= 0.9f; num += 0.1f)
		{
			Vector3 from = Vector3.Lerp(p0, p1, num);
			Vector3 to = Vector3.Lerp(p1, p2, num);
			Gizmos.DrawLine(from, to);
		}
	}

	private void FindReferenceFrame()
	{
		m_HasReferenceFrame = m_ReferenceFrame != null;
		if (m_HasReferenceFrame)
		{
			return;
		}
		if (ComponentLocatorUtility<XROrigin>.TryFindComponent(out var component))
		{
			GameObject origin = component.Origin;
			if (origin != null)
			{
				m_ReferenceFrame = origin.transform;
				m_HasReferenceFrame = true;
			}
			else
			{
				Debug.Log("Reference frame of the curve not set and XROrigin.Origin is not set, using global up as default.", this);
			}
		}
		else
		{
			Debug.Log("Reference frame of the curve not set and XROrigin is not found, using global up as default.", this);
		}
	}

	private void CreateRayOrigin()
	{
		m_HasRayOriginTransform = m_RayOriginTransform != null;
		if (!m_HasRayOriginTransform)
		{
			m_RayOriginTransform = new GameObject("[" + base.gameObject.name + "] Ray Origin").transform;
			m_HasRayOriginTransform = true;
			m_RayOriginTransform.SetParent(base.transform, worldPositionStays: false);
			if (base.attachTransform == null)
			{
				CreateAttachTransform();
			}
			if (base.attachTransform == null)
			{
				m_RayOriginTransform.SetLocalPose(Pose.identity);
			}
			else if (base.attachTransform.parent == base.transform)
			{
				m_RayOriginTransform.SetLocalPose(base.attachTransform.GetLocalPose());
			}
			else
			{
				m_RayOriginTransform.SetWorldPose(base.attachTransform.GetWorldPose());
			}
		}
	}

	Transform IXRRayProvider.GetOrCreateRayOrigin()
	{
		CreateRayOrigin();
		return m_RayOriginTransform;
	}

	Transform IXRRayProvider.GetOrCreateAttachTransform()
	{
		CreateAttachTransform();
		return base.attachTransform;
	}

	void IXRRayProvider.SetRayOrigin(Transform newOrigin)
	{
		rayOriginTransform = newOrigin;
	}

	void IXRRayProvider.SetAttachTransform(Transform newAttach)
	{
		base.attachTransform = newAttach;
	}

	public bool IsOverUIGameObject()
	{
		if (m_EnableUIInteraction && m_RegisteredUIInteractorCache != null)
		{
			return m_RegisteredUIInteractorCache.IsOverUIGameObject();
		}
		return false;
	}

	private bool IsOverScreenSpaceCanvas()
	{
		if (m_EnableUIInteraction && m_RegisteredUIInteractorCache != null && m_RegisteredUIInteractorCache.TryGetCurrentUIGameObject(useAnyPointerId: true, out var currentGameObject))
		{
			Canvas componentInParent = currentGameObject.GetComponentInParent<Canvas>();
			if (componentInParent != null)
			{
				RenderMode renderMode = componentInParent.renderMode;
				if (renderMode != 0)
				{
					return renderMode == RenderMode.ScreenSpaceCamera;
				}
				return true;
			}
			PanelRaycaster component;
			return currentGameObject.TryGetComponent<PanelRaycaster>(out component);
		}
		return false;
	}

	public bool GetLinePoints(ref NativeArray<Vector3> linePoints, out int numPoints, Ray? rayOriginOverride = null)
	{
		if (m_SamplePoints == null || m_SamplePoints.Count < 2)
		{
			numPoints = 0;
			return false;
		}
		if (!m_BlendVisualLinePoints)
		{
			numPoints = m_SamplePoints.Count;
			EnsureCapacity(ref linePoints, numPoints);
			NativeArray<float3> nativeArray = linePoints.Reinterpret<float3>();
			for (int i = 0; i < numPoints; i++)
			{
				nativeArray[i] = m_SamplePoints[i].position;
			}
			return true;
		}
		CreateSamplePointsListsIfNecessary();
		UpdateSamplePoints(m_SamplePoints.Count, s_ScratchSamplePoints, rayOriginOverride);
		if (m_LineType == LineType.StraightLine)
		{
			numPoints = 2;
			EnsureCapacity(ref linePoints, numPoints);
			NativeArray<float3> nativeArray = linePoints.Reinterpret<float3>();
			nativeArray[0] = s_ScratchSamplePoints[0].position;
			nativeArray[1] = m_SamplePoints[m_SamplePoints.Count - 1].position;
			return true;
		}
		int num = closestAnyHitIndex;
		CreateBezierCurve(s_ScratchSamplePoints, num, s_ScratchControlPoints, rayOriginOverride);
		CurveUtility.ElevateQuadraticToCubicBezier(in s_ScratchControlPoints[0], in s_ScratchControlPoints[1], in s_ScratchControlPoints[2], out var c, out var c2, out var c3, out var c4);
		CurveUtility.ElevateQuadraticToCubicBezier(in m_HitChordControlPoints[0], in m_HitChordControlPoints[1], in m_HitChordControlPoints[2], out c4, out c3, out var c5, out var c6);
		if (num > 0 && num != m_SamplePoints.Count - 1 && m_LineType == LineType.ProjectileCurve)
		{
			numPoints = m_SamplePoints.Count;
			EnsureCapacity(ref linePoints, numPoints);
			NativeArray<float3> nativeArray = linePoints.Reinterpret<float3>();
			nativeArray[0] = c;
			float num2 = 1f / (float)num;
			for (int j = 1; j <= num; j++)
			{
				float t = (float)j * num2;
				CurveUtility.SampleCubicBezierPoint(in c, in c2, in c5, in c6, t, out var point);
				nativeArray[j] = point;
			}
			for (int k = num + 1; k < m_SamplePoints.Count; k++)
			{
				nativeArray[k] = m_SamplePoints[k].position;
			}
		}
		else
		{
			numPoints = m_SampleFrequency;
			EnsureCapacity(ref linePoints, numPoints);
			NativeArray<float3> nativeArray = linePoints.Reinterpret<float3>();
			nativeArray[0] = c;
			float num3 = 1f / (float)(m_SampleFrequency - 1);
			for (int l = 1; l < m_SampleFrequency; l++)
			{
				float t2 = (float)l * num3;
				CurveUtility.SampleCubicBezierPoint(in c, in c2, in c5, in c6, t2, out var point2);
				nativeArray[l] = point2;
			}
		}
		return true;
	}

	public bool GetLinePoints(ref Vector3[] linePoints, out int numPoints)
	{
		if (linePoints == null)
		{
			linePoints = Array.Empty<Vector3>();
		}
		NativeArray<Vector3> linePoints2 = new NativeArray<Vector3>(linePoints, Allocator.Temp);
		bool linePoints3 = GetLinePoints(ref linePoints2, out numPoints);
		int length = linePoints2.Length;
		if (linePoints.Length != length)
		{
			linePoints = new Vector3[length];
		}
		linePoints2.CopyTo(linePoints);
		linePoints2.Dispose();
		return linePoints3;
	}

	public void GetLineOriginAndDirection(out Vector3 origin, out Vector3 direction)
	{
		GetLineOriginAndDirection(effectiveRayOrigin, out origin, out direction);
	}

	private void GetLineOriginAndDirection(Ray? rayOriginOverride, out Vector3 origin, out Vector3 direction)
	{
		if (rayOriginOverride.HasValue)
		{
			Ray value = rayOriginOverride.Value;
			origin = value.origin;
			direction = value.direction;
		}
		else
		{
			GetLineOriginAndDirection(out origin, out direction);
		}
	}

	private static void GetLineOriginAndDirection(Transform rayOrigin, out Vector3 origin, out Vector3 direction)
	{
		origin = rayOrigin.position;
		direction = rayOrigin.forward;
	}

	private static void EnsureCapacity(ref NativeArray<Vector3> linePoints, int numPoints)
	{
		if (linePoints.IsCreated && linePoints.Length < numPoints)
		{
			linePoints.Dispose();
			linePoints = new NativeArray<Vector3>(numPoints, Allocator.Persistent);
		}
		else if (!linePoints.IsCreated)
		{
			linePoints = new NativeArray<Vector3>(numPoints, Allocator.Persistent);
		}
	}

	public bool TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget)
	{
		position = default(Vector3);
		normal = default(Vector3);
		positionInLine = 0;
		isValidTarget = false;
		if (!TryGetCurrentRaycast(out var raycastHit, out var raycastHitIndex, out var uiRaycastHit, out var uiRaycastHitIndex, out var isUIHitClosest))
		{
			return false;
		}
		RaycastHit value2;
		if (base.hasSelection && m_InteractableRaycastHits.TryGetValue(base.interactablesSelected[0], out var value))
		{
			raycastHit = value;
		}
		else if (m_ValidTargets.Count > 0 && m_InteractableRaycastHits.TryGetValue(m_ValidTargets[0], out value2))
		{
			raycastHit = value2;
		}
		if (uiRaycastHit.HasValue && isUIHitClosest)
		{
			position = uiRaycastHit.Value.worldPosition;
			normal = uiRaycastHit.Value.worldNormal;
			positionInLine = uiRaycastHitIndex;
			isValidTarget = uiRaycastHit.Value.gameObject != null;
		}
		else if (raycastHit.HasValue)
		{
			position = raycastHit.Value.point;
			normal = raycastHit.Value.normal;
			positionInLine = raycastHitIndex;
			isValidTarget = base.interactionManager.TryGetInteractableForCollider(raycastHit.Value.collider, out var interactable) && IsHovering(interactable);
		}
		return true;
	}

	public virtual void UpdateUIModel(ref TrackedDeviceModel model)
	{
		if (!base.isActiveAndEnabled || m_SamplePoints == null || (m_EnableUIInteraction && m_BlockUIOnInteractableSelection && base.hasSelection) || this.IsBlockedByInteractionWithinGroup())
		{
			model.Reset(resetImplementation: false);
			return;
		}
		Transform obj = effectiveRayOrigin;
		bool select = (base.forceDeprecatedInput ? isUISelectActive : ((!m_HoverToSelect || !m_HoverUISelectActive) ? m_UIPressInput.ReadIsPerformed() : base.allowSelect));
		Vector2 scrollDelta = ((!base.forceDeprecatedInput) ? m_UIScrollInput.ReadValue() : base.uiScrollValue);
		Pose worldPose = obj.GetWorldPose();
		model.position = worldPose.position;
		model.orientation = worldPose.rotation;
		model.select = select;
		model.scrollDelta = scrollDelta;
		model.raycastLayerMask = m_RaycastMask;
		model.interactionType = UIInteractionType.Ray;
		List<Vector3> raycastPoints = model.raycastPoints;
		raycastPoints.Clear();
		UpdateSamplePointsIfNecessary();
		int count = m_SamplePoints.Count;
		if (count > 0)
		{
			if (raycastPoints.Capacity < count)
			{
				raycastPoints.Capacity = count;
			}
			for (int i = 0; i < count; i++)
			{
				raycastPoints.Add(m_SamplePoints[i].position);
			}
		}
	}

	public bool TryGetUIModel(out TrackedDeviceModel model)
	{
		if (m_RegisteredUIInteractorCache == null)
		{
			model = TrackedDeviceModel.invalid;
			return false;
		}
		return m_RegisteredUIInteractorCache.TryGetUIModel(out model);
	}

	public bool TryGetCurrent3DRaycastHit(out RaycastHit raycastHit)
	{
		int raycastEndpointIndex;
		return TryGetCurrent3DRaycastHit(out raycastHit, out raycastEndpointIndex);
	}

	public bool TryGetCurrent3DRaycastHit(out RaycastHit raycastHit, out int raycastEndpointIndex)
	{
		if (m_RaycastHitsCount > 0)
		{
			raycastHit = m_RaycastHits[0];
			raycastEndpointIndex = m_RaycastHitEndpointIndex;
			return true;
		}
		raycastHit = default(RaycastHit);
		raycastEndpointIndex = 0;
		return false;
	}

	public bool TryGetCurrentUIRaycastResult(out RaycastResult raycastResult)
	{
		int raycastEndpointIndex;
		return TryGetCurrentUIRaycastResult(out raycastResult, out raycastEndpointIndex);
	}

	public bool TryGetCurrentUIRaycastResult(out RaycastResult raycastResult, out int raycastEndpointIndex)
	{
		if (TryGetUIModel(out var model) && model.currentRaycast.isValid)
		{
			raycastResult = model.currentRaycast;
			raycastEndpointIndex = model.currentRaycastEndpointIndex;
			return true;
		}
		raycastResult = default(RaycastResult);
		raycastEndpointIndex = 0;
		return false;
	}

	public bool TryGetCurrentRaycast(out RaycastHit? raycastHit, out int raycastHitIndex, out RaycastResult? uiRaycastHit, out int uiRaycastHitIndex, out bool isUIHitClosest)
	{
		raycastHit = m_RaycastHit;
		raycastHitIndex = m_RaycastHitEndpointIndex;
		uiRaycastHit = m_UIRaycastHit;
		uiRaycastHitIndex = m_UIRaycastHitEndpointIndex;
		isUIHitClosest = m_IsUIHitClosest;
		return m_RaycastHitOccurred;
	}

	private void CacheRaycastHit()
	{
		m_RaycastHit = default(RaycastHit);
		m_UIRaycastHit = default(RaycastResult);
		m_IsUIHitClosest = false;
		m_RaycastHitOccurred = false;
		rayEndTransform = null;
		m_RaycastInteractable = null;
		int num = int.MaxValue;
		float num2 = float.MaxValue;
		if (TryGetCurrent3DRaycastHit(out var raycastHit, out var raycastEndpointIndex))
		{
			m_RaycastHit = raycastHit;
			num = raycastEndpointIndex;
			num2 = raycastHit.distance;
			m_RaycastHitOccurred = true;
		}
		if (TryGetCurrentUIRaycastResult(out var raycastResult, out m_UIRaycastHitEndpointIndex))
		{
			m_UIRaycastHit = raycastResult;
			m_IsUIHitClosest = m_UIRaycastHitEndpointIndex > 0 && (m_UIRaycastHitEndpointIndex < num || (m_UIRaycastHitEndpointIndex == num && raycastResult.distance <= num2));
			m_RaycastHitOccurred = true;
		}
		if (m_RaycastHitOccurred)
		{
			if (m_IsUIHitClosest)
			{
				rayEndPoint = m_UIRaycastHit.worldPosition;
				rayEndTransform = m_UIRaycastHit.gameObject.transform;
			}
			else
			{
				rayEndPoint = m_RaycastHit.point;
				rayEndTransform = (base.interactionManager.TryGetInteractableForCollider(m_RaycastHit.collider, out m_RaycastInteractable) ? m_RaycastInteractable.GetAttachTransform(this) : m_RaycastHit.transform);
			}
		}
		else
		{
			UpdateSamplePointsIfNecessary();
			rayEndPoint = m_SamplePoints[m_SamplePoints.Count - 1].position;
		}
	}

	private void UpdateUIHover()
	{
		float num = Time.time - m_LastTimeHoveredUIChanged;
		if (m_IsUIHitClosest && num > m_HoverTimeToSelect && (num < m_HoverTimeToSelect + m_TimeToAutoDeselect || m_BlockUIAutoDeselect))
		{
			m_HoverUISelectActive = true;
		}
		else
		{
			m_HoverUISelectActive = false;
		}
	}

	private void UpdateBezierControlPoints(in float3 lineOrigin, in float3 lineDirection, in float3 curveReferenceUp)
	{
		m_ControlPoints[0] = lineOrigin;
		m_ControlPoints[1] = m_ControlPoints[0] + lineDirection * m_ControlPointDistance + curveReferenceUp * m_ControlPointHeight;
		m_ControlPoints[2] = m_ControlPoints[0] + lineDirection * m_EndPointDistance + curveReferenceUp * m_EndPointHeight;
	}

	private float GetProjectileAngle(Vector3 lineDirection)
	{
		Vector3 vector = referenceUp;
		Vector3 to = Vector3.ProjectOnPlane(lineDirection, vector);
		if (!Mathf.Approximately(Vector3.Angle(lineDirection, to), 0f))
		{
			return Vector3.SignedAngle(lineDirection, to, Vector3.Cross(vector, lineDirection));
		}
		return 0f;
	}

	[BurstCompile]
	private void CalculateProjectileParameters(in float3 lineOrigin, in float3 lineDirection, out float3 initialVelocity, out float3 constantAcceleration, out float flightTime)
	{
		initialVelocity = lineDirection * m_Velocity;
		float3 @float = referenceUp;
		float3 float2 = referencePosition;
		constantAcceleration = @float * (0f - m_Acceleration);
		float angleRad = math.sin(GetProjectileAngle(lineDirection) * (MathF.PI / 180f));
		float height = math.length(math.project(float2 - lineOrigin, @float)) + m_AdditionalGroundHeight;
		CurveUtility.CalculateProjectileFlightTime(m_Velocity, m_Acceleration, angleRad, height, m_AdditionalFlightTime, out flightTime);
	}

	protected virtual void RotateAttachTransform(Transform attach, float directionAmount)
	{
		if (!Mathf.Approximately(directionAmount, 0f))
		{
			float num = directionAmount * (m_RotateSpeed * Time.deltaTime);
			if (m_RotateReferenceFrame != null)
			{
				attach.Rotate(m_RotateReferenceFrame.up, num, Space.World);
			}
			else
			{
				attach.Rotate(Vector3.up, num);
			}
		}
	}

	protected virtual void RotateAttachTransform(Transform attach, Vector2 direction, Quaternion referenceRotation)
	{
		if (!Mathf.Approximately(direction.sqrMagnitude, 0f))
		{
			Quaternion quaternion = Quaternion.AngleAxis(Mathf.Atan2(direction.x, direction.y) * 57.29578f, Vector3.up);
			attach.rotation = referenceRotation * quaternion;
		}
	}

	protected virtual void TranslateAttachTransform(Transform rayOrigin, Transform attach, float directionAmount)
	{
		if (!Mathf.Approximately(directionAmount, 0f))
		{
			GetLineOriginAndDirection(rayOrigin, out var origin, out var direction);
			Vector3 vector = attach.position + direction * (directionAmount * m_TranslateSpeed * Time.deltaTime);
			float num = Vector3.Dot(vector - origin, direction);
			attach.position = ((num > 0f) ? vector : origin);
		}
	}

	public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.PreprocessInteractor(updatePhase);
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			UpdateSamplePointsIfNecessary();
			if (m_SamplePoints != null && m_SamplePoints.Count >= 2)
			{
				UpdateRaycastHits();
				CacheRaycastHit();
				UpdateUIHover();
				CreateBezierCurve(m_SamplePoints, closestAnyHitIndex, m_HitChordControlPoints);
			}
			GetValidTargets(m_ValidTargets);
			IXRInteractable iXRInteractable = ((m_ValidTargets.Count > 0) ? m_ValidTargets[0] : null);
			if (iXRInteractable != currentNearestValidTarget && !base.hasSelection)
			{
				currentNearestValidTarget = iXRInteractable;
				m_LastTimeHoveredObjectChanged = Time.time;
				m_PassedHoverTimeToSelect = false;
			}
			else if (!m_PassedHoverTimeToSelect && iXRInteractable != null && Mathf.Clamp01((Time.time - m_LastTimeHoveredObjectChanged) / GetHoverTimeToSelect(currentNearestValidTarget)) >= 1f && !base.hasSelection)
			{
				m_PassedHoverTimeToSelect = true;
			}
			if (m_AutoDeselect && base.hasSelection && !m_PassedTimeToAutoDeselect && Mathf.Clamp01((Time.time - m_LastTimeAutoSelected) / GetTimeToAutoDeselect(currentNearestValidTarget)) >= 1f)
			{
				m_PassedTimeToAutoDeselect = true;
			}
		}
	}

	public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractor(updatePhase);
		if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			return;
		}
		scaleValue = 0f;
		if (m_ManipulateAttachTransform && base.hasSelection)
		{
			if (base.forceDeprecatedInput)
			{
				ProcessManipulationInputDeviceBasedController();
				ProcessManipulationInputActionBasedController();
				ProcessManipulationInputScreenSpaceController();
			}
			else
			{
				ProcessManipulationInput();
			}
		}
	}

	private void ProcessManipulationInput()
	{
		if (m_ScaleToggleInput.ReadWasPerformedThisFrame())
		{
			m_ScaleInputActive = !m_ScaleInputActive;
		}
		Vector2 value4;
		if (!m_ScaleInputActive)
		{
			switch (m_RotateMode)
			{
			case RotateMode.RotateOverTime:
			{
				if (m_RotateManipulationInput.TryReadValue(out var value2))
				{
					RotateAnchor(base.attachTransform, value2.x);
				}
				break;
			}
			case RotateMode.MatchDirection:
			{
				if (m_DirectionalManipulationInput.TryReadValue(out var value))
				{
					Quaternion referenceRotation = ((m_RotateReferenceFrame != null) ? m_RotateReferenceFrame.rotation : effectiveRayOrigin.rotation);
					RotateAnchor(base.attachTransform, value, referenceRotation);
				}
				break;
			}
			}
			if (m_TranslateManipulationInput.TryReadValue(out var value3))
			{
				TranslateAnchor(effectiveRayOrigin, base.attachTransform, value3.y);
			}
		}
		else if (m_ScaleMode == ScaleMode.ScaleOverTime && m_ScaleOverTimeInput.TryReadValue(out value4))
		{
			scaleValue = value4.y;
		}
		if (m_ScaleMode == ScaleMode.DistanceDelta && m_ScaleDistanceDeltaInput.TryReadValue(out var value5))
		{
			scaleValue = value5;
		}
	}

	public override void GetValidTargets(List<IXRInteractable> targets)
	{
		targets.Clear();
		m_InteractableRaycastHits.Clear();
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (m_RaycastHitsCount > 0)
		{
			RaycastResult raycastResult;
			int raycastEndpointIndex;
			bool flag = TryGetCurrentUIRaycastResult(out raycastResult, out raycastEndpointIndex);
			for (int i = 0; i < m_RaycastHitsCount; i++)
			{
				RaycastHit value = m_RaycastHits[i];
				bool flag2 = raycastResult.gameObject != value.collider.gameObject;
				if ((flag && flag2 && raycastEndpointIndex > 0 && (raycastEndpointIndex < m_RaycastHitEndpointIndex || (raycastEndpointIndex == m_RaycastHitEndpointIndex && raycastResult.distance <= value.distance))) || !base.interactionManager.TryGetInteractableForCollider(value.collider, out var interactable))
				{
					break;
				}
				if (!targets.Contains(interactable))
				{
					targets.Add(interactable);
					m_InteractableRaycastHits.Add(interactable, value);
					if (m_HitClosestOnly)
					{
						break;
					}
				}
			}
		}
		IXRTargetFilter iXRTargetFilter = base.targetFilter;
		if (iXRTargetFilter != null && iXRTargetFilter.canProcess)
		{
			iXRTargetFilter.Process(this, targets, s_Results);
			targets.Clear();
			targets.AddRange(s_Results);
		}
	}

	private void CreateSamplePointsListsIfNecessary()
	{
		if (m_SamplePoints == null || s_ScratchSamplePoints == null)
		{
			int capacity = ((m_LineType == LineType.StraightLine) ? 2 : m_SampleFrequency);
			if (m_SamplePoints == null)
			{
				m_SamplePoints = new List<SamplePoint>(capacity);
			}
			if (s_ScratchSamplePoints == null)
			{
				s_ScratchSamplePoints = new List<SamplePoint>(capacity);
			}
		}
	}

	private void UpdateSamplePointsIfNecessary()
	{
		CreateSamplePointsListsIfNecessary();
		if (m_SamplePointsFrameUpdated != Time.frameCount)
		{
			UpdateSamplePoints(m_SampleFrequency, m_SamplePoints);
			m_SamplePointsFrameUpdated = Time.frameCount;
		}
	}

	private void UpdateSamplePoints(int count, List<SamplePoint> samplePoints, Ray? rayOriginOverride = null)
	{
		GetLineOriginAndDirection(rayOriginOverride, out var origin, out var direction);
		samplePoints.Clear();
		SamplePoint samplePoint = default(SamplePoint);
		samplePoint.position = origin;
		samplePoint.parameter = 0f;
		SamplePoint item = samplePoint;
		samplePoints.Add(item);
		switch (m_LineType)
		{
		case LineType.StraightLine:
			item.position = samplePoints[0].position + (float3)direction * m_MaxRaycastDistance;
			item.parameter = 1f;
			samplePoints.Add(item);
			break;
		case LineType.ProjectileCurve:
		{
			float3 lineOrigin2 = origin;
			float3 lineOrigin = direction;
			CalculateProjectileParameters(in lineOrigin2, in lineOrigin, out var initialVelocity, out var constantAcceleration, out var flightTime);
			float num3 = flightTime / (float)(count - 1);
			for (int j = 1; j < count; j++)
			{
				float num4 = (float)j * num3;
				CurveUtility.SampleProjectilePoint(in lineOrigin2, in initialVelocity, in constantAcceleration, num4, out var point2);
				item.position = point2;
				item.parameter = num4;
				samplePoints.Add(item);
			}
			break;
		}
		case LineType.BezierCurve:
		{
			float3 lineOrigin = origin;
			float3 lineDirection = direction;
			float3 curveReferenceUp = referenceUp;
			UpdateBezierControlPoints(in lineOrigin, in lineDirection, in curveReferenceUp);
			float3 p = m_ControlPoints[0];
			float3 p2 = m_ControlPoints[1];
			float3 p3 = m_ControlPoints[2];
			float num = 1f / (float)(count - 1);
			for (int i = 1; i < count; i++)
			{
				float num2 = (float)i * num;
				CurveUtility.SampleQuadraticBezierPoint(in p, in p2, in p3, num2, out var point);
				item.position = point;
				item.parameter = num2;
				samplePoints.Add(item);
			}
			break;
		}
		}
	}

	private void UpdateRaycastHits()
	{
		m_RaycastHitsCount = 0;
		m_RaycastHitEndpointIndex = 0;
		m_ConeCastDebugInfo.Clear();
		float num = 0f;
		bool flag = false;
		for (int i = 1; i < m_SamplePoints.Count; i++)
		{
			float3 position = m_SamplePoints[0].position;
			float3 position2 = m_SamplePoints[i - 1].position;
			float3 position3 = m_SamplePoints[i].position;
			CheckCollidersBetweenPoints(position2, position3, position);
			if (m_RaycastHitsCount > 0 && !flag)
			{
				for (int j = 0; j < m_RaycastHitsCount; j++)
				{
					m_RaycastHits[j].distance += num;
				}
				m_RaycastHitEndpointIndex = i;
				flag = true;
			}
			if (flag)
			{
				break;
			}
		}
	}

	private void CheckCollidersBetweenPoints(Vector3 from, Vector3 to, Vector3 origin)
	{
		Array.Clear(m_RaycastHits, 0, 10);
		m_RaycastHitsCount = 0;
		Vector3 direction = (to - from).normalized;
		float maxDistance = Vector3.Distance(to, from);
		QueryTriggerInteraction queryTriggerInteraction = ((m_RaycastSnapVolumeInteraction == QuerySnapVolumeInteraction.Collide) ? QueryTriggerInteraction.Collide : m_RaycastTriggerInteraction);
		switch (m_HitDetectionType)
		{
		case HitDetectionType.Raycast:
			m_RaycastHitsCount = m_LocalPhysicsScene.Raycast(from, direction, m_RaycastHits, maxDistance, m_RaycastMask, queryTriggerInteraction);
			break;
		case HitDetectionType.SphereCast:
			m_RaycastHitsCount = m_LocalPhysicsScene.SphereCast(from, m_SphereCastRadius, direction, m_RaycastHits, maxDistance, m_RaycastMask, queryTriggerInteraction);
			break;
		case HitDetectionType.ConeCast:
			m_RaycastHitsCount = FilteredConecast(in from, in direction, in origin, m_RaycastHits, maxDistance, m_RaycastMask, queryTriggerInteraction);
			break;
		}
		if (m_RaycastHitsCount > 0)
		{
			if (m_HitDetectionType != HitDetectionType.ConeCast)
			{
				m_RaycastHitsCount = FilterOutTriggerColliders(base.interactionManager, m_RaycastHits, m_RaycastHitsCount);
			}
			SortingHelpers.Sort(m_RaycastHits, m_RaycastHitComparer, m_RaycastHitsCount);
		}
	}

	private int FilteredConecast(in Vector3 from, in Vector3 direction, in Vector3 origin, RaycastHit[] results, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		s_OptimalHits.Clear();
		float num = math.min(maxDistance, 1000f);
		int num2 = 0;
		int num3 = m_LocalPhysicsScene.Raycast(from, direction, s_SpherecastScratch, maxDistance, layerMask, queryTriggerInteraction);
		IXRInteractable interactable;
		if (num3 > 0)
		{
			num3 = FilterOutTriggerColliders(base.interactionManager, s_SpherecastScratch, num3);
			for (int i = 0; i < num3; i++)
			{
				RaycastHit raycastHit = s_SpherecastScratch[i];
				if (!(raycastHit.distance > num))
				{
					if (!base.interactionManager.TryGetInteractableForCollider(raycastHit.collider, out interactable))
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
			num5 = FilterOutTriggerColliders(base.interactionManager, s_SpherecastScratch, num5);
			for (int j = 0; j < num5 && num2 < results.Length; j++)
			{
				RaycastHit raycastHit2 = s_SpherecastScratch[j];
				if (num4 + raycastHit2.distance > num || s_OptimalHits.Contains(raycastHit2.collider) || !base.interactionManager.TryGetInteractableForCollider(raycastHit2.collider, out interactable))
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

	private int FilterOutTriggerColliders(XRInteractionManager manager, RaycastHit[] raycastHits, int raycastHitCount)
	{
		bool flag = m_RaycastTriggerInteraction == QueryTriggerInteraction.Collide || (m_RaycastTriggerInteraction == QueryTriggerInteraction.UseGlobal && Physics.queriesHitTriggers);
		if (m_RaycastSnapVolumeInteraction == QuerySnapVolumeInteraction.Ignore && flag)
		{
			raycastHitCount = FilterTriggerColliders(manager, raycastHits, raycastHitCount, (XRInteractableSnapVolume snapVolume) => snapVolume != null);
		}
		else if (m_RaycastSnapVolumeInteraction == QuerySnapVolumeInteraction.Collide && !flag)
		{
			raycastHitCount = FilterTriggerColliders(manager, raycastHits, raycastHitCount, (XRInteractableSnapVolume snapVolume) => snapVolume == null);
		}
		return raycastHitCount;
	}

	private static int FilterTriggerColliders(XRInteractionManager interactionManager, RaycastHit[] raycastHits, int count, Func<XRInteractableSnapVolume, bool> removeRule)
	{
		for (int i = 0; i < count; i++)
		{
			Collider collider = raycastHits[i].collider;
			if (collider.isTrigger)
			{
				interactionManager.TryGetInteractableForCollider(collider, out var _, out var snapVolume);
				if (removeRule(snapVolume))
				{
					RemoveAt(raycastHits, i, count);
					count--;
					i--;
				}
			}
		}
		return count;
	}

	private static void RemoveAt<T>(T[] array, int index, int count) where T : struct
	{
		Array.Copy(array, index + 1, array, index, count - index - 1);
		Array.Clear(array, count - 1, 1);
	}

	private void CreateBezierCurve(List<SamplePoint> samplePoints, int endSamplePointIndex, float3[] quadraticControlPoints, Ray? rayOriginOverride = null)
	{
		SamplePoint samplePoint = ((endSamplePointIndex > 0 && endSamplePointIndex < samplePoints.Count) ? samplePoints[endSamplePointIndex] : samplePoints[samplePoints.Count - 1]);
		float3 position = samplePoint.position;
		float3 initialPosition = samplePoints[0].position;
		float3 @float = 0.5f * (initialPosition + position);
		switch (m_LineType)
		{
		case LineType.StraightLine:
			quadraticControlPoints[0] = initialPosition;
			quadraticControlPoints[1] = @float;
			quadraticControlPoints[2] = position;
			break;
		case LineType.ProjectileCurve:
		{
			GetLineOriginAndDirection(rayOriginOverride, out var origin, out var direction);
			float3 lineOrigin = origin;
			float3 lineDirection = direction;
			CalculateProjectileParameters(in lineOrigin, in lineDirection, out var initialVelocity, out var constantAcceleration, out var _);
			float time = 0.5f * samplePoint.parameter;
			CurveUtility.SampleProjectilePoint(in initialPosition, in initialVelocity, in constantAcceleration, time, out var point);
			float3 float2 = @float + 2f * (point - @float);
			quadraticControlPoints[0] = initialPosition;
			quadraticControlPoints[1] = float2;
			quadraticControlPoints[2] = position;
			break;
		}
		case LineType.BezierCurve:
			quadraticControlPoints[0] = m_ControlPoints[0];
			quadraticControlPoints[1] = m_ControlPoints[1];
			quadraticControlPoints[2] = m_ControlPoints[2];
			break;
		}
	}

	public override bool CanHover(IXRHoverInteractable interactable)
	{
		if (base.CanHover(interactable) && (!base.hasSelection || IsSelecting(interactable)))
		{
			if (base.forceDeprecatedInput)
			{
				if (m_IsScreenSpaceController)
				{
					return m_ScreenSpaceController.currentControllerState.isTracked;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	public override bool CanSelect(IXRSelectInteractable interactable)
	{
		if (currentNearestValidTarget == interactable && m_AutoDeselect && base.hasSelection && m_PassedHoverTimeToSelect && m_PassedTimeToAutoDeselect)
		{
			return false;
		}
		if (m_HoverToSelect && m_PassedHoverTimeToSelect && currentNearestValidTarget != interactable)
		{
			return false;
		}
		if (base.CanSelect(interactable))
		{
			if (base.hasSelection)
			{
				return IsSelecting(interactable);
			}
			return true;
		}
		return false;
	}

	protected virtual float GetHoverTimeToSelect(IXRInteractable interactable)
	{
		return m_HoverTimeToSelect;
	}

	protected virtual float GetTimeToAutoDeselect(IXRInteractable interactable)
	{
		return m_TimeToAutoDeselect;
	}

	protected override void OnSelectEntering(SelectEnterEventArgs args)
	{
		base.OnSelectEntering(args);
		if (m_AutoDeselect && m_PassedHoverTimeToSelect)
		{
			m_LastTimeAutoSelected = Time.time;
			m_PassedTimeToAutoDeselect = false;
		}
		if (base.interactablesSelected.Count == 1)
		{
			bool flag = !m_UseForceGrab;
			if (args.interactableObject is IFarAttachProvider { farAttachMode: not InteractableFarAttachMode.DeferToInteractor } farAttachProvider)
			{
				flag = farAttachProvider.farAttachMode == InteractableFarAttachMode.Far;
			}
			if (flag && TryGetCurrent3DRaycastHit(out var raycastHit))
			{
				base.attachTransform.position = raycastHit.point;
			}
		}
	}

	protected override void OnSelectExiting(SelectExitEventArgs args)
	{
		base.OnSelectExiting(args);
		m_PassedHoverTimeToSelect = false;
		m_LastTimeHoveredObjectChanged = Time.time;
		m_PassedTimeToAutoDeselect = false;
		if (!base.hasSelection)
		{
			RestoreAttachTransform();
		}
	}

	void IUIHoverInteractor.OnUIHoverEntered(UIHoverEventArgs args)
	{
		OnUIHoverEntered(args);
	}

	void IUIHoverInteractor.OnUIHoverExited(UIHoverEventArgs args)
	{
		OnUIHoverExited(args);
	}

	protected virtual void OnUIHoverEntered(UIHoverEventArgs args)
	{
		GameObject selectableObject = args.deviceModel.selectableObject;
		if (m_LastUIObject != selectableObject)
		{
			m_LastUIObject = selectableObject;
			if (selectableObject != null)
			{
				m_LastTimeHoveredUIChanged = Time.time;
				m_BlockUIAutoDeselect = m_LastUIObject.GetComponent<UnityEngine.UI.Slider>() != null;
			}
			else
			{
				m_BlockUIAutoDeselect = false;
			}
			m_HoverUISelectActive = false;
		}
		m_UIHoverEntered?.Invoke(args);
	}

	protected virtual void OnUIHoverExited(UIHoverEventArgs args)
	{
		GameObject selectableObject = args.deviceModel.selectableObject;
		if (m_LastUIObject != selectableObject)
		{
			m_LastUIObject = null;
			m_LastTimeHoveredUIChanged = Time.time;
			m_BlockUIAutoDeselect = false;
			m_HoverUISelectActive = false;
		}
		m_UIHoverExited?.Invoke(args);
	}

	private void RestoreAttachTransform()
	{
		Pose localAttachPoseOnSelect = GetLocalAttachPoseOnSelect(base.firstInteractableSelected);
		base.attachTransform.SetLocalPose(localAttachPoseOnSelect);
	}

	private static int SanitizeSampleFrequency(int value)
	{
		return Mathf.Max(value, 2);
	}

	[Obsolete("GetLinePoints with ref int parameter has been deprecated. Use signature with out int parameter instead.", true)]
	public bool GetLinePoints(ref Vector3[] linePoints, ref int numPoints, int _ = 0)
	{
		return false;
	}

	[Obsolete("TryGetHitInfo with ref parameters has been deprecated. Use signature with out parameters instead.", true)]
	public bool TryGetHitInfo(ref Vector3 position, ref Vector3 normal, ref int positionInLine, ref bool isValidTarget, int _ = 0)
	{
		return false;
	}

	[Obsolete("GetCurrentRaycastHit has been deprecated. Use TryGetCurrent3DRaycastHit instead. (UnityUpgradable) -> TryGetCurrent3DRaycastHit(*)", true)]
	public bool GetCurrentRaycastHit(out RaycastHit raycastHit)
	{
		raycastHit = default(RaycastHit);
		return false;
	}

	[Obsolete("ProcessManipulationInputDeviceBasedController has been deprecated in version 3.0.0.")]
	private void ProcessManipulationInputDeviceBasedController()
	{
		if (!m_IsDeviceBasedController || !m_DeviceBasedController.inputDevice.isValid)
		{
			return;
		}
		m_DeviceBasedController.inputDevice.IsPressed(m_DeviceBasedController.moveObjectIn, out var isPressed, m_DeviceBasedController.axisToPressThreshold);
		m_DeviceBasedController.inputDevice.IsPressed(m_DeviceBasedController.moveObjectOut, out var isPressed2, m_DeviceBasedController.axisToPressThreshold);
		if (isPressed || isPressed2)
		{
			float directionAmount = (isPressed ? 1f : (-1f));
			TranslateAnchor(effectiveRayOrigin, base.attachTransform, directionAmount);
		}
		switch (m_RotateMode)
		{
		case RotateMode.RotateOverTime:
		{
			m_DeviceBasedController.inputDevice.IsPressed(m_DeviceBasedController.rotateObjectLeft, out var isPressed3, m_DeviceBasedController.axisToPressThreshold);
			m_DeviceBasedController.inputDevice.IsPressed(m_DeviceBasedController.rotateObjectRight, out var isPressed4, m_DeviceBasedController.axisToPressThreshold);
			if (isPressed3 || isPressed4)
			{
				float directionAmount2 = (isPressed3 ? (-1f) : 1f);
				RotateAnchor(base.attachTransform, directionAmount2);
			}
			break;
		}
		case RotateMode.MatchDirection:
		{
			if (m_DeviceBasedController.inputDevice.TryReadAxis2DValue(m_DeviceBasedController.directionalAnchorRotation, out var value))
			{
				Quaternion referenceRotation = ((m_RotateReferenceFrame != null) ? m_RotateReferenceFrame.rotation : effectiveRayOrigin.rotation);
				RotateAnchor(base.attachTransform, value, referenceRotation);
			}
			break;
		}
		}
	}

	[Obsolete("ProcessManipulationInputActionBasedController has been deprecated in version 3.0.0.")]
	private void ProcessManipulationInputActionBasedController()
	{
		if (!m_IsActionBasedController)
		{
			return;
		}
		if (TryReadButton(m_ActionBasedController.scaleToggleAction.action))
		{
			m_ScaleInputActive = !m_ScaleInputActive;
		}
		Vector2 output4;
		if (!m_ScaleInputActive)
		{
			switch (m_RotateMode)
			{
			case RotateMode.RotateOverTime:
			{
				if (TryRead2DAxis(m_ActionBasedController.rotateAnchorAction.action, out var output2))
				{
					RotateAnchor(base.attachTransform, output2.x);
				}
				break;
			}
			case RotateMode.MatchDirection:
			{
				if (TryRead2DAxis(m_ActionBasedController.directionalAnchorRotationAction.action, out var output))
				{
					Quaternion referenceRotation = ((m_RotateReferenceFrame != null) ? m_RotateReferenceFrame.rotation : effectiveRayOrigin.rotation);
					RotateAnchor(base.attachTransform, output, referenceRotation);
				}
				break;
			}
			}
			if (TryRead2DAxis(m_ActionBasedController.translateAnchorAction.action, out var output3))
			{
				TranslateAnchor(effectiveRayOrigin, base.attachTransform, output3.y);
			}
		}
		else if (m_ScaleMode == ScaleMode.ScaleOverTime && TryRead2DAxis(m_ActionBasedController.scaleDeltaAction.action, out output4))
		{
			scaleValue = output4.y;
		}
	}

	[Obsolete("ProcessManipulationInputScreenSpaceController has been deprecated in version 3.0.0.")]
	private void ProcessManipulationInputScreenSpaceController()
	{
		if (!m_IsScreenSpaceController)
		{
			return;
		}
		RotateMode rotateMode = m_RotateMode;
		if (rotateMode != 0 && rotateMode == RotateMode.MatchDirection)
		{
			if (m_ScreenSpaceController.twistDeltaRotationAction.action != null && m_ScreenSpaceController.twistDeltaRotationAction.action.phase.IsInProgress())
			{
				float directionAmount = 0f - m_ScreenSpaceController.twistDeltaRotationAction.action.ReadValue<float>();
				RotateAnchor(base.attachTransform, directionAmount);
			}
			else if (m_ScreenSpaceController.dragDeltaAction.action != null && m_ScreenSpaceController.dragDeltaAction.action.phase.IsInProgress())
			{
				InputAction action = m_ScreenSpaceController.screenTouchCountAction.action;
				if (action != null && action.ReadValue<int>() > 1)
				{
					Vector2 vector = m_ScreenSpaceController.dragDeltaAction.action.ReadValue<Vector2>();
					float directionAmount2 = (Quaternion.Inverse(Quaternion.LookRotation(base.attachTransform.forward, Vector3.up)) * base.attachTransform.rotation * vector).x / Screen.dpi * -50f;
					RotateAnchor(base.attachTransform, directionAmount2);
				}
			}
		}
		if (m_ScaleMode == ScaleMode.DistanceDelta)
		{
			scaleValue = m_ScreenSpaceController.scaleDelta;
		}
	}

	[Obsolete("RotateAnchor has been renamed in version 3.0.0. Use RotateAttachTransform instead.")]
	protected virtual void RotateAnchor(Transform anchor, float directionAmount)
	{
		RotateAttachTransform(anchor, directionAmount);
	}

	[Obsolete("RotateAnchor has been renamed in version 3.0.0. Use RotateAttachTransform instead.")]
	protected virtual void RotateAnchor(Transform anchor, Vector2 direction, Quaternion referenceRotation)
	{
		RotateAttachTransform(anchor, direction, referenceRotation);
	}

	[Obsolete("TranslateAnchor has been renamed in version 3.0.0. Use TranslateAttachTransform instead.")]
	protected virtual void TranslateAnchor(Transform rayOrigin, Transform anchor, float directionAmount)
	{
		TranslateAttachTransform(rayOrigin, anchor, directionAmount);
	}

	[Obsolete("TryRead2DAxis has been deprecated in version 3.0.0.")]
	private static bool TryRead2DAxis(InputAction action, out Vector2 output)
	{
		if (action != null)
		{
			output = action.ReadValue<Vector2>();
			return true;
		}
		output = default(Vector2);
		return false;
	}

	[Obsolete("TryReadButton has been deprecated in version 3.0.0.")]
	private static bool TryReadButton(InputAction action)
	{
		return action?.WasPerformedThisFrame() ?? false;
	}

	private protected override void OnXRControllerChanged()
	{
		base.OnXRControllerChanged();
		XRBaseController xRBaseController = base.xrController;
		m_ActionBasedController = xRBaseController as ActionBasedController;
		m_IsActionBasedController = m_ActionBasedController != null;
		m_DeviceBasedController = xRBaseController as XRController;
		m_IsDeviceBasedController = m_DeviceBasedController != null;
		m_ScreenSpaceController = xRBaseController as XRScreenSpaceController;
		m_IsScreenSpaceController = m_ScreenSpaceController != null;
		if (base.forceDeprecatedInput && m_IsScreenSpaceController && m_ManipulateAttachTransform && m_RotateMode == RotateMode.RotateOverTime)
		{
			Debug.LogWarning("Rotate Over Time is not a valid value for Rotation Mode when using XR Screen Space Controller. This XR Ray Interactor will not be able to rotate the anchor using screen touches.", this);
		}
	}
}
