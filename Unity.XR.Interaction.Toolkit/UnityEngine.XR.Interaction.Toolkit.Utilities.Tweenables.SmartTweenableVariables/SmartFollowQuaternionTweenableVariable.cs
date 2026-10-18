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
public class SmartFollowQuaternionTweenableVariable : QuaternionTweenableVariable
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ComputeNewTweenTarget_00000416_0024PostfixBurstDelegate(float deltaTime, float angleOffsetDeg, float maxAngleAllowed, float lowerSpeed, float upperSpeed, out float newTweenTarget);

	internal static class ComputeNewTweenTarget_00000416_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ComputeNewTweenTarget_00000416_0024PostfixBurstDelegate>(ComputeNewTweenTarget).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(float deltaTime, float angleOffsetDeg, float maxAngleAllowed, float lowerSpeed, float upperSpeed, out float newTweenTarget)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<float, float, float, float, float, ref float, void>)functionPointer)(deltaTime, angleOffsetDeg, maxAngleAllowed, lowerSpeed, upperSpeed, ref newTweenTarget);
					return;
				}
			}
			ComputeNewTweenTarget_0024BurstManaged(deltaTime, angleOffsetDeg, maxAngleAllowed, lowerSpeed, upperSpeed, out newTweenTarget);
		}
	}

	private float m_LastUpdateTime;

	public float minAngleAllowed { get; set; }

	public float maxAngleAllowed { get; set; }

	public float minToMaxDelaySeconds { get; set; }

	public SmartFollowQuaternionTweenableVariable(float minAngleAllowed = 0.1f, float maxAngleAllowed = 5f, float minToMaxDelaySeconds = 3f)
	{
		this.minAngleAllowed = minAngleAllowed;
		this.maxAngleAllowed = maxAngleAllowed;
		this.minToMaxDelaySeconds = minToMaxDelaySeconds;
	}

	public bool IsNewTargetWithinThreshold(Quaternion newTarget)
	{
		float num = Quaternion.Angle(base.target, newTarget);
		float num2 = Mathf.Lerp(t: Mathf.Clamp01((Time.unscaledTime - m_LastUpdateTime) / minToMaxDelaySeconds), a: minAngleAllowed, b: maxAngleAllowed);
		return num > num2;
	}

	public bool SetTargetWithinThreshold(Quaternion newTarget)
	{
		bool num = IsNewTargetWithinThreshold(newTarget);
		if (num)
		{
			base.target = newTarget;
		}
		return num;
	}

	protected override void OnTargetChanged(Quaternion newTarget)
	{
		m_LastUpdateTime = Time.unscaledTime;
	}

	public void HandleSmartTween(float deltaTime, float lowerSpeed, float upperSpeed)
	{
		float angleOffsetDeg = Quaternion.Angle(base.target, base.Value);
		ComputeNewTweenTarget(deltaTime, angleOffsetDeg, maxAngleAllowed, lowerSpeed, upperSpeed, out var newTweenTarget);
		HandleTween(newTweenTarget);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ETweenables_002ESmartTweenableVariables_002EComputeNewTweenTarget_00000416_0024PostfixBurstDelegate))]
	private static void ComputeNewTweenTarget(float deltaTime, float angleOffsetDeg, float maxAngleAllowed, float lowerSpeed, float upperSpeed, out float newTweenTarget)
	{
		ComputeNewTweenTarget_00000416_0024BurstDirectCall.Invoke(deltaTime, angleOffsetDeg, maxAngleAllowed, lowerSpeed, upperSpeed, out newTweenTarget);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ComputeNewTweenTarget_0024BurstManaged(float deltaTime, float angleOffsetDeg, float maxAngleAllowed, float lowerSpeed, float upperSpeed, out float newTweenTarget)
	{
		float num = 1f - math.clamp(angleOffsetDeg / maxAngleAllowed, 0f, 1f);
		newTweenTarget = deltaTime * math.clamp(num * upperSpeed, lowerSpeed, upperSpeed);
	}
}
