using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Attachment;

[BurstCompile]
[DisallowMultipleComponent]
[AddComponentMenu("XR/Interactors/Interaction Attach Controller", 22)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Attachment.InteractionAttachController.html")]
public class InteractionAttachController : MonoBehaviour, IInteractionAttachController
{
	public enum ManipulationXAxisMode
	{
		None,
		HorizontalRotation
	}

	public enum ManipulationYAxisMode
	{
		None,
		VerticalRotation,
		Translate
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ComputeAmplifiedOffset_00001157_0024PostfixBurstDelegate(in float3 velocityLocal, in float3 startLocalOffsetNormalized, float startLocalOffsetLength, in float3 targetLocalOffsetNormalized, in float3 currentLocalOffset, float minAdditionalVelocityScalar, float maxAdditionalVelocityScalar, float pushVelocityBias, float pullVelocityBias, float zVelocityRampThreshold, bool calculateMomentum, bool applyMomentum, float momentumDecayScale, ref float momentum, ref float pivot, float deltaTime, out float3 newOffset);

	internal static class ComputeAmplifiedOffset_00001157_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeAmplifiedOffset_00001157_0024PostfixBurstDelegate>(ComputeAmplifiedOffset).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 velocityLocal, in float3 startLocalOffsetNormalized, float startLocalOffsetLength, in float3 targetLocalOffsetNormalized, in float3 currentLocalOffset, float minAdditionalVelocityScalar, float maxAdditionalVelocityScalar, float pushVelocityBias, float pullVelocityBias, float zVelocityRampThreshold, bool calculateMomentum, bool applyMomentum, float momentumDecayScale, ref float momentum, ref float pivot, float deltaTime, out float3 newOffset)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, ref float3, ref float3, float, float, float, float, float, bool, bool, float, ref float, ref float, float, ref float3, void>)functionPointer)(ref velocityLocal, ref startLocalOffsetNormalized, startLocalOffsetLength, ref targetLocalOffsetNormalized, ref currentLocalOffset, minAdditionalVelocityScalar, maxAdditionalVelocityScalar, pushVelocityBias, pullVelocityBias, zVelocityRampThreshold, calculateMomentum, applyMomentum, momentumDecayScale, ref momentum, ref pivot, deltaTime, ref newOffset);
					return;
				}
			}
			ComputeAmplifiedOffset_0024BurstManaged(in velocityLocal, in startLocalOffsetNormalized, startLocalOffsetLength, in targetLocalOffsetNormalized, in currentLocalOffset, minAdditionalVelocityScalar, maxAdditionalVelocityScalar, pushVelocityBias, pullVelocityBias, zVelocityRampThreshold, calculateMomentum, applyMomentum, momentumDecayScale, ref momentum, ref pivot, deltaTime, out newOffset);
		}
	}

	[SerializeField]
	private Transform m_TransformToFollow;

	[SerializeField]
	private MotionStabilizationMode m_MotionStabilizationMode = MotionStabilizationMode.WithPositionOffset;

	[SerializeField]
	private float m_PositionStabilization = 0.25f;

	[SerializeField]
	private float m_AngleStabilization = 20f;

	[SerializeField]
	private bool m_SmoothOffset;

	[SerializeField]
	[Range(1f, 30f)]
	private float m_SmoothingSpeed = 10f;

	[SerializeField]
	private bool m_UseDistanceBasedVelocityScaling = true;

	[SerializeField]
	private bool m_UseMomentum = true;

	[SerializeField]
	[Range(0f, 10f)]
	private float m_MomentumDecayScale = 1.25f;

	[SerializeField]
	[Range(0f, 10f)]
	private float m_MomentumDecayScaleFromInput = 5.5f;

	[SerializeField]
	[Range(0f, 5f)]
	private float m_ZVelocityRampThreshold = 0.3f;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_PullVelocityBias = 1f;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_PushVelocityBias = 1.25f;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_MinAdditionalVelocityScalar = 0.05f;

	[SerializeField]
	[Range(0f, 5f)]
	private float m_MaxAdditionalVelocityScalar = 1.5f;

	[SerializeField]
	private bool m_UseManipulationInput;

	[SerializeField]
	private XRInputValueReader<Vector2> m_ManipulationInput = new XRInputValueReader<Vector2>("Manipulation");

	[SerializeField]
	private ManipulationXAxisMode m_ManipulationXAxisMode = ManipulationXAxisMode.HorizontalRotation;

	[SerializeField]
	private ManipulationYAxisMode m_ManipulationYAxisMode = ManipulationYAxisMode.Translate;

	[SerializeField]
	private bool m_CombineManipulationAxes;

	[SerializeField]
	private float m_ManipulationTranslateSpeed = 1f;

	[SerializeField]
	private float m_ManipulationRotateSpeed = 180f;

	[SerializeField]
	private Transform m_ManipulationRotateReferenceFrame;

	[SerializeField]
	private bool m_EnableDebugLines;

	private bool m_FirstMovementFrame;

	private bool m_HasOffset;

	private float m_StartLocalOffsetLength;

	private Vector3 m_StartLocalOffset;

	private Vector3 m_StartLocalOffsetNormalized;

	private Vector3 m_TargetLocalOffsetNormalized;

	private float m_Pivot;

	private float m_Momentum;

	private bool m_MomentumDecayFromInput;

	private bool m_WasVelocityScalingBlocked;

	private bool m_HasSelectInteractor;

	private IXRSelectInteractor m_SelectInteractor;

	private bool m_HasXROrigin;

	private XROrigin m_XROrigin;

	private Transform m_AnchorParent;

	private Transform m_AnchorChild;

	private Vector3 m_LastTargetLocalPosition;

	private Vector3 m_LastTargetOriginSpacePosition;

	private readonly AttachPointVelocityTracker m_VelocityTracker = new AttachPointVelocityTracker();

	public Transform transformToFollow
	{
		get
		{
			return m_TransformToFollow;
		}
		set
		{
			m_TransformToFollow = value;
		}
	}

	public MotionStabilizationMode motionStabilizationMode
	{
		get
		{
			return m_MotionStabilizationMode;
		}
		set
		{
			m_MotionStabilizationMode = value;
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

	public bool smoothOffset
	{
		get
		{
			return m_SmoothOffset;
		}
		set
		{
			m_SmoothOffset = value;
		}
	}

	public float smoothingSpeed
	{
		get
		{
			return m_SmoothingSpeed;
		}
		set
		{
			m_SmoothingSpeed = Mathf.Clamp(value, 1f, 30f);
		}
	}

	public bool useDistanceBasedVelocityScaling
	{
		get
		{
			return m_UseDistanceBasedVelocityScaling;
		}
		set
		{
			m_UseDistanceBasedVelocityScaling = value;
		}
	}

	public bool useMomentum
	{
		get
		{
			return m_UseMomentum;
		}
		set
		{
			m_UseMomentum = value;
		}
	}

	public float momentumDecayScale
	{
		get
		{
			return m_MomentumDecayScale;
		}
		set
		{
			m_MomentumDecayScale = Mathf.Clamp(value, 0f, 10f);
		}
	}

	public float momentumDecayScaleFromInput
	{
		get
		{
			return m_MomentumDecayScaleFromInput;
		}
		set
		{
			m_MomentumDecayScaleFromInput = Mathf.Clamp(value, 0f, 10f);
		}
	}

	public float zVelocityRampThreshold
	{
		get
		{
			return m_ZVelocityRampThreshold;
		}
		set
		{
			m_ZVelocityRampThreshold = Mathf.Clamp(value, 0f, 5f);
		}
	}

	public float pullVelocityBias
	{
		get
		{
			return m_PullVelocityBias;
		}
		set
		{
			m_PullVelocityBias = Mathf.Clamp(value, 0f, 2f);
		}
	}

	public float pushVelocityBias
	{
		get
		{
			return m_PushVelocityBias;
		}
		set
		{
			m_PushVelocityBias = Mathf.Clamp(value, 0f, 2f);
		}
	}

	public float minAdditionalVelocityScalar
	{
		get
		{
			return m_MinAdditionalVelocityScalar;
		}
		set
		{
			m_MinAdditionalVelocityScalar = Mathf.Clamp(value, 0f, 2f);
		}
	}

	public float maxAdditionalVelocityScalar
	{
		get
		{
			return m_MaxAdditionalVelocityScalar;
		}
		set
		{
			m_MaxAdditionalVelocityScalar = Mathf.Clamp(value, 0f, 5f);
		}
	}

	public bool useManipulationInput
	{
		get
		{
			return m_UseManipulationInput;
		}
		set
		{
			m_UseManipulationInput = value;
		}
	}

	public XRInputValueReader<Vector2> manipulationInput
	{
		get
		{
			return m_ManipulationInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ManipulationInput, value, this);
		}
	}

	public ManipulationXAxisMode manipulationXAxisMode
	{
		get
		{
			return m_ManipulationXAxisMode;
		}
		set
		{
			m_ManipulationXAxisMode = value;
		}
	}

	public ManipulationYAxisMode manipulationYAxisMode
	{
		get
		{
			return m_ManipulationYAxisMode;
		}
		set
		{
			m_ManipulationYAxisMode = value;
		}
	}

	public bool combineManipulationAxes
	{
		get
		{
			return m_CombineManipulationAxes;
		}
		set
		{
			m_CombineManipulationAxes = value;
		}
	}

	public float manipulationTranslateSpeed
	{
		get
		{
			return m_ManipulationTranslateSpeed;
		}
		set
		{
			m_ManipulationTranslateSpeed = value;
		}
	}

	public float manipulationRotateSpeed
	{
		get
		{
			return m_ManipulationRotateSpeed;
		}
		set
		{
			m_ManipulationRotateSpeed = value;
		}
	}

	public Transform manipulationRotateReferenceFrame
	{
		get
		{
			return m_ManipulationRotateReferenceFrame;
		}
		set
		{
			m_ManipulationRotateReferenceFrame = value;
		}
	}

	public bool enableDebugLines
	{
		get
		{
			return m_EnableDebugLines;
		}
		set
		{
			m_EnableDebugLines = value;
		}
	}

	public bool hasOffset => m_HasOffset;

	public event Action attachUpdated;

	private Transform GetXROriginTransform()
	{
		if (!InitializeXROrigin())
		{
			return null;
		}
		return m_XROrigin.Origin.transform;
	}

	private bool InitializeXROrigin()
	{
		if (m_XROrigin == null)
		{
			ComponentLocatorUtility<XROrigin>.TryFindComponent(out m_XROrigin);
		}
		m_HasXROrigin = m_XROrigin != null;
		return m_HasXROrigin;
	}

	protected virtual void OnValidate()
	{
		float num = Mathf.Min(m_MinAdditionalVelocityScalar, m_MaxAdditionalVelocityScalar);
		float num2 = Mathf.Max(m_MinAdditionalVelocityScalar, m_MaxAdditionalVelocityScalar);
		m_MinAdditionalVelocityScalar = num;
		m_MaxAdditionalVelocityScalar = num2;
		if (m_TransformToFollow == null)
		{
			m_TransformToFollow = base.transform;
		}
	}

	protected virtual void Awake()
	{
		if (m_TransformToFollow == null)
		{
			m_TransformToFollow = base.transform;
		}
	}

	protected virtual void OnEnable()
	{
		if (!InitializeXROrigin() && m_UseDistanceBasedVelocityScaling)
		{
			Debug.LogWarning($"Missing XR Origin. Disabling distance-based velocity scaling on this {this}.", this);
			m_UseDistanceBasedVelocityScaling = false;
		}
		m_HasSelectInteractor = TryGetComponent<IXRSelectInteractor>(out m_SelectInteractor);
		if (m_AnchorParent != null)
		{
			m_AnchorParent.gameObject.SetActive(value: true);
		}
		m_ManipulationInput.EnableDirectActionIfModeUsed();
	}

	protected virtual void OnDisable()
	{
		if (m_AnchorParent != null)
		{
			m_AnchorParent.gameObject.SetActive(value: false);
		}
		m_ManipulationInput.DisableDirectActionIfModeUsed();
	}

	private void SyncAnchorParent()
	{
		if (m_TransformToFollow == null)
		{
			m_TransformToFollow = base.transform;
		}
		m_AnchorParent.SetWorldPose(m_TransformToFollow.GetWorldPose());
	}

	Transform IInteractionAttachController.GetOrCreateAnchorTransform(bool updateTransform)
	{
		if (m_AnchorParent == null)
		{
			Transform xROriginTransform = GetXROriginTransform();
			string text = GetType().Name;
			string text2 = "";
			if (TryGetComponent<IXRInteractor>(out var component))
			{
				text2 = component.handedness.ToString();
			}
			m_AnchorParent = new GameObject("[" + text2 + " " + text + "] Attach").transform;
			m_AnchorParent.SetParent(xROriginTransform, worldPositionStays: false);
			m_AnchorParent.SetLocalPose(Pose.identity);
			if (m_AnchorChild == null)
			{
				m_AnchorChild = new GameObject("[" + text2 + " " + text + "] Attach Child").transform;
				m_AnchorChild.SetParent(m_AnchorParent, worldPositionStays: false);
				m_AnchorChild.SetLocalPose(Pose.identity);
			}
		}
		if (updateTransform)
		{
			SyncAnchorParent();
		}
		return m_AnchorChild;
	}

	void IInteractionAttachController.MoveTo(Vector3 targetWorldPosition)
	{
		SyncAnchorParent();
		MoveToPosition(targetWorldPosition);
	}

	private void SyncOffset()
	{
		MoveToPosition(m_AnchorChild.position);
	}

	private void MoveToPosition(Vector3 targetWorldPosition)
	{
		m_AnchorChild.position = targetWorldPosition;
		Vector3 direction = targetWorldPosition - m_AnchorParent.position;
		m_StartLocalOffset = m_AnchorParent.InverseTransformDirection(direction);
		m_StartLocalOffsetLength = m_StartLocalOffset.magnitude;
		m_StartLocalOffsetNormalized = ((m_StartLocalOffsetLength > 1E-05f) ? (m_StartLocalOffset / m_StartLocalOffsetLength) : Vector3.zero);
		m_TargetLocalOffsetNormalized = m_StartLocalOffsetNormalized;
		m_LastTargetLocalPosition = m_AnchorChild.localPosition;
		if (m_HasXROrigin)
		{
			m_LastTargetOriginSpacePosition = m_XROrigin.Origin.transform.InverseTransformPoint(m_AnchorChild.position);
		}
		m_Pivot = m_StartLocalOffsetLength;
		m_HasOffset = m_StartLocalOffsetLength > 0f;
		m_Momentum = 0f;
		m_FirstMovementFrame = true;
		m_WasVelocityScalingBlocked = false;
		m_VelocityTracker.ResetVelocityTracking();
	}

	void IInteractionAttachController.ApplyLocalPositionOffset(Vector3 offset)
	{
		SyncAnchorParent();
		MoveToPosition(m_AnchorChild.position + m_AnchorParent.TransformDirection(offset));
	}

	void IInteractionAttachController.ApplyLocalRotationOffset(Quaternion localRotation)
	{
		m_AnchorChild.localRotation *= localRotation;
	}

	public void ResetOffset()
	{
		m_FirstMovementFrame = true;
		m_HasOffset = false;
		m_WasVelocityScalingBlocked = false;
		m_Momentum = 0f;
		m_AnchorChild.SetLocalPose(Pose.identity);
		SyncAnchorParent();
	}

	void IInteractionAttachController.DoUpdate(float deltaTime)
	{
		if (!m_HasXROrigin)
		{
			return;
		}
		Transform transform = m_XROrigin.Origin.transform;
		Vector3 up = transform.up;
		if (m_MotionStabilizationMode == MotionStabilizationMode.Never || (m_MotionStabilizationMode == MotionStabilizationMode.WithPositionOffset && !m_HasOffset))
		{
			SyncAnchorParent();
		}
		else if (!m_HasOffset)
		{
			XRTransformStabilizer.ApplyStabilization(ref m_AnchorParent, in m_TransformToFollow, m_PositionStabilization, m_AngleStabilization, deltaTime);
		}
		else
		{
			float z = m_AnchorChild.localPosition.z;
			float num = 1f + z;
			float num2 = num * m_PositionStabilization;
			float num3 = num * m_AngleStabilization;
			Vector3 position = m_AnchorParent.position;
			Vector3 vector = m_AnchorChild.position - position;
			Vector3 vector2 = ((Vector3.Angle(vector.normalized, up) > 45f) ? Vector3.ProjectOnPlane(vector, up) : vector);
			Vector3 vector3 = position + vector2;
			ref Transform anchorParent = ref m_AnchorParent;
			ref Transform target = ref m_TransformToFollow;
			float3 targetEndpoint = vector3;
			XRTransformStabilizer.ApplyStabilization(ref anchorParent, in target, in targetEndpoint, num2, num3, deltaTime);
		}
		if (!m_HasOffset)
		{
			this.attachUpdated?.Invoke();
			return;
		}
		if (m_UseDistanceBasedVelocityScaling)
		{
			m_VelocityTracker.UpdateAttachPointVelocityData(m_TransformToFollow, transform);
		}
		if ((m_UseDistanceBasedVelocityScaling || m_UseManipulationInput) && !UpdateVelocityScalingBlock())
		{
			DoPositionUpdate(deltaTime);
		}
		else
		{
			UpdatePosition(m_StartLocalOffset, deltaTime);
		}
		bool flag = m_ManipulationYAxisMode == ManipulationYAxisMode.VerticalRotation;
		bool flag2 = m_ManipulationXAxisMode == ManipulationXAxisMode.HorizontalRotation;
		if (m_UseManipulationInput && (flag || flag2) && m_ManipulationInput.TryReadValue(out var value))
		{
			value = FilterManipulationInput(in value);
			float num4 = m_ManipulationRotateSpeed * deltaTime;
			float x = (flag ? (value.y * num4) : 0f);
			float y = (flag2 ? (value.x * num4) : 0f);
			Quaternion quaternion = ((m_ManipulationRotateReferenceFrame != null) ? m_ManipulationRotateReferenceFrame.rotation : m_AnchorParent.rotation);
			m_AnchorChild.rotation = quaternion * Quaternion.Euler(x, y, 0f) * Quaternion.Inverse(quaternion) * m_AnchorChild.rotation;
		}
		this.attachUpdated?.Invoke();
	}

	private void DoPositionUpdate(float deltaTime)
	{
		float3 currentLocalOffset = (m_SmoothOffset ? m_LastTargetLocalPosition : m_AnchorChild.localPosition);
		Vector3 zero = Vector3.zero;
		float3 velocityLocal;
		if (m_FirstMovementFrame)
		{
			velocityLocal = float3.zero;
			m_FirstMovementFrame = false;
		}
		else if (m_UseDistanceBasedVelocityScaling)
		{
			Transform xrOriginTransform = m_XROrigin.Origin.transform;
			zero = m_VelocityTracker.GetAttachPointVelocity(xrOriginTransform);
			velocityLocal = m_AnchorParent.InverseTransformDirection(zero);
		}
		else
		{
			velocityLocal = float3.zero;
		}
		bool applyMomentum = m_UseMomentum;
		if (m_UseManipulationInput && m_ManipulationYAxisMode == ManipulationYAxisMode.Translate && m_ManipulationInput.TryReadValue(out var value))
		{
			float num = FilterManipulationInput(in value).y * m_ManipulationTranslateSpeed;
			velocityLocal += new float3(m_TargetLocalOffsetNormalized.x, m_TargetLocalOffsetNormalized.y, m_TargetLocalOffsetNormalized.z) * num;
			applyMomentum = false;
			m_MomentumDecayFromInput = true;
		}
		float num2 = (m_MomentumDecayFromInput ? m_MomentumDecayScaleFromInput : m_MomentumDecayScale);
		float3 startLocalOffsetNormalized = m_StartLocalOffsetNormalized;
		float startLocalOffsetLength = m_StartLocalOffsetLength;
		float3 targetLocalOffsetNormalized = m_TargetLocalOffsetNormalized;
		ComputeAmplifiedOffset(in velocityLocal, in startLocalOffsetNormalized, startLocalOffsetLength, in targetLocalOffsetNormalized, in currentLocalOffset, m_MinAdditionalVelocityScalar, m_MaxAdditionalVelocityScalar, m_PushVelocityBias, m_PullVelocityBias, m_ZVelocityRampThreshold, m_UseMomentum, applyMomentum, num2, ref m_Momentum, ref m_Pivot, deltaTime, out var newOffset);
		if (math.abs(m_Momentum) < 0.001f)
		{
			m_MomentumDecayFromInput = false;
		}
		if (math.dot(math.normalize(newOffset), m_StartLocalOffsetNormalized) < 0.05f)
		{
			ResetOffset();
		}
		else
		{
			UpdatePosition(newOffset, deltaTime);
		}
	}

	private bool UpdateVelocityScalingBlock()
	{
		if (!m_HasSelectInteractor)
		{
			return false;
		}
		bool flag = false;
		if (m_SelectInteractor.hasSelection)
		{
			IXRSelectInteractable iXRSelectInteractable = m_SelectInteractor.interactablesSelected[0];
			if (iXRSelectInteractable != null && iXRSelectInteractable.interactorsSelecting.Count > 1)
			{
				flag = true;
			}
		}
		if (flag && !m_WasVelocityScalingBlocked)
		{
			SyncOffset();
		}
		m_WasVelocityScalingBlocked = flag;
		return flag;
	}

	private void UpdatePosition(Vector3 targetLocalPosition, float deltaTime)
	{
		if (!m_SmoothOffset || !m_HasXROrigin)
		{
			m_AnchorChild.localPosition = targetLocalPosition;
			m_LastTargetLocalPosition = targetLocalPosition;
			return;
		}
		Transform transform = m_XROrigin.Origin.transform;
		Vector3 start = transform.TransformPoint(m_LastTargetOriginSpacePosition);
		Vector3 end = m_AnchorParent.TransformPoint(targetLocalPosition);
		Vector3 position = BurstLerpUtility.BezierLerp(in start, in end, m_SmoothingSpeed * deltaTime);
		m_AnchorChild.position = position;
		m_LastTargetOriginSpacePosition = transform.InverseTransformPoint(position);
		m_LastTargetLocalPosition = targetLocalPosition;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EAttachment_002EComputeAmplifiedOffset_00001157_0024PostfixBurstDelegate))]
	private static void ComputeAmplifiedOffset(in float3 velocityLocal, in float3 startLocalOffsetNormalized, float startLocalOffsetLength, in float3 targetLocalOffsetNormalized, in float3 currentLocalOffset, float minAdditionalVelocityScalar, float maxAdditionalVelocityScalar, float pushVelocityBias, float pullVelocityBias, float zVelocityRampThreshold, bool calculateMomentum, bool applyMomentum, float momentumDecayScale, ref float momentum, ref float pivot, float deltaTime, out float3 newOffset)
	{
		ComputeAmplifiedOffset_00001157_0024BurstDirectCall.Invoke(in velocityLocal, in startLocalOffsetNormalized, startLocalOffsetLength, in targetLocalOffsetNormalized, in currentLocalOffset, minAdditionalVelocityScalar, maxAdditionalVelocityScalar, pushVelocityBias, pullVelocityBias, zVelocityRampThreshold, calculateMomentum, applyMomentum, momentumDecayScale, ref momentum, ref pivot, deltaTime, out newOffset);
	}

	private Vector2 FilterManipulationInput(in Vector2 input)
	{
		if (m_CombineManipulationAxes || m_ManipulationXAxisMode == ManipulationXAxisMode.None || m_ManipulationYAxisMode == ManipulationYAxisMode.None)
		{
			return input;
		}
		if (!(Mathf.Abs(input.y) >= Mathf.Abs(input.x)))
		{
			return new Vector2(input.x, 0f);
		}
		return new Vector2(0f, input.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ComputeAmplifiedOffset_0024BurstManaged(in float3 velocityLocal, in float3 startLocalOffsetNormalized, float startLocalOffsetLength, in float3 targetLocalOffsetNormalized, in float3 currentLocalOffset, float minAdditionalVelocityScalar, float maxAdditionalVelocityScalar, float pushVelocityBias, float pullVelocityBias, float zVelocityRampThreshold, bool calculateMomentum, bool applyMomentum, float momentumDecayScale, ref float momentum, ref float pivot, float deltaTime, out float3 newOffset)
	{
		float3 x = ((!(math.abs(math.dot(math.normalize(velocityLocal), targetLocalOffsetNormalized)) > 0.866f)) ? float3.zero : math.project(velocityLocal, targetLocalOffsetNormalized));
		float start = minAdditionalVelocityScalar * pivot;
		float end = maxAdditionalVelocityScalar * pivot;
		float num = math.length(currentLocalOffset);
		float num2 = math.sign(math.dot(math.normalize(x), startLocalOffsetNormalized));
		bool flag = num2 > 0f;
		float num3 = math.length(x) * num2;
		float t = math.clamp(math.abs(num) / pivot * (flag ? pushVelocityBias : pullVelocityBias), 0f, 1f);
		float num4 = BurstLerpUtility.BezierLerp(start, end, t);
		float num5 = ((zVelocityRampThreshold > 0f) ? math.clamp(math.abs(num3) / zVelocityRampThreshold, 0f, 1f) : 1f);
		float num6 = num3 * num5 * (1f + num4) * deltaTime;
		if (calculateMomentum)
		{
			float num7 = math.abs(momentum);
			float num8 = math.abs(num6);
			bool flag2 = num5 >= 1f;
			if ((int)math.sign(momentum) != (int)math.sign(num6) && math.abs(num7 - num8) > 0.001f)
			{
				if (flag2)
				{
					momentum = num6 * 0.5f;
				}
				else if (num5 > 0.25f)
				{
					momentum = 0f;
				}
			}
			else if (flag2)
			{
				momentum = math.max(num7, num8 / 2f) * math.sign(num6);
			}
			if (math.abs(momentum) < 0.001f)
			{
				momentum = 0f;
			}
			else
			{
				momentum *= 1f - momentumDecayScale * deltaTime;
			}
		}
		else
		{
			momentum = 0f;
		}
		float num9 = num + num6;
		if (applyMomentum)
		{
			num9 += momentum;
		}
		newOffset = startLocalOffsetNormalized * num9;
		if (num9 > startLocalOffsetLength)
		{
			pivot = num9;
		}
		else
		{
			pivot = math.lerp(pivot, (startLocalOffsetLength + num9) / 2f, deltaTime * num4);
		}
	}
}
