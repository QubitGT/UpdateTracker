using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Curves;

[BurstCompile]
internal static class CurveUtility
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SampleQuadraticBezierPoint_0000043F_0024PostfixBurstDelegate(in float3 p0, in float3 p1, in float3 p2, float t, out float3 point);

	internal static class SampleQuadraticBezierPoint_0000043F_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SampleQuadraticBezierPoint_0000043F_0024PostfixBurstDelegate>(SampleQuadraticBezierPoint).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 p0, in float3 p1, in float3 p2, float t, out float3 point)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, float, ref float3, void>)functionPointer)(ref p0, ref p1, ref p2, t, ref point);
					return;
				}
			}
			SampleQuadraticBezierPoint_0024BurstManaged(in p0, in p1, in p2, t, out point);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SampleCubicBezierPoint_00000440_0024PostfixBurstDelegate(in float3 p0, in float3 p1, in float3 p2, in float3 p3, float t, out float3 point);

	internal static class SampleCubicBezierPoint_00000440_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SampleCubicBezierPoint_00000440_0024PostfixBurstDelegate>(SampleCubicBezierPoint).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 p0, in float3 p1, in float3 p2, in float3 p3, float t, out float3 point)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, float, ref float3, void>)functionPointer)(ref p0, ref p1, ref p2, ref p3, t, ref point);
					return;
				}
			}
			SampleCubicBezierPoint_0024BurstManaged(in p0, in p1, in p2, in p3, t, out point);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ElevateQuadraticToCubicBezier_00000441_0024PostfixBurstDelegate(in float3 p0, in float3 p1, in float3 p2, out float3 c0, out float3 c1, out float3 c2, out float3 c3);

	internal static class ElevateQuadraticToCubicBezier_00000441_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ElevateQuadraticToCubicBezier_00000441_0024PostfixBurstDelegate>(ElevateQuadraticToCubicBezier).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 p0, in float3 p1, in float3 p2, out float3 c0, out float3 c1, out float3 c2, out float3 c3)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, ref float3, ref float3, ref float3, void>)functionPointer)(ref p0, ref p1, ref p2, ref c0, ref c1, ref c2, ref c3);
					return;
				}
			}
			ElevateQuadraticToCubicBezier_0024BurstManaged(in p0, in p1, in p2, out c0, out c1, out c2, out c3);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GenerateCubicBezierCurve_00000442_0024PostfixBurstDelegate(int numTargetPoints, float curveRatio, in float3 lineOrigin, in float3 lineDirection, in float3 endPoint, ref NativeArray<float3> targetPoints);

	internal static class GenerateCubicBezierCurve_00000442_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GenerateCubicBezierCurve_00000442_0024PostfixBurstDelegate>(GenerateCubicBezierCurve).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(int numTargetPoints, float curveRatio, in float3 lineOrigin, in float3 lineDirection, in float3 endPoint, ref NativeArray<float3> targetPoints)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<int, float, ref float3, ref float3, ref float3, ref NativeArray<float3>, void>)functionPointer)(numTargetPoints, curveRatio, ref lineOrigin, ref lineDirection, ref endPoint, ref targetPoints);
					return;
				}
			}
			GenerateCubicBezierCurve_0024BurstManaged(numTargetPoints, curveRatio, in lineOrigin, in lineDirection, in endPoint, ref targetPoints);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool TryGenerateCubicBezierCurve_00000443_0024PostfixBurstDelegate(int numTargetPoints, float curveRatio, in float3 curveOrigin, in float3 curveDirection, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f);

	internal static class TryGenerateCubicBezierCurve_00000443_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<TryGenerateCubicBezierCurve_00000443_0024PostfixBurstDelegate>(TryGenerateCubicBezierCurve).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(int numTargetPoints, float curveRatio, in float3 curveOrigin, in float3 curveDirection, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<int, float, ref float3, ref float3, ref float3, ref NativeArray<float3>, float, float, float, bool>)functionPointer)(numTargetPoints, curveRatio, ref curveOrigin, ref curveDirection, ref endPoint, ref targetPoints, minLineLength, startOffset, endOffset);
				}
			}
			return TryGenerateCubicBezierCurve_0024BurstManaged(numTargetPoints, curveRatio, in curveOrigin, in curveDirection, in endPoint, ref targetPoints, minLineLength, startOffset, endOffset);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool TryGenerateCubicBezierCurve_00000444_0024PostfixBurstDelegate(int numTargetPoints, in float3 curveOrigin, in float3 midPoint, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f);

	internal static class TryGenerateCubicBezierCurve_00000444_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<TryGenerateCubicBezierCurve_00000444_0024PostfixBurstDelegate>(TryGenerateCubicBezierCurve).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(int numTargetPoints, in float3 curveOrigin, in float3 midPoint, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<int, ref float3, ref float3, ref float3, ref NativeArray<float3>, float, float, float, bool>)functionPointer)(numTargetPoints, ref curveOrigin, ref midPoint, ref endPoint, ref targetPoints, minLineLength, startOffset, endOffset);
				}
			}
			return TryGenerateCubicBezierCurve_0024BurstManaged(numTargetPoints, in curveOrigin, in midPoint, in endPoint, ref targetPoints, minLineLength, startOffset, endOffset);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float ApproximateCubicBezierLength_00000446_0024PostfixBurstDelegate(in float3 p0, in float3 p1, in float3 p2, in float3 p3, int subdivisions);

	internal static class ApproximateCubicBezierLength_00000446_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ApproximateCubicBezierLength_00000446_0024PostfixBurstDelegate>(ApproximateCubicBezierLength).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(in float3 p0, in float3 p1, in float3 p2, in float3 p3, int subdivisions)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, ref float3, int, float>)functionPointer)(ref p0, ref p1, ref p2, ref p3, subdivisions);
				}
			}
			return ApproximateCubicBezierLength_0024BurstManaged(in p0, in p1, in p2, in p3, subdivisions);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SampleProjectilePoint_00000447_0024PostfixBurstDelegate(in float3 initialPosition, in float3 initialVelocity, in float3 constantAcceleration, float time, out float3 point);

	internal static class SampleProjectilePoint_00000447_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SampleProjectilePoint_00000447_0024PostfixBurstDelegate>(SampleProjectilePoint).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 initialPosition, in float3 initialVelocity, in float3 constantAcceleration, float time, out float3 point)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, float, ref float3, void>)functionPointer)(ref initialPosition, ref initialVelocity, ref constantAcceleration, time, ref point);
					return;
				}
			}
			SampleProjectilePoint_0024BurstManaged(in initialPosition, in initialVelocity, in constantAcceleration, time, out point);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateProjectileFlightTime_00000448_0024PostfixBurstDelegate(float velocityMagnitude, float gravityAcceleration, float angleRad, float height, float extraFlightTime, out float flightTime);

	internal static class CalculateProjectileFlightTime_00000448_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateProjectileFlightTime_00000448_0024PostfixBurstDelegate>(CalculateProjectileFlightTime).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(float velocityMagnitude, float gravityAcceleration, float angleRad, float height, float extraFlightTime, out float flightTime)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<float, float, float, float, float, ref float, void>)functionPointer)(velocityMagnitude, gravityAcceleration, angleRad, height, extraFlightTime, ref flightTime);
					return;
				}
			}
			CalculateProjectileFlightTime_0024BurstManaged(velocityMagnitude, gravityAcceleration, angleRad, height, extraFlightTime, out flightTime);
		}
	}

	private const float k_EightEpsilon = 9.536743E-07f;

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002ESampleQuadraticBezierPoint_0000043F_0024PostfixBurstDelegate))]
	public static void SampleQuadraticBezierPoint(in float3 p0, in float3 p1, in float3 p2, float t, out float3 point)
	{
		SampleQuadraticBezierPoint_0000043F_0024BurstDirectCall.Invoke(in p0, in p1, in p2, t, out point);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002ESampleCubicBezierPoint_00000440_0024PostfixBurstDelegate))]
	public static void SampleCubicBezierPoint(in float3 p0, in float3 p1, in float3 p2, in float3 p3, float t, out float3 point)
	{
		SampleCubicBezierPoint_00000440_0024BurstDirectCall.Invoke(in p0, in p1, in p2, in p3, t, out point);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002EElevateQuadraticToCubicBezier_00000441_0024PostfixBurstDelegate))]
	public static void ElevateQuadraticToCubicBezier(in float3 p0, in float3 p1, in float3 p2, out float3 c0, out float3 c1, out float3 c2, out float3 c3)
	{
		ElevateQuadraticToCubicBezier_00000441_0024BurstDirectCall.Invoke(in p0, in p1, in p2, out c0, out c1, out c2, out c3);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002EGenerateCubicBezierCurve_00000442_0024PostfixBurstDelegate))]
	public static void GenerateCubicBezierCurve(int numTargetPoints, float curveRatio, in float3 lineOrigin, in float3 lineDirection, in float3 endPoint, ref NativeArray<float3> targetPoints)
	{
		GenerateCubicBezierCurve_00000442_0024BurstDirectCall.Invoke(numTargetPoints, curveRatio, in lineOrigin, in lineDirection, in endPoint, ref targetPoints);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002ETryGenerateCubicBezierCurve_00000443_0024PostfixBurstDelegate))]
	public static bool TryGenerateCubicBezierCurve(int numTargetPoints, float curveRatio, in float3 curveOrigin, in float3 curveDirection, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f)
	{
		return TryGenerateCubicBezierCurve_00000443_0024BurstDirectCall.Invoke(numTargetPoints, curveRatio, in curveOrigin, in curveDirection, in endPoint, ref targetPoints, minLineLength, startOffset, endOffset);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002ETryGenerateCubicBezierCurve_00000444_0024PostfixBurstDelegate))]
	public static bool TryGenerateCubicBezierCurve(int numTargetPoints, in float3 curveOrigin, in float3 midPoint, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f)
	{
		return TryGenerateCubicBezierCurve_00000444_0024BurstDirectCall.Invoke(numTargetPoints, in curveOrigin, in midPoint, in endPoint, ref targetPoints, minLineLength, startOffset, endOffset);
	}

	private static bool TryGenerateCubicBezierCurveCore(int numTargetPoints, in float3 curveOrigin, in float3 midPoint, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f)
	{
		ElevateQuadraticToCubicBezier(in curveOrigin, in midPoint, in endPoint, out var c, out var c2, out var c3, out var c4);
		bool flag = startOffset > 0f;
		bool flag2 = endOffset > 0f;
		float num = 0f;
		float num2 = 1f;
		if (flag || flag2)
		{
			float num3 = startOffset + endOffset;
			float num4 = ApproximateCubicBezierLength(in c, in c2, in c3, in c4, math.max(numTargetPoints / 2, 4));
			if (num3 > num4 || num4 - num3 < minLineLength)
			{
				return false;
			}
			if (flag)
			{
				num = startOffset / num4;
			}
			if (flag2)
			{
				num2 = (num4 - endOffset) / num4;
			}
		}
		float num5 = (num2 - num) / (float)(numTargetPoints - 1);
		for (int i = 0; i < numTargetPoints; i++)
		{
			float t = num + (float)i * num5;
			SampleCubicBezierPoint(in c, in c2, in c3, in c4, t, out var point);
			targetPoints[i] = point;
		}
		return true;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002EApproximateCubicBezierLength_00000446_0024PostfixBurstDelegate))]
	public static float ApproximateCubicBezierLength(in float3 p0, in float3 p1, in float3 p2, in float3 p3, int subdivisions)
	{
		return ApproximateCubicBezierLength_00000446_0024BurstDirectCall.Invoke(in p0, in p1, in p2, in p3, subdivisions);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002ESampleProjectilePoint_00000447_0024PostfixBurstDelegate))]
	public static void SampleProjectilePoint(in float3 initialPosition, in float3 initialVelocity, in float3 constantAcceleration, float time, out float3 point)
	{
		SampleProjectilePoint_00000447_0024BurstDirectCall.Invoke(in initialPosition, in initialVelocity, in constantAcceleration, time, out point);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ECurves_002ECalculateProjectileFlightTime_00000448_0024PostfixBurstDelegate))]
	public static void CalculateProjectileFlightTime(float velocityMagnitude, float gravityAcceleration, float angleRad, float height, float extraFlightTime, out float flightTime)
	{
		CalculateProjectileFlightTime_00000448_0024BurstDirectCall.Invoke(velocityMagnitude, gravityAcceleration, angleRad, height, extraFlightTime, out flightTime);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void SampleQuadraticBezierPoint_0024BurstManaged(in float3 p0, in float3 p1, in float3 p2, float t, out float3 point)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = t * t;
		point = num2 * p0 + 2f * num * t * p1 + num3 * p2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void SampleCubicBezierPoint_0024BurstManaged(in float3 p0, in float3 p1, in float3 p2, in float3 p3, float t, out float3 point)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = num2 * num;
		float num4 = t * t;
		float num5 = num4 * t;
		point = num3 * p0 + 3f * num2 * t * p1 + 3f * num * num4 * p2 + num5 * p3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ElevateQuadraticToCubicBezier_0024BurstManaged(in float3 p0, in float3 p1, in float3 p2, out float3 c0, out float3 c1, out float3 c2, out float3 c3)
	{
		c0 = p0;
		c1 = p0 + 2f / 3f * (p1 - p0);
		c2 = p2 + 2f / 3f * (p1 - p2);
		c3 = p2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GenerateCubicBezierCurve_0024BurstManaged(int numTargetPoints, float curveRatio, in float3 lineOrigin, in float3 lineDirection, in float3 endPoint, ref NativeArray<float3> targetPoints)
	{
		float num = math.length(endPoint - lineOrigin);
		float3 p = lineOrigin + lineDirection * num * curveRatio;
		ElevateQuadraticToCubicBezier(in lineOrigin, in p, in endPoint, out var c, out var c2, out var c3, out var c4);
		targetPoints[0] = lineOrigin;
		float num2 = 1f / (float)(numTargetPoints - 1);
		for (int i = 1; i < numTargetPoints; i++)
		{
			float t = (float)i * num2;
			SampleCubicBezierPoint(in c, in c2, in c3, in c4, t, out var point);
			targetPoints[i] = point;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool TryGenerateCubicBezierCurve_0024BurstManaged(int numTargetPoints, float curveRatio, in float3 curveOrigin, in float3 curveDirection, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f)
	{
		float num = math.length(endPoint - curveOrigin);
		float num2 = startOffset + endOffset;
		if (num2 > num || num - num2 < minLineLength)
		{
			return false;
		}
		float3 midPoint = ((!(curveRatio > 0f)) ? math.lerp(curveOrigin, endPoint, 0.5f) : (curveOrigin + curveDirection * num * curveRatio));
		return TryGenerateCubicBezierCurveCore(numTargetPoints, in curveOrigin, in midPoint, in endPoint, ref targetPoints, minLineLength, startOffset, endOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool TryGenerateCubicBezierCurve_0024BurstManaged(int numTargetPoints, in float3 curveOrigin, in float3 midPoint, in float3 endPoint, ref NativeArray<float3> targetPoints, float minLineLength = 0.005f, float startOffset = 0f, float endOffset = 0f)
	{
		float num = math.length(endPoint - curveOrigin);
		float num2 = startOffset + endOffset;
		if (num2 > num || num - num2 < minLineLength)
		{
			return false;
		}
		return TryGenerateCubicBezierCurveCore(numTargetPoints, in curveOrigin, in midPoint, in endPoint, ref targetPoints, minLineLength, startOffset, endOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float ApproximateCubicBezierLength_0024BurstManaged(in float3 p0, in float3 p1, in float3 p2, in float3 p3, int subdivisions)
	{
		float num = 0f;
		float3 y = p0;
		for (int i = 1; i <= subdivisions; i++)
		{
			float t = (float)i / (float)subdivisions;
			SampleCubicBezierPoint(in p0, in p1, in p2, in p3, t, out var point);
			num += math.distance(point, y);
			y = point;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void SampleProjectilePoint_0024BurstManaged(in float3 initialPosition, in float3 initialVelocity, in float3 constantAcceleration, float time, out float3 point)
	{
		point = initialPosition + initialVelocity * time + constantAcceleration * (0.5f * time * time);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateProjectileFlightTime_0024BurstManaged(float velocityMagnitude, float gravityAcceleration, float angleRad, float height, float extraFlightTime, out float flightTime)
	{
		float num = velocityMagnitude * angleRad;
		if (height < 0f)
		{
			flightTime = 0f;
		}
		else if (math.abs(height) < 9.536743E-07f)
		{
			flightTime = 2f * num / gravityAcceleration;
		}
		else
		{
			flightTime = (num + math.sqrt(num * num + 2f * gravityAcceleration * height)) / gravityAcceleration;
		}
		flightTime = math.max(flightTime + extraFlightTime, 0f);
	}
}
