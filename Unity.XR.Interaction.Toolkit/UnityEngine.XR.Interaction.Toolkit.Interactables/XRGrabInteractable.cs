using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.XR.CoreUtils;
using UnityEngine.Pool;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Gaze;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[SelectionBase]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("XR/XR Grab Interactable", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable.html")]
[BurstCompile]
public class XRGrabInteractable : XRBaseInteractable, IFarAttachProvider
{
	[Obsolete("AttachPointCompatibilityMode has been deprecated and will be removed in a future version of XRI.", true)]
	public enum AttachPointCompatibilityMode
	{
		[Obsolete("Default has been deprecated and will be removed in a future version of XRI. It is the only mode now.", true)]
		Default,
		[Obsolete("Legacy has been deprecated and will be removed in a future version of XRI.", true)]
		Legacy
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void EaseAttachBurst_00000F9A_0024PostfixBurstDelegate(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime, float attachEaseInTime, ref float currentAttachEaseTime);

	internal static class EaseAttachBurst_00000F9A_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<EaseAttachBurst_00000F9A_0024PostfixBurstDelegate>(EaseAttachBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime, float attachEaseInTime, ref float currentAttachEaseTime)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Pose, ref Vector3, ref Pose, ref Vector3, float, float, ref float, void>)functionPointer)(ref targetPose, ref targetLocalScale, ref rawTargetPose, ref rawTargetLocalScale, deltaTime, attachEaseInTime, ref currentAttachEaseTime);
					return;
				}
			}
			EaseAttachBurst_0024BurstManaged(ref targetPose, ref targetLocalScale, in rawTargetPose, in rawTargetLocalScale, deltaTime, attachEaseInTime, ref currentAttachEaseTime);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void StepSmoothingBurst_00000F9B_0024PostfixBurstDelegate(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime, bool smoothPos, float smoothPosAmount, float tightenPos, bool smoothRot, float smoothRotAmount, float tightenRot, bool smoothScale, float smoothScaleAmount, float tightenScale);

	internal static class StepSmoothingBurst_00000F9B_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<StepSmoothingBurst_00000F9B_0024PostfixBurstDelegate>(StepSmoothingBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime, bool smoothPos, float smoothPosAmount, float tightenPos, bool smoothRot, float smoothRotAmount, float tightenRot, bool smoothScale, float smoothScaleAmount, float tightenScale)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Pose, ref Vector3, ref Pose, ref Vector3, float, bool, float, float, bool, float, float, bool, float, float, void>)functionPointer)(ref targetPose, ref targetLocalScale, ref rawTargetPose, ref rawTargetLocalScale, deltaTime, smoothPos, smoothPosAmount, tightenPos, smoothRot, smoothRotAmount, tightenRot, smoothScale, smoothScaleAmount, tightenScale);
					return;
				}
			}
			StepSmoothingBurst_0024BurstManaged(ref targetPose, ref targetLocalScale, in rawTargetPose, in rawTargetLocalScale, deltaTime, smoothPos, smoothPosAmount, tightenPos, smoothRot, smoothRotAmount, tightenRot, smoothScale, smoothScaleAmount, tightenScale);
		}
	}

	private const float k_DefaultTighteningAmount = 0.1f;

	private const float k_DefaultSmoothingAmount = 8f;

	private const float k_LinearVelocityDamping = 1f;

	private const float k_LinearVelocityScale = 1f;

	private const float k_AngularVelocityDamping = 1f;

	private const float k_AngularVelocityScale = 1f;

	private const int k_ThrowSmoothingFrameCount = 20;

	private const float k_DefaultAttachEaseInTime = 0.15f;

	private const float k_DefaultThrowSmoothingDuration = 0.25f;

	private const float k_DefaultThrowLinearVelocityScale = 1.5f;

	private const float k_DefaultThrowAngularVelocityScale = 1f;

	private const float k_DeltaTimeThreshold = 0.001f;

	private const float k_DefaultMaxLinearVelocityDelta = 10f;

	private const float k_DefaultMaxAngularVelocityDelta = 20f;

	[SerializeField]
	private Transform m_AttachTransform;

	[SerializeField]
	private Transform m_SecondaryAttachTransform;

	[SerializeField]
	private bool m_UseDynamicAttach;

	[SerializeField]
	private bool m_MatchAttachPosition = true;

	[SerializeField]
	private bool m_MatchAttachRotation = true;

	[SerializeField]
	private bool m_SnapToColliderVolume = true;

	[SerializeField]
	private bool m_ReinitializeDynamicAttachEverySingleGrab = true;

	[SerializeField]
	private float m_AttachEaseInTime = 0.15f;

	[SerializeField]
	private MovementType m_MovementType = MovementType.Instantaneous;

	[SerializeField]
	[FormerlySerializedAs("m_VisualsTransform")]
	private Transform m_PredictedVisualsTransform;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_VelocityDamping = 1f;

	[SerializeField]
	private float m_VelocityScale = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_AngularVelocityDamping = 1f;

	[SerializeField]
	private float m_AngularVelocityScale = 1f;

	[SerializeField]
	private bool m_TrackPosition = true;

	[SerializeField]
	private bool m_SmoothPosition;

	[SerializeField]
	[Range(0f, 20f)]
	private float m_SmoothPositionAmount = 8f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_TightenPosition = 0.1f;

	[SerializeField]
	private bool m_TrackRotation = true;

	[SerializeField]
	private bool m_SmoothRotation;

	[SerializeField]
	[Range(0f, 20f)]
	private float m_SmoothRotationAmount = 8f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_TightenRotation = 0.1f;

	[SerializeField]
	private bool m_TrackScale = true;

	[SerializeField]
	private bool m_SmoothScale;

	[SerializeField]
	[Range(0f, 20f)]
	private float m_SmoothScaleAmount = 8f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_TightenScale = 0.1f;

	[SerializeField]
	private bool m_ThrowOnDetach = true;

	[SerializeField]
	private float m_ThrowSmoothingDuration = 0.25f;

	[SerializeField]
	private AnimationCurve m_ThrowSmoothingCurve = AnimationCurve.Linear(1f, 1f, 1f, 0f);

	[SerializeField]
	private float m_ThrowVelocityScale = 1.5f;

	[SerializeField]
	private float m_ThrowAngularVelocityScale = 1f;

	[SerializeField]
	[FormerlySerializedAs("m_GravityOnDetach")]
	private bool m_ForceGravityOnDetach;

	[SerializeField]
	private bool m_RetainTransformParent = true;

	[SerializeField]
	private List<XRBaseGrabTransformer> m_StartingSingleGrabTransformers = new List<XRBaseGrabTransformer>();

	[SerializeField]
	private List<XRBaseGrabTransformer> m_StartingMultipleGrabTransformers = new List<XRBaseGrabTransformer>();

	[SerializeField]
	private bool m_AddDefaultGrabTransformers = true;

	[SerializeField]
	private InteractableFarAttachMode m_FarAttachMode;

	[SerializeField]
	private bool m_LimitLinearVelocity;

	[SerializeField]
	private bool m_LimitAngularVelocity;

	[SerializeField]
	private float m_MaxLinearVelocityDelta = 10f;

	[SerializeField]
	private float m_MaxAngularVelocityDelta = 20f;

	private readonly SmallRegistrationList<IXRGrabTransformer> m_SingleGrabTransformers = new SmallRegistrationList<IXRGrabTransformer>();

	private readonly SmallRegistrationList<IXRGrabTransformer> m_MultipleGrabTransformers = new SmallRegistrationList<IXRGrabTransformer>();

	private List<IXRGrabTransformer> m_GrabTransformersAddedWhenGrabbed;

	private bool m_GrabCountChanged;

	private (int, int) m_GrabCountBeforeAndAfterChange;

	private bool m_IsProcessingGrabTransformers;

	private int m_DropTransformersCount;

	private static readonly UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling.LinkedPool<DropEventArgs> s_DropEventArgs = new UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling.LinkedPool<DropEventArgs>(() => new DropEventArgs(), null, null, null, collectionCheck: false);

	private Pose m_TargetPose;

	private Vector3 m_TargetLocalScale;

	private bool m_IsTargetPoseDirty;

	private bool m_IsTargetLocalScaleDirty;

	private Transform m_Transform;

	private float m_CurrentAttachEaseTime;

	private MovementType m_CurrentMovementType;

	private bool m_DetachInLateUpdate;

	private Vector3 m_DetachLinearVelocity;

	private Vector3 m_DetachAngularVelocity;

	private int m_ThrowSmoothingCurrentFrame;

	private readonly float[] m_ThrowSmoothingFrameTimes = new float[20];

	private readonly Vector3[] m_ThrowSmoothingLinearVelocityFrames = new Vector3[20];

	private readonly Vector3[] m_ThrowSmoothingAngularVelocityFrames = new Vector3[20];

	private bool m_ThrowSmoothingFirstUpdate;

	private Pose m_LastThrowReferencePose;

	private IXRAimAssist m_ThrowAssist;

	private Rigidbody m_Rigidbody;

	private bool m_RigidbodyColliding;

	private bool m_WasKinematic;

	private bool m_UsedGravity;

	private RigidbodyInterpolation m_InterpolationOnGrab;

	private float m_LinearDampingOnGrab;

	private float m_AngularDampingOnGrab;

	private int m_LastFixedFrame;

	private float m_LastFixedDynamicTime;

	private Pose m_InitialVisualsTransformLocalPose;

	private bool m_InitialVisualsTransformLocalPoseIsIdentity = true;

	private Vector3 m_InitialVisualsTransformLocalScale;

	private bool m_IgnoringCharacterCollision;

	private bool m_StopIgnoringCollisionInLateUpdate;

	private CharacterController m_SelectingCharacterController;

	private readonly HashSet<IXRSelectInteractor> m_SelectingCharacterInteractors = new HashSet<IXRSelectInteractor>();

	private readonly List<Collider> m_RigidbodyColliders = new List<Collider>();

	private readonly HashSet<Collider> m_CollidersThatAllowedCharacterCollision = new HashSet<Collider>();

	private Transform m_OriginalSceneParent;

	private TeleportationMonitor m_TeleportationMonitor;

	private readonly Dictionary<IXRSelectInteractor, Transform> m_DynamicAttachTransforms = new Dictionary<IXRSelectInteractor, Transform>();

	private readonly Dictionary<IXRSelectInteractor, Transform> m_VisualAttachTransforms = new Dictionary<IXRSelectInteractor, Transform>();

	private static readonly UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling.LinkedPool<Transform> s_DynamicAttachTransformPool = new UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling.LinkedPool<Transform>(OnCreatePooledItem, OnGetPooledItem, OnReleasePooledItem, OnDestroyPooledItem);

	private static readonly ProfilerMarker s_ProcessGrabTransformersMarker = new ProfilerMarker("XRI.ProcessGrabTransformers");

	private const string k_AttachPointCompatibilityModeDeprecated = "attachPointCompatibilityMode has been deprecated and will be removed in a future version of XRI.";

	private const string k_GravityOnDetachDeprecated = "gravityOnDetach has been deprecated. Use forceGravityOnDetach instead. (UnityUpgradable) -> forceGravityOnDetach";

	public Transform attachTransform
	{
		get
		{
			return m_AttachTransform;
		}
		set
		{
			m_AttachTransform = value;
		}
	}

	public Transform secondaryAttachTransform
	{
		get
		{
			return m_SecondaryAttachTransform;
		}
		set
		{
			m_SecondaryAttachTransform = value;
		}
	}

	public bool useDynamicAttach
	{
		get
		{
			return m_UseDynamicAttach;
		}
		set
		{
			m_UseDynamicAttach = value;
		}
	}

	public bool matchAttachPosition
	{
		get
		{
			return m_MatchAttachPosition;
		}
		set
		{
			m_MatchAttachPosition = value;
		}
	}

	public bool matchAttachRotation
	{
		get
		{
			return m_MatchAttachRotation;
		}
		set
		{
			m_MatchAttachRotation = value;
		}
	}

	public bool snapToColliderVolume
	{
		get
		{
			return m_SnapToColliderVolume;
		}
		set
		{
			m_SnapToColliderVolume = value;
		}
	}

	public bool reinitializeDynamicAttachEverySingleGrab
	{
		get
		{
			return m_ReinitializeDynamicAttachEverySingleGrab;
		}
		set
		{
			m_ReinitializeDynamicAttachEverySingleGrab = value;
		}
	}

	public float attachEaseInTime
	{
		get
		{
			return m_AttachEaseInTime;
		}
		set
		{
			m_AttachEaseInTime = value;
		}
	}

	public MovementType movementType
	{
		get
		{
			return m_MovementType;
		}
		set
		{
			m_MovementType = value;
			UpdateCurrentMovementType();
		}
	}

	public Transform predictedVisualsTransform
	{
		get
		{
			return m_PredictedVisualsTransform;
		}
		set
		{
			m_PredictedVisualsTransform = value;
		}
	}

	public float velocityDamping
	{
		get
		{
			return m_VelocityDamping;
		}
		set
		{
			m_VelocityDamping = value;
		}
	}

	public float velocityScale
	{
		get
		{
			return m_VelocityScale;
		}
		set
		{
			m_VelocityScale = value;
		}
	}

	public float angularVelocityDamping
	{
		get
		{
			return m_AngularVelocityDamping;
		}
		set
		{
			m_AngularVelocityDamping = value;
		}
	}

	public float angularVelocityScale
	{
		get
		{
			return m_AngularVelocityScale;
		}
		set
		{
			m_AngularVelocityScale = value;
		}
	}

	public bool trackPosition
	{
		get
		{
			return m_TrackPosition;
		}
		set
		{
			m_TrackPosition = value;
		}
	}

	public bool smoothPosition
	{
		get
		{
			return m_SmoothPosition;
		}
		set
		{
			m_SmoothPosition = value;
		}
	}

	public float smoothPositionAmount
	{
		get
		{
			return m_SmoothPositionAmount;
		}
		set
		{
			m_SmoothPositionAmount = value;
		}
	}

	public float tightenPosition
	{
		get
		{
			return m_TightenPosition;
		}
		set
		{
			m_TightenPosition = value;
		}
	}

	public bool trackRotation
	{
		get
		{
			return m_TrackRotation;
		}
		set
		{
			m_TrackRotation = value;
		}
	}

	public bool smoothRotation
	{
		get
		{
			return m_SmoothRotation;
		}
		set
		{
			m_SmoothRotation = value;
		}
	}

	public float smoothRotationAmount
	{
		get
		{
			return m_SmoothRotationAmount;
		}
		set
		{
			m_SmoothRotationAmount = value;
		}
	}

	public float tightenRotation
	{
		get
		{
			return m_TightenRotation;
		}
		set
		{
			m_TightenRotation = value;
		}
	}

	public bool trackScale
	{
		get
		{
			return m_TrackScale;
		}
		set
		{
			m_TrackScale = value;
		}
	}

	public bool smoothScale
	{
		get
		{
			return m_SmoothScale;
		}
		set
		{
			m_SmoothScale = value;
		}
	}

	public float smoothScaleAmount
	{
		get
		{
			return m_SmoothScaleAmount;
		}
		set
		{
			m_SmoothScaleAmount = value;
		}
	}

	public float tightenScale
	{
		get
		{
			return m_TightenScale;
		}
		set
		{
			m_TightenScale = value;
		}
	}

	public bool throwOnDetach
	{
		get
		{
			return m_ThrowOnDetach;
		}
		set
		{
			m_ThrowOnDetach = value;
		}
	}

	public float throwSmoothingDuration
	{
		get
		{
			return m_ThrowSmoothingDuration;
		}
		set
		{
			m_ThrowSmoothingDuration = value;
		}
	}

	public AnimationCurve throwSmoothingCurve
	{
		get
		{
			return m_ThrowSmoothingCurve;
		}
		set
		{
			m_ThrowSmoothingCurve = value;
		}
	}

	public float throwVelocityScale
	{
		get
		{
			return m_ThrowVelocityScale;
		}
		set
		{
			m_ThrowVelocityScale = value;
		}
	}

	public float throwAngularVelocityScale
	{
		get
		{
			return m_ThrowAngularVelocityScale;
		}
		set
		{
			m_ThrowAngularVelocityScale = value;
		}
	}

	public bool forceGravityOnDetach
	{
		get
		{
			return m_ForceGravityOnDetach;
		}
		set
		{
			m_ForceGravityOnDetach = value;
		}
	}

	public bool retainTransformParent
	{
		get
		{
			return m_RetainTransformParent;
		}
		set
		{
			m_RetainTransformParent = value;
		}
	}

	public List<XRBaseGrabTransformer> startingSingleGrabTransformers
	{
		get
		{
			return m_StartingSingleGrabTransformers;
		}
		set
		{
			m_StartingSingleGrabTransformers = value;
		}
	}

	public List<XRBaseGrabTransformer> startingMultipleGrabTransformers
	{
		get
		{
			return m_StartingMultipleGrabTransformers;
		}
		set
		{
			m_StartingMultipleGrabTransformers = value;
		}
	}

	public bool addDefaultGrabTransformers
	{
		get
		{
			return m_AddDefaultGrabTransformers;
		}
		set
		{
			m_AddDefaultGrabTransformers = value;
		}
	}

	public InteractableFarAttachMode farAttachMode
	{
		get
		{
			return m_FarAttachMode;
		}
		set
		{
			m_FarAttachMode = value;
		}
	}

	public bool limitLinearVelocity
	{
		get
		{
			return m_LimitLinearVelocity;
		}
		set
		{
			m_LimitLinearVelocity = value;
		}
	}

	public bool limitAngularVelocity
	{
		get
		{
			return m_LimitAngularVelocity;
		}
		set
		{
			m_LimitAngularVelocity = value;
		}
	}

	public float maxLinearVelocityDelta
	{
		get
		{
			return m_MaxLinearVelocityDelta;
		}
		set
		{
			m_MaxLinearVelocityDelta = Mathf.Max(0f, value);
		}
	}

	public float maxAngularVelocityDelta
	{
		get
		{
			return m_MaxAngularVelocityDelta;
		}
		set
		{
			m_MaxAngularVelocityDelta = Mathf.Max(0f, value);
		}
	}

	private bool isRigidbodyMovement
	{
		get
		{
			if (m_CurrentMovementType != 0)
			{
				return m_CurrentMovementType == MovementType.Kinematic;
			}
			return true;
		}
	}

	public int singleGrabTransformersCount => m_SingleGrabTransformers.flushedCount;

	public int multipleGrabTransformersCount => m_MultipleGrabTransformers.flushedCount;

	protected bool allowVisualAttachTransform { get; set; }

	private bool isTransformDirty
	{
		get
		{
			if (!m_IsTargetPoseDirty)
			{
				return m_IsTargetLocalScaleDirty;
			}
			return true;
		}
		set
		{
			m_IsTargetPoseDirty = value;
			m_IsTargetLocalScaleDirty = value;
		}
	}

	[Obsolete("attachPointCompatibilityMode has been deprecated and will be removed in a future version of XRI.", true)]
	public AttachPointCompatibilityMode attachPointCompatibilityMode
	{
		get
		{
			Debug.LogError("attachPointCompatibilityMode has been deprecated and will be removed in a future version of XRI.", this);
			throw new NotSupportedException("attachPointCompatibilityMode has been deprecated and will be removed in a future version of XRI.");
		}
		set
		{
			Debug.LogError("attachPointCompatibilityMode has been deprecated and will be removed in a future version of XRI.", this);
			throw new NotSupportedException("attachPointCompatibilityMode has been deprecated and will be removed in a future version of XRI.");
		}
	}

	[Obsolete("gravityOnDetach has been deprecated. Use forceGravityOnDetach instead. (UnityUpgradable) -> forceGravityOnDetach", true)]
	public bool gravityOnDetach
	{
		get
		{
			Debug.LogError("gravityOnDetach has been deprecated. Use forceGravityOnDetach instead. (UnityUpgradable) -> forceGravityOnDetach", this);
			throw new NotSupportedException("gravityOnDetach has been deprecated. Use forceGravityOnDetach instead. (UnityUpgradable) -> forceGravityOnDetach");
		}
		set
		{
			Debug.LogError("gravityOnDetach has been deprecated. Use forceGravityOnDetach instead. (UnityUpgradable) -> forceGravityOnDetach", this);
			throw new NotSupportedException("gravityOnDetach has been deprecated. Use forceGravityOnDetach instead. (UnityUpgradable) -> forceGravityOnDetach");
		}
	}

	protected override void Reset()
	{
		Transform transform = null;
		int num = 0;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.TryGetComponent<MeshRenderer>(out var _) || child.TryGetComponent<SkinnedMeshRenderer>(out var _))
			{
				if (num == 0)
				{
					transform = child;
				}
				num++;
			}
		}
		if (num == 1 && transform != null && transform.GetComponentInChildren<Collider>() == null)
		{
			m_PredictedVisualsTransform = transform;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_TeleportationMonitor = new TeleportationMonitor();
		m_TeleportationMonitor.teleported += OnTeleported;
		m_CurrentMovementType = m_MovementType;
		if (!TryGetComponent<Rigidbody>(out m_Rigidbody))
		{
			Debug.LogError("XR Grab Interactable does not have a required Rigidbody.", this);
		}
		m_Rigidbody.GetComponentsInChildren(includeInactive: true, m_RigidbodyColliders);
		for (int num = m_RigidbodyColliders.Count - 1; num >= 0; num--)
		{
			if (m_RigidbodyColliders[num].attachedRigidbody != m_Rigidbody)
			{
				m_RigidbodyColliders.RemoveAt(num);
			}
		}
		m_Transform = base.transform;
		InitializeTargetPoseAndScale(m_Transform);
		FindStartingGrabTransformers();
		RegisterStartingGrabTransformers();
		FlushRegistration();
	}

	protected override void OnDestroy()
	{
		ClearSingleGrabTransformers();
		ClearMultipleGrabTransformers();
		base.OnDestroy();
	}

	protected virtual void OnCollisionStay()
	{
		m_RigidbodyColliding = true;
	}

	public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractable(updatePhase);
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			AddDefaultGrabTransformers();
		}
		FlushRegistration();
		allowVisualAttachTransform = false;
		switch (updatePhase)
		{
		case XRInteractionUpdateOrder.UpdatePhase.Fixed:
			m_RigidbodyColliding = false;
			if ((base.isSelected || isTransformDirty) && isRigidbodyMovement)
			{
				if (m_IsTargetLocalScaleDirty && !m_IsTargetPoseDirty && !base.isSelected)
				{
					ApplyTargetScale();
				}
				else if (m_CurrentMovementType == MovementType.Kinematic)
				{
					PerformKinematicUpdate();
				}
				else if (m_CurrentMovementType == MovementType.VelocityTracking)
				{
					PerformVelocityTrackingUpdate(Time.deltaTime);
				}
				m_LastFixedFrame = Time.frameCount;
			}
			if (m_IgnoringCharacterCollision && !m_StopIgnoringCollisionInLateUpdate && m_SelectingCharacterInteractors.Count == 0 && m_SelectingCharacterController != null && IsOutsideCharacterCollider(m_SelectingCharacterController))
			{
				m_StopIgnoringCollisionInLateUpdate = true;
			}
			break;
		case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
		case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
			if (isTransformDirty)
			{
				if (m_IsTargetLocalScaleDirty && !m_IsTargetPoseDirty)
				{
					ApplyTargetScale();
				}
				else
				{
					PerformInstantaneousUpdate();
				}
			}
			if (base.isSelected || (m_GrabCountChanged && m_DropTransformersCount > 0))
			{
				UpdateTarget(updatePhase, Time.deltaTime);
				if (m_LastFixedFrame == Time.frameCount)
				{
					m_LastFixedDynamicTime = Time.time;
				}
				if (m_CurrentMovementType == MovementType.Instantaneous)
				{
					PerformInstantaneousUpdate();
				}
				else if (m_CurrentMovementType == MovementType.Kinematic)
				{
					PerformKinematicVisualsUpdate();
				}
				else if (m_CurrentMovementType == MovementType.VelocityTracking)
				{
					PerformVelocityVisualsUpdate();
				}
			}
			if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender && base.isSelected && isRigidbodyMovement && m_PredictedVisualsTransform != null)
			{
				PerformVisualAttachUpdate();
			}
			break;
		case XRInteractionUpdateOrder.UpdatePhase.Late:
			if (m_DetachInLateUpdate)
			{
				if (!base.isSelected)
				{
					Detach();
				}
				m_DetachInLateUpdate = false;
			}
			if (m_StopIgnoringCollisionInLateUpdate)
			{
				if (m_IgnoringCharacterCollision && m_SelectingCharacterController != null)
				{
					StopIgnoringCharacterCollision(m_SelectingCharacterController);
					m_SelectingCharacterController = null;
				}
				m_StopIgnoringCollisionInLateUpdate = false;
			}
			break;
		}
	}

	public override Transform GetAttachTransform(IXRInteractor interactor)
	{
		IXRSelectInteractor iXRSelectInteractor = interactor as IXRSelectInteractor;
		bool flag = iXRSelectInteractor != null;
		if (allowVisualAttachTransform && base.isSelected && isRigidbodyMovement && m_PredictedVisualsTransform != null && flag && m_VisualAttachTransforms.TryGetValue(iXRSelectInteractor, out var value))
		{
			return value;
		}
		bool flag2 = base.interactorsSelecting.Count <= 1 || interactor == base.interactorsSelecting[0];
		bool flag3 = m_UseDynamicAttach || (!flag2 && m_SecondaryAttachTransform == null);
		if (flag3 && flag && m_DynamicAttachTransforms.TryGetValue(iXRSelectInteractor, out var value2))
		{
			if (value2 != null)
			{
				return value2;
			}
			m_DynamicAttachTransforms.Remove(iXRSelectInteractor);
			Debug.LogWarning($"Dynamic Attach Transform created by {this} for {interactor} was destroyed after being created." + " Continuing as if Use Dynamic Attach was disabled for this pair.", this);
		}
		if (!flag2 && !flag3)
		{
			return m_SecondaryAttachTransform;
		}
		if (!(m_AttachTransform != null))
		{
			return base.GetAttachTransform(interactor);
		}
		return m_AttachTransform;
	}

	public void AddSingleGrabTransformer(IXRGrabTransformer transformer)
	{
		AddGrabTransformer(transformer, m_SingleGrabTransformers);
	}

	public void AddMultipleGrabTransformer(IXRGrabTransformer transformer)
	{
		AddGrabTransformer(transformer, m_MultipleGrabTransformers);
	}

	public bool RemoveSingleGrabTransformer(IXRGrabTransformer transformer)
	{
		return RemoveGrabTransformer(transformer, m_SingleGrabTransformers);
	}

	public bool RemoveMultipleGrabTransformer(IXRGrabTransformer transformer)
	{
		return RemoveGrabTransformer(transformer, m_MultipleGrabTransformers);
	}

	public void ClearSingleGrabTransformers()
	{
		ClearGrabTransformers(m_SingleGrabTransformers);
	}

	public void ClearMultipleGrabTransformers()
	{
		ClearGrabTransformers(m_MultipleGrabTransformers);
	}

	public void GetSingleGrabTransformers(List<IXRGrabTransformer> results)
	{
		GetGrabTransformers(m_SingleGrabTransformers, results);
	}

	public void GetMultipleGrabTransformers(List<IXRGrabTransformer> results)
	{
		GetGrabTransformers(m_MultipleGrabTransformers, results);
	}

	public IXRGrabTransformer GetSingleGrabTransformerAt(int index)
	{
		return m_SingleGrabTransformers.GetRegisteredItemAt(index);
	}

	public IXRGrabTransformer GetMultipleGrabTransformerAt(int index)
	{
		return m_MultipleGrabTransformers.GetRegisteredItemAt(index);
	}

	public void MoveSingleGrabTransformerTo(IXRGrabTransformer transformer, int newIndex)
	{
		MoveGrabTransformerTo(transformer, newIndex, m_SingleGrabTransformers);
	}

	public void MoveMultipleGrabTransformerTo(IXRGrabTransformer transformer, int newIndex)
	{
		MoveGrabTransformerTo(transformer, newIndex, m_MultipleGrabTransformers);
	}

	public Pose GetTargetPose()
	{
		return m_TargetPose;
	}

	public void SetTargetPose(Pose pose)
	{
		m_TargetPose = pose;
		m_IsTargetPoseDirty = base.interactorsSelecting.Count == 0;
	}

	public Vector3 GetTargetLocalScale()
	{
		return m_TargetLocalScale;
	}

	public void SetTargetLocalScale(Vector3 localScale)
	{
		m_TargetLocalScale = localScale;
		m_IsTargetLocalScaleDirty = base.interactorsSelecting.Count == 0;
	}

	private void InitializeTargetPoseAndScale(Transform thisTransform)
	{
		if (!m_IsTargetPoseDirty)
		{
			m_TargetPose = thisTransform.GetWorldPose();
		}
		if (!m_IsTargetLocalScaleDirty)
		{
			m_TargetLocalScale = thisTransform.localScale;
		}
	}

	private void AddGrabTransformer(IXRGrabTransformer transformer, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
	{
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		if (m_IsProcessingGrabTransformers)
		{
			Debug.LogWarning($"{transformer} added while {base.name} is processing grab transformers. It won't be processed until the next process.", this);
		}
		if (grabTransformers.Register(transformer))
		{
			OnAddedGrabTransformer(transformer);
		}
	}

	private bool RemoveGrabTransformer(IXRGrabTransformer transformer, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
	{
		if (grabTransformers.Unregister(transformer))
		{
			OnRemovedGrabTransformer(transformer);
			return true;
		}
		return false;
	}

	private void ClearGrabTransformers(BaseRegistrationList<IXRGrabTransformer> grabTransformers)
	{
		for (int num = grabTransformers.flushedCount - 1; num >= 0; num--)
		{
			IXRGrabTransformer registeredItemAt = grabTransformers.GetRegisteredItemAt(num);
			RemoveGrabTransformer(registeredItemAt, grabTransformers);
		}
	}

	private static void GetGrabTransformers(BaseRegistrationList<IXRGrabTransformer> grabTransformers, List<IXRGrabTransformer> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		grabTransformers.GetRegisteredItems(results);
	}

	private void MoveGrabTransformerTo(IXRGrabTransformer transformer, int newIndex, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
	{
		if (transformer == null)
		{
			throw new ArgumentNullException("transformer");
		}
		if (m_IsProcessingGrabTransformers)
		{
			Debug.LogError($"Cannot move {transformer} while {base.name} is processing grab transformers.", this);
			return;
		}
		grabTransformers.Flush();
		if (grabTransformers.MoveItemImmediately(transformer, newIndex))
		{
			OnAddedGrabTransformer(transformer);
		}
	}

	private void FindStartingGrabTransformers()
	{
		if (m_StartingSingleGrabTransformers.Count != 0 || m_StartingMultipleGrabTransformers.Count != 0)
		{
			return;
		}
		List<IXRGrabTransformer> value;
		using (UnityEngine.Pool.CollectionPool<List<IXRGrabTransformer>, IXRGrabTransformer>.Get(out value))
		{
			GetComponents(value);
			if (value.Count == 0)
			{
				return;
			}
			bool flag = false;
			foreach (IXRGrabTransformer item in value)
			{
				if (item is XRBaseGrabTransformer xRBaseGrabTransformer)
				{
					switch (xRBaseGrabTransformer.GetRegistrationMode())
					{
					case XRBaseGrabTransformer.RegistrationMode.Single:
						m_StartingSingleGrabTransformers.Add(xRBaseGrabTransformer);
						break;
					case XRBaseGrabTransformer.RegistrationMode.Multiple:
						m_StartingMultipleGrabTransformers.Add(xRBaseGrabTransformer);
						break;
					case XRBaseGrabTransformer.RegistrationMode.SingleAndMultiple:
						m_StartingSingleGrabTransformers.Add(xRBaseGrabTransformer);
						m_StartingMultipleGrabTransformers.Add(xRBaseGrabTransformer);
						break;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				string text = "XR Grab Interactable \"" + base.name + "\" has a custom IXRGrabTransformer component on the same GameObject that cannot be added to either the Starting Multiple Grab Transformers or Starting Single Grab Transformers lists. Custom transformers must be registered during runtime using methods like AddSingleGrabTransformer and AddMultipleGrabTransformer.";
				if (m_StartingSingleGrabTransformers.Count > 0 || m_StartingMultipleGrabTransformers.Count > 0)
				{
					text += " The other XRBaseGrabTransformer derived grab transformers have been added to the starting lists.";
				}
				Debug.LogWarning(text, this);
			}
		}
	}

	private void RegisterStartingGrabTransformers()
	{
		if (m_SingleGrabTransformers.flushedCount > 0)
		{
			int num = 0;
			foreach (XRBaseGrabTransformer startingSingleGrabTransformer in m_StartingSingleGrabTransformers)
			{
				if (startingSingleGrabTransformer != null)
				{
					MoveSingleGrabTransformerTo(startingSingleGrabTransformer, num++);
				}
			}
		}
		else
		{
			foreach (XRBaseGrabTransformer startingSingleGrabTransformer2 in m_StartingSingleGrabTransformers)
			{
				if (startingSingleGrabTransformer2 != null)
				{
					AddSingleGrabTransformer(startingSingleGrabTransformer2);
				}
			}
		}
		if (m_MultipleGrabTransformers.flushedCount > 0)
		{
			int num2 = 0;
			{
				foreach (XRBaseGrabTransformer startingMultipleGrabTransformer in m_StartingMultipleGrabTransformers)
				{
					if (startingMultipleGrabTransformer != null)
					{
						MoveMultipleGrabTransformerTo(startingMultipleGrabTransformer, num2++);
					}
				}
				return;
			}
		}
		foreach (XRBaseGrabTransformer startingMultipleGrabTransformer2 in m_StartingMultipleGrabTransformers)
		{
			if (startingMultipleGrabTransformer2 != null)
			{
				AddMultipleGrabTransformer(startingMultipleGrabTransformer2);
			}
		}
	}

	private void FlushRegistration()
	{
		m_SingleGrabTransformers.Flush();
		m_MultipleGrabTransformers.Flush();
	}

	private void InvokeGrabTransformersOnGrab()
	{
		m_IsProcessingGrabTransformers = true;
		if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
		{
			foreach (IXRGrabTransformer item in m_SingleGrabTransformers.registeredSnapshot)
			{
				if (m_SingleGrabTransformers.IsStillRegistered(item))
				{
					item.OnGrab(this);
				}
			}
		}
		if (m_MultipleGrabTransformers.registeredSnapshot.Count > 0)
		{
			foreach (IXRGrabTransformer item2 in m_MultipleGrabTransformers.registeredSnapshot)
			{
				if (m_MultipleGrabTransformers.IsStillRegistered(item2))
				{
					item2.OnGrab(this);
				}
			}
		}
		m_IsProcessingGrabTransformers = false;
	}

	private void InvokeGrabTransformersOnDrop(DropEventArgs args)
	{
		m_IsProcessingGrabTransformers = true;
		if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
		{
			foreach (IXRGrabTransformer item in m_SingleGrabTransformers.registeredSnapshot)
			{
				if (item is IXRDropTransformer iXRDropTransformer && m_SingleGrabTransformers.IsStillRegistered(item))
				{
					iXRDropTransformer.OnDrop(this, args);
				}
			}
		}
		if (m_MultipleGrabTransformers.registeredSnapshot.Count > 0)
		{
			foreach (IXRGrabTransformer item2 in m_MultipleGrabTransformers.registeredSnapshot)
			{
				if (item2 is IXRDropTransformer iXRDropTransformer2 && m_MultipleGrabTransformers.IsStillRegistered(item2))
				{
					iXRDropTransformer2.OnDrop(this, args);
				}
			}
		}
		m_IsProcessingGrabTransformers = false;
	}

	private void InvokeGrabTransformersProcess(XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
	{
		m_IsProcessingGrabTransformers = true;
		using (s_ProcessGrabTransformersMarker.Auto())
		{
			bool flag = base.isSelected;
			bool flag2 = m_SingleGrabTransformers.registeredSnapshot.Count > 0;
			bool flag3 = m_MultipleGrabTransformers.registeredSnapshot.Count > 0;
			if (m_GrabCountChanged)
			{
				if (flag)
				{
					if (flag2)
					{
						foreach (IXRGrabTransformer item in m_SingleGrabTransformers.registeredSnapshot)
						{
							if (m_SingleGrabTransformers.IsStillRegistered(item))
							{
								item.OnGrabCountChanged(this, targetPose, localScale);
							}
						}
					}
					if (flag3)
					{
						foreach (IXRGrabTransformer item2 in m_MultipleGrabTransformers.registeredSnapshot)
						{
							if (m_MultipleGrabTransformers.IsStillRegistered(item2))
							{
								item2.OnGrabCountChanged(this, targetPose, localScale);
							}
						}
					}
				}
				m_GrabCountChanged = false;
				m_GrabTransformersAddedWhenGrabbed?.Clear();
			}
			else
			{
				List<IXRGrabTransformer> grabTransformersAddedWhenGrabbed = m_GrabTransformersAddedWhenGrabbed;
				if (grabTransformersAddedWhenGrabbed != null && grabTransformersAddedWhenGrabbed.Count > 0)
				{
					if (flag)
					{
						foreach (IXRGrabTransformer item3 in m_GrabTransformersAddedWhenGrabbed)
						{
							item3.OnGrabCountChanged(this, targetPose, localScale);
						}
					}
					m_GrabTransformersAddedWhenGrabbed.Clear();
				}
			}
			if (flag)
			{
				bool flag4 = false;
				if (flag3 && (base.interactorsSelecting.Count > 1 || !CanProcessAnySingleGrabTransformer()))
				{
					foreach (IXRGrabTransformer item4 in m_MultipleGrabTransformers.registeredSnapshot)
					{
						if (m_MultipleGrabTransformers.IsStillRegistered(item4) && item4.canProcess)
						{
							item4.Process(this, updatePhase, ref targetPose, ref localScale);
							flag4 = true;
						}
					}
				}
				if (!flag4 && flag2)
				{
					foreach (IXRGrabTransformer item5 in m_SingleGrabTransformers.registeredSnapshot)
					{
						if (m_SingleGrabTransformers.IsStillRegistered(item5) && item5.canProcess)
						{
							item5.Process(this, updatePhase, ref targetPose, ref localScale);
						}
					}
				}
			}
			else
			{
				if (flag3)
				{
					foreach (IXRGrabTransformer item6 in m_MultipleGrabTransformers.registeredSnapshot)
					{
						if (item6 is IXRDropTransformer iXRDropTransformer && m_MultipleGrabTransformers.IsStillRegistered(item6) && iXRDropTransformer.canProcessOnDrop && item6.canProcess)
						{
							item6.Process(this, updatePhase, ref targetPose, ref localScale);
						}
					}
				}
				if (flag2)
				{
					foreach (IXRGrabTransformer item7 in m_SingleGrabTransformers.registeredSnapshot)
					{
						if (item7 is IXRDropTransformer iXRDropTransformer2 && m_SingleGrabTransformers.IsStillRegistered(item7) && iXRDropTransformer2.canProcessOnDrop && item7.canProcess)
						{
							item7.Process(this, updatePhase, ref targetPose, ref localScale);
						}
					}
				}
			}
		}
		m_IsProcessingGrabTransformers = false;
	}

	private bool CanProcessAnySingleGrabTransformer()
	{
		if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
		{
			foreach (IXRGrabTransformer item in m_SingleGrabTransformers.registeredSnapshot)
			{
				if (m_SingleGrabTransformers.IsStillRegistered(item) && item.canProcess)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnAddedGrabTransformer(IXRGrabTransformer transformer)
	{
		if (transformer is IXRDropTransformer)
		{
			m_DropTransformersCount++;
		}
		transformer.OnLink(this);
		if (base.interactorsSelecting.Count != 0)
		{
			transformer.OnGrab(this);
			if (m_GrabTransformersAddedWhenGrabbed == null)
			{
				m_GrabTransformersAddedWhenGrabbed = new List<IXRGrabTransformer>();
			}
			m_GrabTransformersAddedWhenGrabbed.Add(transformer);
		}
	}

	private void OnRemovedGrabTransformer(IXRGrabTransformer transformer)
	{
		if (transformer is IXRDropTransformer)
		{
			m_DropTransformersCount--;
		}
		transformer.OnUnlink(this);
		m_GrabTransformersAddedWhenGrabbed?.Remove(transformer);
	}

	private bool AddDefaultGrabTransformers()
	{
		if (!m_AddDefaultGrabTransformers)
		{
			return false;
		}
		bool result = false;
		if (m_SingleGrabTransformers.flushedCount == 0)
		{
			AddDefaultSingleGrabTransformer();
			result = true;
		}
		if (base.selectMode == InteractableSelectMode.Multiple && base.interactorsSelecting.Count > 1 && m_MultipleGrabTransformers.flushedCount == 0)
		{
			AddDefaultMultipleGrabTransformer();
			result = true;
		}
		return result;
	}

	protected virtual void AddDefaultSingleGrabTransformer()
	{
		if (m_SingleGrabTransformers.flushedCount == 0)
		{
			IXRGrabTransformer orAddDefaultGrabTransformer = GetOrAddDefaultGrabTransformer();
			AddSingleGrabTransformer(orAddDefaultGrabTransformer);
		}
	}

	protected virtual void AddDefaultMultipleGrabTransformer()
	{
		if (m_MultipleGrabTransformers.flushedCount == 0)
		{
			IXRGrabTransformer orAddDefaultGrabTransformer = GetOrAddDefaultGrabTransformer();
			AddMultipleGrabTransformer(orAddDefaultGrabTransformer);
		}
	}

	private IXRGrabTransformer GetOrAddDefaultGrabTransformer()
	{
		return GetOrAddComponent<XRGeneralGrabTransformer>();
	}

	private T GetOrAddComponent<T>() where T : Component
	{
		if (!TryGetComponent<T>(out var component))
		{
			return base.gameObject.AddComponent<T>();
		}
		return component;
	}

	private void UpdateTarget(XRInteractionUpdateOrder.UpdatePhase updatePhase, float deltaTime)
	{
		if (m_ReinitializeDynamicAttachEverySingleGrab && m_GrabCountChanged && m_GrabCountBeforeAndAfterChange.Item2 < m_GrabCountBeforeAndAfterChange.Item1 && base.interactorsSelecting.Count == 1 && m_DynamicAttachTransforms.Count > 0 && m_DynamicAttachTransforms.TryGetValue(base.interactorsSelecting[0], out var value))
		{
			InitializeDynamicAttachPoseInternal(base.interactorsSelecting[0], value);
		}
		Pose targetPose = m_TargetPose;
		Vector3 localScale = m_TargetLocalScale;
		InvokeGrabTransformersProcess(updatePhase, ref targetPose, ref localScale);
		if (!base.isSelected)
		{
			m_TargetPose = targetPose;
			m_TargetLocalScale = localScale;
			return;
		}
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			StepThrowSmoothing(targetPose, deltaTime);
		}
		StepSmoothing(in targetPose, in localScale, deltaTime);
	}

	private void StepSmoothing(in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime)
	{
		if (m_AttachEaseInTime > 0f && m_CurrentAttachEaseTime <= m_AttachEaseInTime)
		{
			EaseAttachBurst(ref m_TargetPose, ref m_TargetLocalScale, in rawTargetPose, in rawTargetLocalScale, deltaTime, m_AttachEaseInTime, ref m_CurrentAttachEaseTime);
		}
		else
		{
			StepSmoothingBurst(ref m_TargetPose, ref m_TargetLocalScale, in rawTargetPose, in rawTargetLocalScale, deltaTime, m_SmoothPosition, m_SmoothPositionAmount, m_TightenPosition, m_SmoothRotation, m_SmoothRotationAmount, m_TightenRotation, m_SmoothScale, m_SmoothScaleAmount, m_TightenScale);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractables_002EEaseAttachBurst_00000F9A_0024PostfixBurstDelegate))]
	private static void EaseAttachBurst(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime, float attachEaseInTime, ref float currentAttachEaseTime)
	{
		EaseAttachBurst_00000F9A_0024BurstDirectCall.Invoke(ref targetPose, ref targetLocalScale, in rawTargetPose, in rawTargetLocalScale, deltaTime, attachEaseInTime, ref currentAttachEaseTime);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractables_002EStepSmoothingBurst_00000F9B_0024PostfixBurstDelegate))]
	private static void StepSmoothingBurst(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime, bool smoothPos, float smoothPosAmount, float tightenPos, bool smoothRot, float smoothRotAmount, float tightenRot, bool smoothScale, float smoothScaleAmount, float tightenScale)
	{
		StepSmoothingBurst_00000F9B_0024BurstDirectCall.Invoke(ref targetPose, ref targetLocalScale, in rawTargetPose, in rawTargetLocalScale, deltaTime, smoothPos, smoothPosAmount, tightenPos, smoothRot, smoothRotAmount, tightenRot, smoothScale, smoothScaleAmount, tightenScale);
	}

	private void PerformInstantaneousUpdate()
	{
		if (m_TrackPosition && m_TrackRotation)
		{
			m_Transform.SetWorldPose(m_TargetPose);
		}
		else if (m_TrackPosition)
		{
			m_Transform.position = m_TargetPose.position;
		}
		else if (m_TrackRotation)
		{
			m_Transform.rotation = m_TargetPose.rotation;
		}
		ApplyTargetScale();
		isTransformDirty = false;
	}

	private void PerformKinematicUpdate()
	{
		if (m_TrackPosition)
		{
			m_Rigidbody.MovePosition(m_TargetPose.position);
		}
		if (m_TrackRotation)
		{
			m_Rigidbody.MoveRotation(m_TargetPose.rotation);
		}
		ApplyTargetScale();
		isTransformDirty = false;
	}

	private void PerformVelocityTrackingUpdate(float fixedDeltaTime)
	{
		if (fixedDeltaTime < 0.001f)
		{
			return;
		}
		if (m_TrackPosition)
		{
			Vector3 linearVelocity = m_Rigidbody.linearVelocity;
			linearVelocity *= 1f - m_VelocityDamping;
			Vector3 vector = m_TargetPose.position - m_Rigidbody.position;
			Vector3 vector2 = linearVelocity + vector / fixedDeltaTime * m_VelocityScale;
			Vector3 linearVelocity2 = (m_LimitLinearVelocity ? Vector3.MoveTowards(linearVelocity, vector2, m_MaxLinearVelocityDelta) : vector2);
			m_Rigidbody.linearVelocity = linearVelocity2;
		}
		if (m_TrackRotation)
		{
			Vector3 angularVelocity = m_Rigidbody.angularVelocity;
			angularVelocity *= 1f - m_AngularVelocityDamping;
			(m_TargetPose.rotation * Quaternion.Inverse(m_Rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);
			if (angle > 180f)
			{
				angle -= 360f;
			}
			if (Mathf.Abs(angle) > Mathf.Epsilon)
			{
				axis = axis.normalized;
				Vector3 vector3 = axis * (angle * (MathF.PI / 180f)) / fixedDeltaTime;
				Vector3 vector4 = angularVelocity + vector3 * m_AngularVelocityScale;
				m_Rigidbody.angularVelocity = (m_LimitAngularVelocity ? Vector3.MoveTowards(angularVelocity, vector4, m_MaxAngularVelocityDelta) : vector4);
			}
			else
			{
				m_Rigidbody.angularVelocity = angularVelocity;
			}
		}
		ApplyTargetScale();
		isTransformDirty = false;
	}

	private void PerformVelocityVisualsUpdate()
	{
		if (m_PredictedVisualsTransform == null)
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		float deltaTime = Time.deltaTime;
		if (fixedDeltaTime < 0.001f || deltaTime < 0.001f)
		{
			return;
		}
		if (m_RigidbodyColliding || m_Rigidbody.IsSleeping())
		{
			m_PredictedVisualsTransform.SetLocalPose(m_InitialVisualsTransformLocalPose);
			m_PredictedVisualsTransform.localScale = m_InitialVisualsTransformLocalScale;
			m_Rigidbody.interpolation = m_InterpolationOnGrab;
			return;
		}
		m_Rigidbody.interpolation = RigidbodyInterpolation.None;
		Pose identity = Pose.identity;
		if (m_TrackPosition)
		{
			Vector3 linearVelocity = m_Rigidbody.linearVelocity;
			linearVelocity *= 1f - m_VelocityDamping;
			Vector3 vector = m_TargetPose.position - m_Rigidbody.position;
			Vector3 vector2 = linearVelocity + vector / fixedDeltaTime * m_VelocityScale;
			Vector3 vector3 = vector2;
			if (m_LimitLinearVelocity)
			{
				float maxDistanceDelta = Mathf.Min(m_Rigidbody.maxLinearVelocity, m_MaxLinearVelocityDelta);
				vector3 = Vector3.MoveTowards(linearVelocity, vector2, maxDistanceDelta);
			}
			Vector3 vector4 = (vector3 - linearVelocity) * fixedDeltaTime;
			if (Mathf.Abs(vector4.x) <= 0.001f && Mathf.Abs(vector4.y) <= 0.001f && Mathf.Abs(vector4.z) <= 0.001f)
			{
				identity.position = m_Rigidbody.position;
			}
			else
			{
				identity.position = m_Rigidbody.position + vector4;
			}
		}
		if (m_TrackRotation)
		{
			Vector3 angularVelocity = m_Rigidbody.angularVelocity;
			angularVelocity *= 1f - m_AngularVelocityDamping;
			(m_TargetPose.rotation * Quaternion.Inverse(m_Rigidbody.rotation)).ToAngleAxis(out var angle, out var axis);
			if (angle > 180f)
			{
				angle -= 360f;
			}
			if (Mathf.Abs(angle) > Mathf.Epsilon)
			{
				axis = axis.normalized;
				Vector3 vector5 = axis * (angle * (MathF.PI / 180f)) / fixedDeltaTime;
				Vector3 vector6 = angularVelocity + vector5 * m_AngularVelocityScale;
				float num = (m_LimitAngularVelocity ? Mathf.Min(m_Rigidbody.maxAngularVelocity, m_MaxAngularVelocityDelta) : m_Rigidbody.maxAngularVelocity);
				if (vector6.sqrMagnitude <= num * num)
				{
					identity.rotation = m_TargetPose.rotation;
				}
				else
				{
					float num2 = Time.time - m_LastFixedDynamicTime + deltaTime;
					if (num2 >= Time.fixedDeltaTime)
					{
						num2 = Time.fixedDeltaTime;
					}
					float maxDegreesDelta = num * 57.29578f * num2;
					Quaternion rotation = Quaternion.RotateTowards(m_Rigidbody.rotation, m_TargetPose.rotation, maxDegreesDelta);
					identity.rotation = rotation;
				}
			}
			else
			{
				identity.rotation = m_Rigidbody.rotation;
			}
		}
		ApplyVisuals(identity);
	}

	private void PerformKinematicVisualsUpdate()
	{
		if (!(m_PredictedVisualsTransform == null))
		{
			ApplyVisuals(m_TargetPose);
		}
	}

	private void ApplyVisuals(Pose visualsPose)
	{
		Pose pose;
		if (m_InitialVisualsTransformLocalPoseIsIdentity)
		{
			pose = visualsPose;
		}
		else
		{
			Pose initialVisualsTransformLocalPose = m_InitialVisualsTransformLocalPose;
			initialVisualsTransformLocalPose.position = Vector3.Scale(initialVisualsTransformLocalPose.position, m_TrackScale ? m_TargetLocalScale : m_Transform.localScale);
			pose = initialVisualsTransformLocalPose.GetTransformedBy(visualsPose);
		}
		if (m_TrackPosition && m_TrackRotation)
		{
			m_PredictedVisualsTransform.SetWorldPose(pose);
		}
		else if (m_TrackPosition)
		{
			m_PredictedVisualsTransform.position = pose.position;
		}
		else if (m_TrackRotation)
		{
			m_PredictedVisualsTransform.rotation = pose.rotation;
		}
		if (m_TrackScale)
		{
			Vector3 a = m_TargetLocalScale.SafeDivide(m_Transform.localScale);
			m_PredictedVisualsTransform.localScale = Vector3.Scale(a, m_InitialVisualsTransformLocalScale);
		}
	}

	private void ApplyTargetScale()
	{
		if (m_TrackScale)
		{
			m_Transform.localScale = m_TargetLocalScale;
		}
		m_IsTargetLocalScaleDirty = false;
	}

	private void PerformVisualAttachUpdate()
	{
		allowVisualAttachTransform = false;
		foreach (IXRSelectInteractor item in base.interactorsSelecting)
		{
			if (m_VisualAttachTransforms.TryGetValue(item, out var value))
			{
				Pose pose;
				if (m_InitialVisualsTransformLocalPoseIsIdentity)
				{
					pose = m_PredictedVisualsTransform.GetWorldPose();
				}
				else
				{
					Quaternion quaternion = Quaternion.Inverse(m_InitialVisualsTransformLocalPose.rotation);
					Vector3 position = -(quaternion * m_InitialVisualsTransformLocalPose.position);
					pose = new Pose(position, quaternion).GetTransformedBy(m_PredictedVisualsTransform.GetWorldPose());
				}
				Transform transform = GetAttachTransform(item);
				if (transform == m_Transform)
				{
					value.SetWorldPose(pose);
				}
				else
				{
					Pose pose2 = ((transform.parent == m_Transform) ? transform.GetLocalPose() : m_Transform.InverseTransformPose(transform.GetWorldPose()));
					value.SetWorldPose(pose2.GetTransformedBy(pose));
				}
				value.localPosition = Vector3.Scale(value.localPosition, m_TrackScale ? m_TargetLocalScale : m_Transform.localScale);
			}
		}
		allowVisualAttachTransform = true;
	}

	private void UpdateCurrentMovementType()
	{
		if (!base.isSelected)
		{
			m_CurrentMovementType = m_MovementType;
			return;
		}
		MovementType? movementType = null;
		for (int num = base.interactorsSelecting.Count - 1; num >= 0; num--)
		{
			XRBaseInteractor xRBaseInteractor = base.interactorsSelecting[num] as XRBaseInteractor;
			if (xRBaseInteractor != null && xRBaseInteractor.selectedInteractableMovementTypeOverride.HasValue)
			{
				if (!movementType.HasValue)
				{
					movementType = xRBaseInteractor.selectedInteractableMovementTypeOverride.Value;
				}
				else if (movementType != xRBaseInteractor.selectedInteractableMovementTypeOverride)
				{
					Debug.LogWarning("Multiple interactors selecting \"" + base.name + "\" have different movement type override values set (selectedInteractableMovementTypeOverride)." + $" Conflict resolved using {movementType.Value} from the most recent interactor to select this object with an override.", this);
				}
			}
		}
		MovementType movementType2 = movementType ?? m_MovementType;
		if (movementType2 != m_CurrentMovementType)
		{
			SetupRigidbodyDrop(m_Rigidbody);
			m_CurrentMovementType = movementType2;
			SetupRigidbodyGrab(m_Rigidbody);
			if (m_CurrentMovementType == MovementType.Instantaneous && m_PredictedVisualsTransform != null)
			{
				m_PredictedVisualsTransform.SetLocalPose(m_InitialVisualsTransformLocalPose);
				m_PredictedVisualsTransform.localScale = m_InitialVisualsTransformLocalScale;
			}
		}
	}

	protected override void OnHoverEntering(HoverEnterEventArgs args)
	{
		base.OnHoverEntering(args);
		AddDefaultGrabTransformers();
	}

	protected override void OnSelectEntering(SelectEnterEventArgs args)
	{
		Transform dynamicAttachTransform = CreateDynamicAttachTransform(args.interactorObject);
		InitializeDynamicAttachPoseInternal(args.interactorObject, dynamicAttachTransform);
		if (m_PredictedVisualsTransform != null)
		{
			Transform value = CreateVisualAttachTransform(args.interactorObject);
			m_VisualAttachTransforms.Remove(args.interactorObject);
			m_VisualAttachTransforms[args.interactorObject] = value;
			value.SetWorldPose(dynamicAttachTransform.GetWorldPose());
		}
		int count = base.interactorsSelecting.Count;
		base.OnSelectEntering(args);
		int count2 = base.interactorsSelecting.Count;
		m_GrabCountChanged = true;
		m_GrabCountBeforeAndAfterChange = (count, count2);
		m_CurrentAttachEaseTime = 0f;
		ResetThrowSmoothing();
		if (!m_IgnoringCharacterCollision)
		{
			m_SelectingCharacterController = args.interactorObject.transform.GetComponentInParent<CharacterController>();
			if (m_SelectingCharacterController != null)
			{
				m_SelectingCharacterInteractors.Add(args.interactorObject);
				StartIgnoringCharacterCollision(m_SelectingCharacterController);
			}
		}
		else if (m_SelectingCharacterController != null && args.interactorObject.transform.IsChildOf(m_SelectingCharacterController.transform))
		{
			m_SelectingCharacterInteractors.Add(args.interactorObject);
		}
		if (base.interactorsSelecting.Count == 1)
		{
			Grab();
			if (!AddDefaultGrabTransformers())
			{
				InvokeGrabTransformersOnGrab();
			}
		}
		else
		{
			UpdateCurrentMovementType();
		}
		SubscribeTeleportationProvider(args.interactorObject);
	}

	protected override void OnSelectExiting(SelectExitEventArgs args)
	{
		int count = base.interactorsSelecting.Count;
		base.OnSelectExiting(args);
		int count2 = base.interactorsSelecting.Count;
		m_GrabCountChanged = true;
		m_GrabCountBeforeAndAfterChange = (count, count2);
		m_CurrentAttachEaseTime = 0f;
		if (base.interactorsSelecting.Count == 0)
		{
			if (m_ThrowOnDetach)
			{
				m_ThrowAssist = args.interactorObject.transform.GetComponentInParent<IXRAimAssist>();
			}
			Drop();
			if (m_DropTransformersCount > 0)
			{
				DropEventArgs v;
				using (s_DropEventArgs.Get(out v))
				{
					v.selectExitEventArgs = args;
					InvokeGrabTransformersOnDrop(v);
				}
			}
		}
		else
		{
			UpdateCurrentMovementType();
		}
		m_SelectingCharacterInteractors.Remove(args.interactorObject);
		UnsubscribeTeleportationProvider(args.interactorObject);
	}

	protected override void OnSelectExited(SelectExitEventArgs args)
	{
		base.OnSelectExited(args);
		ReleaseDynamicAttachTransform(args.interactorObject);
	}

	private Transform CreateDynamicAttachTransform(IXRSelectInteractor interactor)
	{
		Transform transform;
		do
		{
			transform = s_DynamicAttachTransformPool.Get();
		}
		while (transform == null);
		transform.SetParent(m_Transform, worldPositionStays: false);
		return transform;
	}

	private Transform CreateVisualAttachTransform(IXRSelectInteractor interactor)
	{
		Transform transform;
		do
		{
			transform = s_DynamicAttachTransformPool.Get();
		}
		while (transform == null);
		transform.SetParent(m_PredictedVisualsTransform, worldPositionStays: false);
		return transform;
	}

	private void InitializeDynamicAttachPoseInternal(IXRSelectInteractor interactor, Transform dynamicAttachTransform)
	{
		InitializeDynamicAttachPoseWithStatic(interactor, dynamicAttachTransform);
		InitializeDynamicAttachPose(interactor, dynamicAttachTransform);
	}

	private void InitializeDynamicAttachPoseWithStatic(IXRSelectInteractor interactor, Transform dynamicAttachTransform)
	{
		m_DynamicAttachTransforms.Remove(interactor);
		Transform transform = GetAttachTransform(interactor);
		m_DynamicAttachTransforms[interactor] = dynamicAttachTransform;
		if (transform == m_Transform)
		{
			dynamicAttachTransform.SetLocalPose(Pose.identity);
		}
		else if (transform.parent == m_Transform)
		{
			dynamicAttachTransform.SetLocalPose(transform.GetLocalPose());
		}
		else
		{
			dynamicAttachTransform.SetWorldPose(transform.GetWorldPose());
		}
	}

	private void ReleaseDynamicAttachTransform(IXRSelectInteractor interactor)
	{
		Release(m_DynamicAttachTransforms, interactor);
		Release(m_VisualAttachTransforms, interactor);
		static void Release(Dictionary<IXRSelectInteractor, Transform> transforms, IXRSelectInteractor interactor)
		{
			if (transforms.Count > 0 && transforms.TryGetValue(interactor, out var value))
			{
				if (value != null)
				{
					s_DynamicAttachTransformPool.Release(value);
				}
				transforms.Remove(interactor);
			}
		}
	}

	protected virtual bool ShouldMatchAttachPosition(IXRSelectInteractor interactor)
	{
		if (!m_MatchAttachPosition)
		{
			return false;
		}
		if (interactor is XRSocketInteractor || interactor is XRRayInteractor { useForceGrab: not false })
		{
			return false;
		}
		return true;
	}

	protected virtual bool ShouldMatchAttachRotation(IXRSelectInteractor interactor)
	{
		if (m_MatchAttachRotation)
		{
			return !(interactor is XRSocketInteractor);
		}
		return false;
	}

	protected virtual bool ShouldSnapToColliderVolume(IXRSelectInteractor interactor)
	{
		return m_SnapToColliderVolume;
	}

	protected virtual void InitializeDynamicAttachPose(IXRSelectInteractor interactor, Transform dynamicAttachTransform)
	{
		bool flag = ShouldMatchAttachPosition(interactor);
		bool flag2 = ShouldMatchAttachRotation(interactor);
		if (flag || flag2)
		{
			Pose worldPose = interactor.GetAttachTransform(this).GetWorldPose();
			if (flag && ShouldSnapToColliderVolume(interactor) && XRInteractableUtility.TryGetClosestPointOnCollider(this, worldPose.position, out var distanceInfo))
			{
				worldPose.position = distanceInfo.point;
			}
			if (flag && flag2)
			{
				dynamicAttachTransform.SetWorldPose(worldPose);
			}
			else if (flag)
			{
				dynamicAttachTransform.position = worldPose.position;
			}
			else
			{
				dynamicAttachTransform.rotation = worldPose.rotation;
			}
		}
	}

	protected virtual void Grab()
	{
		m_OriginalSceneParent = m_Transform.parent;
		m_Transform.SetParent(null);
		if (m_PredictedVisualsTransform != null)
		{
			m_InitialVisualsTransformLocalPose = m_PredictedVisualsTransform.GetLocalPose();
			m_InitialVisualsTransformLocalPoseIsIdentity = m_InitialVisualsTransformLocalPose == Pose.identity;
			m_InitialVisualsTransformLocalScale = m_PredictedVisualsTransform.localScale;
		}
		else
		{
			m_InitialVisualsTransformLocalPose = Pose.identity;
			m_InitialVisualsTransformLocalPoseIsIdentity = true;
			m_InitialVisualsTransformLocalScale = Vector3.one;
		}
		UpdateCurrentMovementType();
		SetupRigidbodyGrab(m_Rigidbody);
		m_DetachLinearVelocity = Vector3.zero;
		m_DetachAngularVelocity = Vector3.zero;
		InitializeTargetPoseAndScale(m_Transform);
	}

	protected virtual void Drop()
	{
		if (0 == 0 && m_RetainTransformParent && m_OriginalSceneParent != null)
		{
			if (!m_OriginalSceneParent.gameObject.activeInHierarchy)
			{
				Debug.LogWarning("Retain Transform Parent is set to true, and has a non-null Original Scene Parent. However, the old parent is deactivated so we are choosing not to re-parent upon dropping.", this);
			}
			else if (base.gameObject.activeInHierarchy)
			{
				m_Transform.SetParent(m_OriginalSceneParent);
			}
		}
		SetupRigidbodyDrop(m_Rigidbody);
		m_CurrentMovementType = m_MovementType;
		m_DetachInLateUpdate = true;
		EndThrowSmoothing();
		if (m_PredictedVisualsTransform != null)
		{
			m_PredictedVisualsTransform.SetLocalPose(m_InitialVisualsTransformLocalPose);
			m_PredictedVisualsTransform.localScale = m_InitialVisualsTransformLocalScale;
		}
	}

	protected virtual void Detach()
	{
		if (!m_ThrowOnDetach)
		{
			return;
		}
		if (m_Rigidbody.isKinematic)
		{
			Debug.LogWarning("Cannot throw a kinematic Rigidbody since updating the velocity and angular velocity of a kinematic Rigidbody is not supported. Disable Throw On Detach or Is Kinematic to fix this issue.", this);
			return;
		}
		if (m_ThrowAssist != null)
		{
			IXRAimAssist throwAssist = m_ThrowAssist;
			Vector3 source = m_Rigidbody.position;
			m_DetachLinearVelocity = throwAssist.GetAssistedVelocity(in source, in m_DetachLinearVelocity, m_Rigidbody.useGravity ? (0f - Physics.gravity.y) : 0f);
			m_ThrowAssist = null;
		}
		else if (m_LimitLinearVelocity)
		{
			m_DetachLinearVelocity = Vector3.ClampMagnitude(m_DetachLinearVelocity, m_MaxLinearVelocityDelta);
		}
		if (m_LimitAngularVelocity)
		{
			m_DetachAngularVelocity = Vector3.ClampMagnitude(m_DetachAngularVelocity, m_MaxAngularVelocityDelta);
		}
		m_Rigidbody.linearVelocity = m_DetachLinearVelocity;
		m_Rigidbody.angularVelocity = m_DetachAngularVelocity;
	}

	protected virtual void SetupRigidbodyGrab(Rigidbody rigidbody)
	{
		m_WasKinematic = rigidbody.isKinematic;
		m_UsedGravity = rigidbody.useGravity;
		m_InterpolationOnGrab = rigidbody.interpolation;
		m_LinearDampingOnGrab = rigidbody.linearDamping;
		m_AngularDampingOnGrab = rigidbody.angularDamping;
		rigidbody.isKinematic = m_CurrentMovementType == MovementType.Kinematic || m_CurrentMovementType == MovementType.Instantaneous;
		rigidbody.useGravity = false;
		if (isRigidbodyMovement && m_PredictedVisualsTransform != null)
		{
			rigidbody.interpolation = RigidbodyInterpolation.None;
		}
		rigidbody.linearDamping = 0f;
		rigidbody.angularDamping = 0f;
	}

	protected virtual void SetupRigidbodyDrop(Rigidbody rigidbody)
	{
		rigidbody.isKinematic = m_WasKinematic;
		rigidbody.useGravity = m_UsedGravity;
		if (m_PredictedVisualsTransform != null)
		{
			rigidbody.interpolation = m_InterpolationOnGrab;
		}
		rigidbody.linearDamping = m_LinearDampingOnGrab;
		rigidbody.angularDamping = m_AngularDampingOnGrab;
		if (!base.isSelected)
		{
			m_Rigidbody.useGravity |= m_ForceGravityOnDetach;
		}
	}

	private void ResetThrowSmoothing()
	{
		Array.Clear(m_ThrowSmoothingFrameTimes, 0, m_ThrowSmoothingFrameTimes.Length);
		Array.Clear(m_ThrowSmoothingLinearVelocityFrames, 0, m_ThrowSmoothingLinearVelocityFrames.Length);
		Array.Clear(m_ThrowSmoothingAngularVelocityFrames, 0, m_ThrowSmoothingAngularVelocityFrames.Length);
		m_ThrowSmoothingCurrentFrame = 0;
		m_ThrowSmoothingFirstUpdate = true;
	}

	private void EndThrowSmoothing()
	{
		if (m_ThrowOnDetach)
		{
			Vector3 smoothedVelocityValue = GetSmoothedVelocityValue(m_ThrowSmoothingLinearVelocityFrames);
			Vector3 smoothedVelocityValue2 = GetSmoothedVelocityValue(m_ThrowSmoothingAngularVelocityFrames);
			m_DetachLinearVelocity = smoothedVelocityValue * m_ThrowVelocityScale;
			m_DetachAngularVelocity = smoothedVelocityValue2 * m_ThrowAngularVelocityScale;
		}
	}

	private void StepThrowSmoothing(Pose targetPose, float deltaTime)
	{
		if (!(deltaTime < 0.001f))
		{
			if (m_ThrowSmoothingFirstUpdate)
			{
				m_ThrowSmoothingFirstUpdate = false;
			}
			else
			{
				m_ThrowSmoothingLinearVelocityFrames[m_ThrowSmoothingCurrentFrame] = (targetPose.position - m_LastThrowReferencePose.position) / deltaTime;
				Vector3 eulerAngles = (targetPose.rotation * Quaternion.Inverse(m_LastThrowReferencePose.rotation)).eulerAngles;
				Vector3 vector = new Vector3(Mathf.DeltaAngle(0f, eulerAngles.x), Mathf.DeltaAngle(0f, eulerAngles.y), Mathf.DeltaAngle(0f, eulerAngles.z));
				m_ThrowSmoothingAngularVelocityFrames[m_ThrowSmoothingCurrentFrame] = vector / deltaTime * (MathF.PI / 180f);
			}
			m_ThrowSmoothingFrameTimes[m_ThrowSmoothingCurrentFrame] = Time.time;
			m_ThrowSmoothingCurrentFrame = (m_ThrowSmoothingCurrentFrame + 1) % 20;
			m_LastThrowReferencePose = targetPose;
		}
	}

	private Vector3 GetSmoothedVelocityValue(Vector3[] velocityFrames)
	{
		Vector3 zero = Vector3.zero;
		float num = 0f;
		for (int i = 0; i < 20; i++)
		{
			int num2 = ((m_ThrowSmoothingCurrentFrame - i - 1) % 20 + 20) % 20;
			if (m_ThrowSmoothingFrameTimes[num2] == 0f)
			{
				break;
			}
			float num3 = (Time.time - m_ThrowSmoothingFrameTimes[num2]) / m_ThrowSmoothingDuration;
			float num4 = m_ThrowSmoothingCurve.Evaluate(Mathf.Clamp(1f - num3, 0f, 1f));
			zero += velocityFrames[num2] * num4;
			num += num4;
			if (Time.time - m_ThrowSmoothingFrameTimes[num2] > m_ThrowSmoothingDuration)
			{
				break;
			}
		}
		if (num > 0f)
		{
			return zero / num;
		}
		return Vector3.zero;
	}

	private void SubscribeTeleportationProvider(IXRInteractor interactor)
	{
		m_TeleportationMonitor.AddInteractor(interactor);
	}

	private void UnsubscribeTeleportationProvider(IXRInteractor interactor)
	{
		m_TeleportationMonitor.RemoveInteractor(interactor);
	}

	private void OnTeleported(Pose beforePose, Pose afterPose, Pose deltaPose)
	{
		Quaternion rotation = deltaPose.rotation;
		for (int i = 0; i < 20 && m_ThrowSmoothingFrameTimes[i] != 0f; i++)
		{
			m_ThrowSmoothingLinearVelocityFrames[i] = rotation * m_ThrowSmoothingLinearVelocityFrames[i];
		}
		Vector3 vector = m_LastThrowReferencePose.position - beforePose.position;
		Vector3 vector2 = rotation * vector;
		m_LastThrowReferencePose.position = afterPose.position + vector2;
		m_LastThrowReferencePose.rotation = rotation * m_LastThrowReferencePose.rotation;
	}

	private void StartIgnoringCharacterCollision(Collider characterCollider)
	{
		m_IgnoringCharacterCollision = true;
		m_CollidersThatAllowedCharacterCollision.Clear();
		for (int i = 0; i < m_RigidbodyColliders.Count; i++)
		{
			Collider collider = m_RigidbodyColliders[i];
			if (!(collider == null) && !collider.isTrigger && !Physics.GetIgnoreCollision(collider, characterCollider))
			{
				m_CollidersThatAllowedCharacterCollision.Add(collider);
				Physics.IgnoreCollision(collider, characterCollider, ignore: true);
			}
		}
	}

	private bool IsOutsideCharacterCollider(Collider characterCollider)
	{
		Bounds bounds = characterCollider.bounds;
		foreach (Collider item in m_CollidersThatAllowedCharacterCollision)
		{
			if (!(item == null) && item.bounds.Intersects(bounds))
			{
				return false;
			}
		}
		return true;
	}

	private void StopIgnoringCharacterCollision(Collider characterCollider)
	{
		m_IgnoringCharacterCollision = false;
		foreach (Collider item in m_CollidersThatAllowedCharacterCollision)
		{
			if (item != null)
			{
				Physics.IgnoreCollision(item, characterCollider, ignore: false);
			}
		}
	}

	private static Transform OnCreatePooledItem()
	{
		Transform obj = new GameObject().transform;
		obj.SetLocalPose(Pose.identity);
		obj.localScale = Vector3.one;
		return obj;
	}

	private static void OnGetPooledItem(Transform item)
	{
		if (!(item == null))
		{
			item.hideFlags &= ~HideFlags.HideInHierarchy;
		}
	}

	private static void OnReleasePooledItem(Transform item)
	{
		if (!(item == null))
		{
			item.hideFlags |= HideFlags.HideInHierarchy;
		}
	}

	private static void OnDestroyPooledItem(Transform item)
	{
		if (!(item == null))
		{
			Object.Destroy(item.gameObject);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void EaseAttachBurst_0024BurstManaged(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime, float attachEaseInTime, ref float currentAttachEaseTime)
	{
		float t = currentAttachEaseTime / attachEaseInTime;
		targetPose.position = math.lerp(targetPose.position, rawTargetPose.position, t);
		targetPose.rotation = math.slerp(targetPose.rotation, rawTargetPose.rotation, t);
		targetLocalScale = math.lerp(targetLocalScale, rawTargetLocalScale, t);
		currentAttachEaseTime += deltaTime;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void StepSmoothingBurst_0024BurstManaged(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime, bool smoothPos, float smoothPosAmount, float tightenPos, bool smoothRot, float smoothRotAmount, float tightenRot, bool smoothScale, float smoothScaleAmount, float tightenScale)
	{
		if (smoothPos)
		{
			targetPose.position = math.lerp(targetPose.position, rawTargetPose.position, smoothPosAmount * deltaTime);
			targetPose.position = math.lerp(targetPose.position, rawTargetPose.position, tightenPos);
		}
		else
		{
			targetPose.position = rawTargetPose.position;
		}
		if (smoothRot)
		{
			targetPose.rotation = math.slerp(targetPose.rotation, rawTargetPose.rotation, smoothRotAmount * deltaTime);
			targetPose.rotation = math.slerp(targetPose.rotation, rawTargetPose.rotation, tightenRot);
		}
		else
		{
			targetPose.rotation = rawTargetPose.rotation;
		}
		if (smoothScale)
		{
			targetLocalScale = math.lerp(targetLocalScale, rawTargetLocalScale, smoothScaleAmount * deltaTime);
			targetLocalScale = math.lerp(targetLocalScale, rawTargetLocalScale, tightenScale);
		}
		else
		{
			targetLocalScale = rawTargetLocalScale;
		}
	}
}
