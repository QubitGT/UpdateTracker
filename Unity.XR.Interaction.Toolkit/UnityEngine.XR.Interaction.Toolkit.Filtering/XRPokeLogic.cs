using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.Interaction.Toolkit.Attachment;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[BurstCompile]
internal class XRPokeLogic : IDisposable
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculatePokeParams_00001082_0024PostfixBurstDelegate(in float3 interactionPoint, in float3 pokableAttachPosition, in float3 axisNormal, out float interactionDepth, out float entranceVectorDot);

	internal static class CalculatePokeParams_00001082_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculatePokeParams_00001082_0024PostfixBurstDelegate>(CalculatePokeParams).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 interactionPoint, in float3 pokableAttachPosition, in float3 axisNormal, out float interactionDepth, out float entranceVectorDot)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float, ref float, void>)functionPointer)(ref interactionPoint, ref pokableAttachPosition, ref axisNormal, ref interactionDepth, ref entranceVectorDot);
					return;
				}
			}
			CalculatePokeParams_0024BurstManaged(in interactionPoint, in pokableAttachPosition, in axisNormal, out interactionDepth, out entranceVectorDot);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateInteractionPoint_00001083_0024PostfixBurstDelegate(in float3 pokerAttachPosition, in float3 axisNormal, float combinedPokeOffset, out float3 interactionPoint);

	internal static class CalculateInteractionPoint_00001083_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateInteractionPoint_00001083_0024PostfixBurstDelegate>(CalculateInteractionPoint).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 pokerAttachPosition, in float3 axisNormal, float combinedPokeOffset, out float3 interactionPoint)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, ref float3, void>)functionPointer)(ref pokerAttachPosition, ref axisNormal, combinedPokeOffset, ref interactionPoint);
					return;
				}
			}
			CalculateInteractionPoint_0024BurstManaged(in pokerAttachPosition, in axisNormal, combinedPokeOffset, out interactionPoint);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool IsVelocitySufficient_00001085_0024PostfixBurstDelegate(in float3 velocity, float threshold);

	internal static class IsVelocitySufficient_00001085_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<IsVelocitySufficient_00001085_0024PostfixBurstDelegate>(IsVelocitySufficient).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(in float3 velocity, float threshold)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, float, bool>)functionPointer)(ref velocity, threshold);
				}
			}
			return IsVelocitySufficient_0024BurstManaged(in velocity, threshold);
		}
	}

	private readonly BindableVariable<PokeStateData> m_PokeStateData = new BindableVariable<PokeStateData>();

	private Transform m_InitialTransform;

	private PokeThresholdData m_PokeThresholdData;

	private float m_SelectEntranceVectorDotThreshold;

	private readonly Dictionary<object, Transform> m_LastHoveredTransform = new Dictionary<object, Transform>();

	private readonly Dictionary<object, bool> m_HoldingHoverCheck = new Dictionary<object, bool>();

	private readonly Dictionary<Transform, HashSetList<object>> m_HoveredInteractorsOnThisTransform = new Dictionary<Transform, HashSetList<object>>();

	private readonly Dictionary<object, float> m_LastInteractorPressDepth = new Dictionary<object, float>();

	private readonly Dictionary<object, bool> m_LastRequirementsMet = new Dictionary<object, bool>();

	private const float k_DepthPercentActivationThreshold = 0.025f;

	private const float k_SquareVelocityHoverThreshold = 0.0001f;

	private float interactionAxisLength { get; set; } = 1f;


	public IReadOnlyBindableVariable<PokeStateData> pokeStateData => m_PokeStateData;

	public void Initialize(Transform associatedTransform, PokeThresholdData pokeThresholdData, Collider collider)
	{
		m_InitialTransform = associatedTransform;
		m_PokeThresholdData = pokeThresholdData;
		m_SelectEntranceVectorDotThreshold = pokeThresholdData.GetSelectEntranceVectorDotThreshold();
		if (collider != null)
		{
			interactionAxisLength = ComputeInteractionAxisLength(ComputeBounds(collider));
		}
		ResetPokeStateData(m_InitialTransform);
	}

	public void SetPokeDepth(float pokeDepth)
	{
		interactionAxisLength = pokeDepth;
	}

	public void Dispose()
	{
	}

	public bool MeetsRequirementsForSelectAction(object interactor, Vector3 pokableAttachPosition, Vector3 pokerAttachPosition, float pokeInteractionOffset, Transform pokedTransform)
	{
		if (!IsPokeDataValid(pokedTransform))
		{
			return false;
		}
		Vector3 vector = ComputeRotatedDepthEvaluationAxis(pokedTransform);
		float3 interactionPoint = CalculateInteractionPoint(pokerAttachPosition, vector, pokeInteractionOffset);
		float3 pokableAttachPosition2 = pokableAttachPosition;
		float3 axisNormal = vector;
		CalculatePokeParams(in interactionPoint, in pokableAttachPosition2, in axisNormal, out var interactionDepth, out var entranceVectorDot);
		bool isOverObject = entranceVectorDot > 0f;
		float value = CalculateDepthPercent(interactionDepth, entranceVectorDot, interactionAxisLength);
		bool meetsHoverRequirements = CalculateHoverRequirements(interactor, isOverObject, vector);
		float clampedDepthPercent = ((!meetsHoverRequirements) ? 1f : Mathf.Clamp01(value));
		bool flag = CalculateRequirements(ref meetsHoverRequirements, clampedDepthPercent, interactor);
		UpdatePokeStateData(flag, meetsHoverRequirements, clampedDepthPercent, interactor, pokerAttachPosition, pokableAttachPosition, vector, pokedTransform);
		return flag;
	}

	private bool IsPokeDataValid(Transform pokedTransform)
	{
		if (m_PokeThresholdData != null)
		{
			return pokedTransform != null;
		}
		return false;
	}

	private Vector3 CalculateInteractionPoint(Vector3 pokerAttachPosition, Vector3 axisNormal, float pokeInteractionOffset)
	{
		float combinedPokeOffset = pokeInteractionOffset + m_PokeThresholdData.interactionDepthOffset;
		float3 pokerAttachPosition2 = pokerAttachPosition;
		float3 axisNormal2 = axisNormal;
		CalculateInteractionPoint(in pokerAttachPosition2, in axisNormal2, combinedPokeOffset, out var interactionPoint);
		return interactionPoint;
	}

	private bool CalculateHoverRequirements(object interactor, bool isOverObject, float3 axisNormal)
	{
		if (!m_PokeThresholdData.enablePokeAngleThreshold)
		{
			return true;
		}
		bool result = true;
		if (!m_HoldingHoverCheck.TryGetValue(interactor, out var value) || !value)
		{
			result = CheckVelocity(interactor, isOverObject, axisNormal);
		}
		return result;
	}

	private bool CheckVelocity(object interactor, bool isOverObject, Vector3 axisNormal)
	{
		if (!isOverObject)
		{
			return false;
		}
		if (!(interactor is IAttachPointVelocityProvider attachPointVelocityProvider))
		{
			return true;
		}
		Vector3 attachPointVelocity = attachPointVelocityProvider.GetAttachPointVelocity();
		float3 velocity = attachPointVelocity;
		if (!IsVelocitySufficient(in velocity, 0.0001f))
		{
			return false;
		}
		return Vector3.Dot(-attachPointVelocity.normalized, axisNormal) > m_SelectEntranceVectorDotThreshold;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EFiltering_002ECalculatePokeParams_00001082_0024PostfixBurstDelegate))]
	private static void CalculatePokeParams(in float3 interactionPoint, in float3 pokableAttachPosition, in float3 axisNormal, out float interactionDepth, out float entranceVectorDot)
	{
		CalculatePokeParams_00001082_0024BurstDirectCall.Invoke(in interactionPoint, in pokableAttachPosition, in axisNormal, out interactionDepth, out entranceVectorDot);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EFiltering_002ECalculateInteractionPoint_00001083_0024PostfixBurstDelegate))]
	private static void CalculateInteractionPoint(in float3 pokerAttachPosition, in float3 axisNormal, float combinedPokeOffset, out float3 interactionPoint)
	{
		CalculateInteractionPoint_00001083_0024BurstDirectCall.Invoke(in pokerAttachPosition, in axisNormal, combinedPokeOffset, out interactionPoint);
	}

	[BurstCompile]
	private float CalculateDepthPercent(float interactionDepth, float entranceVectorDot, float axisLength)
	{
		return math.sign(entranceVectorDot) * interactionDepth / axisLength;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EFiltering_002EIsVelocitySufficient_00001085_0024PostfixBurstDelegate))]
	private static bool IsVelocitySufficient(in float3 velocity, float threshold)
	{
		return IsVelocitySufficient_00001085_0024BurstDirectCall.Invoke(in velocity, threshold);
	}

	private bool CalculateRequirements(ref bool meetsHoverRequirements, float clampedDepthPercent, object interactor)
	{
		bool flag = meetsHoverRequirements && clampedDepthPercent < 0.025f;
		if (m_LastRequirementsMet.TryGetValue(interactor, out var value) && value && !flag)
		{
			meetsHoverRequirements = false;
		}
		return flag;
	}

	private void UpdatePokeStateData(bool meetsRequirements, bool meetsHoverRequirements, float clampedDepthPercent, object interactor, Vector3 pokerAttachPosition, Vector3 pokableAttachPosition, Vector3 axisNormal, Transform pokedTransform)
	{
		m_HoldingHoverCheck[interactor] = meetsHoverRequirements;
		m_LastRequirementsMet[interactor] = meetsRequirements;
		m_LastInteractorPressDepth[interactor] = clampedDepthPercent;
		if (!meetsRequirements && m_HoveredInteractorsOnThisTransform.TryGetValue(pokedTransform, out var value))
		{
			int count = value.Count;
			if (count > 1)
			{
				IReadOnlyList<object> readOnlyList = value.AsList();
				for (int i = 0; i < count; i++)
				{
					object obj = readOnlyList[i];
					if (obj != interactor && m_LastInteractorPressDepth[obj] < clampedDepthPercent)
					{
						return;
					}
				}
			}
		}
		float num = ((clampedDepthPercent < 1f && !meetsRequirements) ? m_PokeThresholdData.interactionDepthOffset : 0f);
		float num2 = Mathf.Clamp(clampedDepthPercent * interactionAxisLength + num, 0f, interactionAxisLength);
		m_PokeStateData.Value = new PokeStateData
		{
			meetsRequirements = meetsRequirements,
			pokeInteractionPoint = pokerAttachPosition,
			axisAlignedPokeInteractionPoint = pokableAttachPosition + num2 * axisNormal,
			interactionStrength = 1f - clampedDepthPercent,
			axisNormal = axisNormal,
			target = pokedTransform
		};
	}

	private Vector3 ComputeRotatedDepthEvaluationAxis(Transform associatedTransform, bool isWorldSpace = true)
	{
		if (m_PokeThresholdData == null || associatedTransform == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = Vector3.zero;
		switch (m_PokeThresholdData.pokeDirection)
		{
		case PokeAxis.X:
		case PokeAxis.NegativeX:
			vector = (isWorldSpace ? associatedTransform.right : Vector3.right);
			break;
		case PokeAxis.Y:
		case PokeAxis.NegativeY:
			vector = (isWorldSpace ? associatedTransform.up : Vector3.up);
			break;
		case PokeAxis.Z:
		case PokeAxis.NegativeZ:
			vector = (isWorldSpace ? associatedTransform.forward : Vector3.forward);
			break;
		}
		PokeAxis pokeDirection = m_PokeThresholdData.pokeDirection;
		if ((uint)(pokeDirection - 1) <= 2u)
		{
			vector = -vector;
		}
		return vector;
	}

	private float ComputeInteractionAxisLength(Bounds bounds)
	{
		if (m_PokeThresholdData == null || m_InitialTransform == null)
		{
			return 0f;
		}
		Vector3 size = bounds.size;
		Vector3 position = m_InitialTransform.position;
		float result = 0f;
		switch (m_PokeThresholdData.pokeDirection)
		{
		case PokeAxis.X:
		case PokeAxis.NegativeX:
		{
			float num = bounds.center.x - position.x;
			result = size.x / 2f + num;
			break;
		}
		case PokeAxis.Y:
		case PokeAxis.NegativeY:
		{
			float num = bounds.center.y - position.y;
			result = size.y / 2f + num;
			break;
		}
		case PokeAxis.Z:
		case PokeAxis.NegativeZ:
		{
			float num = bounds.center.z - position.z;
			result = size.z / 2f + num;
			break;
		}
		}
		return result;
	}

	public void OnHoverEntered(object interactor, Pose updatedPose, Transform pokedTransform)
	{
		m_LastHoveredTransform[interactor] = pokedTransform;
		m_LastInteractorPressDepth[interactor] = 1f;
		m_HoldingHoverCheck[interactor] = false;
		m_LastRequirementsMet[interactor] = false;
		if (!m_HoveredInteractorsOnThisTransform.TryGetValue(pokedTransform, out var value))
		{
			value = new HashSetList<object>();
			m_HoveredInteractorsOnThisTransform[pokedTransform] = value;
		}
		value.Add(interactor);
	}

	public void OnHoverExited(object interactor)
	{
		m_HoldingHoverCheck[interactor] = false;
		m_LastInteractorPressDepth[interactor] = 1f;
		m_LastRequirementsMet[interactor] = false;
		if (m_LastHoveredTransform.TryGetValue(interactor, out var value))
		{
			if (m_HoveredInteractorsOnThisTransform.TryGetValue(value, out var value2))
			{
				value2.Remove(interactor);
			}
			ResetPokeStateData(value);
			m_LastHoveredTransform.Remove(interactor);
		}
		else if (m_LastHoveredTransform.Count == 0)
		{
			ResetPokeStateData(m_InitialTransform);
		}
	}

	private void ResetPokeStateData(Transform transform)
	{
		if (!(transform == null))
		{
			Vector3 position = transform.position;
			Vector3 vector = ComputeRotatedDepthEvaluationAxis(transform);
			Vector3 vector2 = position + vector * interactionAxisLength;
			m_PokeStateData.Value = new PokeStateData
			{
				meetsRequirements = false,
				pokeInteractionPoint = vector2,
				axisAlignedPokeInteractionPoint = vector2,
				interactionStrength = 0f,
				axisNormal = Vector3.zero,
				target = null
			};
		}
	}

	private static Bounds ComputeBounds(Collider targetCollider, bool rotateBoundsScale = false, Space targetSpace = Space.World)
	{
		Bounds bounds = default(Bounds);
		if (targetCollider is BoxCollider boxCollider)
		{
			bounds = new Bounds(boxCollider.center, boxCollider.size);
		}
		else if (targetCollider is SphereCollider sphereCollider)
		{
			bounds = new Bounds(sphereCollider.center, Vector3.one * (sphereCollider.radius * 2f));
		}
		else if (targetCollider is CapsuleCollider capsuleCollider)
		{
			Vector3 size = Vector3.zero;
			float num = capsuleCollider.radius * 2f;
			float height = capsuleCollider.height;
			switch (capsuleCollider.direction)
			{
			case 0:
				size = new Vector3(height, num, num);
				break;
			case 1:
				size = new Vector3(num, height, num);
				break;
			case 2:
				size = new Vector3(num, num, height);
				break;
			}
			bounds = new Bounds(capsuleCollider.center, size);
		}
		if (targetSpace == Space.Self)
		{
			return bounds;
		}
		return BoundsLocalToWorld(bounds, targetCollider.transform, rotateBoundsScale);
	}

	private static Bounds BoundsLocalToWorld(Bounds targetBounds, Transform targetTransform, bool rotateBoundsScale = false)
	{
		Vector3 lossyScale = targetTransform.lossyScale;
		Vector3 vector = lossyScale.Multiply(targetBounds.size);
		Vector3 size = (rotateBoundsScale ? (targetTransform.rotation * vector) : vector);
		return new Bounds(targetTransform.position + lossyScale.Multiply(targetBounds.center), size);
	}

	public void DrawGizmos()
	{
		if (m_PokeThresholdData != null && !(m_InitialTransform == null))
		{
			Vector3 position = m_InitialTransform.position;
			Vector3 vector = ComputeRotatedDepthEvaluationAxis(m_InitialTransform);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(position, position + vector * interactionAxisLength);
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(position, position + vector * m_PokeThresholdData.interactionDepthOffset);
			if (m_PokeStateData != null && m_PokeStateData.Value.interactionStrength > 0f)
			{
				Gizmos.color = (m_PokeStateData.Value.meetsRequirements ? Color.green : Color.yellow);
				Gizmos.DrawWireSphere(m_PokeStateData.Value.pokeInteractionPoint, 0.01f);
				Gizmos.DrawWireSphere(m_PokeStateData.Value.axisAlignedPokeInteractionPoint, 0.01f);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculatePokeParams_0024BurstManaged(in float3 interactionPoint, in float3 pokableAttachPosition, in float3 axisNormal, out float interactionDepth, out float entranceVectorDot)
	{
		float3 @float = interactionPoint - pokableAttachPosition;
		float3 x = math.project(@float, axisNormal);
		interactionDepth = math.length(x);
		entranceVectorDot = math.dot(axisNormal, math.normalizesafe(@float));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateInteractionPoint_0024BurstManaged(in float3 pokerAttachPosition, in float3 axisNormal, float combinedPokeOffset, out float3 interactionPoint)
	{
		float3 @float = axisNormal * combinedPokeOffset;
		interactionPoint = pokerAttachPosition - @float;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool IsVelocitySufficient_0024BurstManaged(in float3 velocity, float threshold)
	{
		return math.lengthsq(velocity) > threshold;
	}
}
