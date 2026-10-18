using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Rendering;

[BurstCompile]
internal static class InstanceCullerBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void SetupCullingJobInput_0000014D_0024PostfixBurstDelegate(float lodBias, float meshLodThreshold, BatchCullingContext* context, ReceiverPlanes* receiverPlanes, ReceiverSphereCuller* receiverSphereCuller, FrustumPlaneCuller* frustumPlaneCuller, float* screenRelativeMetric, float* meshLodConstant);

	internal static class SetupCullingJobInput_0000014D_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<SetupCullingJobInput_0000014D_0024PostfixBurstDelegate>(SetupCullingJobInput).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(float lodBias, float meshLodThreshold, BatchCullingContext* context, ReceiverPlanes* receiverPlanes, ReceiverSphereCuller* receiverSphereCuller, FrustumPlaneCuller* frustumPlaneCuller, float* screenRelativeMetric, float* meshLodConstant)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<float, float, BatchCullingContext*, ReceiverPlanes*, ReceiverSphereCuller*, FrustumPlaneCuller*, float*, float*, void>)functionPointer)(lodBias, meshLodThreshold, context, receiverPlanes, receiverSphereCuller, frustumPlaneCuller, screenRelativeMetric, meshLodConstant);
					return;
				}
			}
			SetupCullingJobInput_0024BurstManaged(lodBias, meshLodThreshold, context, receiverPlanes, receiverSphereCuller, frustumPlaneCuller, screenRelativeMetric, meshLodConstant);
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002ESetupCullingJobInput_0000014D_0024PostfixBurstDelegate))]
	public unsafe static void SetupCullingJobInput(float lodBias, float meshLodThreshold, BatchCullingContext* context, ReceiverPlanes* receiverPlanes, ReceiverSphereCuller* receiverSphereCuller, FrustumPlaneCuller* frustumPlaneCuller, float* screenRelativeMetric, float* meshLodConstant)
	{
		SetupCullingJobInput_0000014D_0024BurstDirectCall.Invoke(lodBias, meshLodThreshold, context, receiverPlanes, receiverSphereCuller, frustumPlaneCuller, screenRelativeMetric, meshLodConstant);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal unsafe static void SetupCullingJobInput_0024BurstManaged(float lodBias, float meshLodThreshold, BatchCullingContext* context, ReceiverPlanes* receiverPlanes, ReceiverSphereCuller* receiverSphereCuller, FrustumPlaneCuller* frustumPlaneCuller, float* screenRelativeMetric, float* meshLodConstant)
	{
		*receiverPlanes = ReceiverPlanes.Create(in *context, Allocator.TempJob);
		*receiverSphereCuller = ReceiverSphereCuller.Create(in *context, Allocator.TempJob);
		*frustumPlaneCuller = FrustumPlaneCuller.Create(in *context, receiverPlanes->planes.AsArray(), in *receiverSphereCuller, Allocator.TempJob);
		*screenRelativeMetric = LODRenderingUtils.CalculateScreenRelativeMetricNoBias(context->lodParameters);
		*meshLodConstant = LODRenderingUtils.CalculateMeshLodConstant(context->lodParameters, *screenRelativeMetric, meshLodThreshold);
		*screenRelativeMetric /= lodBias;
	}
}
