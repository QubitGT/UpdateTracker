using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers;

[AddComponentMenu("XR/Transformers/XR General Grab Transformer", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRGeneralGrabTransformer.html")]
[BurstCompile]
public class XRGeneralGrabTransformer : XRBaseGrabTransformer
{
	[Flags]
	public enum ManipulationAxes
	{
		X = 1,
		Y = 2,
		Z = 4,
		All = 7
	}

	public enum ConstrainedAxisDisplacementMode
	{
		ObjectRelative,
		ObjectRelativeWithLockedWorldUp,
		WorldAxisRelative
	}

	public enum TwoHandedRotationMode
	{
		FirstHandOnly,
		FirstHandDirectedTowardsSecondHand,
		TwoHandedAverage
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ComputeNewObjectPosition_00000905_0024PostfixBurstDelegate(in float3 interactorPosition, in quaternion interactorRotation, in quaternion objectRotation, in float3 objectScale, bool trackRotation, in float3 offsetPosition, in float3 objectLocalGrabPoint, in float3 interactorLocalGrabPoint, out Vector3 newPosition);

	internal static class ComputeNewObjectPosition_00000905_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeNewObjectPosition_00000905_0024PostfixBurstDelegate>(ComputeNewObjectPosition).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 interactorPosition, in quaternion interactorRotation, in quaternion objectRotation, in float3 objectScale, bool trackRotation, in float3 offsetPosition, in float3 objectLocalGrabPoint, in float3 interactorLocalGrabPoint, out Vector3 newPosition)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref quaternion, ref quaternion, ref float3, bool, ref float3, ref float3, ref float3, ref Vector3, void>)functionPointer)(ref interactorPosition, ref interactorRotation, ref objectRotation, ref objectScale, trackRotation, ref offsetPosition, ref objectLocalGrabPoint, ref interactorLocalGrabPoint, ref newPosition);
					return;
				}
			}
			ComputeNewObjectPosition_0024BurstManaged(in interactorPosition, in interactorRotation, in objectRotation, in objectScale, trackRotation, in offsetPosition, in objectLocalGrabPoint, in interactorLocalGrabPoint, out newPosition);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void AdjustPositionForPermittedAxesBurst_00000909_0024PostfixBurstDelegate(in Vector3 targetPosition, in Pose originalObjectPose, ConstrainedAxisDisplacementMode axisDisplacementMode, bool hasX, bool hasY, bool hasZ, out Vector3 adjustedTargetPosition);

	internal static class AdjustPositionForPermittedAxesBurst_00000909_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<AdjustPositionForPermittedAxesBurst_00000909_0024PostfixBurstDelegate>(AdjustPositionForPermittedAxesBurst).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 targetPosition, in Pose originalObjectPose, ConstrainedAxisDisplacementMode axisDisplacementMode, bool hasX, bool hasY, bool hasZ, out Vector3 adjustedTargetPosition)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Pose, ConstrainedAxisDisplacementMode, bool, bool, bool, ref Vector3, void>)functionPointer)(ref targetPosition, ref originalObjectPose, axisDisplacementMode, hasX, hasY, hasZ, ref adjustedTargetPosition);
					return;
				}
			}
			AdjustPositionForPermittedAxesBurst_0024BurstManaged(in targetPosition, in originalObjectPose, axisDisplacementMode, hasX, hasY, hasZ, out adjustedTargetPosition);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ComputeNewOneHandedScale_0000090B_0024PostfixBurstDelegate(in Vector3 currentScale, in Vector3 initialScaleProportions, bool clampScale, in Vector3 minScale, in Vector3 maxScale, float scaleInput, float deltaTime, float scaleSpeed, out Vector3 newScale);

	internal static class ComputeNewOneHandedScale_0000090B_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeNewOneHandedScale_0000090B_0024PostfixBurstDelegate>(ComputeNewOneHandedScale).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 currentScale, in Vector3 initialScaleProportions, bool clampScale, in Vector3 minScale, in Vector3 maxScale, float scaleInput, float deltaTime, float scaleSpeed, out Vector3 newScale)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, bool, ref Vector3, ref Vector3, float, float, float, ref Vector3, void>)functionPointer)(ref currentScale, ref initialScaleProportions, clampScale, ref minScale, ref maxScale, scaleInput, deltaTime, scaleSpeed, ref newScale);
					return;
				}
			}
			ComputeNewOneHandedScale_0024BurstManaged(in currentScale, in initialScaleProportions, clampScale, in minScale, in maxScale, scaleInput, deltaTime, scaleSpeed, out newScale);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ComputeNewTwoHandedScale_0000090C_0024PostfixBurstDelegate(in Vector3 startScale, in Vector3 currentScale, in Vector3 startHandleBar, in Vector3 newHandleBar, bool clampScale, float scaleMultiplier, float thresholdMoveRatioForScale, in Vector3 minScale, in Vector3 maxScale, out Vector3 newScale);

	internal static class ComputeNewTwoHandedScale_0000090C_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeNewTwoHandedScale_0000090C_0024PostfixBurstDelegate>(ComputeNewTwoHandedScale).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 startScale, in Vector3 currentScale, in Vector3 startHandleBar, in Vector3 newHandleBar, bool clampScale, float scaleMultiplier, float thresholdMoveRatioForScale, in Vector3 minScale, in Vector3 maxScale, out Vector3 newScale)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Vector3, ref Vector3, bool, float, float, ref Vector3, ref Vector3, ref Vector3, void>)functionPointer)(ref startScale, ref currentScale, ref startHandleBar, ref newHandleBar, clampScale, scaleMultiplier, thresholdMoveRatioForScale, ref minScale, ref maxScale, ref newScale);
					return;
				}
			}
			ComputeNewTwoHandedScale_0024BurstManaged(in startScale, in currentScale, in startHandleBar, in newHandleBar, clampScale, scaleMultiplier, thresholdMoveRatioForScale, in minScale, in maxScale, out newScale);
		}
	}

	[Header("Translation Constraints")]
	[SerializeField]
	[Tooltip("Permitted axes for translation displacement relative to the object's initial rotation.")]
	private ManipulationAxes m_PermittedDisplacementAxes = ManipulationAxes.All;

	[SerializeField]
	[Tooltip("Determines how the constrained axis displacement mode is computed.")]
	private ConstrainedAxisDisplacementMode m_ConstrainedAxisDisplacementMode = ConstrainedAxisDisplacementMode.ObjectRelativeWithLockedWorldUp;

	[Header("Rotation Constraints")]
	[SerializeField]
	[Tooltip("Determines how rotation is calculated when using two hands for the grab interaction.")]
	private TwoHandedRotationMode m_TwoHandedRotationMode = TwoHandedRotationMode.FirstHandDirectedTowardsSecondHand;

	[Header("Scaling Constraints")]
	[SerializeField]
	[Tooltip("Allow one handed scaling using the scale value provider if available.")]
	private bool m_AllowOneHandedScaling = true;

	[SerializeField]
	[Tooltip("Allow scaling when using multi-grab interaction.")]
	private bool m_AllowTwoHandedScaling;

	[SerializeField]
	[Tooltip("Scaling speed over time for one handed scaling based on the scale value provider.")]
	[Range(0f, 32f)]
	private float m_OneHandedScaleSpeed = 0.5f;

	[SerializeField]
	[Tooltip("(Two Handed Scaling) Percentage as a measure of 0 to 1 of scaled relative hand displacement required to trigger scale operation.\nIf this value is 0f, scaling happens the moment both grab interactors move closer or further away from each other.\nOtherwise, this percentage is used as a threshold before any scaling happens.")]
	[Range(0f, 1f)]
	private float m_ThresholdMoveRatioForScale = 0.05f;

	[Space]
	[SerializeField]
	[Tooltip("If enabled, scaling will abide by ratio ranges defined below.")]
	private bool m_ClampScaling = true;

	[SerializeField]
	[Tooltip("Minimum scale multiplier applied to the initial scale captured on start.")]
	[Range(0.01f, 1f)]
	private float m_MinimumScaleRatio = 0.25f;

	[SerializeField]
	[Tooltip("Maximum scale multiplier applied to the initial scale captured on start.")]
	[Range(1f, 10f)]
	private float m_MaximumScaleRatio = 2f;

	[Space]
	[SerializeField]
	[Range(0.1f, 5f)]
	[Tooltip("Scales the distance of displacement between interactors needed to modify the scale interactable.")]
	private float m_ScaleMultiplier = 0.25f;

	private Pose m_OriginalObjectPose;

	private Pose m_OffsetPose;

	private Pose m_OriginalInteractorPose;

	private Vector3 m_InteractorLocalGrabPoint;

	private Vector3 m_ObjectLocalGrabPoint;

	private IXRInteractor m_OriginalInteractor;

	private int m_LastGrabCount;

	private Vector3 m_StartHandleBar;

	private Vector3 m_StartHandleBarNormalized;

	private Vector3 m_StartHandleBarUp;

	private Quaternion m_StartHandleBarLookRotation;

	private Quaternion m_InverseStartHandleBarLookRotation;

	private Quaternion m_LastHandleBarLocalRotation;

	private Vector3 m_ScaleAtGrabStart;

	private bool m_FirstFrameSinceTwoHandedGrab;

	private Vector3 m_LastTwoHandedUp;

	private Vector3 m_InitialScale;

	private Vector3 m_InitialScaleProportions;

	private Vector3 m_MinimumScale;

	private Vector3 m_MaximumScale;

	private ConstrainedAxisDisplacementMode m_ConstrainedAxisDisplacementModeOnGrab;

	private ManipulationAxes m_PermittedDisplacementAxesOnGrab;

	private IXRScaleValueProvider m_ScaleValueProvider;

	private bool m_HasScaleValueProvider;

	public ManipulationAxes permittedDisplacementAxes
	{
		get
		{
			return m_PermittedDisplacementAxes;
		}
		set
		{
			m_PermittedDisplacementAxes = value;
		}
	}

	public ConstrainedAxisDisplacementMode constrainedAxisDisplacementMode
	{
		get
		{
			return m_ConstrainedAxisDisplacementMode;
		}
		set
		{
			m_ConstrainedAxisDisplacementMode = value;
		}
	}

	public TwoHandedRotationMode allowTwoHandedRotation
	{
		get
		{
			return m_TwoHandedRotationMode;
		}
		set
		{
			m_TwoHandedRotationMode = value;
		}
	}

	public bool allowOneHandedScaling
	{
		get
		{
			return m_AllowOneHandedScaling;
		}
		set
		{
			m_AllowOneHandedScaling = value;
		}
	}

	public bool allowTwoHandedScaling
	{
		get
		{
			return m_AllowTwoHandedScaling;
		}
		set
		{
			m_AllowTwoHandedScaling = value;
		}
	}

	public float oneHandedScaleSpeed
	{
		get
		{
			return m_OneHandedScaleSpeed;
		}
		set
		{
			m_OneHandedScaleSpeed = Mathf.Max(value, 0f);
		}
	}

	public float thresholdMoveRatioForScale
	{
		get
		{
			return m_ThresholdMoveRatioForScale;
		}
		set
		{
			m_ThresholdMoveRatioForScale = value;
		}
	}

	public bool clampScaling
	{
		get
		{
			return m_ClampScaling;
		}
		set
		{
			m_ClampScaling = value;
		}
	}

	public float minimumScaleRatio
	{
		get
		{
			return m_MinimumScaleRatio;
		}
		set
		{
			m_MinimumScaleRatio = Mathf.Min(1f, value);
			m_MinimumScale = m_InitialScale * m_MinimumScaleRatio;
		}
	}

	public float maximumScaleRatio
	{
		get
		{
			return m_MaximumScaleRatio;
		}
		set
		{
			m_MaximumScaleRatio = Mathf.Max(1f, value);
			m_MaximumScale = m_InitialScale * m_MaximumScaleRatio;
		}
	}

	public float scaleMultiplier
	{
		get
		{
			return m_ScaleMultiplier;
		}
		set
		{
			m_ScaleMultiplier = value;
		}
	}

	protected override RegistrationMode registrationMode => RegistrationMode.SingleAndMultiple;

	protected void Awake()
	{
	}

	public override void OnLink(XRGrabInteractable grabInteractable)
	{
		base.OnLink(grabInteractable);
		m_InitialScale = grabInteractable.transform.localScale;
		float num = Mathf.Max(Mathf.Abs(m_InitialScale.x), Mathf.Abs(m_InitialScale.y), Mathf.Abs(m_InitialScale.z));
		m_InitialScaleProportions = m_InitialScale.SafeDivide(new Vector3(num, num, num));
	}

	public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
	{
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic || updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
		{
			UpdateTarget(grabInteractable, ref targetPose, ref localScale);
		}
	}

	public override void OnGrab(XRGrabInteractable grabInteractable)
	{
		base.OnGrab(grabInteractable);
		IXRSelectInteractor iXRSelectInteractor = grabInteractable.interactorsSelecting[0];
		Transform transform = grabInteractable.transform;
		Transform attachTransform = grabInteractable.GetAttachTransform(iXRSelectInteractor);
		m_ScaleValueProvider = iXRSelectInteractor as IXRScaleValueProvider;
		m_HasScaleValueProvider = m_ScaleValueProvider != null;
		m_OriginalObjectPose = transform.GetWorldPose();
		m_OriginalInteractorPose = iXRSelectInteractor.GetAttachTransform(grabInteractable).GetWorldPose();
		m_OriginalInteractor = iXRSelectInteractor;
		m_LastGrabCount = 1;
		Vector3 value = Vector3.zero;
		Quaternion quaternion = Quaternion.identity;
		Quaternion rotation = m_OriginalObjectPose.rotation;
		if (grabInteractable.trackRotation)
		{
			rotation = m_OriginalInteractorPose.rotation;
			quaternion = Quaternion.Inverse(Quaternion.Inverse(m_OriginalObjectPose.rotation) * attachTransform.rotation);
		}
		Vector3 targetPosition = m_OriginalObjectPose.position;
		if (grabInteractable.trackPosition)
		{
			targetPosition = m_OriginalInteractorPose.position;
			Vector3 vector = m_OriginalObjectPose.position - attachTransform.position;
			value = (grabInteractable.trackRotation ? attachTransform.InverseTransformDirection(vector) : vector);
		}
		m_ConstrainedAxisDisplacementModeOnGrab = m_ConstrainedAxisDisplacementMode;
		m_PermittedDisplacementAxesOnGrab = m_PermittedDisplacementAxes;
		targetPosition = AdjustPositionForPermittedAxes(in targetPosition, in m_OriginalObjectPose, m_PermittedDisplacementAxesOnGrab, m_ConstrainedAxisDisplacementModeOnGrab);
		m_OriginalObjectPose = new Pose(targetPosition, rotation);
		Vector3 localScale = transform.localScale;
		TranslateSetup(m_OriginalInteractorPose, m_OriginalInteractorPose.position, m_OriginalObjectPose, localScale);
		Quaternion rotation2 = quaternion * Quaternion.Inverse(m_OriginalInteractorPose.rotation) * m_OriginalObjectPose.rotation;
		Vector3 position = value.Divide(localScale);
		m_OffsetPose = new Pose(position, rotation2);
	}

	public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
	{
		base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
		int count = grabInteractable.interactorsSelecting.Count;
		if (count == 1)
		{
			if (grabInteractable.interactorsSelecting[0] != m_OriginalInteractor || count < m_LastGrabCount)
			{
				OnGrab(grabInteractable);
			}
		}
		else if (count > 1)
		{
			IXRSelectInteractor iXRSelectInteractor = grabInteractable.interactorsSelecting[0];
			IXRSelectInteractor interactor = grabInteractable.interactorsSelecting[1];
			Transform attachTransform = iXRSelectInteractor.GetAttachTransform(grabInteractable);
			Transform attachTransform2 = grabInteractable.GetAttachTransform(interactor);
			m_ScaleAtGrabStart = localScale;
			m_StartHandleBar = attachTransform.InverseTransformPoint(attachTransform2.position);
			m_StartHandleBarNormalized = m_StartHandleBar.normalized;
			m_StartHandleBarLookRotation = Quaternion.LookRotation(m_StartHandleBarNormalized, BurstMathUtility.Orthogonal(m_StartHandleBarNormalized));
			m_StartHandleBarUp = m_StartHandleBarLookRotation * Vector3.up;
			m_InverseStartHandleBarLookRotation = Quaternion.Inverse(m_StartHandleBarLookRotation);
			m_LastHandleBarLocalRotation = m_StartHandleBarLookRotation;
			m_FirstFrameSinceTwoHandedGrab = true;
		}
		m_LastGrabCount = count;
		m_MinimumScale = m_InitialScale * m_MinimumScaleRatio;
		m_MaximumScale = m_InitialScale * m_MaximumScaleRatio;
	}

	private void ComputeAdjustedInteractorPose(XRGrabInteractable grabInteractable, out Vector3 newHandleBar, out Vector3 adjustedInteractorPosition, out Quaternion adjustedInteractorRotation)
	{
		if (grabInteractable.interactorsSelecting.Count == 1 || m_TwoHandedRotationMode == TwoHandedRotationMode.FirstHandOnly)
		{
			newHandleBar = m_StartHandleBar;
			Pose worldPose = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable).GetWorldPose();
			adjustedInteractorPosition = worldPose.position;
			adjustedInteractorRotation = worldPose.rotation;
		}
		else if (grabInteractable.interactorsSelecting.Count > 1)
		{
			IXRSelectInteractor iXRSelectInteractor = grabInteractable.interactorsSelecting[0];
			IXRSelectInteractor iXRSelectInteractor2 = grabInteractable.interactorsSelecting[1];
			Transform attachTransform = iXRSelectInteractor.GetAttachTransform(grabInteractable);
			Transform attachTransform2 = iXRSelectInteractor2.GetAttachTransform(grabInteractable);
			newHandleBar = attachTransform.InverseTransformPoint(attachTransform2.position);
			Quaternion quaternion2;
			if (m_TwoHandedRotationMode == TwoHandedRotationMode.FirstHandDirectedTowardsSecondHand)
			{
				Vector3 normalized = newHandleBar.normalized;
				Vector3 vector = m_LastHandleBarLocalRotation * Vector3.up;
				float num = Vector3.Dot(m_StartHandleBarUp, vector);
				Vector3 upwards = vector;
				if (num > 0f)
				{
					float num2 = num * 0.5f;
					float t = num2 * num2;
					upwards = Vector3.Lerp(vector, m_StartHandleBarUp, t);
				}
				Quaternion quaternion = (m_LastHandleBarLocalRotation = Quaternion.LookRotation(normalized, upwards)) * m_InverseStartHandleBarLookRotation;
				quaternion2 = attachTransform.rotation * quaternion;
			}
			else if (m_TwoHandedRotationMode == TwoHandedRotationMode.TwoHandedAverage)
			{
				Vector3 normalized2 = (attachTransform2.position - attachTransform.position).normalized;
				Vector3 rhs = Vector3.Slerp(attachTransform.right, attachTransform2.right, 0.5f);
				Vector3 vector2 = Vector3.Slerp(attachTransform.up, attachTransform2.up, 0.5f);
				Vector3 a = Vector3.Cross(normalized2, rhs);
				float num3 = Mathf.PingPong(Vector3.Angle(vector2, normalized2), 90f);
				vector2 = Vector3.Slerp(a, vector2, num3 / 90f);
				Vector3 rhs2 = Vector3.Cross(vector2, normalized2);
				vector2 = Vector3.Cross(normalized2, rhs2);
				if (m_FirstFrameSinceTwoHandedGrab)
				{
					m_FirstFrameSinceTwoHandedGrab = false;
				}
				else if (Vector3.Dot(vector2, m_LastTwoHandedUp) <= 0f)
				{
					vector2 = -vector2;
				}
				m_LastTwoHandedUp = vector2;
				quaternion2 = Quaternion.LookRotation(normalized2, vector2) * Quaternion.Inverse(m_OffsetPose.rotation);
			}
			else
			{
				quaternion2 = attachTransform.rotation;
			}
			adjustedInteractorPosition = attachTransform.position;
			adjustedInteractorRotation = quaternion2;
		}
		else
		{
			newHandleBar = m_StartHandleBar;
			adjustedInteractorPosition = Vector3.zero;
			adjustedInteractorRotation = Quaternion.identity;
		}
	}

	private void TranslateSetup(Pose interactorCentroidPose, Vector3 grabCentroid, Pose objectPose, Vector3 objectScale)
	{
		Quaternion quaternion = Quaternion.Inverse(interactorCentroidPose.rotation);
		m_InteractorLocalGrabPoint = quaternion * (grabCentroid - interactorCentroidPose.position);
		m_ObjectLocalGrabPoint = Quaternion.Inverse(objectPose.rotation) * (grabCentroid - objectPose.position);
		m_ObjectLocalGrabPoint = m_ObjectLocalGrabPoint.Divide(objectScale);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002ETransformers_002EComputeNewObjectPosition_00000905_0024PostfixBurstDelegate))]
	private static void ComputeNewObjectPosition(in float3 interactorPosition, in quaternion interactorRotation, in quaternion objectRotation, in float3 objectScale, bool trackRotation, in float3 offsetPosition, in float3 objectLocalGrabPoint, in float3 interactorLocalGrabPoint, out Vector3 newPosition)
	{
		ComputeNewObjectPosition_00000905_0024BurstDirectCall.Invoke(in interactorPosition, in interactorRotation, in objectRotation, in objectScale, trackRotation, in offsetPosition, in objectLocalGrabPoint, in interactorLocalGrabPoint, out newPosition);
	}

	private static float3 Scale(float3 a, float3 b)
	{
		return new float3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	private Quaternion ComputeNewObjectRotation(in Quaternion interactorRotation, bool trackRotation)
	{
		if (!trackRotation)
		{
			return m_OriginalObjectPose.rotation;
		}
		return interactorRotation * m_OffsetPose.rotation;
	}

	private static Vector3 AdjustPositionForPermittedAxes(in Vector3 targetPosition, in Pose originalObjectPose, ManipulationAxes permittedAxes, ConstrainedAxisDisplacementMode axisDisplacementMode)
	{
		bool flag = (permittedAxes & ManipulationAxes.X) != 0;
		bool flag2 = (permittedAxes & ManipulationAxes.Y) != 0;
		bool flag3 = (permittedAxes & ManipulationAxes.Z) != 0;
		if (flag && flag2 && flag3)
		{
			return targetPosition;
		}
		if (!flag && !flag2 && !flag3)
		{
			return originalObjectPose.position;
		}
		AdjustPositionForPermittedAxesBurst(in targetPosition, in originalObjectPose, axisDisplacementMode, flag, flag2, flag3, out var adjustedTargetPosition);
		return adjustedTargetPosition;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002ETransformers_002EAdjustPositionForPermittedAxesBurst_00000909_0024PostfixBurstDelegate))]
	private static void AdjustPositionForPermittedAxesBurst(in Vector3 targetPosition, in Pose originalObjectPose, ConstrainedAxisDisplacementMode axisDisplacementMode, bool hasX, bool hasY, bool hasZ, out Vector3 adjustedTargetPosition)
	{
		AdjustPositionForPermittedAxesBurst_00000909_0024BurstDirectCall.Invoke(in targetPosition, in originalObjectPose, axisDisplacementMode, hasX, hasY, hasZ, out adjustedTargetPosition);
	}

	private Vector3 ComputeNewScale(in XRGrabInteractable grabInteractable, in Vector3 startScale, in Vector3 currentScale, in Vector3 startHandleBar, in Vector3 newHandleBar, bool trackScale)
	{
		int count = grabInteractable.interactorsSelecting.Count;
		if (trackScale && count == 1 && m_AllowOneHandedScaling && m_HasScaleValueProvider && m_ScaleValueProvider.scaleMode == ScaleMode.ScaleOverTime)
		{
			float scaleValue = m_ScaleValueProvider.scaleValue;
			if (Mathf.Approximately(scaleValue, 0f))
			{
				return currentScale;
			}
			ComputeNewOneHandedScale(in currentScale, in m_InitialScaleProportions, m_ClampScaling, in m_MinimumScale, in m_MaximumScale, scaleValue, Time.deltaTime, m_OneHandedScaleSpeed, out var newScale);
			return newScale;
		}
		if (trackScale && count > 1 && m_AllowTwoHandedScaling)
		{
			ComputeNewTwoHandedScale(in startScale, in currentScale, in startHandleBar, in newHandleBar, m_ClampScaling, m_ScaleMultiplier, m_ThresholdMoveRatioForScale, in m_MinimumScale, in m_MaximumScale, out var newScale2);
			return newScale2;
		}
		return currentScale;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002ETransformers_002EComputeNewOneHandedScale_0000090B_0024PostfixBurstDelegate))]
	private static void ComputeNewOneHandedScale(in Vector3 currentScale, in Vector3 initialScaleProportions, bool clampScale, in Vector3 minScale, in Vector3 maxScale, float scaleInput, float deltaTime, float scaleSpeed, out Vector3 newScale)
	{
		ComputeNewOneHandedScale_0000090B_0024BurstDirectCall.Invoke(in currentScale, in initialScaleProportions, clampScale, in minScale, in maxScale, scaleInput, deltaTime, scaleSpeed, out newScale);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002ETransformers_002EComputeNewTwoHandedScale_0000090C_0024PostfixBurstDelegate))]
	private static void ComputeNewTwoHandedScale(in Vector3 startScale, in Vector3 currentScale, in Vector3 startHandleBar, in Vector3 newHandleBar, bool clampScale, float scaleMultiplier, float thresholdMoveRatioForScale, in Vector3 minScale, in Vector3 maxScale, out Vector3 newScale)
	{
		ComputeNewTwoHandedScale_0000090C_0024BurstDirectCall.Invoke(in startScale, in currentScale, in startHandleBar, in newHandleBar, clampScale, scaleMultiplier, thresholdMoveRatioForScale, in minScale, in maxScale, out newScale);
	}

	private void UpdateTarget(XRGrabInteractable grabInteractable, ref Pose targetPose, ref Vector3 localScale)
	{
		ComputeAdjustedInteractorPose(grabInteractable, out var newHandleBar, out var adjustedInteractorPosition, out var adjustedInteractorRotation);
		localScale = ComputeNewScale(in grabInteractable, in m_ScaleAtGrabStart, in localScale, in m_StartHandleBar, in newHandleBar, grabInteractable.trackScale);
		targetPose.rotation = ComputeNewObjectRotation(in adjustedInteractorRotation, grabInteractable.trackRotation);
		float3 interactorPosition = adjustedInteractorPosition;
		quaternion interactorRotation = adjustedInteractorRotation;
		quaternion objectRotation = targetPose.rotation;
		float3 objectScale = localScale;
		bool trackRotation = grabInteractable.trackRotation;
		float3 offsetPosition = m_OffsetPose.position;
		float3 objectLocalGrabPoint = m_ObjectLocalGrabPoint;
		float3 interactorLocalGrabPoint = m_InteractorLocalGrabPoint;
		ComputeNewObjectPosition(in interactorPosition, in interactorRotation, in objectRotation, in objectScale, trackRotation, in offsetPosition, in objectLocalGrabPoint, in interactorLocalGrabPoint, out var newPosition);
		targetPose.position = AdjustPositionForPermittedAxes(in newPosition, in m_OriginalObjectPose, m_PermittedDisplacementAxesOnGrab, m_ConstrainedAxisDisplacementModeOnGrab);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ComputeNewObjectPosition_0024BurstManaged(in float3 interactorPosition, in quaternion interactorRotation, in quaternion objectRotation, in float3 objectScale, bool trackRotation, in float3 offsetPosition, in float3 objectLocalGrabPoint, in float3 interactorLocalGrabPoint, out Vector3 newPosition)
	{
		float3 @float = Scale(offsetPosition, objectScale);
		float3 float2 = math.mul(interactorRotation, @float);
		float3 float3 = (trackRotation ? float2 : @float);
		float3 float4 = interactorPosition + float3;
		float3 v = Scale(objectLocalGrabPoint, objectScale);
		float3 v2 = interactorLocalGrabPoint;
		v2 = math.mul(interactorRotation, v2);
		float3 float5 = math.mul(objectRotation, v);
		newPosition = v2 - float5 + float4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void AdjustPositionForPermittedAxesBurst_0024BurstManaged(in Vector3 targetPosition, in Pose originalObjectPose, ConstrainedAxisDisplacementMode axisDisplacementMode, bool hasX, bool hasY, bool hasZ, out Vector3 adjustedTargetPosition)
	{
		float3 @float = float3.zero;
		float3 float2 = float3.zero;
		float3 float3 = float3.zero;
		float3 float4 = new float3(1f, 0f, 0f);
		float3 planeNormal = new float3(0f, 1f, 0f);
		float3 float5 = new float3(0f, 0f, 1f);
		float3 vector = targetPosition - originalObjectPose.position;
		float3 projectedVector = float3.zero;
		float3 float6 = originalObjectPose.position;
		quaternion q = originalObjectPose.rotation;
		switch (axisDisplacementMode)
		{
		case ConstrainedAxisDisplacementMode.WorldAxisRelative:
			if (hasX)
			{
				@float = math.project(vector, float4);
			}
			if (hasY)
			{
				float2 = math.project(vector, planeNormal);
			}
			if (hasZ)
			{
				float3 = math.project(vector, float5);
			}
			projectedVector = @float + float2 + float3;
			break;
		case ConstrainedAxisDisplacementMode.ObjectRelative:
			if (hasX)
			{
				float3 ontoB4 = math.mul(q, float4);
				@float = math.project(vector, ontoB4);
			}
			if (hasY)
			{
				float3 ontoB5 = math.mul(q, planeNormal);
				float2 = math.project(vector, ontoB5);
			}
			if (hasZ)
			{
				float3 ontoB6 = math.mul(q, float5);
				float3 = math.project(vector, ontoB6);
			}
			projectedVector = @float + float2 + float3;
			break;
		case ConstrainedAxisDisplacementMode.ObjectRelativeWithLockedWorldUp:
		{
			if (hasX && hasZ)
			{
				BurstMathUtility.ProjectOnPlane(in vector, in planeNormal, out projectedVector);
				break;
			}
			float3 float7 = Vector3.zero;
			if (hasX)
			{
				float3 ontoB = math.mul(q, float4);
				@float = math.project(vector, ontoB);
			}
			if (hasY)
			{
				float3 ontoB2 = math.mul(q, planeNormal);
				float2 = math.project(vector, ontoB2);
				float7 = math.project(vector, planeNormal);
			}
			if (hasZ)
			{
				float3 ontoB3 = math.mul(q, float5);
				float3 = math.project(vector, ontoB3);
			}
			float3 vector2 = @float + float2 + float3;
			BurstMathUtility.ProjectOnPlane(in vector2, in planeNormal, out var projectedVector2);
			projectedVector = projectedVector2 + float7;
			break;
		}
		}
		adjustedTargetPosition = float6 + projectedVector;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ComputeNewOneHandedScale_0024BurstManaged(in Vector3 currentScale, in Vector3 initialScaleProportions, bool clampScale, in Vector3 minScale, in Vector3 maxScale, float scaleInput, float deltaTime, float scaleSpeed, out Vector3 newScale)
	{
		newScale = currentScale;
		float num = scaleInput * deltaTime * scaleSpeed;
		float3 a = new float3(num, num, num);
		float3 b = initialScaleProportions;
		BurstMathUtility.Scale(in a, in b, out var result);
		float3 @float = (float3)currentScale + result;
		if (!clampScale)
		{
			newScale = math.max(@float, float3.zero);
		}
		else if (num > 0f)
		{
			bool flag = math.abs(@float.x) > math.abs(maxScale.x) || math.abs(@float.y) > math.abs(maxScale.y) || math.abs(@float.z) > math.abs(maxScale.z);
			newScale = (flag ? maxScale : ((Vector3)@float));
		}
		else if (num < 0f)
		{
			bool flag2 = math.abs(@float.x) < math.abs(minScale.x) || math.abs(@float.y) < math.abs(minScale.y) || math.abs(@float.z) < math.abs(minScale.z);
			newScale = (flag2 ? minScale : ((Vector3)@float));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ComputeNewTwoHandedScale_0024BurstManaged(in Vector3 startScale, in Vector3 currentScale, in Vector3 startHandleBar, in Vector3 newHandleBar, bool clampScale, float scaleMultiplier, float thresholdMoveRatioForScale, in Vector3 minScale, in Vector3 maxScale, out Vector3 newScale)
	{
		newScale = currentScale;
		float num = math.length(newHandleBar) / math.length(startHandleBar);
		if (num > 1f)
		{
			float num2 = (num - 1f) * scaleMultiplier - thresholdMoveRatioForScale;
			if (!(num2 < 0f))
			{
				Vector3 vector = (1f + num2) * startScale;
				bool flag = math.abs(vector.x) > math.abs(maxScale.x) || math.abs(vector.y) > math.abs(maxScale.y) || math.abs(vector.z) > math.abs(maxScale.z);
				newScale = ((flag && clampScale) ? maxScale : vector);
			}
		}
		else if (num < 1f)
		{
			float num3 = (1f / num - 1f) * scaleMultiplier - thresholdMoveRatioForScale;
			if (!(num3 < 0f))
			{
				float num4 = 1f + num3;
				Vector3 vector2 = 1f / num4 * startScale;
				bool flag2 = math.abs(vector2.x) < math.abs(minScale.x) || math.abs(vector2.y) < math.abs(minScale.y) || math.abs(vector2.z) < math.abs(minScale.z);
				newScale = ((flag2 && clampScale) ? minScale : vector2);
			}
		}
	}
}
