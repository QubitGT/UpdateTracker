using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Curves;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[DisallowMultipleComponent]
[AddComponentMenu("XR/Visual/Curve Visual Controller", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.CurveVisualController.html")]
[BurstCompile]
public class CurveVisualController : MonoBehaviour
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetAdjustedEndPointForMaxDistance_00000D24_0024PostfixBurstDelegate(in float3 origin, in float3 endPoint, float maxDistance, out float3 newEndPoint);

	internal static class GetAdjustedEndPointForMaxDistance_00000D24_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetAdjustedEndPointForMaxDistance_00000D24_0024PostfixBurstDelegate>(GetAdjustedEndPointForMaxDistance).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 origin, in float3 endPoint, float maxDistance, out float3 newEndPoint)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, ref float3, void>)functionPointer)(ref origin, ref endPoint, maxDistance, ref newEndPoint);
					return;
				}
			}
			GetAdjustedEndPointForMaxDistance_0024BurstManaged(in origin, in endPoint, maxDistance, out newEndPoint);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetClosestPointOnLine_00000D25_0024PostfixBurstDelegate(in float3 origin, in float3 direction, in float3 point, out float3 newPoint);

	internal static class GetClosestPointOnLine_00000D25_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetClosestPointOnLine_00000D25_0024PostfixBurstDelegate>(GetClosestPointOnLine).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 origin, in float3 direction, in float3 point, out float3 newPoint)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, void>)functionPointer)(ref origin, ref direction, ref point, ref newPoint);
					return;
				}
			}
			GetClosestPointOnLine_0024BurstManaged(in origin, in direction, in point, out newPoint);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void AdjustCastHitEndPoint_00000D26_0024PostfixBurstDelegate(in float3 worldOrigin, in float3 worldDirection, in float3 hitEndPoint, in float3 sampleEndPoint, out float validHitDistance, out float3 endPoint);

	internal static class AdjustCastHitEndPoint_00000D26_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<AdjustCastHitEndPoint_00000D26_0024PostfixBurstDelegate>(AdjustCastHitEndPoint).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 worldOrigin, in float3 worldDirection, in float3 hitEndPoint, in float3 sampleEndPoint, out float validHitDistance, out float3 endPoint)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, ref float, ref float3, void>)functionPointer)(ref worldOrigin, ref worldDirection, ref hitEndPoint, ref sampleEndPoint, ref validHitDistance, ref endPoint);
					return;
				}
			}
			AdjustCastHitEndPoint_0024BurstManaged(in worldOrigin, in worldDirection, in hitEndPoint, in sampleEndPoint, out validHitDistance, out endPoint);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool ComputeFallBackLine_00000D27_0024PostfixBurstDelegate(in float3 curveOrigin, in float3 endPoint, float startOffset, float endOffset, ref NativeArray<float3> fallBackTargetPoints);

	internal static class ComputeFallBackLine_00000D27_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeFallBackLine_00000D27_0024PostfixBurstDelegate>(ComputeFallBackLine).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(in float3 curveOrigin, in float3 endPoint, float startOffset, float endOffset, ref NativeArray<float3> fallBackTargetPoints)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, float, ref NativeArray<float3>, bool>)functionPointer)(ref curveOrigin, ref endPoint, startOffset, endOffset, ref fallBackTargetPoints);
				}
			}
			return ComputeFallBackLine_0024BurstManaged(in curveOrigin, in endPoint, startOffset, endOffset, ref fallBackTargetPoints);
		}
	}

	[SerializeField]
	private LineRenderer m_LineRenderer;

	[SerializeField]
	[RequireInterface(typeof(ICurveInteractionDataProvider))]
	private Object m_CurveVisualObject;

	private readonly UnityObjectReferenceCache<ICurveInteractionDataProvider, Object> m_CurveDataProviderObjectRef = new UnityObjectReferenceCache<ICurveInteractionDataProvider, Object>();

	[SerializeField]
	private bool m_OverrideLineOrigin = true;

	[SerializeField]
	private Transform m_LineOriginTransform;

	[SerializeField]
	private int m_VisualPointCount = 20;

	[SerializeField]
	private float m_MaxVisualCurveDistance = 10f;

	[SerializeField]
	private float m_RestingVisualLineLength = 0.15f;

	[SerializeField]
	private LineDynamicsMode m_LineDynamicsMode;

	[SerializeField]
	private float m_RetractDelay = 1f;

	[SerializeField]
	private float m_RetractDuration = 0.5f;

	[SerializeField]
	private bool m_ExtendLineToEmptyHit;

	[SerializeField]
	[Range(0f, 30f)]
	private float m_ExtensionRate = 10f;

	[SerializeField]
	private float m_EndPointExpansionRate = 10f;

	[SerializeField]
	private bool m_ComputeMidPointWithComplexCurves;

	[SerializeField]
	private bool m_SnapToSelectedAttachIfAvailable = true;

	[SerializeField]
	private bool m_SnapToSnapVolumeIfAvailable = true;

	[SerializeField]
	private float m_CurveStartOffset;

	[SerializeField]
	private float m_CurveEndOffset = 0.005f;

	[SerializeField]
	private bool m_CustomizeLinePropertiesForState;

	[SerializeField]
	private float m_LinePropertyAnimationSpeed = 8f;

	[SerializeField]
	private LineProperties m_NoValidHitProperties;

	[SerializeField]
	private LineProperties m_UIHitProperties;

	[SerializeField]
	private LineProperties m_UIPressHitProperties;

	[SerializeField]
	private LineProperties m_SelectHitProperties;

	[SerializeField]
	private LineProperties m_HoverHitProperties;

	[SerializeField]
	private bool m_RenderLineInWorldSpace = true;

	[SerializeField]
	private bool m_SwapMaterials;

	[SerializeField]
	private Material m_BaseLineMaterial;

	[SerializeField]
	private Material m_EmptyHitMaterial;

	private const float k_CurveFallbackLength = 0.06f;

	private const float k_DisableSquaredLength = 0.0001f;

	private const int k_FallBackLinePointCount = 3;

	private NativeArray<Vector3> m_InternalSamplePoints;

	private NativeArray<Vector3> m_FallBackSamplePoints;

	private Transform m_ParentTransform;

	private float m_LastHitTime;

	private float m_LengthToLastHit;

	private float m_LineLength;

	private int m_LastPosCount;

	private float m_RenderLengthMultiplier;

	private bool m_CanSwapMaterials;

	private float m_LastLineStartWidth;

	private float m_LastLineEndWidth;

	private float m_EndPointTypeChangeTime;

	private float m_LastBendRatio = 0.5f;

	private bool m_UseCustomOrigin;

	private EndPointType m_LastEndPointType;

	private bool m_LastValidSelectState;

	private Gradient m_LerpGradient;

	public LineRenderer lineRenderer
	{
		get
		{
			return m_LineRenderer;
		}
		set
		{
			m_LineRenderer = value;
			m_LineRenderer.useWorldSpace = false;
		}
	}

	public ICurveInteractionDataProvider curveInteractionDataProvider
	{
		get
		{
			return m_CurveDataProviderObjectRef.Get(m_CurveVisualObject);
		}
		set
		{
			m_CurveDataProviderObjectRef.Set(ref m_CurveVisualObject, value);
		}
	}

	public bool overrideLineOrigin
	{
		get
		{
			return m_OverrideLineOrigin;
		}
		set
		{
			m_OverrideLineOrigin = value;
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
			m_UseCustomOrigin = value != null;
		}
	}

	public int visualPointCount
	{
		get
		{
			return m_VisualPointCount;
		}
		set
		{
			m_VisualPointCount = value;
		}
	}

	public float maxVisualCurveDistance
	{
		get
		{
			return m_MaxVisualCurveDistance;
		}
		set
		{
			m_MaxVisualCurveDistance = value;
		}
	}

	public float restingVisualLineLength
	{
		get
		{
			return m_RestingVisualLineLength;
		}
		set
		{
			m_RestingVisualLineLength = value;
		}
	}

	public LineDynamicsMode lineDynamicsMode
	{
		get
		{
			return m_LineDynamicsMode;
		}
		set
		{
			m_LineDynamicsMode = value;
		}
	}

	public float retractDelay
	{
		get
		{
			return m_RetractDelay;
		}
		set
		{
			m_RetractDelay = value;
		}
	}

	public float retractDuration
	{
		get
		{
			return m_RetractDuration;
		}
		set
		{
			m_RetractDuration = value;
		}
	}

	public bool extendLineToEmptyHit
	{
		get
		{
			return m_ExtendLineToEmptyHit;
		}
		set
		{
			m_ExtendLineToEmptyHit = value;
		}
	}

	public float extensionRate
	{
		get
		{
			return m_ExtensionRate;
		}
		set
		{
			m_ExtensionRate = Mathf.Clamp(value, 0f, 30f);
		}
	}

	public float endPointExpansionRate
	{
		get
		{
			return m_EndPointExpansionRate;
		}
		set
		{
			m_EndPointExpansionRate = value;
		}
	}

	public bool computeMidPointWithComplexCurves
	{
		get
		{
			return m_ComputeMidPointWithComplexCurves;
		}
		set
		{
			m_ComputeMidPointWithComplexCurves = value;
		}
	}

	public bool snapToSelectedAttachIfAvailable
	{
		get
		{
			return m_SnapToSelectedAttachIfAvailable;
		}
		set
		{
			m_SnapToSelectedAttachIfAvailable = value;
		}
	}

	public bool snapToSnapVolumeIfAvailable
	{
		get
		{
			return m_SnapToSnapVolumeIfAvailable;
		}
		set
		{
			m_SnapToSnapVolumeIfAvailable = value;
		}
	}

	public float curveStartOffset
	{
		get
		{
			return m_CurveStartOffset;
		}
		set
		{
			m_CurveStartOffset = value;
		}
	}

	public float curveEndOffset
	{
		get
		{
			return m_CurveEndOffset;
		}
		set
		{
			m_CurveEndOffset = value;
		}
	}

	public bool customizeLinePropertiesForState
	{
		get
		{
			return m_CustomizeLinePropertiesForState;
		}
		set
		{
			m_CustomizeLinePropertiesForState = value;
		}
	}

	public float linePropertyAnimationSpeed
	{
		get
		{
			return m_LinePropertyAnimationSpeed;
		}
		set
		{
			m_LinePropertyAnimationSpeed = value;
		}
	}

	public LineProperties noValidHitProperties
	{
		get
		{
			return m_NoValidHitProperties;
		}
		set
		{
			m_NoValidHitProperties = value;
		}
	}

	public LineProperties uiHitProperties
	{
		get
		{
			return m_UIHitProperties;
		}
		set
		{
			m_UIHitProperties = value;
		}
	}

	public LineProperties uiPressHitProperties
	{
		get
		{
			return m_UIPressHitProperties;
		}
		set
		{
			m_UIPressHitProperties = value;
		}
	}

	public LineProperties selectHitProperties
	{
		get
		{
			return m_SelectHitProperties;
		}
		set
		{
			m_SelectHitProperties = value;
		}
	}

	public LineProperties hoverHitProperties
	{
		get
		{
			return m_HoverHitProperties;
		}
		set
		{
			m_HoverHitProperties = value;
		}
	}

	public bool renderLineInWorldSpace
	{
		get
		{
			return m_RenderLineInWorldSpace;
		}
		set
		{
			m_RenderLineInWorldSpace = value;
			if (m_LineRenderer != null)
			{
				m_LineRenderer.useWorldSpace = value;
			}
		}
	}

	public bool swapMaterials
	{
		get
		{
			return m_SwapMaterials;
		}
		set
		{
			m_SwapMaterials = value;
		}
	}

	public Material baseLineMaterial
	{
		get
		{
			return m_BaseLineMaterial;
		}
		set
		{
			m_BaseLineMaterial = value;
		}
	}

	public Material emptyHitMaterial
	{
		get
		{
			return m_EmptyHitMaterial;
		}
		set
		{
			m_EmptyHitMaterial = value;
		}
	}

	protected void Awake()
	{
		if (m_LineRenderer == null)
		{
			m_LineRenderer = GetComponentInChildren<LineRenderer>();
			if (m_LineRenderer == null)
			{
				Debug.LogError($"Missing Line Renderer component on Curve Caster Visual Controller {this}.", this);
				base.enabled = false;
				return;
			}
		}
		if (curveInteractionDataProvider == null)
		{
			Debug.LogError($"Missing {typeof(ICurveInteractionDataProvider)} Disabling {this}.", this);
			base.enabled = false;
			return;
		}
		m_LineRenderer.useWorldSpace = m_RenderLineInWorldSpace;
		m_ParentTransform = base.transform.parent;
		m_FallBackSamplePoints = new NativeArray<Vector3>(3, Allocator.Persistent);
		if (m_OverrideLineOrigin && m_LineOriginTransform == null)
		{
			m_LineOriginTransform = base.transform;
		}
		m_UseCustomOrigin = m_LineOriginTransform != null;
		m_CanSwapMaterials = m_SwapMaterials && m_EmptyHitMaterial != null && m_BaseLineMaterial != null;
		m_LastLineStartWidth = m_LineRenderer.startWidth;
		m_LastLineEndWidth = m_LineRenderer.endWidth;
		m_LerpGradient = m_LineRenderer.colorGradient;
	}

	protected void OnEnable()
	{
		Application.onBeforeRender += OnBeforeRenderLineVisual;
	}

	protected void OnDisable()
	{
		Application.onBeforeRender -= OnBeforeRenderLineVisual;
	}

	protected void OnDestroy()
	{
		if (m_FallBackSamplePoints.IsCreated)
		{
			m_FallBackSamplePoints.Dispose();
		}
		if (m_InternalSamplePoints.IsCreated)
		{
			m_InternalSamplePoints.Dispose();
		}
	}

	protected void LateUpdate()
	{
	}

	[BeforeRenderOrder(101)]
	private void OnBeforeRenderLineVisual()
	{
		UpdateLineVisual();
	}

	private void UpdateLineVisual()
	{
		ICurveInteractionDataProvider curveInteractionDataProvider = this.curveInteractionDataProvider;
		if (!curveInteractionDataProvider.isActive)
		{
			m_LineRenderer.enabled = false;
			return;
		}
		m_LineRenderer.enabled = true;
		ValidatePointCount();
		GetLineOriginAndDirection(out var worldOrigin, out var worldDirection);
		float validHitDistance = m_MaxVisualCurveDistance;
		Vector3 endPoint;
		EndPointType endpointInformation = GetEndpointInformation(worldOrigin, worldDirection, ref validHitDistance, out endPoint);
		float num = UpdateTargetDistance(endpointInformation, validHitDistance, m_RestingVisualLineLength, m_MaxVisualCurveDistance, m_LineDynamicsMode == LineDynamicsMode.RetractOnHitLoss, m_RetractDelay, m_RetractDuration, m_ExtensionRate);
		if (num < validHitDistance)
		{
			float3 origin = worldOrigin;
			float3 endPoint2 = endPoint;
			GetAdjustedEndPointForMaxDistance(in origin, in endPoint2, num, out var newEndPoint);
			endPoint = newEndPoint;
		}
		if (CheckIfVisualStateChanged(endpointInformation, curveInteractionDataProvider.hasValidSelect))
		{
			SwapMaterials(endpointInformation);
		}
		DetermineOffsets(endpointInformation, num, out var startOffset, out var endOffset);
		UpdateLineWidth(endpointInformation, num);
		UpdateGradient(endpointInformation);
		UpdateLinePoints(endpointInformation, worldOrigin, endPoint, worldDirection, startOffset, endOffset);
	}

	private bool CheckIfVisualStateChanged(EndPointType newPointType, bool hasValidSelect)
	{
		if (newPointType == m_LastEndPointType && m_LastValidSelectState == hasValidSelect)
		{
			return false;
		}
		m_EndPointTypeChangeTime = Time.unscaledTime;
		m_LastEndPointType = newPointType;
		m_LastValidSelectState = hasValidSelect;
		return true;
	}

	private void GetLineOriginAndDirection(out Vector3 worldOrigin, out Vector3 worldDirection)
	{
		if (m_UseCustomOrigin)
		{
			worldOrigin = m_LineOriginTransform.position;
			worldDirection = m_LineOriginTransform.forward;
		}
		else
		{
			worldOrigin = curveInteractionDataProvider.curveOrigin.position;
			worldDirection = curveInteractionDataProvider.curveOrigin.forward;
		}
	}

	private EndPointType GetEndpointInformation(Vector3 worldOrigin, Vector3 worldDirection, ref float validHitDistance, out Vector3 endPoint)
	{
		Vector3 endPoint2;
		EndPointType endPointType = curveInteractionDataProvider.TryGetCurveEndPoint(out endPoint2, m_SnapToSelectedAttachIfAvailable, m_SnapToSnapVolumeIfAvailable);
		switch (endPointType)
		{
		case EndPointType.AttachPoint:
		case EndPointType.UI:
			validHitDistance = math.distance(worldOrigin, endPoint2);
			endPoint = endPoint2;
			break;
		case EndPointType.EmptyCastHit:
		case EndPointType.ValidCastHit:
		{
			float3 worldOrigin2 = worldOrigin;
			float3 worldDirection2 = worldDirection;
			float3 hitEndPoint = endPoint2;
			float3 sampleEndPoint = curveInteractionDataProvider.lastSamplePoint;
			AdjustCastHitEndPoint(in worldOrigin2, in worldDirection2, in hitEndPoint, in sampleEndPoint, out validHitDistance, out var endPoint3);
			endPoint = endPoint3;
			break;
		}
		default:
			endPoint = curveInteractionDataProvider.lastSamplePoint;
			break;
		}
		return endPointType;
	}

	private void UpdateLinePoints(EndPointType endPointType, Vector3 worldOrigin, Vector3 worldEndPoint, Vector3 worldDirection, float startOffset = 0f, float endOffset = 0f, bool forceStraightLineFallback = false)
	{
		NativeArray<float3> targetPoints = m_InternalSamplePoints.Reinterpret<float3>();
		float lineBendRatio = GetLineBendRatio(endPointType);
		bool num = forceStraightLineFallback || lineBendRatio < 1f;
		bool flag = false;
		Vector3 vector = (m_RenderLineInWorldSpace ? worldOrigin : m_ParentTransform.InverseTransformPoint(worldOrigin));
		Vector3 vector2 = (m_RenderLineInWorldSpace ? worldEndPoint : m_ParentTransform.InverseTransformPoint(worldEndPoint));
		if (num)
		{
			float3 curveOrigin;
			float3 midPoint2;
			float3 endPoint;
			if (m_ComputeMidPointWithComplexCurves)
			{
				ICurveInteractionDataProvider curveInteractionDataProvider = this.curveInteractionDataProvider;
				if (TryGetMidPointFromCurveSamples(in curveInteractionDataProvider, out var midPoint))
				{
					Vector3 vector3 = (m_RenderLineInWorldSpace ? midPoint : m_ParentTransform.InverseTransformPoint(midPoint));
					int numTargetPoints = m_VisualPointCount;
					curveOrigin = vector;
					midPoint2 = vector3;
					endPoint = vector2;
					flag = CurveUtility.TryGenerateCubicBezierCurve(numTargetPoints, in curveOrigin, in midPoint2, in endPoint, ref targetPoints, 0.06f, startOffset, endOffset);
					goto IL_011d;
				}
			}
			Vector3 vector4 = (m_RenderLineInWorldSpace ? worldDirection : m_ParentTransform.InverseTransformDirection(worldDirection));
			int numTargetPoints2 = m_VisualPointCount;
			curveOrigin = vector;
			midPoint2 = vector4;
			endPoint = vector2;
			flag = CurveUtility.TryGenerateCubicBezierCurve(numTargetPoints2, lineBendRatio, in curveOrigin, in midPoint2, in endPoint, ref targetPoints, 0.06f, startOffset, endOffset);
		}
		goto IL_011d;
		IL_011d:
		if (!flag)
		{
			NativeArray<float3> fallBackTargetPoints = m_FallBackSamplePoints.Reinterpret<float3>();
			float3 curveOrigin = vector;
			float3 midPoint2 = vector2;
			if (ComputeFallBackLine(in curveOrigin, in midPoint2, startOffset, endOffset, ref fallBackTargetPoints))
			{
				SetLinePositions(m_FallBackSamplePoints, 3);
			}
			else
			{
				m_LineRenderer.enabled = false;
			}
		}
		else
		{
			SetLinePositions(m_InternalSamplePoints, m_VisualPointCount);
		}
	}

	private static bool TryGetMidPointFromCurveSamples(in ICurveInteractionDataProvider curveInteractionDataProvider, out Vector3 midPoint)
	{
		int length = curveInteractionDataProvider.samplePoints.Length;
		if (length > 2)
		{
			int index = length / 2;
			midPoint = curveInteractionDataProvider.samplePoints[index];
			return true;
		}
		if (length == 2)
		{
			midPoint = (curveInteractionDataProvider.samplePoints[0] + curveInteractionDataProvider.samplePoints[1]) / 2f;
			return true;
		}
		midPoint = default(Vector3);
		return false;
	}

	private bool TryGetLineProperties(EndPointType endPointType, out LineProperties properties)
	{
		if (!m_CustomizeLinePropertiesForState)
		{
			properties = null;
			return false;
		}
		properties = endPointType switch
		{
			EndPointType.None => m_NoValidHitProperties, 
			EndPointType.EmptyCastHit => m_NoValidHitProperties, 
			EndPointType.ValidCastHit => curveInteractionDataProvider.hasValidSelect ? m_SelectHitProperties : m_HoverHitProperties, 
			EndPointType.AttachPoint => m_SelectHitProperties, 
			EndPointType.UI => curveInteractionDataProvider.hasValidSelect ? m_UIPressHitProperties : m_UIHitProperties, 
			_ => m_NoValidHitProperties, 
		};
		return true;
	}

	private float GetLineBendRatio(EndPointType endPointType)
	{
		if (!TryGetLineProperties(endPointType, out var properties))
		{
			return 0.5f;
		}
		if (!properties.smoothlyCurveLine)
		{
			return 1f;
		}
		if (m_LinePropertyAnimationSpeed > 0f)
		{
			m_LastBendRatio = Mathf.Lerp(m_LastBendRatio, properties.lineBendRatio, Time.unscaledDeltaTime * m_LinePropertyAnimationSpeed);
			return m_LastBendRatio;
		}
		return properties.lineBendRatio;
	}

	private void DetermineOffsets(EndPointType endPointType, float lineDistance, out float startOffset, out float endOffset)
	{
		startOffset = m_CurveStartOffset;
		endOffset = m_CurveEndOffset;
		if (m_LineDynamicsMode == LineDynamicsMode.ExpandFromHitPoint)
		{
			float num = m_CurveEndOffset;
			float t = Time.unscaledDeltaTime * m_EndPointExpansionRate;
			float num2 = lineDistance;
			float end = m_RenderLengthMultiplier;
			if (!TryGetLineProperties(endPointType, out var properties))
			{
				end = endPointType switch
				{
					EndPointType.AttachPoint => 0.25f, 
					EndPointType.ValidCastHit => 0.75f, 
					_ => 1f, 
				};
			}
			else if (properties.customizeExpandLineDrawPercent)
			{
				end = Mathf.Clamp01(1f - properties.expandModeLineDrawPercent);
			}
			m_RenderLengthMultiplier = BurstLerpUtility.BezierLerp(m_RenderLengthMultiplier, end, t);
			num2 *= m_RenderLengthMultiplier;
			startOffset = Mathf.Max(num2 - (num + 0.001f), startOffset);
			endOffset = num;
		}
	}

	private void UpdateLineWidth(EndPointType endPointType, float targetDistance)
	{
		if (!TryGetLineProperties(endPointType, out var properties) || !properties.adjustWidth || (Mathf.Approximately(m_LastLineStartWidth, properties.starWidth) && Mathf.Approximately(m_LastLineEndWidth, properties.endWidth)))
		{
			return;
		}
		float starWidth = properties.starWidth;
		float num = ((properties.endWidthScaleDistanceFactor > 0f) ? (1f + properties.endWidthScaleDistanceFactor * targetDistance / maxVisualCurveDistance) : 1f);
		float num2 = properties.endWidth * num;
		if (m_LinePropertyAnimationSpeed > 0f)
		{
			if (Mathf.Abs(m_LastLineStartWidth - starWidth) < 0.0001f && Mathf.Abs(m_LastLineEndWidth - num2) < 0.0001f)
			{
				m_LastLineStartWidth = starWidth;
				m_LastLineEndWidth = num2;
			}
			else
			{
				float t = Time.unscaledDeltaTime * m_LinePropertyAnimationSpeed;
				m_LastLineStartWidth = Mathf.Lerp(m_LastLineStartWidth, starWidth, t);
				m_LastLineEndWidth = Mathf.Lerp(m_LastLineEndWidth, num2, t);
			}
		}
		else
		{
			m_LastLineStartWidth = starWidth;
			m_LastLineEndWidth = num2;
		}
		m_LineRenderer.startWidth = m_LastLineStartWidth;
		m_LineRenderer.endWidth = m_LastLineEndWidth;
	}

	private void UpdateGradient(EndPointType endPointType)
	{
		if (TryGetLineProperties(endPointType, out var properties) && properties.adjustGradient)
		{
			float num = Time.unscaledTime - m_EndPointTypeChangeTime;
			if (m_LinePropertyAnimationSpeed > 0f && num < 1f)
			{
				GradientUtility.Lerp(m_LerpGradient, properties.gradient, m_LerpGradient, Time.unscaledDeltaTime * m_LinePropertyAnimationSpeed);
			}
			else
			{
				GradientUtility.CopyGradient(properties.gradient, m_LerpGradient);
			}
			m_LineRenderer.colorGradient = m_LerpGradient;
		}
	}

	private void SetLinePositions(NativeArray<Vector3> targetPoints, int numPoints)
	{
		if (numPoints != m_LastPosCount)
		{
			m_LineRenderer.positionCount = numPoints;
			m_LastPosCount = numPoints;
		}
		m_LineRenderer.SetPositions(targetPoints);
	}

	private float UpdateTargetDistance(EndPointType endPointType, float validHitDistance, float minLength, float maxLength, bool retractOnHitLoss, float retractionDelay, float retractionDuration, float curveExtensionRate)
	{
		float unscaledTime = Time.unscaledTime;
		if (endPointType != 0)
		{
			if (endPointType != EndPointType.EmptyCastHit)
			{
				m_LastHitTime = unscaledTime;
			}
			if (!m_ExtendLineToEmptyHit && endPointType == EndPointType.EmptyCastHit)
			{
				m_LengthToLastHit = Mathf.Min(validHitDistance, m_LengthToLastHit);
			}
			else
			{
				m_LengthToLastHit = Mathf.Min(validHitDistance, maxLength);
			}
			m_LengthToLastHit = Mathf.Max(m_LengthToLastHit, minLength);
			if (m_LineLength > m_LengthToLastHit)
			{
				m_LineLength = m_LengthToLastHit;
				return m_LineLength;
			}
		}
		float num = unscaledTime - m_LastHitTime;
		if (retractOnHitLoss && num > retractionDelay)
		{
			float num2 = num - retractionDelay;
			if (num2 > retractionDuration)
			{
				m_LineLength = minLength;
				return m_LineLength;
			}
			m_LineLength = BurstLerpUtility.BezierLerp(m_LengthToLastHit, minLength, Mathf.Clamp01(num2 / retractionDuration));
		}
		else
		{
			float num3 = Mathf.Max(m_LengthToLastHit, minLength);
			m_LineLength = ((curveExtensionRate > 0f) ? BurstLerpUtility.BezierLerp(m_LineLength, num3, Time.unscaledDeltaTime * curveExtensionRate) : num3);
		}
		return m_LineLength;
	}

	private void SwapMaterials(EndPointType endPointType)
	{
		if (m_CanSwapMaterials && m_SwapMaterials)
		{
			m_LineRenderer.sharedMaterial = ((endPointType == EndPointType.EmptyCastHit) ? m_EmptyHitMaterial : m_BaseLineMaterial);
		}
	}

	private void ValidatePointCount()
	{
		bool isCreated = m_InternalSamplePoints.IsCreated;
		if (!isCreated || m_InternalSamplePoints.Length != m_VisualPointCount)
		{
			if (isCreated)
			{
				m_InternalSamplePoints.Dispose();
			}
			m_InternalSamplePoints = new NativeArray<Vector3>(m_VisualPointCount, Allocator.Persistent);
			m_LineRenderer.positionCount = m_VisualPointCount;
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractors_002EVisuals_002EGetAdjustedEndPointForMaxDistance_00000D24_0024PostfixBurstDelegate))]
	private static void GetAdjustedEndPointForMaxDistance(in float3 origin, in float3 endPoint, float maxDistance, out float3 newEndPoint)
	{
		GetAdjustedEndPointForMaxDistance_00000D24_0024BurstDirectCall.Invoke(in origin, in endPoint, maxDistance, out newEndPoint);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractors_002EVisuals_002EGetClosestPointOnLine_00000D25_0024PostfixBurstDelegate))]
	private static void GetClosestPointOnLine(in float3 origin, in float3 direction, in float3 point, out float3 newPoint)
	{
		GetClosestPointOnLine_00000D25_0024BurstDirectCall.Invoke(in origin, in direction, in point, out newPoint);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractors_002EVisuals_002EAdjustCastHitEndPoint_00000D26_0024PostfixBurstDelegate))]
	private static void AdjustCastHitEndPoint(in float3 worldOrigin, in float3 worldDirection, in float3 hitEndPoint, in float3 sampleEndPoint, out float validHitDistance, out float3 endPoint)
	{
		AdjustCastHitEndPoint_00000D26_0024BurstDirectCall.Invoke(in worldOrigin, in worldDirection, in hitEndPoint, in sampleEndPoint, out validHitDistance, out endPoint);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInteractors_002EVisuals_002EComputeFallBackLine_00000D27_0024PostfixBurstDelegate))]
	private static bool ComputeFallBackLine(in float3 curveOrigin, in float3 endPoint, float startOffset, float endOffset, ref NativeArray<float3> fallBackTargetPoints)
	{
		return ComputeFallBackLine_00000D27_0024BurstDirectCall.Invoke(in curveOrigin, in endPoint, startOffset, endOffset, ref fallBackTargetPoints);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetAdjustedEndPointForMaxDistance_0024BurstManaged(in float3 origin, in float3 endPoint, float maxDistance, out float3 newEndPoint)
	{
		float3 @float = math.normalize(endPoint - origin);
		newEndPoint = origin + @float * maxDistance;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetClosestPointOnLine_0024BurstManaged(in float3 origin, in float3 direction, in float3 point, out float3 newPoint)
	{
		float3 @float = math.dot(point - origin, direction) * direction;
		newPoint = origin + @float;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void AdjustCastHitEndPoint_0024BurstManaged(in float3 worldOrigin, in float3 worldDirection, in float3 hitEndPoint, in float3 sampleEndPoint, out float validHitDistance, out float3 endPoint)
	{
		GetClosestPointOnLine(in worldOrigin, in worldDirection, in hitEndPoint, out var newPoint);
		validHitDistance = math.length(newPoint - worldOrigin);
		float3 @float = math.normalize(sampleEndPoint - worldOrigin);
		endPoint = worldOrigin + @float * validHitDistance;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool ComputeFallBackLine_0024BurstManaged(in float3 curveOrigin, in float3 endPoint, float startOffset, float endOffset, ref NativeArray<float3> fallBackTargetPoints)
	{
		float3 @float = endPoint - curveOrigin;
		float num = math.lengthsq(@float);
		if (num < 0.0001f)
		{
			return false;
		}
		float3 float2 = math.rsqrt(num) * @float;
		float3 float3 = curveOrigin + float2 * startOffset;
		float3 float4 = endPoint - float2 * endOffset;
		float3 x = math.normalize(endPoint - float3);
		float3 y = math.normalize(float4 - float3);
		if (math.dot(x, y) < 0f)
		{
			return false;
		}
		fallBackTargetPoints[0] = float3;
		fallBackTargetPoints[1] = math.lerp(float3, float4, 0.5f);
		fallBackTargetPoints[2] = float4;
		return true;
	}
}
