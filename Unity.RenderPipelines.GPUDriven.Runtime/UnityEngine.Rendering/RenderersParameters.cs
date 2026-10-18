using System;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering;

internal struct RenderersParameters
{
	[Flags]
	public enum Flags
	{
		None = 0,
		UseBoundingSphereParameter = 1
	}

	public static class ParamNames
	{
		public static readonly int _BaseColor;

		public static readonly int unity_SpecCube0_HDR;

		public static readonly int unity_SHCoefficients;

		public static readonly int unity_LightmapST;

		public static readonly int unity_ObjectToWorld;

		public static readonly int unity_WorldToObject;

		public static readonly int unity_MatrixPreviousM;

		public static readonly int unity_MatrixPreviousMI;

		public static readonly int unity_WorldBoundingSphere;

		public static readonly int[] DOTS_ST_WindParams;

		public static readonly int[] DOTS_ST_WindHistoryParams;

		static ParamNames()
		{
			_BaseColor = Shader.PropertyToID("_BaseColor");
			unity_SpecCube0_HDR = Shader.PropertyToID("unity_SpecCube0_HDR");
			unity_SHCoefficients = Shader.PropertyToID("unity_SHCoefficients");
			unity_LightmapST = Shader.PropertyToID("unity_LightmapST");
			unity_ObjectToWorld = Shader.PropertyToID("unity_ObjectToWorld");
			unity_WorldToObject = Shader.PropertyToID("unity_WorldToObject");
			unity_MatrixPreviousM = Shader.PropertyToID("unity_MatrixPreviousM");
			unity_MatrixPreviousMI = Shader.PropertyToID("unity_MatrixPreviousMI");
			unity_WorldBoundingSphere = Shader.PropertyToID("unity_WorldBoundingSphere");
			DOTS_ST_WindParams = new int[16];
			DOTS_ST_WindHistoryParams = new int[16];
			for (int i = 0; i < 16; i++)
			{
				DOTS_ST_WindParams[i] = Shader.PropertyToID($"DOTS_ST_WindParam{i}");
				DOTS_ST_WindHistoryParams[i] = Shader.PropertyToID($"DOTS_ST_WindHistoryParam{i}");
			}
		}
	}

	public struct ParamInfo
	{
		public int index;

		public int gpuAddress;

		public int uintOffset;

		public bool valid => index != 0;
	}

	private static int s_uintSize = UnsafeUtility.SizeOf<uint>();

	public ParamInfo lightmapScale;

	public ParamInfo localToWorld;

	public ParamInfo worldToLocal;

	public ParamInfo matrixPreviousM;

	public ParamInfo matrixPreviousMI;

	public ParamInfo shCoefficients;

	public ParamInfo boundingSphere;

	public ParamInfo[] windParams;

	public ParamInfo[] windHistoryParams;

