using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

[BurstCompile]
[AddComponentMenu("XR/XR Transform Stabilizer", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.XRTransformStabilizer.html")]
[DefaultExecutionOrder(-29985)]
public class XRTransformStabilizer : MonoBehaviour
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void StabilizeTransform_000011D4_0024PostfixBurstDelegate(in float3 startPos, in quaternion startRot, in float3 targetPos, in quaternion targetRot, float deltaTime, float positionStabilization, float angleStabilization, out float3 resultPos, out quaternion resultRot);

	internal static class StabilizeTransform_000011D4_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<StabilizeTransform_000011D4_0024PostfixBurstDelegate>(StabilizeTransform).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 startPos, in quaternion startRot, in float3 targetPos, in quaternion targetRot, float deltaTime, float positionStabilization, float angleStabilization, out float3 resultPos, out quaternion resultRot)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref quaternion, ref float3, ref quaternion, float, float, float, ref float3, ref quaternion, void>)functionPointer)(ref startPos, ref startRot, ref targetPos, ref targetRot, deltaTime, positionStabilization, angleStabilization, ref resultPos, ref resultRot);
					return;
				}
			}
			StabilizeTransform_0024BurstManaged(in startPos, in startRot, in targetPos, in targetRot, deltaTime, positionStabilization, angleStabilization, out resultPos, out resultRot);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void StabilizePosition_000011D5_0024PostfixBurstDelegate(in float3 startPos, in float3 targetPos, float deltaTime, float positionStabilization, out float3 resultPos);

	internal static class StabilizePosition_000011D5_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<StabilizePosition_000011D5_0024PostfixBurstDelegate>(StabilizePosition).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 startPos, in float3 targetPos, float deltaTime, float positionStabilization, out float3 resultPos)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, float, ref float3, void>)functionPointer)(ref startPos, ref targetPos, deltaTime, positionStabilization, ref resultPos);
					return;
				}
			}
			StabilizePosition_0024BurstManaged(in startPos, in targetPos, deltaTime, positionStabilization, out resultPos);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void StabilizeOptimalRotation_000011D6_0024PostfixBurstDelegate(in quaternion startRot, in quaternion targetRot, in quaternion alternateStartRot, float deltaTime, float angleStabilization, float alternateStabilization, float scaleFactor, out quaternion resultRot);

	internal static class StabilizeOptimalRotation_000011D6_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<StabilizeOptimalRotation_000011D6_0024PostfixBurstDelegate>(StabilizeOptimalRotation).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in quaternion startRot, in quaternion targetRot, in quaternion alternateStartRot, float deltaTime, float angleStabilization, float alternateStabilization, float scaleFactor, out quaternion resultRot)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref quaternion, ref quaternion, ref quaternion, float, float, float, float, ref quaternion, void>)functionPointer)(ref startRot, ref targetRot, ref alternateStartRot, deltaTime, angleStabilization, alternateStabilization, scaleFactor, ref resultRot);
					return;
				}
			}
			StabilizeOptimalRotation_0024BurstManaged(in startRot, in targetRot, in alternateStartRot, deltaTime, angleStabilization, alternateStabilization, scaleFactor, out resultRot);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float CalculateStabilizedLerp_000011D7_0024PostfixBurstDelegate(float distance, float timeSlice);

	internal static class CalculateStabilizedLerp_000011D7_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateStabilizedLerp_000011D7_0024PostfixBurstDelegate>(CalculateStabilizedLerp).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(float distance, float timeSlice)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<float, float, float>)functionPointer)(distance, timeSlice);
				}
			}
			return CalculateStabilizedLerp_0024BurstManaged(distance, timeSlice);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateRotationParams_000011D8_0024PostfixBurstDelegate(in float3 currentPosition, in float3 resultPosition, in float3 forward, in float3 up, in float3 rayEnd, float invScale, float angleStabilization, out quaternion antiRotation, out float scaleFactor, out float targetAngleScale);

	internal static class CalculateRotationParams_000011D8_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateRotationParams_000011D8_0024PostfixBurstDelegate>(CalculateRotationParams).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 currentPosition, in float3 resultPosition, in float3 forward, in float3 up, in float3 rayEnd, float invScale, float angleStabilization, out quaternion antiRotation, out float scaleFactor, out float targetAngleScale)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, ref float3, float, float, ref quaternion, ref float, ref float, void>)functionPointer)(ref currentPosition, ref resultPosition, ref forward, ref up, ref rayEnd, invScale, angleStabilization, ref antiRotation, ref scaleFactor, ref targetAngleScale);
					return;
				}
			}
			CalculateRotationParams_0024BurstManaged(in currentPosition, in resultPosition, in forward, in up, in rayEnd, invScale, angleStabilization, out antiRotation, out scaleFactor, out targetAngleScale);
		}
	}

	private const float k_90FPS = 1f / 90f;

	[SerializeField]
	[Tooltip("The Transform component whose position and rotation will be matched and stabilized.")]
	private Transform m_Target;

	[SerializeField]
	[RequireInterface(typeof(IXRRayProvider))]
	[Tooltip("Optional - When provided a ray, the stabilizer will calculate the rotation that keeps a ray's endpoint stable.")]
	private Object m_AimTargetObject;

	private IXRRayProvider m_AimTarget;

	[SerializeField]
	[Tooltip("If enabled, will read the target and apply stabilization in local space. Otherwise, in world space.")]
	private bool m_UseLocalSpace;

	[Header("Stabilization Parameters")]
	[SerializeField]
	[Tooltip("Maximum distance (in degrees) that stabilization will be applied.")]
	private float m_AngleStabilization = 20f;

	[SerializeField]
	[Tooltip("Maximum distance (in meters) that stabilization will be applied.")]
	private float m_PositionStabilization = 0.25f;

	private Transform m_ThisTransform;

	public Transform targetTransform
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
		}
	}

	public IXRRayProvider aimTarget
	{
		get
		{
			return m_AimTarget;
		}
		set
		{
			m_AimTarget = value;
			m_AimTargetObject = value as Object;
		}
	}

	public bool useLocalSpace
	{
		get
		{
			return m_UseLocalSpace;
		}
		set
		{
			m_UseLocalSpace = value;
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

	protected void Awake()
	{
		m_ThisTransform = base.transform;
		if (m_AimTarget == null)
		{
			m_AimTarget = m_AimTargetObject as IXRRayProvider;
		}
	}

	protected void OnEnable()
	{
		if (m_AimTarget == null)
		{
			m_AimTarget = m_AimTargetObject as IXRRayProvider;
		}
		if (!(m_Target == null))
		{
			if (m_UseLocalSpace)
			{
				m_ThisTransform.SetLocalPose(m_Target.GetLocalPose());
			}
			else
			{
				m_ThisTransform.SetWorldPose(m_Target.GetWorldPose());
			}
		}
	}

	protected void Update()
	{
		if (!(m_Target == null))
		{
			if (m_AimTarget != null && m_AimTargetObject == null && m_AimTarget == m_AimTargetObject)
			{
				Debug.LogWarning("The reference assigned to Aim Target Object has been destroyed, clearing property on XR Transform Stabilizer.", this);
				aimTarget = null;
			}
			ApplyStabilization(ref m_ThisTransform, in m_Target, in m_AimTarget, m_PositionStabilization, m_AngleStabilization, Time.deltaTime, m_UseLocalSpace);
		}
	}

	public static void ApplyStabilization(ref Transform toStabilize, in Transform target, float positionStabilization, float angleStabilization, float deltaTime, bool useLocalSpace = false)
	{
		CalculatePoses(toStabilize, target, useLocalSpace, out var currentPose, out var targetPose);
		float localScale = CalculateScaleFactor(toStabilize, useLocalSpace);
		ProcessStabilizationWithoutAimTarget(currentPose, targetPose, positionStabilization, angleStabilization, deltaTime, localScale, toStabilize, useLocalSpace);
	}

	public static void ApplyStabilization(ref Transform toStabilize, in Transform target, in float3 targetEndpoint, float positionStabilization, float angleStabilization, float deltaTime, bool useLocalSpace = false)
	{
		CalculatePoses(toStabilize, target, useLocalSpace, out var currentPose, out var targetPose);
		float localScale = CalculateScaleFactor(toStabilize, useLocalSpace);
		ProcessStabilization(currentPose, targetPose, targetEndpoint, positionStabilization, angleStabilization, deltaTime, localScale, toStabilize, useLocalSpace);
	}

	public static void ApplyStabilization(ref Transform toStabilize, in Transform target, in IXRRayProvider aimTarget, float positionStabilization, float angleStabilization, float deltaTime, bool useLocalSpace = false)
	{
		if (aimTarget == null)
		{
			ApplyStabilization(ref toStabilize, in target, positionStabilization, angleStabilization, deltaTime, useLocalSpace);
			return;
		}
		float3 targetEndpoint = aimTarget.rayEndPoint;
		ApplyStabilization(ref toStabilize, in target, in targetEndpoint, positionStabilization, angleStabilization, deltaTime);
	}

	private static void ProcessStabilization(Pose currentPose, Pose targetPose, Vector3 targetEndpoint, float positionStabilization, float angleStabilization, float deltaTime, float localScale, Transform toStabilize, bool useLocalSpace)
	{
		float3 startPos = currentPose.position;
		quaternion startRot = currentPose.rotation;
		float3 targetPos = targetPose.position;
		quaternion targetRot = targetPose.rotation;
		float invScale = 1f / localScale;
		StabilizePosition(in startPos, in targetPos, deltaTime, positionStabilization * localScale, out var resultPos);
		float3 forward = toStabilize.forward;
		float3 up = toStabilize.up;
		float3 rayEnd = targetEndpoint;
		CalculateRotationParams(in startPos, in resultPos, in forward, in up, in rayEnd, invScale, angleStabilization, out var antiRotation, out var scaleFactor, out var targetAngleScale);
		StabilizeOptimalRotation(in startRot, in targetRot, in antiRotation, deltaTime, angleStabilization, targetAngleScale, scaleFactor, out var resultRot);
		Pose pose = new Pose(resultPos, resultRot);
		if (useLocalSpace)
		{
			toStabilize.SetLocalPose(pose);
		}
		else
		{
			toStabilize.SetWorldPose(pose);
		}
	}

	private static void ProcessStabilizationWithoutAimTarget(Pose currentPose, Pose targetPose, float positionStabilization, float angleStabilization, float deltaTime, float localScale, Transform toStabilize, bool useLocalSpace)
	{
		float3 startPos = currentPose.position;
		quaternion startRot = currentPose.rotation;
		float3 targetPos = targetPose.position;
		quaternion targetRot = targetPose.rotation;
		StabilizeTransform(in startPos, in startRot, in targetPos, in targetRot, deltaTime, positionStabilization * localScale, angleStabilization, out var resultPos, out var resultRot);
		Pose pose = new Pose(resultPos, resultRot);
		if (useLocalSpace)
		{
			toStabilize.SetLocalPose(pose);
		}
		else
		{
			toStabilize.SetWorldPose(pose);
		}
	}

	private static void CalculatePoses(Transform toStabilize, Transform target, bool useLocalSpace, out Pose currentPose, out Pose targetPose)
	{
		currentPose = (useLocalSpace ? toStabilize.GetLocalPose() : toStabilize.GetWorldPose());
		targetPose = (useLocalSpace ? target.GetLocalPose() : target.GetWorldPose());
	}

	private static float CalculateScaleFactor(Transform toStabilize, bool useLocalSpace)
	{
		float num = (useLocalSpace ? toStabilize.lossyScale.x : 1f);
		if (!(Mathf.Abs(num) < 0.01f))
		{
			return num;
		}
		return 0.01f;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInputs_002EStabilizeTransform_000011D4_0024PostfixBurstDelegate))]
	private static void StabilizeTransform(in float3 startPos, in quaternion startRot, in float3 targetPos, in quaternion targetRot, float deltaTime, float positionStabilization, float angleStabilization, out float3 resultPos, out quaternion resultRot)
	{
		StabilizeTransform_000011D4_0024BurstDirectCall.Invoke(in startPos, in startRot, in targetPos, in targetRot, deltaTime, positionStabilization, angleStabilization, out resultPos, out resultRot);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInputs_002EStabilizePosition_000011D5_0024PostfixBurstDelegate))]
	private static void StabilizePosition(in float3 startPos, in float3 targetPos, float deltaTime, float positionStabilization, out float3 resultPos)
	{
		StabilizePosition_000011D5_0024BurstDirectCall.Invoke(in startPos, in targetPos, deltaTime, positionStabilization, out resultPos);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInputs_002EStabilizeOptimalRotation_000011D6_0024PostfixBurstDelegate))]
	private static void StabilizeOptimalRotation(in quaternion startRot, in quaternion targetRot, in quaternion alternateStartRot, float deltaTime, float angleStabilization, float alternateStabilization, float scaleFactor, out quaternion resultRot)
	{
		StabilizeOptimalRotation_000011D6_0024BurstDirectCall.Invoke(in startRot, in targetRot, in alternateStartRot, deltaTime, angleStabilization, alternateStabilization, scaleFactor, out resultRot);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInputs_002ECalculateStabilizedLerp_000011D7_0024PostfixBurstDelegate))]
	private static float CalculateStabilizedLerp(float distance, float timeSlice)
	{
		return CalculateStabilizedLerp_000011D7_0024BurstDirectCall.Invoke(distance, timeSlice);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EInputs_002ECalculateRotationParams_000011D8_0024PostfixBurstDelegate))]
	private static void CalculateRotationParams(in float3 currentPosition, in float3 resultPosition, in float3 forward, in float3 up, in float3 rayEnd, float invScale, float angleStabilization, out quaternion antiRotation, out float scaleFactor, out float targetAngleScale)
	{
		CalculateRotationParams_000011D8_0024BurstDirectCall.Invoke(in currentPosition, in resultPosition, in forward, in up, in rayEnd, invScale, angleStabilization, out antiRotation, out scaleFactor, out targetAngleScale);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void StabilizeTransform_0024BurstManaged(in float3 startPos, in quaternion startRot, in float3 targetPos, in quaternion targetRot, float deltaTime, float positionStabilization, float angleStabilization, out float3 resultPos, out quaternion resultRot)
	{
		if (positionStabilization > 0f)
		{
			float t = CalculateStabilizedLerp(math.length(targetPos - startPos) / positionStabilization, deltaTime);
			resultPos = math.lerp(startPos, targetPos, t);
		}
		else
		{
			resultPos = targetPos;
		}
		if (angleStabilization > 0f)
		{
			BurstMathUtility.Angle(in targetRot, in startRot, out var angle);
			float t2 = CalculateStabilizedLerp(angle / angleStabilization, deltaTime);
			resultRot = math.slerp(startRot, targetRot, t2);
		}
		else
		{
			resultRot = targetRot;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void StabilizePosition_0024BurstManaged(in float3 startPos, in float3 targetPos, float deltaTime, float positionStabilization, out float3 resultPos)
	{
		if (positionStabilization > 0f)
		{
			float t = CalculateStabilizedLerp(math.length(targetPos - startPos) / positionStabilization, deltaTime);
			resultPos = math.lerp(startPos, targetPos, t);
		}
		else
		{
			resultPos = targetPos;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void StabilizeOptimalRotation_0024BurstManaged(in quaternion startRot, in quaternion targetRot, in quaternion alternateStartRot, float deltaTime, float angleStabilization, float alternateStabilization, float scaleFactor, out quaternion resultRot)
	{
		if (angleStabilization > 0f)
		{
			BurstMathUtility.Angle(in targetRot, in startRot, out var angle);
			float num = angle / angleStabilization;
			BurstMathUtility.Angle(in targetRot, in alternateStartRot, out var angle2);
			float num2 = angle2 / alternateStabilization;
			if (num2 < num)
			{
				num2 = CalculateStabilizedLerp(num2, deltaTime * scaleFactor);
				resultRot = math.slerp(alternateStartRot, targetRot, num2);
			}
			else
			{
				num = CalculateStabilizedLerp(num, deltaTime * scaleFactor);
				resultRot = math.slerp(startRot, targetRot, num);
			}
		}
		else
		{
			resultRot = targetRot;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float CalculateStabilizedLerp_0024BurstManaged(float distance, float timeSlice)
	{
		if (distance >= 1f)
		{
			return 1f;
		}
		if (distance <= 0f)
		{
			return 0f;
		}
		float num = distance - distance * distance;
		float num2 = num * num;
		float num3 = timeSlice / (1f / 90f);
		float num4 = math.clamp(num3, 0f, 1f);
		float num5 = math.clamp(num3 - 1f, 0f, 1f);
		float num6 = math.clamp(num3 - 2f, 0f, 1f);
		return distance * num4 + num * num5 + num2 * num6;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateRotationParams_0024BurstManaged(in float3 currentPosition, in float3 resultPosition, in float3 forward, in float3 up, in float3 rayEnd, float invScale, float angleStabilization, out quaternion antiRotation, out float scaleFactor, out float targetAngleScale)
	{
		float num = math.length(rayEnd - currentPosition);
		float3 @float = currentPosition + forward * num;
		antiRotation = quaternion.LookRotationSafe(@float - resultPosition, up);
		scaleFactor = 1f + math.log(math.max(num * invScale, 1f));
		targetAngleScale = angleStabilization * math.clamp(scaleFactor, 1f, 3f);
	}
}
