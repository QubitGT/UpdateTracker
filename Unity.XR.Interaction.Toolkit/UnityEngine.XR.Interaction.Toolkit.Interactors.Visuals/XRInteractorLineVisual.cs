using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Curves;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[AddComponentMenu("XR/Visual/XR Interactor Line Visual", 11)]
[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
[DefaultExecutionOrder(100)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual.html")]
[BurstCompile]
public class XRInteractorLineVisual : MonoBehaviour, IXRCustomReticleProvider
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateLineCurveRenderPoints_00000D7C_0024PostfixBurstDelegate(int numTargetPoints, float curveRatio, in Vector3 lineOrigin, in Vector3 lineDirection, in Vector3 endPoint, ref NativeArray<Vector3> targetPoints);

	internal static class CalculateLineCurveRenderPoints_00000D7C_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateLineCurveRenderPoints_00000D7C_0024PostfixBurstDelegate>(CalculateLineCurveRenderPoints).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(int numTargetPoints, float curveRatio, in Vector3 lineOrigin, in Vector3 lineDirection, in Vector3 endPoint, ref NativeArray<Vector3> targetPoints)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<int, float, ref Vector3, ref Vector3, ref Vector3, ref NativeArray<Vector3>, void>)functionPointer)(numTargetPoints, curveRatio, ref lineOrigin, ref lineDirection, ref endPoint, ref targetPoints);
					return;
				}
			}
			CalculateLineCurveRenderPoints_0024BurstManaged(numTargetPoints, curveRatio, in lineOrigin, in lineDirection, in endPoint, ref targetPoints);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int ComputeNewRenderPoints_00000D7D_0024PostfixBurstDelegate(int numRenderPoints, int numTargetPoints, float targetLineLength, bool shouldSmoothPoints, bool shouldOverwritePoints, float pointSmoothIncrement, ref NativeArray<float3> targetPoints, ref NativeArray<float3> previousRenderPoints, ref NativeArray<float3> renderPoints);

	internal static class ComputeNewRenderPoints_00000D7D_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeNewRenderPoints_00000D7D_0024PostfixBurstDelegate>(ComputeNewRenderPoints).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(int numRenderPoints, int numTargetPoints, float targetLineLength, bool shouldSmoothPoints, bool shouldOverwritePoints, float pointSmoothIncrement, ref NativeArray<float3> targetPoints, ref NativeArray<float3> previousRenderPoints, ref NativeArray<float3> renderPoints)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<int, int, float, bool, bool, float, ref NativeArray<float3>, ref NativeArray<float3>, ref NativeArray<float3>, int>)functionPointer)(numRenderPoints, numTargetPoints, targetLineLength, shouldSmoothPoints, shouldOverwritePoints, pointSmoothIncrement, ref targetPoints, ref previousRenderPoints, ref renderPoints);
				}
			}
			return ComputeNewRenderPoints_0024BurstManaged(numRenderPoints, numTargetPoints, targetLineLength, shouldSmoothPoints, shouldOverwritePoints, pointSmoothIncrement, ref targetPoints, ref previousRenderPoints, ref renderPoints);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool EvaluateLineEndPoint_00000D7E_0024PostfixBurstDelegate(float targetLineLength, bool shouldSmoothPoint, in float3 unsmoothedTargetPoint, in float3 lastRenderPoint, ref float3 newRenderPoint, ref float lineLength);

	internal static class EvaluateLineEndPoint_00000D7E_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<EvaluateLineEndPoint_00000D7E_0024PostfixBurstDelegate>(EvaluateLineEndPoint).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(float targetLineLength, bool shouldSmoothPoint, in float3 unsmoothedTargetPoint, in float3 lastRenderPoint, ref float3 newRenderPoint, ref float lineLength)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, bool, ref float3, ref float3, ref float3, ref float, bool>)functionPointer)(targetLineLength, shouldSmoothPoint, ref unsmoothedTargetPoint, ref lastRenderPoint, ref newRenderPoint, ref lineLength);
				}
			}
			return EvaluateLineEndPoint_0024BurstManaged(targetLineLength, shouldSmoothPoint, in unsmoothedTargetPoint, in lastRenderPoint, ref newRenderPoint, ref lineLength);
		}
	}

	private const float k_MinLineWidth = 0.0001f;

	private const float k_MaxLineWidth = 0.05f;

	private const float k_MinLineBendRatio = 0.01f;

	private const float k_MaxLineBendRatio = 1f;

	[SerializeField]
	[Range(0.0001f, 0.05f)]
	private float m_LineWidth = 0.005f;

	[SerializeField]
	private bool m_OverrideInteractorLineLength = true;

	[SerializeField]
	private float m_LineLength = 10f;

	[SerializeField]
	private bool m_AutoAdjustLineLength;

	[SerializeField]
	private float m_MinLineLength = 0.5f;

	[SerializeField]
	private bool m_UseDistanceToHitAsMaxLineLength = true;

	[SerializeField]
	private float m_LineRetractionDelay = 0.5f;

	[SerializeField]
	private float m_LineLengthChangeSpeed = 12f;

	[SerializeField]
	private AnimationCurve m_WidthCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[SerializeField]
	private bool m_SetLineColorGradient = true;

	[SerializeField]
	private Gradient m_ValidColorGradient = new Gradient
	{
		colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(Color.white, 0f),
			new GradientColorKey(Color.white, 1f)
		},
		alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		}
	};

	[SerializeField]
	private Gradient m_InvalidColorGradient = new Gradient
	{
		colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(Color.red, 0f),
			new GradientColorKey(Color.red, 1f)
		},
		alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		}
	};

	[SerializeField]
	private Gradient m_BlockedColorGradient = new Gradient
	{
		colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(Color.yellow, 0f),
			new GradientColorKey(Color.yellow, 1f)
		},
		alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		}
	};

	[SerializeField]
	private bool m_TreatSelectionAsValidState;

	[SerializeField]
	private bool m_SmoothMovement;

	[SerializeField]
	private float m_FollowTightness = 10f;

	[SerializeField]
	private float m_SnapThresholdDistance = 10f;

	[SerializeField]
	private GameObject m_Reticle;

	[SerializeField]
	private GameObject m_BlockedReticle;

	[SerializeField]
	private bool m_StopLineAtFirstRaycastHit = true;

	[SerializeField]
	private bool m_StopLineAtSelection;

	[SerializeField]
	private bool m_SnapEndpointIfAvailable = true;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float m_LineBendRatio = 0.5f;

	[SerializeField]
	private InteractionLayerMask m_BendingEnabledInteractionLayers = -1;

	[SerializeField]
	private bool m_OverrideInteractorLineOrigin = true;

	[SerializeField]
	private Transform m_LineOriginTransform;

	[SerializeField]
	private float m_LineOriginOffset;

	private float m_SquareSnapThresholdDistance;

	private Vector3 m_ReticlePos;

	private Vector3 m_ReticleNormal;

	private int m_EndPositionInLine;

	private bool m_SnapCurve = true;

	private bool m_PerformSetup;

	private GameObject m_ReticleToUse;

	private LineRenderer m_LineRenderer;

	private ILineRenderable m_LineRenderable;

	private IAdvancedLineRenderable m_AdvancedLineRenderable;

	private bool m_HasAdvancedLineRenderable;

	private IXRSelectInteractor m_LineRenderableAsSelectInteractor;

	private IXRHoverInteractor m_LineRenderableAsHoverInteractor;

	private XRBaseInteractor m_LineRenderableAsBaseInteractor;

	private XRRayInteractor m_LineRenderableAsRayInteractor;

	private NativeArray<Vector3> m_TargetPoints;

	private int m_NumTargetPoints = -1;

	private Vector3[] m_TargetPointsFallback = Array.Empty<Vector3>();

	private NativeArray<Vector3> m_RenderPoints;

	private int m_NumRenderPoints = -1;

	private NativeArray<Vector3> m_PreviousRenderPoints;

	private int m_NumPreviousRenderPoints = -1;

	private readonly Vector3[] m_ClearArray = new Vector3[2]
	{
		Vector3.zero,
		Vector3.zero
	};

	private GameObject m_CustomReticle;

	private bool m_CustomReticleAttached;

	private XRInteractableSnapVolume m_XRInteractableSnapVolume;

	private const int k_NumberOfSegmentsForBendableLine = 20;

	private bool m_PreviousShouldBendLine;

	private Vector3 m_PreviousLineDirection;

	private Vector3 m_CurrentHitPoint;

	private bool m_HasHitInfo;

	private bool m_ValidHit;

	private float m_LastValidHitTime;

	private float m_LastValidLineLength;

	private Collider m_PreviousCollider;

	private XROrigin m_XROrigin;

	private bool m_HasRayInteractor;

	private bool m_HasBaseInteractor;

	private bool m_HasHoverInteractor;

	private bool m_HasSelectInteractor;

	private readonly BindableVariable<float> m_UserScaleVar = new BindableVariable<float>(0f);

	private readonly FloatTweenableVariable m_LineLengthOverrideTweenableVariable = new FloatTweenableVariable();

	private readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

	public float lineWidth
	{
		get
		{
			return m_LineWidth;
		}
		set
		{
			m_LineWidth = value;
			m_PerformSetup = true;
			m_UserScaleVar.BroadcastValue();
		}
	}

	public bool overrideInteractorLineLength
	{
		get
		{
			return m_OverrideInteractorLineLength;
		}
		set
		{
			m_OverrideInteractorLineLength = value;
		}
	}

	public float lineLength
	{
		get
		{
			return m_LineLength;
		}
		set
		{
			m_LineLength = value;
		}
	}

	public bool autoAdjustLineLength
	{
		get
		{
			return m_AutoAdjustLineLength;
		}
		set
		{
			m_AutoAdjustLineLength = value;
		}
	}

	public float minLineLength
	{
		get
		{
			return m_MinLineLength;
		}
		set
		{
			m_MinLineLength = value;
		}
	}

	public bool useDistanceToHitAsMaxLineLength
	{
		get
		{
			return m_UseDistanceToHitAsMaxLineLength;
		}
		set
		{
			m_UseDistanceToHitAsMaxLineLength = value;
		}
	}

	public float lineRetractionDelay
	{
		get
		{
			return m_LineRetractionDelay;
		}
		set
		{
			m_LineRetractionDelay = value;
		}
	}

	public float lineLengthChangeSpeed
	{
		get
		{
			return m_LineLengthChangeSpeed;
		}
		set
		{
			m_LineLengthChangeSpeed = value;
		}
	}

	public AnimationCurve widthCurve
	{
		get
		{
			return m_WidthCurve;
		}
		set
		{
			m_WidthCurve = value;
			m_PerformSetup = true;
		}
	}

	public bool setLineColorGradient
	{
		get
		{
			return m_SetLineColorGradient;
		}
		set
		{
			m_SetLineColorGradient = value;
		}
	}

	public Gradient validColorGradient
	{
		get
		{
			return m_ValidColorGradient;
		}
		set
		{
			m_ValidColorGradient = value;
		}
	}

	public Gradient invalidColorGradient
	{
		get
		{
			return m_InvalidColorGradient;
		}
		set
		{
			m_InvalidColorGradient = value;
		}
	}

	public Gradient blockedColorGradient
	{
		get
		{
			return m_BlockedColorGradient;
		}
		set
		{
			m_BlockedColorGradient = value;
		}
	}

	public bool treatSelectionAsValidState
	{
		get
		{
			return m_TreatSelectionAsValidState;
		}
		set
		{
			m_TreatSelectionAsValidState = value;
		}
	}

	public bool smoothMovement
	{
		get
		{
			return m_SmoothMovement;
		}
		set
		{
			m_SmoothMovement = value;
		}
	}

	public float followTightness
	{
		get
		{
			return m_FollowTightness;
		}
		set
		{
			m_FollowTightness = value;
		}
	}

	public float snapThresholdDistance
	{
		get
		{
			return m_SnapThresholdDistance;
		}
		set
		{
			m_SnapThresholdDistance = value;
			m_SquareSnapThresholdDistance = m_SnapThresholdDistance * m_SnapThresholdDistance;
		}
	}

	public GameObject reticle
	{
		get
		{
			return m_Reticle;
		}
		set
		{
			m_Reticle = value;
			if (Application.isPlaying)
			{
				SetupReticle();
			}
		}
	}

	public GameObject blockedReticle
	{
		get
		{
			return m_BlockedReticle;
		}
		set
		{
			m_BlockedReticle = value;
			if (Application.isPlaying)
			{
				SetupBlockedReticle();
			}
		}
	}

	public bool stopLineAtFirstRaycastHit
	{
		get
		{
			return m_StopLineAtFirstRaycastHit;
		}
		set
		{
			m_StopLineAtFirstRaycastHit = value;
		}
	}

	public bool stopLineAtSelection
	{
		get
		{
			return m_StopLineAtSelection;
		}
		set
		{
			m_StopLineAtSelection = value;
		}
	}

	public bool snapEndpointIfAvailable
	{
		get
		{
			return m_SnapEndpointIfAvailable;
		}
		set
		{
			m_SnapEndpointIfAvailable = value;
		}
	}

	public float lineBendRatio
	{
		get
		{
			return m_LineBendRatio;
		}
		set
		{
			m_LineBendRatio = Mathf.Clamp(value, 0.01f, 1f);
		}
	}

	public InteractionLayerMask bendingEnabledInteractionLayers
	{
		get
		{
			return m_BendingEnabledInteractionLayers;
		}
		set
		{
			m_BendingEnabledInteractionLayers = value;
		}
	}

	public bool overrideInteractorLineOrigin
	{
		get
		{
			return m_OverrideInteractorLineOrigin;
		}
		set
		{
			m_OverrideInteractorLineOrigin = value;
		}
	}

	public Transform lineOriginTransform
	{
		get
		{
			return m_LineOriginTransform;
		}
		set
		{
			m_LineOriginTransform = value;
		}
	}

	public float lineOriginOffset
	{
		get
		{
			return m_LineOriginOffset;
		}
		set
		{
			m_LineOriginOffset = value;
		}
	}

	protected void Reset()
	{
	}

	protected void OnValidate()
	{
		if (Application.isPlaying)
		{
			UpdateSettings();
		}
	}

	protected void Awake()
	{
		m_LineRenderable = GetComponent<ILineRenderable>();
		m_AdvancedLineRenderable = m_LineRenderable as IAdvancedLineRenderable;
		m_HasAdvancedLineRenderable = m_AdvancedLineRenderable != null;
		if (m_LineRenderable != null)
		{
			if (m_LineRenderable is XRBaseInteractor lineRenderableAsBaseInteractor)
			{
				m_LineRenderableAsBaseInteractor = lineRenderableAsBaseInteractor;
				m_HasBaseInteractor = true;
			}
			if (m_LineRenderable is IXRSelectInteractor lineRenderableAsSelectInteractor)
			{
				m_LineRenderableAsSelectInteractor = lineRenderableAsSelectInteractor;
				m_HasSelectInteractor = true;
			}
			if (m_LineRenderable is IXRHoverInteractor lineRenderableAsHoverInteractor)
			{
				m_LineRenderableAsHoverInteractor = lineRenderableAsHoverInteractor;
				m_HasHoverInteractor = true;
			}
			if (m_LineRenderable is XRRayInteractor lineRenderableAsRayInteractor)
			{
				m_LineRenderableAsRayInteractor = lineRenderableAsRayInteractor;
				m_HasRayInteractor = true;
			}
		}
		FindXROrigin();
		SetupReticle();
		SetupBlockedReticle();
		ClearLineRenderer();
		UpdateSettings();
	}

	protected void OnEnable()
	{
		if (m_LineRenderer == null)
		{
			XRLoggingUtils.LogError($"Missing Line Renderer component on {this}. Disabling line visual.", this);
			base.enabled = false;
			return;
		}
		if (m_LineRenderable == null)
		{
			XRLoggingUtils.LogError(string.Format("Missing {0} / Ray Interactor component on {1}. Disabling line visual.", "ILineRenderable", this), this);
			base.enabled = false;
			m_LineRenderer.enabled = false;
			return;
		}
		m_SnapCurve = true;
		if (m_ReticleToUse != null)
		{
			m_ReticleToUse.SetActive(value: false);
			m_ReticleToUse = null;
		}
		m_BindingsGroup.AddBinding(m_UserScaleVar.Subscribe(delegate(float userScale)
		{
			m_LineRenderer.widthMultiplier = userScale * Mathf.Clamp(m_LineWidth, 0.0001f, 0.05f);
		}));
		Application.onBeforeRender += OnBeforeRenderLineVisual;
	}

	protected void OnDisable()
	{
		m_BindingsGroup.Clear();
		if (m_LineRenderer != null)
		{
			m_LineRenderer.enabled = false;
		}
		if (m_ReticleToUse != null)
		{
			m_ReticleToUse.SetActive(value: false);
			m_ReticleToUse = null;
		}
		Application.onBeforeRender -= OnBeforeRenderLineVisual;
	}

	protected void OnDestroy()
	{
		if (m_TargetPoints.IsCreated)
		{
			m_TargetPoints.Dispose();
		}
		if (m_RenderPoints.IsCreated)
		{
			m_RenderPoints.Dispose();
		}
		if (m_PreviousRenderPoints.IsCreated)
		{
			m_PreviousRenderPoints.Dispose();
		}
		m_LineLengthOverrideTweenableVariable.Dispose();
	}

	protected void LateUpdate()
	{
		if (m_PerformSetup)
		{
			UpdateSettings();
			m_PerformSetup = false;
		}
		if (m_LineRenderer.useWorldSpace && m_XROrigin != null)
		{
			GameObject origin = m_XROrigin.Origin;
			float value = ((origin != null) ? origin.transform.localScale.x : 1f);
			m_UserScaleVar.Value = value;
		}
	}

	[BeforeRenderOrder(101)]
	private void OnBeforeRenderLineVisual()
	{
		UpdateLineVisual();
	}

	internal void UpdateLineVisual()
	{
		if (m_LineRenderableAsBaseInteractor != null && m_LineRenderableAsBaseInteractor.disableVisualsWhenBlockedInGroup && m_LineRenderableAsBaseInteractor.IsBlockedByInteractionWithinGroup())
		{
			m_LineRenderer.enabled = false;
			return;
		}
		m_NumRenderPoints = 0;
		if (!GetLinePoints(ref m_TargetPoints, out m_NumTargetPoints) || m_NumTargetPoints == 0)
		{
			m_LineRenderer.enabled = false;
			return;
		}
		bool flag = m_HasSelectInteractor && m_LineRenderableAsSelectInteractor.hasSelection;
		bool flag2 = m_HasRayInteractor && m_LineRenderableAsRayInteractor.lineType == XRRayInteractor.LineType.StraightLine;
		GetLineOriginAndDirection(ref m_TargetPoints, m_NumTargetPoints, flag2, out var lineOrigin, out var lineDirection);
		m_ValidHit = ExtractHitInformation(ref m_TargetPoints, m_NumTargetPoints, out var targetEndPoint, out var hitSnapVolume);
		bool flag3 = false;
		if (flag)
		{
			for (int i = 0; i < m_LineRenderableAsSelectInteractor.interactablesSelected.Count; i++)
			{
				flag3 = ((int)bendingEnabledInteractionLayers & (int)m_LineRenderableAsSelectInteractor.interactablesSelected[i].interactionLayers) != 0;
				if (flag3)
				{
					break;
				}
			}
		}
		bool flag4 = flag && flag2 && flag3;
		bool flag5 = m_OverrideInteractorLineOrigin && m_ValidHit && flag2;
		bool flag6 = (hitSnapVolume || flag4 || flag5) && m_LineBendRatio < 1f;
		if (flag6)
		{
			m_NumTargetPoints = 20;
			m_EndPositionInLine = m_NumTargetPoints - 1;
			if (flag4)
			{
				FindClosestInteractableAttachPoint(in lineOrigin, out targetEndPoint);
			}
		}
		EnsureSize(ref m_TargetPoints, m_NumTargetPoints);
		if (!EnsureSize(ref m_RenderPoints, m_NumTargetPoints))
		{
			m_NumRenderPoints = 0;
		}
		if (!EnsureSize(ref m_PreviousRenderPoints, m_NumTargetPoints))
		{
			m_NumPreviousRenderPoints = 0;
		}
		if (flag6)
		{
			if (m_SmoothMovement)
			{
				if (m_PreviousShouldBendLine && m_NumPreviousRenderPoints > 0)
				{
					float t = m_FollowTightness * Time.deltaTime;
					lineDirection = Vector3.Lerp(m_PreviousLineDirection, lineDirection, t);
					lineOrigin = Vector3.Lerp(m_PreviousRenderPoints[0], lineOrigin, t);
				}
				m_PreviousLineDirection = lineDirection;
			}
			CalculateLineCurveRenderPoints(m_NumTargetPoints, m_LineBendRatio, in lineOrigin, in lineDirection, in targetEndPoint, ref m_TargetPoints);
		}
		m_PreviousShouldBendLine = flag6;
		if (m_NumPreviousRenderPoints != m_NumTargetPoints)
		{
			m_SnapCurve = true;
		}
		else if (m_SmoothMovement && m_NumPreviousRenderPoints > 0 && m_NumPreviousRenderPoints <= m_PreviousRenderPoints.Length && m_NumTargetPoints > 0 && m_NumTargetPoints <= m_TargetPoints.Length)
		{
			int index = m_NumPreviousRenderPoints - 1;
			int index2 = m_NumTargetPoints - 1;
			m_SnapCurve = Vector3.SqrMagnitude(m_PreviousRenderPoints[index] - m_TargetPoints[index2]) > m_SquareSnapThresholdDistance;
		}
		AdjustLineAndReticle(flag, flag6, in lineOrigin, in targetEndPoint);
		bool flag7 = !flag6 && m_SmoothMovement && m_NumPreviousRenderPoints == m_NumTargetPoints && !m_SnapCurve;
		if (m_OverrideInteractorLineLength || flag7)
		{
			NativeArray<float3> targetPoints = m_TargetPoints.Reinterpret<float3>();
			NativeArray<float3> previousRenderPoints = m_PreviousRenderPoints.Reinterpret<float3>();
			NativeArray<float3> renderPoints = m_RenderPoints.Reinterpret<float3>();
			float targetLineLength = ((m_OverrideInteractorLineLength && m_AutoAdjustLineLength) ? UpdateTargetLineLength(in lineOrigin, in targetEndPoint, m_MinLineLength, m_LineLength, m_LineRetractionDelay, m_LineLengthChangeSpeed, m_ValidHit || flag, m_UseDistanceToHitAsMaxLineLength) : m_LineLength);
			m_NumRenderPoints = ComputeNewRenderPoints(m_NumRenderPoints, m_NumTargetPoints, targetLineLength, flag7, m_OverrideInteractorLineLength, m_FollowTightness * Time.deltaTime, ref targetPoints, ref previousRenderPoints, ref renderPoints);
		}
		else
		{
			NativeArray<Vector3>.Copy(m_TargetPoints, 0, m_RenderPoints, 0, m_NumTargetPoints);
			m_NumRenderPoints = m_NumTargetPoints;
		}
		if (m_ValidHit || (m_TreatSelectionAsValidState && flag))
		{
			bool flag8 = false;
			if (!flag && m_HasBaseInteractor && m_LineRenderableAsBaseInteractor.hasHover)
			{
				XRInteractionManager interactionManager = m_LineRenderableAsBaseInteractor.interactionManager;
				bool flag9 = false;
				foreach (IXRHoverInteractable item in m_LineRenderableAsBaseInteractor.interactablesHovered)
				{
					if (item is IXRSelectInteractable interactable && interactionManager.IsSelectPossible(m_LineRenderableAsBaseInteractor, interactable))
					{
						flag9 = true;
						break;
					}
				}
				flag8 = !flag9;
			}
			SetColorGradient(flag8 ? m_BlockedColorGradient : m_ValidColorGradient);
			AssignReticle(flag8);
		}
		else
		{
			ClearReticle();
			SetColorGradient(m_InvalidColorGradient);
		}
		if (m_NumRenderPoints >= 2)
		{
			m_LineRenderer.enabled = true;
			m_LineRenderer.positionCount = m_NumRenderPoints;
			m_LineRenderer.SetPositions(m_RenderPoints);
			NativeArray<Vector3>.Copy(m_RenderPoints, 0, m_PreviousRenderPoints, 0, m_NumRenderPoints);
			m_NumPreviousRenderPoints = m_NumRenderPoints;
			m_SnapCurve = false;
		}
		else
		{
			m_LineRenderer.enabled = false;
		}
	}

	private bool GetLinePoints(ref NativeArray<Vector3> linePoints, out int numPoints)
	{
		if (m_HasAdvancedLineRenderable)
		{
			Ray? rayOriginOverride = null;
			if (m_OverrideInteractorLineOrigin && m_LineOriginTransform != null)
			{
				Pose worldPose = m_LineOriginTransform.GetWorldPose();
				rayOriginOverride = new Ray(worldPose.position, worldPose.forward);
			}
			return m_AdvancedLineRenderable.GetLinePoints(ref linePoints, out numPoints, rayOriginOverride);
		}
		bool linePoints2 = m_LineRenderable.GetLinePoints(ref m_TargetPointsFallback, out numPoints);
		EnsureSize(ref linePoints, numPoints);
		NativeArray<Vector3>.Copy(m_TargetPointsFallback, linePoints, numPoints);
		return linePoints2;
	}

	private void AdjustLineAndReticle(bool hasSelection, bool bendLine, in Vector3 lineOrigin, in Vector3 targetEndPoint)
	{
		if (m_HasHitInfo)
		{
			m_ReticlePos = targetEndPoint;
			if ((m_ValidHit || m_StopLineAtFirstRaycastHit) && m_EndPositionInLine > 0 && m_EndPositionInLine < m_NumTargetPoints)
			{
				Vector3 vector = m_TargetPoints[m_EndPositionInLine - 1];
				Vector3 vector2 = m_TargetPoints[m_EndPositionInLine] - vector;
				Vector3 vector3 = Vector3.Project(m_ReticlePos - vector, vector2);
				if (Vector3.Dot(vector3, vector2) < 0f)
				{
					vector3 = Vector3.zero;
				}
				m_ReticlePos = vector + vector3;
				m_TargetPoints[m_EndPositionInLine] = m_ReticlePos;
				m_NumTargetPoints = m_EndPositionInLine + 1;
			}
		}
		if (!(m_StopLineAtSelection && hasSelection) || bendLine)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(targetEndPoint - lineOrigin);
		float num2 = Vector3.SqrMagnitude(m_TargetPoints[m_EndPositionInLine] - lineOrigin);
		if (!(num < num2) && m_EndPositionInLine != 0)
		{
			return;
		}
		int num3 = 1;
		float num4 = Vector3.SqrMagnitude(m_TargetPoints[num3] - targetEndPoint);
		for (int i = 2; i < m_NumTargetPoints; i++)
		{
			float num5 = Vector3.SqrMagnitude(m_TargetPoints[i] - targetEndPoint);
			if (!(num5 < num4))
			{
				break;
			}
			num3 = i;
			num4 = num5;
		}
		m_EndPositionInLine = num3;
		m_NumTargetPoints = m_EndPositionInLine + 1;
		m_ReticlePos = targetEndPoint;
		if (!m_HasHitInfo)
		{
			m_ReticleNormal = Vector3.Normalize(m_TargetPoints[m_EndPositionInLine - 1] - m_ReticlePos);
		}
		m_TargetPoints[m_EndPositionInLine] = m_ReticlePos;
	}

	private void FindClosestInteractableAttachPoint(in Vector3 lineOrigin, out Vector3 closestPoint)
	{
		List<IXRSelectInteractable> interactablesSelected = m_LineRenderableAsSelectInteractor.interactablesSelected;
		closestPoint = interactablesSelected[0].GetAttachTransform(m_LineRenderableAsSelectInteractor).position;
		if (interactablesSelected.Count <= 1)
		{
			return;
		}
		float num = Vector3.SqrMagnitude(closestPoint - lineOrigin);
		for (int i = 1; i < interactablesSelected.Count; i++)
		{
			Vector3 position = interactablesSelected[i].GetAttachTransform(m_LineRenderableAsSelectInteractor).position;
			float num2 = Vector3.SqrMagnitude(position - lineOrigin);
			if (num2 < num)
			{
				closestPoint = position;
				num = num2;
			}
		}
	}

	private static bool EnsureSize(ref NativeArray<Vector3> array, int targetSize)
	{
		if (array.IsCreated && array.Length >= targetSize)
		{
			return true;
		}
		if (array.IsCreated)
		{
			array.Dispose();
		}
		array = new NativeArray<Vector3>(targetSize, Allocator.Persistent);
		return false;
	}

	private void GetLineOriginAndDirection(ref NativeArray<Vector3> targetPoints, int numTargetPoints, bool isLineStraight, out Vector3 lineOrigin, out Vector3 lineDirection)
	{
		if (m_OverrideInteractorLineOrigin && m_LineOriginTransform != null)
		{
			Pose worldPose = m_LineOriginTransform.GetWorldPose();
			lineOrigin = worldPose.position;
			lineDirection = worldPose.forward;
		}
		else if (m_HasAdvancedLineRenderable)
		{
			m_AdvancedLineRenderable.GetLineOriginAndDirection(out lineOrigin, out lineDirection);
		}
		else
		{
			lineOrigin = targetPoints[0];
			Vector3 vector = targetPoints[numTargetPoints - 1];
			lineDirection = (vector - lineOrigin).normalized;
		}
		if (isLineStraight && m_LineOriginOffset > 0f && (!m_OverrideInteractorLineLength || m_LineOriginOffset < m_LineLength))
		{
			lineOrigin += lineDirection * m_LineOriginOffset;
		}
		targetPoints[0] = lineOrigin;
	}

	private bool ExtractHitInformation(ref NativeArray<Vector3> targetPoints, int numTargetPoints, out Vector3 targetEndPoint, out bool hitSnapVolume)
	{
		Collider collider = null;
		hitSnapVolume = false;
		targetEndPoint = targetPoints[numTargetPoints - 1];
		m_HasHitInfo = m_LineRenderable.TryGetHitInfo(out m_CurrentHitPoint, out m_ReticleNormal, out m_EndPositionInLine, out var isValidTarget);
		if (m_HasHitInfo)
		{
			targetEndPoint = m_CurrentHitPoint;
			if (isValidTarget && m_SnapEndpointIfAvailable && m_HasRayInteractor && m_LineRenderableAsRayInteractor.TryGetCurrentRaycast(out var raycastHit, out var _, out var _, out var _, out var isUIHitClosest) && !isUIHitClosest)
			{
				if (raycastHit.HasValue)
				{
					collider = raycastHit.Value.collider;
				}
				if (collider != m_PreviousCollider && collider != null)
				{
					m_LineRenderableAsBaseInteractor.interactionManager.TryGetInteractableForCollider(collider, out var _, out m_XRInteractableSnapVolume);
				}
				if (m_XRInteractableSnapVolume != null)
				{
					targetEndPoint = (m_LineRenderableAsRayInteractor.hasSelection ? m_XRInteractableSnapVolume.GetClosestPointOfAttachTransform(m_LineRenderableAsRayInteractor) : m_XRInteractableSnapVolume.GetClosestPoint(targetEndPoint));
					m_EndPositionInLine = 19;
					hitSnapVolume = true;
				}
			}
		}
		if (collider == null)
		{
			m_XRInteractableSnapVolume = null;
		}
		m_PreviousCollider = collider;
		return isValidTarget;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractors_002EVisuals_002ECalculateLineCurveRenderPoints_00000D7C_0024PostfixBurstDelegate))]
	private static void CalculateLineCurveRenderPoints(int numTargetPoints, float curveRatio, in Vector3 lineOrigin, in Vector3 lineDirection, in Vector3 endPoint, ref NativeArray<Vector3> targetPoints)
	{
		CalculateLineCurveRenderPoints_00000D7C_0024BurstDirectCall.Invoke(numTargetPoints, curveRatio, in lineOrigin, in lineDirection, in endPoint, ref targetPoints);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractors_002EVisuals_002EComputeNewRenderPoints_00000D7D_0024PostfixBurstDelegate))]
	private static int ComputeNewRenderPoints(int numRenderPoints, int numTargetPoints, float targetLineLength, bool shouldSmoothPoints, bool shouldOverwritePoints, float pointSmoothIncrement, ref NativeArray<float3> targetPoints, ref NativeArray<float3> previousRenderPoints, ref NativeArray<float3> renderPoints)
	{
		return ComputeNewRenderPoints_00000D7D_0024BurstDirectCall.Invoke(numRenderPoints, numTargetPoints, targetLineLength, shouldSmoothPoints, shouldOverwritePoints, pointSmoothIncrement, ref targetPoints, ref previousRenderPoints, ref renderPoints);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractors_002EVisuals_002EEvaluateLineEndPoint_00000D7E_0024PostfixBurstDelegate))]
	private static bool EvaluateLineEndPoint(float targetLineLength, bool shouldSmoothPoint, in float3 unsmoothedTargetPoint, in float3 lastRenderPoint, ref float3 newRenderPoint, ref float lineLength)
	{
		return EvaluateLineEndPoint_00000D7E_0024BurstDirectCall.Invoke(targetLineLength, shouldSmoothPoint, in unsmoothedTargetPoint, in lastRenderPoint, ref newRenderPoint, ref lineLength);
	}

	private float UpdateTargetLineLength(in Vector3 lineOrigin, in Vector3 hitPoint, float minimumLineLength, float maximumLineLength, float lineRetractionDelaySeconds, float lineRetractionScalar, bool hasHit, bool deriveMaxLineLength)
	{
		float unscaledTime = Time.unscaledTime;
		if (hasHit)
		{
			m_LastValidHitTime = Time.unscaledTime;
			m_LastValidLineLength = (deriveMaxLineLength ? Mathf.Min(Vector3.Distance(lineOrigin, hitPoint), maximumLineLength) : maximumLineLength);
		}
		float num = unscaledTime - m_LastValidHitTime;
		if (num > lineRetractionDelaySeconds)
		{
			m_LineLengthOverrideTweenableVariable.target = minimumLineLength;
			float num2 = (num - lineRetractionDelaySeconds) * lineRetractionScalar;
			m_LineLengthOverrideTweenableVariable.HandleTween(Time.unscaledDeltaTime * num2);
		}
		else
		{
			m_LineLengthOverrideTweenableVariable.target = Mathf.Max(m_LastValidLineLength, minimumLineLength);
			m_LineLengthOverrideTweenableVariable.HandleTween(Time.unscaledDeltaTime * lineRetractionScalar);
		}
		return m_LineLengthOverrideTweenableVariable.Value;
	}

	private void AssignReticle(bool useBlockedVisuals)
	{
		GameObject reticleToUse = m_ReticleToUse;
		GameObject gameObject = (useBlockedVisuals ? m_BlockedReticle : m_Reticle);
		m_ReticleToUse = (m_CustomReticleAttached ? m_CustomReticle : gameObject);
		if (reticleToUse != null && reticleToUse != m_ReticleToUse)
		{
			reticleToUse.SetActive(value: false);
		}
		if (!(m_ReticleToUse != null))
		{
			return;
		}
		if (m_HasHoverInteractor && m_LineRenderableAsHoverInteractor.GetOldestInteractableHovered() is IXRReticleDirectionProvider iXRReticleDirectionProvider)
		{
			iXRReticleDirectionProvider.GetReticleDirection(m_LineRenderableAsHoverInteractor, m_ReticleNormal, out var reticleUp, out var optionalReticleForward);
			Quaternion lookRotation;
			if (optionalReticleForward.HasValue)
			{
				Vector3 forward = optionalReticleForward.Value;
				BurstMathUtility.LookRotationWithForwardProjectedOnPlane(in forward, in reticleUp, out lookRotation);
			}
			else
			{
				Vector3 forward = m_ReticleToUse.transform.forward;
				BurstMathUtility.LookRotationWithForwardProjectedOnPlane(in forward, in reticleUp, out lookRotation);
			}
			m_ReticleToUse.transform.SetWorldPose(new Pose(m_ReticlePos, lookRotation));
		}
		else
		{
			m_ReticleToUse.transform.SetWorldPose(new Pose(m_ReticlePos, Quaternion.LookRotation(-m_ReticleNormal)));
		}
		m_ReticleToUse.SetActive(value: true);
	}

	private void ClearReticle()
	{
		if (m_ReticleToUse != null)
		{
			m_ReticleToUse.SetActive(value: false);
			m_ReticleToUse = null;
		}
	}

	private void SetColorGradient(Gradient colorGradient)
	{
		if (m_SetLineColorGradient)
		{
			m_LineRenderer.colorGradient = colorGradient;
		}
	}

	private void UpdateSettings()
	{
		m_SquareSnapThresholdDistance = m_SnapThresholdDistance * m_SnapThresholdDistance;
		if (TryFindLineRenderer())
		{
			m_LineRenderer.widthMultiplier = Mathf.Clamp(m_LineWidth, 0.0001f, 0.05f);
			m_LineRenderer.widthCurve = m_WidthCurve;
			m_SnapCurve = true;
		}
		m_LineLengthOverrideTweenableVariable.target = lineLength;
		m_LineLengthOverrideTweenableVariable.HandleTween(1f);
	}

	private bool TryFindLineRenderer()
	{
		m_LineRenderer = GetComponent<LineRenderer>();
		if (m_LineRenderer == null)
		{
			Debug.LogWarning("No Line Renderer found for Interactor Line Visual.", this);
			base.enabled = false;
			return false;
		}
		return true;
	}

	private void ClearLineRenderer()
	{
		if (TryFindLineRenderer())
		{
			m_LineRenderer.SetPositions(m_ClearArray);
			m_LineRenderer.positionCount = 0;
		}
	}

	private void FindXROrigin()
	{
		if (m_XROrigin == null)
		{
			ComponentLocatorUtility<XROrigin>.TryFindComponent(out m_XROrigin);
		}
	}

	private void SetupReticle()
	{
		if (!(m_Reticle == null))
		{
			if (!m_Reticle.scene.IsValid())
			{
				m_Reticle = Object.Instantiate(m_Reticle);
			}
			m_Reticle.SetActive(value: false);
		}
	}

	private void SetupBlockedReticle()
	{
		if (!(m_BlockedReticle == null))
		{
			if (!m_BlockedReticle.scene.IsValid())
			{
				m_BlockedReticle = Object.Instantiate(m_BlockedReticle);
			}
			m_BlockedReticle.SetActive(value: false);
		}
	}

	public bool AttachCustomReticle(GameObject reticleInstance)
	{
		m_CustomReticle = reticleInstance;
		m_CustomReticleAttached = true;
		return true;
	}

	public bool RemoveCustomReticle()
	{
		m_CustomReticle = null;
		m_CustomReticleAttached = false;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateLineCurveRenderPoints_0024BurstManaged(int numTargetPoints, float curveRatio, in Vector3 lineOrigin, in Vector3 lineDirection, in Vector3 endPoint, ref NativeArray<Vector3> targetPoints)
	{
		NativeArray<float3> targetPoints2 = targetPoints.Reinterpret<float3>();
		float3 lineOrigin2 = lineOrigin;
		float3 lineDirection2 = lineDirection;
		float3 endPoint2 = endPoint;
		CurveUtility.GenerateCubicBezierCurve(numTargetPoints, curveRatio, in lineOrigin2, in lineDirection2, in endPoint2, ref targetPoints2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static int ComputeNewRenderPoints_0024BurstManaged(int numRenderPoints, int numTargetPoints, float targetLineLength, bool shouldSmoothPoints, bool shouldOverwritePoints, float pointSmoothIncrement, ref NativeArray<float3> targetPoints, ref NativeArray<float3> previousRenderPoints, ref NativeArray<float3> renderPoints)
	{
		float num = 0f;
		int length = renderPoints.Length;
		int num2 = numRenderPoints;
		for (int i = 0; i < numTargetPoints; i++)
		{
			if (num2 >= length)
			{
				break;
			}
			float3 unsmoothedTargetPoint = targetPoints[i];
			float3 newRenderPoint = ((!shouldSmoothPoints) ? unsmoothedTargetPoint : math.lerp(previousRenderPoints[i], unsmoothedTargetPoint, pointSmoothIncrement));
			if (shouldOverwritePoints && num2 > 0 && length > 0)
			{
				float3 lastRenderPoint = renderPoints[num2 - 1];
				if (EvaluateLineEndPoint(targetLineLength, shouldSmoothPoints, in unsmoothedTargetPoint, in lastRenderPoint, ref newRenderPoint, ref num))
				{
					renderPoints[num2] = newRenderPoint;
					num2++;
					break;
				}
			}
			renderPoints[num2] = newRenderPoint;
			num2++;
		}
		return num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool EvaluateLineEndPoint_0024BurstManaged(float targetLineLength, bool shouldSmoothPoint, in float3 unsmoothedTargetPoint, in float3 lastRenderPoint, ref float3 newRenderPoint, ref float lineLength)
	{
		float3 x = newRenderPoint - lastRenderPoint;
		float num = math.length(x);
		if (shouldSmoothPoint)
		{
			float num2 = math.distance(lastRenderPoint, unsmoothedTargetPoint);
			if (num2 < num)
			{
				newRenderPoint = lastRenderPoint + math.normalize(x) * num2;
				num = num2;
			}
		}
		lineLength += num;
		if (lineLength <= targetLineLength)
		{
			return false;
		}
		float num3 = lineLength - targetLineLength;
		float t = 1f - num3 / num;
		newRenderPoint = math.lerp(lastRenderPoint, newRenderPoint, t);
		return true;
	}
}
