using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

[BurstCompile]
public static class BurstPhysicsUtils
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetSphereOverlapParameters_0000035F_0024PostfixBurstDelegate(in Vector3 overlapStart, in Vector3 overlapEnd, out Vector3 normalizedOverlapVector, out float overlapSqrMagnitude, out float overlapDistance);

	internal static class GetSphereOverlapParameters_0000035F_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetSphereOverlapParameters_0000035F_0024PostfixBurstDelegate>(GetSphereOverlapParameters).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 overlapStart, in Vector3 overlapEnd, out Vector3 normalizedOverlapVector, out float overlapSqrMagnitude, out float overlapDistance)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Vector3, ref float, ref float, void>)functionPointer)(ref overlapStart, ref overlapEnd, ref normalizedOverlapVector, ref overlapSqrMagnitude, ref overlapDistance);
					return;
				}
			}
			GetSphereOverlapParameters_0024BurstManaged(in overlapStart, in overlapEnd, out normalizedOverlapVector, out overlapSqrMagnitude, out overlapDistance);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetConecastParameters_00000360_0024PostfixBurstDelegate(float angleRadius, float offset, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax);

	internal static class GetConecastParameters_00000360_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetConecastParameters_00000360_0024PostfixBurstDelegate>(GetConecastParameters).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(float angleRadius, float offset, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<float, float, float, ref Vector3, ref Vector3, ref float, ref float, void>)functionPointer)(angleRadius, offset, maxOffset, ref direction, ref originOffset, ref radius, ref castMax);
					return;
				}
			}
			GetConecastParameters_0024BurstManaged(angleRadius, offset, maxOffset, in direction, out originOffset, out radius, out castMax);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetMultiSegmentConecastParameters_00000361_0024PostfixBurstDelegate(float angleRadius, float segmentOffset, float offsetFromOrigin, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax);

	internal static class GetMultiSegmentConecastParameters_00000361_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetMultiSegmentConecastParameters_00000361_0024PostfixBurstDelegate>(GetMultiSegmentConecastParameters).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(float angleRadius, float segmentOffset, float offsetFromOrigin, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<float, float, float, float, ref Vector3, ref Vector3, ref float, ref float, void>)functionPointer)(angleRadius, segmentOffset, offsetFromOrigin, maxOffset, ref direction, ref originOffset, ref radius, ref castMax);
					return;
				}
			}
			GetMultiSegmentConecastParameters_0024BurstManaged(angleRadius, segmentOffset, offsetFromOrigin, maxOffset, in direction, out originOffset, out radius, out castMax);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetConecastOffset_00000362_0024PostfixBurstDelegate(in float3 origin, in float3 conePoint, in float3 direction, out float coneOffset);

	internal static class GetConecastOffset_00000362_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetConecastOffset_00000362_0024PostfixBurstDelegate>(GetConecastOffset).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 origin, in float3 conePoint, in float3 direction, out float coneOffset)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float, void>)functionPointer)(ref origin, ref conePoint, ref direction, ref coneOffset);
					return;
				}
			}
			GetConecastOffset_0024BurstManaged(in origin, in conePoint, in direction, out coneOffset);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EGetSphereOverlapParameters_0000035F_0024PostfixBurstDelegate))]
	public static void GetSphereOverlapParameters(in Vector3 overlapStart, in Vector3 overlapEnd, out Vector3 normalizedOverlapVector, out float overlapSqrMagnitude, out float overlapDistance)
	{
		GetSphereOverlapParameters_0000035F_0024BurstDirectCall.Invoke(in overlapStart, in overlapEnd, out normalizedOverlapVector, out overlapSqrMagnitude, out overlapDistance);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EGetConecastParameters_00000360_0024PostfixBurstDelegate))]
	public static void GetConecastParameters(float angleRadius, float offset, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax)
	{
		GetConecastParameters_00000360_0024BurstDirectCall.Invoke(angleRadius, offset, maxOffset, in direction, out originOffset, out radius, out castMax);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EGetMultiSegmentConecastParameters_00000361_0024PostfixBurstDelegate))]
	internal static void GetMultiSegmentConecastParameters(float angleRadius, float segmentOffset, float offsetFromOrigin, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax)
	{
		GetMultiSegmentConecastParameters_00000361_0024BurstDirectCall.Invoke(angleRadius, segmentOffset, offsetFromOrigin, maxOffset, in direction, out originOffset, out radius, out castMax);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EGetConecastOffset_00000362_0024PostfixBurstDelegate))]
	public static void GetConecastOffset(in float3 origin, in float3 conePoint, in float3 direction, out float coneOffset)
	{
		GetConecastOffset_00000362_0024BurstDirectCall.Invoke(in origin, in conePoint, in direction, out coneOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetSphereOverlapParameters_0024BurstManaged(in Vector3 overlapStart, in Vector3 overlapEnd, out Vector3 normalizedOverlapVector, out float overlapSqrMagnitude, out float overlapDistance)
	{
		Vector3 vector = overlapEnd - overlapStart;
		overlapSqrMagnitude = math.distancesq(overlapStart, overlapEnd);
		overlapDistance = math.sqrt(overlapSqrMagnitude);
		normalizedOverlapVector = vector / overlapDistance;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetConecastParameters_0024BurstManaged(float angleRadius, float offset, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax)
	{
		castMax = math.clamp(offset, 0.125f, maxOffset);
		radius = angleRadius * (offset + castMax);
		originOffset = direction * (offset - radius);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetMultiSegmentConecastParameters_0024BurstManaged(float angleRadius, float segmentOffset, float offsetFromOrigin, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax)
	{
		castMax = math.clamp(segmentOffset, 0.125f, maxOffset);
		if (segmentOffset + castMax > maxOffset)
		{
			castMax = math.clamp(maxOffset - segmentOffset, 0.125f, castMax);
		}
		radius = angleRadius * (offsetFromOrigin + castMax);
		originOffset = direction * (segmentOffset - radius);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetConecastOffset_0024BurstManaged(in float3 origin, in float3 conePoint, in float3 direction, out float coneOffset)
	{
		float3 @float = conePoint - origin;
		float num = math.dot(@float, direction);
		float3 x = @float - direction * num;
		coneOffset = math.length(x);
	}
}
