using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class ForwardLights
{
	private static class LightConstantBuffer
	{
		public static int _MainLightPosition;

		public static int _MainLightColor;

		public static int _MainLightOcclusionProbesChannel;

		public static int _MainLightLayerMask;

		public static int _AdditionalLightsCount;

		public static int _AdditionalLightsPosition;

		public static int _AdditionalLightsColor;

		public static int _AdditionalLightsAttenuation;

		public static int _AdditionalLightsSpotDir;

		public static int _AdditionalLightOcclusionProbeChannel;

		public static int _AdditionalLightsLayerMasks;
	}

	internal struct InitParams
	{
		public LightCookieManager lightCookieManager;

		public bool forwardPlus;

		internal static InitParams Create()
		{
			LightCookieManager.Settings settings = LightCookieManager.Settings.Create();
			UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
			if ((bool)asset)
			{
				settings.atlas.format = asset.additionalLightsCookieFormat;
				settings.atlas.resolution = asset.additionalLightsCookieResolution;
			}
			InitParams result = default(InitParams);
			result.lightCookieManager = new LightCookieManager(ref settings);
			result.forwardPlus = false;
			return result;
		}
	}

	private class SetupLightPassData
	{
		internal UniversalRenderingData renderingData;

		internal UniversalCameraData cameraData;

		internal UniversalLightData lightData;

		internal ForwardLights forwardLights;
	}

	private int m_AdditionalLightsBufferId;

	private int m_AdditionalLightsIndicesId;

	private const string k_SetupLightConstants = "Setup Light Constants";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Setup Light Constants");

	private static readonly ProfilingSampler m_ProfilingSamplerFPSetup = new ProfilingSampler("Forward+ Setup");

	private static readonly ProfilingSampler m_ProfilingSamplerFPComplete = new ProfilingSampler("Forward+ Complete");

	private static readonly ProfilingSampler m_ProfilingSamplerFPUpload = new ProfilingSampler("Forward+ Upload");

	private MixedLightingSetup m_MixedLightingSetup;

	private Vector4[] m_AdditionalLightPositions;

	private Vector4[] m_AdditionalLightColors;

	private Vector4[] m_AdditionalLightAttenuations;

	private Vector4[] m_AdditionalLightSpotDirections;

	private Vector4[] m_AdditionalLightOcclusionProbeChannels;

	private float[] m_AdditionalLightsLayerMasks;

	private bool m_UseStructuredBuffer;

	private bool m_UseForwardPlus;

	private int m_DirectionalLightCount;

	private int m_ActualTileWidth;

	private int2 m_TileResolution;

	private JobHandle m_CullingHandle;

	private NativeArray<uint> m_ZBins;

	private GraphicsBuffer m_ZBinsBuffer;

	private NativeArray<uint> m_TileMasks;

	private GraphicsBuffer m_TileMasksBuffer;

	private LightCookieManager m_LightCookieManager;

	private ReflectionProbeManager m_ReflectionProbeManager;

	private int m_WordsPerTile;

	private float m_ZBinScale;

	private float m_ZBinOffset;

	private int m_LightCount;

	private int m_BinCount;

	private static ProfilingSampler s_SetupForwardLights = new ProfilingSampler("Setup Forward Lights");

	internal ReflectionProbeManager reflectionProbeManager => m_ReflectionProbeManager;

	public ForwardLights()
		: this(InitParams.Create())
	{
	}

	internal ForwardLights(InitParams initParams)
	{
		m_UseStructuredBuffer = RenderingUtils.useStructuredBuffer;
		m_UseForwardPlus = initParams.forwardPlus;
		LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
		LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
		LightConstantBuffer._MainLightOcclusionProbesChannel = Shader.PropertyToID("_MainLightOcclusionProbes");
		LightConstantBuffer._MainLightLayerMask = Shader.PropertyToID("_MainLightLayerMask");
		LightConstantBuffer._AdditionalLightsCount = Shader.PropertyToID("_AdditionalLightsCount");
		if (m_UseStructuredBuffer)
		{
			m_AdditionalLightsBufferId = Shader.PropertyToID("_AdditionalLightsBuffer");
			m_AdditionalLightsIndicesId = Shader.PropertyToID("_AdditionalLightsIndices");
		}
		else
		{
			LightConstantBuffer._AdditionalLightsPosition = Shader.PropertyToID("_AdditionalLightsPosition");
			LightConstantBuffer._AdditionalLightsColor = Shader.PropertyToID("_AdditionalLightsColor");
			LightConstantBuffer._AdditionalLightsAttenuation = Shader.PropertyToID("_AdditionalLightsAttenuation");
			LightConstantBuffer._AdditionalLightsSpotDir = Shader.PropertyToID("_AdditionalLightsSpotDir");
			LightConstantBuffer._AdditionalLightOcclusionProbeChannel = Shader.PropertyToID("_AdditionalLightsOcclusionProbes");
			LightConstantBuffer._AdditionalLightsLayerMasks = Shader.PropertyToID("_AdditionalLightsLayerMasks");
			int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
			m_AdditionalLightPositions = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightColors = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightAttenuations = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightSpotDirections = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightOcclusionProbeChannels = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightsLayerMasks = new float[maxVisibleAdditionalLights];
		}
		if (m_UseForwardPlus)
		{
			CreateForwardPlusBuffers();
			m_ReflectionProbeManager = ReflectionProbeManager.Create();
		}
		m_LightCookieManager = initParams.lightCookieManager;
	}

	private void CreateForwardPlusBuffers()
	{
		m_ZBins = new NativeArray<uint>(UniversalRenderPipeline.maxZBinWords, Allocator.Persistent);
		m_ZBinsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, UniversalRenderPipeline.maxZBinWords / 4, UnsafeUtility.SizeOf<float4>());
		m_ZBinsBuffer.name = "URP Z-Bin Buffer";
		m_TileMasks = new NativeArray<uint>(UniversalRenderPipeline.maxTileWords, Allocator.Persistent);
		m_TileMasksBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, UniversalRenderPipeline.maxTileWords / 4, UnsafeUtility.SizeOf<float4>());
		m_TileMasksBuffer.name = "URP Tile Buffer";
	}

	private static int AlignByteCount(int count, int align)
	{
		return align * ((count + align - 1) / align);
	}

	private static void GetViewParams(bool isOrthographic, float4x4 viewToClip, out float viewPlaneBot, out float viewPlaneTop, out float4 viewToViewportScaleBias)
	{
		float2 @float = math.float2(viewToClip[0][0], viewToClip[1][1]);
		float2 float2 = math.rcp(@float);
		float2 float3 = (isOrthographic ? (-math.float2(viewToClip[3][0], viewToClip[3][1])) : math.float2(viewToClip[2][0], viewToClip[2][1]));
		viewPlaneBot = float3.y * float2.y - float2.y;
		viewPlaneTop = float3.y * float2.y + float2.y;
		viewToViewportScaleBias = math.float4(@float * 0.5f, -float3 * 0.5f + 0.5f);
	}

	internal static JobHandle ScheduleClusteringJobs(NativeArray<VisibleLight> lights, NativeArray<VisibleReflectionProbe> probes, NativeArray<uint> zBins, NativeArray<uint> tileMasks, Fixed2<float4x4> worldToViews, Fixed2<float4x4> viewToClips, int viewCount, int2 screenResolution, float nearClipPlane, float farClipPlane, bool isOrthographic, out int localLightCount, out int directionalLightCount, out int binCount, out float zBinScale, out float zBinOffset, out int2 tileResolution, out int actualTileWidth, out int wordsPerTile)
	{
		localLightCount = lights.Length;
		int i;
		for (i = 0; i < localLightCount && lights[i].lightType == LightType.Directional; i++)
		{
		}
		localLightCount -= i;
		directionalLightCount = ((i > 0) ? (i - 1) : 0);
		NativeArray<VisibleLight> subArray = lights.GetSubArray(i, localLightCount);
		int num = math.min(probes.Length, UniversalRenderPipeline.maxVisibleReflectionProbes);
		int num2 = subArray.Length + num;
		wordsPerTile = (num2 + 31) / 32;
		actualTileWidth = 4;
		do
		{
			actualTileWidth <<= 1;
			tileResolution = (screenResolution + actualTileWidth - 1) / actualTileWidth;
		}
		while (tileResolution.x * tileResolution.y * wordsPerTile * viewCount > UniversalRenderPipeline.maxTileWords);
		if (!isOrthographic)
		{
			zBinScale = (float)(UniversalRenderPipeline.maxZBinWords / viewCount) / ((math.log2(farClipPlane) - math.log2(nearClipPlane)) * (float)(2 + wordsPerTile));
			zBinOffset = (0f - math.log2(nearClipPlane)) * zBinScale;
			binCount = (int)(math.log2(farClipPlane) * zBinScale + zBinOffset);
		}
		else
		{
			zBinScale = (float)(UniversalRenderPipeline.maxZBinWords / viewCount) / ((farClipPlane - nearClipPlane) * (float)(2 + wordsPerTile));
			zBinOffset = (0f - nearClipPlane) * zBinScale;
			binCount = (int)(farClipPlane * zBinScale + zBinOffset);
		}
		binCount = Math.Max(binCount, 0);
		for (int j = 1; j < num; j++)
		{
			VisibleReflectionProbe visibleReflectionProbe = probes[j];
			int num3 = j - 1;
			while (num3 >= 0 && IsProbeGreater(probes[num3], visibleReflectionProbe))
			{
				probes[num3 + 1] = probes[num3];
				num3--;
			}
			probes[num3 + 1] = visibleReflectionProbe;
		}
		NativeArray<float2> minMaxZs = new NativeArray<float2>(num2 * viewCount, Allocator.TempJob);
		LightMinMaxZJob jobData = default(LightMinMaxZJob);
		jobData.worldToViews = worldToViews;
		jobData.lights = subArray;
		jobData.minMaxZs = minMaxZs.GetSubArray(0, localLightCount * viewCount);
		JobHandle dependency = jobData.ScheduleParallel(localLightCount * viewCount, 32, default(JobHandle));
		ReflectionProbeMinMaxZJob jobData2 = default(ReflectionProbeMinMaxZJob);
		jobData2.worldToViews = worldToViews;
		jobData2.reflectionProbes = probes;
		jobData2.minMaxZs = minMaxZs.GetSubArray(localLightCount * viewCount, num * viewCount);
		JobHandle dependency2 = jobData2.ScheduleParallel(num * viewCount, 32, dependency);
		int num4 = (binCount + 128 - 1) / 128;
		ZBinningJob jobData3 = default(ZBinningJob);
		jobData3.bins = zBins;
		jobData3.minMaxZs = minMaxZs;
		jobData3.zBinScale = zBinScale;
		jobData3.zBinOffset = zBinOffset;
		jobData3.binCount = binCount;
		jobData3.wordsPerTile = wordsPerTile;
		jobData3.lightCount = localLightCount;
		jobData3.reflectionProbeCount = num;
		jobData3.batchCount = num4;
		jobData3.viewCount = viewCount;
		jobData3.isOrthographic = isOrthographic;
		JobHandle inputDeps = jobData3.ScheduleParallel(num4 * viewCount, 1, dependency2);
		dependency2.Complete();
		GetViewParams(isOrthographic, viewToClips[0], out var viewPlaneBot, out var viewPlaneTop, out var viewToViewportScaleBias);
		GetViewParams(isOrthographic, viewToClips[1], out var viewPlaneBot2, out var viewPlaneTop2, out var viewToViewportScaleBias2);
		int num5 = AlignByteCount((1 + tileResolution.y) * UnsafeUtility.SizeOf<InclusiveRange>(), 128) / UnsafeUtility.SizeOf<InclusiveRange>();
		NativeArray<InclusiveRange> tileRanges = new NativeArray<InclusiveRange>(num5 * num2 * viewCount, Allocator.TempJob);
		TilingJob jobData4 = default(TilingJob);
		jobData4.lights = subArray;
		jobData4.reflectionProbes = probes;
		jobData4.tileRanges = tileRanges;
		jobData4.itemsPerTile = num2;
		jobData4.rangesPerItem = num5;
		jobData4.worldToViews = worldToViews;
		jobData4.tileScale = (float2)screenResolution / (float)actualTileWidth;
		jobData4.tileScaleInv = (float)actualTileWidth / (float2)screenResolution;
		jobData4.viewPlaneBottoms = new Fixed2<float>(viewPlaneBot, viewPlaneBot2);
		jobData4.viewPlaneTops = new Fixed2<float>(viewPlaneTop, viewPlaneTop2);
		jobData4.viewToViewportScaleBiases = new Fixed2<float4>(viewToViewportScaleBias, viewToViewportScaleBias2);
		jobData4.tileCount = tileResolution;
		jobData4.near = nearClipPlane;
		jobData4.isOrthographic = isOrthographic;
		JobHandle dependency3 = jobData4.ScheduleParallel(num2 * viewCount, 1, dependency2);
		TileRangeExpansionJob jobData5 = default(TileRangeExpansionJob);
		jobData5.tileRanges = tileRanges;
		jobData5.tileMasks = tileMasks;
		jobData5.rangesPerItem = num5;
		jobData5.itemsPerTile = num2;
		jobData5.wordsPerTile = wordsPerTile;
		jobData5.tileResolution = tileResolution;
		JobHandle inputDeps2 = jobData5.ScheduleParallel(tileResolution.y * viewCount, 1, dependency3);
		return JobHandle.CombineDependencies(minMaxZs.Dispose(inputDeps), tileRanges.Dispose(inputDeps2));
		static bool IsProbeGreater(VisibleReflectionProbe probe, VisibleReflectionProbe otherProbe)
		{
			if (probe.importance >= otherProbe.importance)
			{
				if (probe.importance == otherProbe.importance)
				{
					return probe.bounds.extents.sqrMagnitude > otherProbe.bounds.extents.sqrMagnitude;
				}
				return false;
			}
			return true;
		}
	}

	internal unsafe void PreSetup(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
	{
		if (!m_UseForwardPlus)
		{
			return;
		}
		using (new ProfilingScope(m_ProfilingSamplerFPSetup))
		{
			if (!m_CullingHandle.IsCompleted)
			{
				throw new InvalidOperationException("Forward+ jobs have not completed yet.");
			}
			if (m_TileMasks.Length != UniversalRenderPipeline.maxTileWords)
			{
				m_ZBins.Dispose();
				m_ZBinsBuffer.Dispose();
				m_TileMasks.Dispose();
				m_TileMasksBuffer.Dispose();
				CreateForwardPlusBuffers();
			}
			else
			{
				UnsafeUtility.MemClear(m_ZBins.GetUnsafePtr(), m_ZBins.Length * 4);
				UnsafeUtility.MemClear(m_TileMasks.GetUnsafePtr(), m_TileMasks.Length * 4);
			}
			int num = ((!cameraData.xr.enabled || !cameraData.xr.singlePassEnabled) ? 1 : 2);
			Fixed2<float4x4> worldToViews = new Fixed2<float4x4>(cameraData.GetViewMatrix(), cameraData.GetViewMatrix(math.min(1, num - 1)));
			Fixed2<float4x4> viewToClips = new Fixed2<float4x4>(cameraData.GetProjectionMatrix(), cameraData.GetProjectionMatrix(math.min(1, num - 1)));
			m_CullingHandle = ScheduleClusteringJobs(lightData.visibleLights, renderingData.cullResults.visibleReflectionProbes, m_ZBins, m_TileMasks, worldToViews, viewToClips, num, math.int2(cameraData.pixelWidth, cameraData.pixelHeight), cameraData.camera.nearClipPlane, cameraData.camera.farClipPlane, cameraData.camera.orthographic, out m_LightCount, out m_DirectionalLightCount, out m_BinCount, out m_ZBinScale, out m_ZBinOffset, out m_TileResolution, out m_ActualTileWidth, out m_WordsPerTile);
			JobHandle.ScheduleBatchedJobs();
		}
	}

	public void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ContextContainer frameData = renderingData.frameData;
		UniversalRenderingData renderingData2 = frameData.Get<UniversalRenderingData>();
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalLightData lightData = frameData.Get<UniversalLightData>();
		SetupLights(CommandBufferHelpers.GetUnsafeCommandBuffer(renderingData.commandBuffer), renderingData2, cameraData, lightData);
	}

	internal void SetupRenderGraphLights(RenderGraph renderGraph, UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
	{
		SetupLightPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<SetupLightPassData>(s_SetupForwardLights.name, out passData, s_SetupForwardLights, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\ForwardLights.cs", 448);
		passData.renderingData = renderingData;
		passData.cameraData = cameraData;
		passData.lightData = lightData;
		passData.forwardLights = this;
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(SetupLightPassData data, UnsafeGraphContext rgContext)
		{
			data.forwardLights.SetupLights(rgContext.cmd, data.renderingData, data.cameraData, data.lightData);
		});
	}

	internal void SetupLights(UnsafeCommandBuffer cmd, UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData)
	{
		int additionalLightsCount = lightData.additionalLightsCount;
		bool shadeAdditionalLightsPerVertex = lightData.shadeAdditionalLightsPerVertex;
		using (new ProfilingScope(m_ProfilingSampler))
		{
			if (m_UseForwardPlus)
			{
				if (lightData.reflectionProbeAtlas)
				{
					m_ReflectionProbeManager.UpdateGpuData(CommandBufferHelpers.GetNativeCommandBuffer(cmd), ref renderingData.cullResults);
				}
				using (new ProfilingScope(m_ProfilingSamplerFPComplete))
				{
					m_CullingHandle.Complete();
				}
				using (new ProfilingScope(m_ProfilingSamplerFPUpload))
				{
					m_ZBinsBuffer.SetData(m_ZBins.Reinterpret<float4>(UnsafeUtility.SizeOf<uint>()));
					m_TileMasksBuffer.SetData(m_TileMasks.Reinterpret<float4>(UnsafeUtility.SizeOf<uint>()));
					cmd.SetGlobalConstantBuffer(m_ZBinsBuffer, "urp_ZBinBuffer", 0, UniversalRenderPipeline.maxZBinWords * 4);
					cmd.SetGlobalConstantBuffer(m_TileMasksBuffer, "urp_TileBuffer", 0, UniversalRenderPipeline.maxTileWords * 4);
				}
				cmd.SetGlobalVector("_FPParams0", math.float4(m_ZBinScale, m_ZBinOffset, m_LightCount, m_DirectionalLightCount));
				cmd.SetGlobalVector("_FPParams1", math.float4(cameraData.pixelRect.size / m_ActualTileWidth, m_TileResolution.x, m_WordsPerTile));
				cmd.SetGlobalVector("_FPParams2", math.float4(m_BinCount, m_TileResolution.x * m_TileResolution.y, 0f, 0f));
			}
			SetupShaderLightConstants(cmd, ref renderingData.cullResults, lightData);
			bool flag = (cameraData.renderer.stripAdditionalLightOffVariants && lightData.supportsAdditionalLights) || additionalLightsCount > 0;
			cmd.SetKeyword(in ShaderGlobalKeywords.AdditionalLightsVertex, flag && shadeAdditionalLightsPerVertex && !m_UseForwardPlus);
			cmd.SetKeyword(in ShaderGlobalKeywords.AdditionalLightsPixel, flag && !shadeAdditionalLightsPerVertex && !m_UseForwardPlus);
			cmd.SetKeyword(in ShaderGlobalKeywords.ClusterLightLoop, m_UseForwardPlus);
			cmd.SetKeyword(in ShaderGlobalKeywords.ForwardPlus, m_UseForwardPlus);
			bool flag2 = lightData.supportsMixedLighting && m_MixedLightingSetup == MixedLightingSetup.ShadowMask;
			bool flag3 = flag2 && QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask;
			bool flag4 = lightData.supportsMixedLighting && m_MixedLightingSetup == MixedLightingSetup.Subtractive;
			cmd.SetKeyword(in ShaderGlobalKeywords.LightmapShadowMixing, flag4 || flag3);
			cmd.SetKeyword(in ShaderGlobalKeywords.ShadowsShadowMask, flag2);
			cmd.SetKeyword(in ShaderGlobalKeywords.MixedLightingSubtractive, flag4);
			cmd.SetKeyword(in ShaderGlobalKeywords.ReflectionProbeBlending, lightData.reflectionProbeBlending);
			cmd.SetKeyword(in ShaderGlobalKeywords.ReflectionProbeBoxProjection, lightData.reflectionProbeBoxProjection);
			cmd.SetKeyword(in ShaderGlobalKeywords.ReflectionProbeAtlas, lightData.reflectionProbeAtlas);
			UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
			bool flag5 = asset != null && asset.lightProbeSystem == LightProbeSystem.ProbeVolumes;
			ProbeVolumeSHBands probeVolumeSHBands = asset.probeVolumeSHBands;
			cmd.SetKeyword(in ShaderGlobalKeywords.ProbeVolumeL1, flag5 && probeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL1);
			cmd.SetKeyword(in ShaderGlobalKeywords.ProbeVolumeL2, flag5 && probeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2);
			ShEvalMode shEvalMode = PlatformAutoDetect.ShAutoDetect(asset.shEvalMode);
			cmd.SetKeyword(in ShaderGlobalKeywords.EVALUATE_SH_MIXED, shEvalMode == ShEvalMode.Mixed);
			cmd.SetKeyword(in ShaderGlobalKeywords.EVALUATE_SH_VERTEX, shEvalMode == ShEvalMode.PerVertex);
			VolumeStack stack = VolumeManager.instance.stack;
			bool flag6 = ProbeReferenceVolume.instance.UpdateShaderVariablesProbeVolumes(CommandBufferHelpers.GetNativeCommandBuffer(cmd), stack.GetComponent<ProbeVolumesOptions>(), cameraData.IsTemporalAAEnabled() ? Time.frameCount : 0, lightData.supportsLightLayers);
			cmd.SetGlobalInt("_EnableProbeVolumes", flag6 ? 1 : 0);
			cmd.SetKeyword(in ShaderGlobalKeywords.LightLayers, lightData.supportsLightLayers && !CoreUtils.IsSceneLightingDisabled(cameraData.camera));
			if (m_LightCookieManager != null)
			{
				m_LightCookieManager.Setup(CommandBufferHelpers.GetNativeCommandBuffer(cmd), lightData);
			}
			else
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.LightCookies, value: false);
			}
			if (GraphicsSettings.TryGetRenderPipelineSettings<LightmapSamplingSettings>(out var settings))
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.LIGHTMAP_BICUBIC_SAMPLING, settings.useBicubicLightmapSampling);
			}
			else
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.LIGHTMAP_BICUBIC_SAMPLING, value: false);
			}
		}
	}

	internal void Cleanup()
	{
		if (m_UseForwardPlus)
		{
			m_CullingHandle.Complete();
			m_ZBins.Dispose();
			m_TileMasks.Dispose();
			m_ZBinsBuffer.Dispose();
			m_ZBinsBuffer = null;
			m_TileMasksBuffer.Dispose();
			m_TileMasksBuffer = null;
			m_ReflectionProbeManager.Dispose();
		}
		m_LightCookieManager?.Dispose();
		m_LightCookieManager = null;
	}

	private void InitializeLightConstants(NativeArray<VisibleLight> lights, int lightIndex, bool supportsLightLayers, out Vector4 lightPos, out Vector4 lightColor, out Vector4 lightAttenuation, out Vector4 lightSpotDir, out Vector4 lightOcclusionProbeChannel, out uint lightLayerMask, out bool isSubtractive)
	{
		UniversalRenderPipeline.InitializeLightConstants_Common(lights, lightIndex, out lightPos, out lightColor, out lightAttenuation, out lightSpotDir, out lightOcclusionProbeChannel);
		lightLayerMask = 0u;
		isSubtractive = false;
		if (lightIndex < 0)
		{
			return;
		}
		ref VisibleLight reference = ref lights.UnsafeElementAtMutable(lightIndex);
		Light light = reference.light;
		LightBakingOutput bakingOutput = light.bakingOutput;
		isSubtractive = bakingOutput.isBaked && bakingOutput.lightmapBakeType == LightmapBakeType.Mixed && bakingOutput.mixedLightingMode == MixedLightingMode.Subtractive;
		if (light == null)
		{
			return;
		}
		if (bakingOutput.lightmapBakeType == LightmapBakeType.Mixed && reference.light.shadows != 0 && m_MixedLightingSetup == MixedLightingSetup.None)
		{
			switch (bakingOutput.mixedLightingMode)
			{
			case MixedLightingMode.Subtractive:
				m_MixedLightingSetup = MixedLightingSetup.Subtractive;
				break;
			case MixedLightingMode.Shadowmask:
				m_MixedLightingSetup = MixedLightingSetup.ShadowMask;
				break;
			}
		}
		if (supportsLightLayers)
		{
			UniversalAdditionalLightData universalAdditionalLightData = light.GetUniversalAdditionalLightData();
			lightLayerMask = RenderingLayerUtils.ToValidRenderingLayers(universalAdditionalLightData.renderingLayers);
		}
	}

	private void SetupShaderLightConstants(UnsafeCommandBuffer cmd, ref CullingResults cullResults, UniversalLightData lightData)
	{
		m_MixedLightingSetup = MixedLightingSetup.None;
		SetupMainLightConstants(cmd, lightData);
		SetupAdditionalLightConstants(cmd, ref cullResults, lightData);
	}

	private void SetupMainLightConstants(UnsafeCommandBuffer cmd, UniversalLightData lightData)
	{
		bool supportsLightLayers = lightData.supportsLightLayers;
		InitializeLightConstants(lightData.visibleLights, lightData.mainLightIndex, supportsLightLayers, out var lightPos, out var lightColor, out var _, out var _, out var lightOcclusionProbeChannel, out var lightLayerMask, out var isSubtractive);
		lightColor.w = (isSubtractive ? 0f : 1f);
		cmd.SetGlobalVector(LightConstantBuffer._MainLightPosition, lightPos);
		cmd.SetGlobalVector(LightConstantBuffer._MainLightColor, lightColor);
		cmd.SetGlobalVector(LightConstantBuffer._MainLightOcclusionProbesChannel, lightOcclusionProbeChannel);
		if (supportsLightLayers)
		{
			cmd.SetGlobalInt(LightConstantBuffer._MainLightLayerMask, (int)lightLayerMask);
		}
	}

	private void SetupAdditionalLightConstants(UnsafeCommandBuffer cmd, ref CullingResults cullResults, UniversalLightData lightData)
	{
		bool supportsLightLayers = lightData.supportsLightLayers;
		NativeArray<VisibleLight> visibleLights = lightData.visibleLights;
		int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
		int num = SetupPerObjectLightIndices(cullResults, lightData);
		if (num > 0)
		{
			if (m_UseStructuredBuffer)
			{
				NativeArray<ShaderInput.LightData> data = new NativeArray<ShaderInput.LightData>(num, Allocator.Temp);
				int i = 0;
				int num2 = 0;
				ShaderInput.LightData value = default(ShaderInput.LightData);
				for (; i < visibleLights.Length; i++)
				{
					if (num2 >= maxVisibleAdditionalLights)
					{
						break;
					}
					if (lightData.mainLightIndex != i)
					{
						InitializeLightConstants(visibleLights, i, supportsLightLayers, out value.position, out value.color, out value.attenuation, out value.spotDirection, out value.occlusionProbeChannels, out value.layerMask, out var _);
						data[num2] = value;
						num2++;
					}
				}
				ComputeBuffer lightDataBuffer = ShaderData.instance.GetLightDataBuffer(num);
				lightDataBuffer.SetData(data);
				int lightAndReflectionProbeIndexCount = cullResults.lightAndReflectionProbeIndexCount;
				ComputeBuffer lightIndicesBuffer = ShaderData.instance.GetLightIndicesBuffer(lightAndReflectionProbeIndexCount);
				cmd.SetGlobalBuffer(m_AdditionalLightsBufferId, lightDataBuffer);
				cmd.SetGlobalBuffer(m_AdditionalLightsIndicesId, lightIndicesBuffer);
				data.Dispose();
			}
			else
			{
				int j = 0;
				int num3 = 0;
				for (; j < visibleLights.Length; j++)
				{
					if (num3 >= maxVisibleAdditionalLights)
					{
						break;
					}
					if (lightData.mainLightIndex != j)
					{
						InitializeLightConstants(visibleLights, j, supportsLightLayers, out m_AdditionalLightPositions[num3], out m_AdditionalLightColors[num3], out m_AdditionalLightAttenuations[num3], out m_AdditionalLightSpotDirections[num3], out m_AdditionalLightOcclusionProbeChannels[num3], out var lightLayerMask, out var isSubtractive2);
						if (supportsLightLayers)
						{
							m_AdditionalLightsLayerMasks[num3] = math.asfloat(lightLayerMask);
						}
						m_AdditionalLightColors[num3].w = (isSubtractive2 ? 1f : 0f);
						num3++;
					}
				}
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsPosition, m_AdditionalLightPositions);
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsColor, m_AdditionalLightColors);
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsAttenuation, m_AdditionalLightAttenuations);
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsSpotDir, m_AdditionalLightSpotDirections);
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightOcclusionProbeChannel, m_AdditionalLightOcclusionProbeChannels);
				if (supportsLightLayers)
				{
					cmd.SetGlobalFloatArray(LightConstantBuffer._AdditionalLightsLayerMasks, m_AdditionalLightsLayerMasks);
				}
			}
			cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, new Vector4(lightData.maxPerObjectAdditionalLightsCount, 0f, 0f, 0f));
		}
		else
		{
			cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, Vector4.zero);
		}
	}

	private int SetupPerObjectLightIndices(CullingResults cullResults, UniversalLightData lightData)
	{
		if (lightData.additionalLightsCount == 0 || m_UseForwardPlus)
		{
			return lightData.additionalLightsCount;
		}
		NativeArray<int> lightIndexMap = cullResults.GetLightIndexMap(Allocator.Temp);
		int num = 0;
		int num2 = 0;
		int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
		int length = lightData.visibleLights.Length;
		for (int i = 0; i < length; i++)
		{
			if (num2 >= maxVisibleAdditionalLights)
			{
				break;
			}
			if (i == lightData.mainLightIndex)
			{
				lightIndexMap[i] = -1;
				num++;
				continue;
			}
			if (lightData.visibleLights[i].lightType == LightType.Directional || lightData.visibleLights[i].lightType == LightType.Spot || lightData.visibleLights[i].lightType == LightType.Point)
			{
				lightIndexMap[i] -= num;
			}
			else
			{
				lightIndexMap[i] = -1;
			}
			num2++;
		}
		for (int j = num + num2; j < lightIndexMap.Length; j++)
		{
			lightIndexMap[j] = -1;
		}
		cullResults.SetLightIndexMap(lightIndexMap);
		if (m_UseStructuredBuffer && num2 > 0)
		{
			int lightAndReflectionProbeIndexCount = cullResults.lightAndReflectionProbeIndexCount;
			cullResults.FillLightAndReflectionProbeIndices(ShaderData.instance.GetLightIndicesBuffer(lightAndReflectionProbeIndexCount));
		}
		lightIndexMap.Dispose();
		return num2;
	}
}
