using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.SmartTweenableVariables;

[BurstCompile]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class SmartFollowVector3TweenableVariable : Vector3TweenableVariable
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ComputeNewTweenTarget_00000422_0024PostfixBurstDelegate(in float3 currentValue, in float3 targetValue, float sqrMaxDistanceAllowed, float deltaTime, float lowerSpeed, float upperSpeed, out float newTweenTarget);

	internal static class ComputeNewTweenTarget_00000422_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeNewTweenTarget_00000422_0024PostfixBurstDelegate>(ComputeNewTweenTarget).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 currentValue, in float3 targetValue, float sqrMaxDistanceAllowed, float deltaTime, float lowerSpeed, float upperSpeed, out float newTweenTarget)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, float, float, float, ref float, void>)functionPointer)(ref currentValue, ref targetValue, sqrMaxDistanceAllowed, deltaTime, lowerSpeed, upperSpeed, ref newTweenTarget);
					return;
				}
			}
			ComputeNewTweenTarget_0024BurstManaged(in currentValue, in targetValue, sqrMaxDistanceAllowed, deltaTime, lowerSpeed, upperSpeed, out newTweenTarget);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool IsNewTargetWithinThreshold_00000423_0024PostfixBurstDelegate(in float3 currentValue, in float3 targetValue, float minDistanceAllowed, float maxDistanceAllowed, float timeSinceLastUpdate, float minToMaxDelaySeconds);

	internal static class IsNewTargetWithinThreshold_00000423_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<IsNewTargetWithinThreshold_00000423_0024PostfixBurstDelegate>(IsNewTargetWithinThreshold).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(in float3 currentValue, in float3 targetValue, float minDistanceAllowed, float maxDistanceAllowed, float timeSinceLastUpdate, float minToMaxDelaySeconds)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, float, float, float, bool>)functionPointer)(ref currentValue, ref targetValue, minDistanceAllowed, maxDistanceAllowed, timeSinceLastUpdate, minToMaxDelaySeconds);
				}
			}
			return IsNewTargetWithinThreshold_0024BurstManaged(in currentValue, in targetValue, minDistanceAllowed, maxDistanceAllowed, timeSinceLastUpdate, minToMaxDelaySeconds);
		}
	}

	private float m_MaxDistanceAllowed;

	private float m_SqrMaxDistanceAllowed;

	private float m_LastUpdateTime;

	public float minDistanceAllowed { get; set; }

	public float maxDistanceAllowed
	{
		get
		{
			return m_MaxDistanceAllowed;
		}
		set
		{
			m_MaxDistanceAllowed = value;
			m_SqrMaxDistanceAllowed = m_MaxDistanceAllowed * m_MaxDistanceAllowed;
		}
	}

	public float minToMaxDelaySeconds { get; set; }

	public SmartFollowVector3TweenableVariable(float minDistanceAllowed = 0.01f, float maxDistanceAllowed = 0.3f, float minToMaxDelaySeconds = 3f)
	{
		this.minDistanceAllowed = minDistanceAllowed;
		this.maxDistanceAllowed = maxDistanceAllowed;
		this.minToMaxDelaySeconds = minToMaxDelaySeconds;
	}

	public bool IsNewTargetWithinThreshold(float3 newTarget)
	{
		float3 currentValue = base.Value;
		return IsNewTargetWithinThreshold(in currentValue, in newTarget, minDistanceAllowed, m_MaxDistanceAllowed, Time.unscaledTime - m_LastUpdateTime, minToMaxDelaySeconds);
	}

	public bool SetTargetWithinThreshold(float3 newTarget)
	{
		bool num = IsNewTargetWithinThreshold(newTarget);
		if (num)
		{
			base.target = newTarget;
		}
		return num;
	}

	protected override void OnTargetChanged(float3 newTarget)
	{
		base.OnTargetChanged(newTarget);
		m_LastUpdateTime = Time.unscaledTime;
	}

	public void HandleSmartTween(float deltaTime, float lowerSpeed, float upperSpeed)
	{
		float3 currentValue = base.Value;
		float3 targetValue = base.target;
		ComputeNewTweenTarget(in currentValue, in targetValue, m_SqrMaxDistanceAllowed, deltaTime, lowerSpeed, upperSpeed, out var newTweenTarget);
		HandleTween(newTweenTarget);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ETweenables_002ESmartTweenableVariables_002EComputeNewTweenTarget_00000422_0024PostfixBurstDelegate))]
	private static void ComputeNewTweenTarget(in float3 currentValue, in float3 targetValue, float sqrMaxDistanceAllowed, float deltaTime, float lowerSpeed, float upperSpeed, out float newTweenTarget)
	{
		ComputeNewTweenTarget_00000422_0024BurstDirectCall.Invoke(in currentValue, in targetValue, sqrMaxDistanceAllowed, deltaTime, lowerSpeed, upperSpeed, out newTweenTarget);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ETweenables_002ESmartTweenableVariables_002EIsNewTargetWithinThreshold_00000423_0024PostfixBurstDelegate))]
	private static bool IsNewTargetWithinThreshold(in float3 currentValue, in float3 targetValue, float minDistanceAllowed, float maxDistanceAllowed, float timeSinceLastUpdate, float minToMaxDelaySeconds)
	{
		return IsNewTargetWithinThreshold_00000423_0024BurstDirectCall.Invoke(in currentValue, in targetValue, minDistanceAllowed, maxDistanceAllowed, timeSinceLastUpdate, minToMaxDelaySeconds);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ComputeNewTweenTarget_0024BurstManaged(in float3 currentValue, in float3 targetValue, float sqrMaxDistanceAllowed, float deltaTime, float lowerSpeed, float upperSpeed, out float newTweenTarget)
	{
		float num = math.distancesq(currentValue, targetValue);
		float num2 = math.clamp((1f - math.clamp(num / sqrMaxDistanceAllowed, 0f, 1f)) * upperSpeed, lowerSpeed, upperSpeed);
		newTweenTarget = deltaTime * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool IsNewTargetWithinThreshold_0024BurstManaged(in float3 currentValue, in float3 targetValue, float minDistanceAllowed, float maxDistanceAllowed, float timeSinceLastUpdate, float minToMaxDelaySeconds)
	{
		float num = math.distancesq(currentValue, targetValue);
		float num2 = math.lerp(minDistanceAllowed, maxDistanceAllowed, math.clamp(timeSinceLastUpdate / minToMaxDelaySeconds, 0f, 1f));
		return num > num2 * num2;
	}
}