	public static GPUInstanceDataBuffer CreateInstanceDataBuffer(Flags flags, in InstanceNumInfo instanceNumInfo)
	{
		using GPUInstanceDataBufferBuilder gPUInstanceDataBufferBuilder = default(GPUInstanceDataBufferBuilder);
		gPUInstanceDataBufferBuilder.AddComponent<Vector4>(ParamNames._BaseColor, isOverriden: false, isPerInstance: false, InstanceType.MeshRenderer);
		gPUInstanceDataBufferBuilder.AddComponent<Vector4>(ParamNames.unity_SpecCube0_HDR, isOverriden: false, isPerInstance: false, InstanceType.MeshRenderer);
		gPUInstanceDataBufferBuilder.AddComponent<SHCoefficients>(ParamNames.unity_SHCoefficients, isOverriden: true, isPerInstance: true, InstanceType.MeshRenderer, InstanceComponentGroup.LightProbe);
		gPUInstanceDataBufferBuilder.AddComponent<Vector4>(ParamNames.unity_LightmapST, isOverriden: true, isPerInstance: true, InstanceType.MeshRenderer, InstanceComponentGroup.Lightmap);
		gPUInstanceDataBufferBuilder.AddComponent<PackedMatrix>(ParamNames.unity_ObjectToWorld, isOverriden: true, isPerInstance: true, InstanceType.MeshRenderer);
		gPUInstanceDataBufferBuilder.AddComponent<PackedMatrix>(ParamNames.unity_WorldToObject, isOverriden: true, isPerInstance: true, InstanceType.MeshRenderer);
		gPUInstanceDataBufferBuilder.AddComponent<PackedMatrix>(ParamNames.unity_MatrixPreviousM, isOverriden: true, isPerInstance: true, InstanceType.MeshRenderer);
		gPUInstanceDataBufferBuilder.AddComponent<PackedMatrix>(ParamNames.unity_MatrixPreviousMI, isOverriden: true, isPerInstance: true, InstanceType.MeshRenderer);
		if ((flags & Flags.UseBoundingSphereParameter) != 0)
		{
			gPUInstanceDataBufferBuilder.AddComponent<Vector4>(ParamNames.unity_WorldBoundingSphere, isOverriden: true, isPerInstance: true, InstanceType.MeshRenderer);
		}
		for (int i = 0; i < 16; i++)
		{
			gPUInstanceDataBufferBuilder.AddComponent<Vector4>(ParamNames.DOTS_ST_WindParams[i], isOverriden: true, isPerInstance: true, InstanceType.SpeedTree, InstanceComponentGroup.Wind);
		}
		for (int j = 0; j < 16; j++)
		{
			gPUInstanceDataBufferBuilder.AddComponent<Vector4>(ParamNames.DOTS_ST_WindHistoryParams[j], isOverriden: true, isPerInstance: true, InstanceType.SpeedTree, InstanceComponentGroup.Wind);
		}
		return gPUInstanceDataBufferBuilder.Build(in instanceNumInfo);
	}

	public RenderersParameters(in GPUInstanceDataBuffer instanceDataBuffer)
	{
		lightmapScale = GetParamInfo(in instanceDataBuffer, ParamNames.unity_LightmapST);
		localToWorld = GetParamInfo(in instanceDataBuffer, ParamNames.unity_ObjectToWorld);
		worldToLocal = GetParamInfo(in instanceDataBuffer, ParamNames.unity_WorldToObject);
		matrixPreviousM = GetParamInfo(in instanceDataBuffer, ParamNames.unity_MatrixPreviousM);
		matrixPreviousMI = GetParamInfo(in instanceDataBuffer, ParamNames.unity_MatrixPreviousMI);
		shCoefficients = GetParamInfo(in instanceDataBuffer, ParamNames.unity_SHCoefficients);
		boundingSphere = GetParamInfo(in instanceDataBuffer, ParamNames.unity_WorldBoundingSphere, assertOnFail: false);
		windParams = new ParamInfo[16];
		windHistoryParams = new ParamInfo[16];
		for (int i = 0; i < 16; i++)
		{
			windParams[i] = GetParamInfo(in instanceDataBuffer, ParamNames.DOTS_ST_WindParams[i]);
			windHistoryParams[i] = GetParamInfo(in instanceDataBuffer, ParamNames.DOTS_ST_WindHistoryParams[i]);
		}
		static ParamInfo GetParamInfo(in GPUInstanceDataBuffer instanceDataBuffer, int paramNameIdx, bool assertOnFail = true)
		{
			int gpuAddress = instanceDataBuffer.GetGpuAddress(paramNameIdx, assertOnFail);
			int propertyIndex = instanceDataBuffer.GetPropertyIndex(paramNameIdx, assertOnFail);
			ParamInfo result = default(ParamInfo);
			result.index = propertyIndex;
			result.gpuAddress = gpuAddress;
			result.uintOffset = gpuAddress / s_uintSize;
			return result;
		}
	}
}
