using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.RenderPipelines.Core.Runtime.Shared;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.VFX;

namespace UnityEngine.Rendering.Universal;

public sealed class UniversalRenderPipeline : RenderPipeline
{
	internal static class CameraMetadataCache
	{
		public class CameraMetadataCacheEntry
		{
			public string name;

			public ProfilingSampler sampler;
		}

		private static Dictionary<int, CameraMetadataCacheEntry> s_MetadataCache = new Dictionary<int, CameraMetadataCacheEntry>();

		private static readonly CameraMetadataCacheEntry k_NoAllocEntry = new CameraMetadataCacheEntry
		{
			name = "Unknown",
			sampler = new ProfilingSampler("Unknown")
		};

		public static CameraMetadataCacheEntry GetCached(Camera camera)
		{
			int hashCode = camera.GetHashCode();
			if (!s_MetadataCache.TryGetValue(hashCode, out var value))
			{
				string name = camera.name;
				value = new CameraMetadataCacheEntry
				{
					name = name,
					sampler = new ProfilingSampler("UniversalRenderPipeline.RenderSingleCameraInternal: " + name)
				};
				s_MetadataCache.Add(hashCode, value);
			}
			return value;
		}
	}

	internal static class Profiling
	{
		public static class Pipeline
		{
			public static class Renderer
			{
				private const string k_Name = "ScriptableRenderer";

				public static readonly ProfilingSampler setupCullingParameters = new ProfilingSampler("ScriptableRenderer.SetupCullingParameters");

				public static readonly ProfilingSampler setup = new ProfilingSampler("ScriptableRenderer.Setup");
			}

			public static class Context
			{
				private const string k_Name = "ScriptableRenderContext";

				public static readonly ProfilingSampler submit = new ProfilingSampler("ScriptableRenderContext.Submit");
			}

			private const string k_Name = "UniversalRenderPipeline";

			public static readonly ProfilingSampler initializeCameraData = new ProfilingSampler("UniversalRenderPipeline.CreateCameraData");

			public static readonly ProfilingSampler initializeStackedCameraData = new ProfilingSampler("UniversalRenderPipeline.InitializeStackedCameraData");

			public static readonly ProfilingSampler initializeAdditionalCameraData = new ProfilingSampler("UniversalRenderPipeline.InitializeAdditionalCameraData");

			public static readonly ProfilingSampler initializeRenderingData = new ProfilingSampler("UniversalRenderPipeline.CreateRenderingData");

			public static readonly ProfilingSampler initializeShadowData = new ProfilingSampler("UniversalRenderPipeline.CreateShadowData");

			public static readonly ProfilingSampler initializeLightData = new ProfilingSampler("UniversalRenderPipeline.CreateLightData");

			public static readonly ProfilingSampler buildAdditionalLightsShadowAtlasLayout = new ProfilingSampler("UniversalRenderPipeline.BuildAdditionalLightsShadowAtlasLayout");

			public static readonly ProfilingSampler getPerObjectLightFlags = new ProfilingSampler("UniversalRenderPipeline.GetPerObjectLightFlags");

			public static readonly ProfilingSampler getMainLightIndex = new ProfilingSampler("UniversalRenderPipeline.GetMainLightIndex");

			public static readonly ProfilingSampler setupPerFrameShaderConstants = new ProfilingSampler("UniversalRenderPipeline.SetupPerFrameShaderConstants");

			public static readonly ProfilingSampler setupPerCameraShaderConstants = new ProfilingSampler("UniversalRenderPipeline.SetupPerCameraShaderConstants");
		}
	}

	private readonly struct CameraRenderingScope : IDisposable
	{
		private static readonly ProfilingSampler beginCameraRenderingSampler = new ProfilingSampler("RenderPipeline.BeginCameraRendering");

		private static readonly ProfilingSampler endCameraRenderingSampler = new ProfilingSampler("RenderPipeline.EndCameraRendering");

		private readonly ScriptableRenderContext m_Context;

		private readonly Camera m_Camera;

		public CameraRenderingScope(ScriptableRenderContext context, Camera camera)
		{
			using (new ProfilingScope(beginCameraRenderingSampler))
			{
				m_Context = context;
				m_Camera = camera;
				RenderPipeline.BeginCameraRendering(context, camera);
			}
		}

		public void Dispose()
		{
			using (new ProfilingScope(endCameraRenderingSampler))
			{
				RenderPipeline.EndCameraRendering(m_Context, m_Camera);
			}
		}
	}

	private readonly struct ContextRenderingScope : IDisposable
	{
		private static readonly ProfilingSampler beginContextRenderingSampler = new ProfilingSampler("RenderPipeline.BeginContextRendering");

		private static readonly ProfilingSampler endContextRenderingSampler = new ProfilingSampler("RenderPipeline.EndContextRendering");

		private readonly ScriptableRenderContext m_Context;

		private readonly List<Camera> m_Cameras;

		public ContextRenderingScope(ScriptableRenderContext context, List<Camera> cameras)
		{
			m_Context = context;
			m_Cameras = cameras;
			using (new ProfilingScope(beginContextRenderingSampler))
			{
				RenderPipeline.BeginContextRendering(m_Context, m_Cameras);
			}
		}

		public void Dispose()
		{
			using (new ProfilingScope(endContextRenderingSampler))
			{
				RenderPipeline.EndContextRendering(m_Context, m_Cameras);
			}
		}
	}

	public class SingleCameraRequest
	{
		public RenderTexture destination;

		public int mipLevel;

		public CubemapFace face = CubemapFace.Unknown;

		public int slice;
	}

	public const string k_ShaderTagName = "UniversalPipeline";

	internal const int k_DefaultRenderingLayerMask = 1;

	private readonly DebugDisplaySettingsUI m_DebugDisplaySettingsUI = new DebugDisplaySettingsUI();

	private UniversalRenderPipelineGlobalSettings m_GlobalSettings;

	internal static bool stackedOverlayCamerasRequireDepthForPostProcessing = false;

	internal static RenderGraph s_RenderGraph;

	internal static RTHandleResourcePool s_RTHandlePool;

	internal static bool useRenderGraph;

	internal bool apvIsEnabled;

	private readonly UniversalRenderPipelineAsset pipelineAsset;

	internal bool enableHDROnce = true;

	private static Vector4 k_DefaultLightPosition = new Vector4(0f, 0f, 1f, 0f);

	private static Vector4 k_DefaultLightColor = Color.black;

	private static Vector4 k_DefaultLightAttenuation = new Vector4(0f, 1f, 0f, 1f);

	private static Vector4 k_DefaultLightSpotDirection = new Vector4(0f, 0f, 1f, 0f);

	private static Vector4 k_DefaultLightsProbeChannel = new Vector4(0f, 0f, 0f, 0f);

	private static List<Vector4> m_ShadowBiasData = new List<Vector4>();

	private static List<int> m_ShadowResolutionData = new List<int>();

	private Comparison<Camera> cameraComparison = (Camera camera1, Camera camera2) => (int)camera1.depth - (int)camera2.depth;

	private static Lightmapping.RequestLightsDelegate lightsDelegate = delegate(Light[] requests, NativeArray<LightDataGI> lightsOutput)
	{
		LightDataGI value = default(LightDataGI);
		if (!SupportedRenderingFeatures.active.enlighten || (SupportedRenderingFeatures.active.lightmapBakeTypes | LightmapBakeType.Realtime) == (LightmapBakeType)0)
		{
			for (int i = 0; i < requests.Length; i++)
			{
				Light light = requests[i];
				value.InitNoBake(light.GetInstanceID());
				lightsOutput[i] = value;
			}
		}
		else
		{
			for (int j = 0; j < requests.Length; j++)
			{
				Light light2 = requests[j];
				switch (light2.type)
				{
				case LightType.Directional:
				{
					DirectionalLight dir = default(DirectionalLight);
					LightmapperUtils.Extract(light2, ref dir);
					value.Init(ref dir);
					break;
				}
				case LightType.Point:
				{
					PointLight point = default(PointLight);
					LightmapperUtils.Extract(light2, ref point);
					value.Init(ref point);
					break;
				}
				case LightType.Spot:
				{
					SpotLight spot = default(SpotLight);
					LightmapperUtils.Extract(light2, ref spot);
					spot.innerConeAngle = light2.innerSpotAngle * (MathF.PI / 180f);
					spot.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
					value.Init(ref spot);
					break;
				}
				case LightType.Area:
					value.InitNoBake(light2.GetInstanceID());
					break;
				case LightType.Disc:
					value.InitNoBake(light2.GetInstanceID());
					break;
				default:
					value.InitNoBake(light2.GetInstanceID());
					break;
				}
				value.falloff = FalloffType.InverseSquared;
				lightsOutput[j] = value;
			}
		}
	};

	public static float maxShadowBias => 10f;

	public static float minRenderScale => 0.1f;

	public static float maxRenderScale => 2f;

	public static int maxNumIterationsEnclosingSphere => 1000;

	public static int maxPerObjectLights => 8;

	public static int maxVisibleAdditionalLights
	{
		get
		{
			bool isShaderAPIMobileDefined = PlatformAutoDetect.isShaderAPIMobileDefined;
			if (isShaderAPIMobileDefined && SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && Graphics.minOpenGLESVersion <= OpenGLESVersion.OpenGLES30)
			{
				return 16;
			}
			if (!isShaderAPIMobileDefined && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.WebGPU)
			{
				return 256;
			}
			return 32;
		}
	}

	internal static int lightsPerTile => (maxVisibleAdditionalLights + 31) / 32 * 32;

	internal static int maxZBinWords => 4096;

	internal static int maxTileWords => ((maxVisibleAdditionalLights <= 32) ? 1024 : 4096) * 4;

	internal static int maxVisibleReflectionProbes => Math.Min(maxVisibleAdditionalLights, 64);

	internal UniversalRenderPipelineRuntimeTextures runtimeTextures { get; private set; }

	public override RenderPipelineGlobalSettings defaultSettings => m_GlobalSettings;

	internal static bool canOptimizeScreenMSAASamples { get; private set; }

	internal static int startFrameScreenMSAASamples { get; private set; }

	public static UniversalRenderPipelineAsset asset => GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

	public override string ToString()
	{
		return pipelineAsset?.ToString();
	}

	public UniversalRenderPipeline(UniversalRenderPipelineAsset asset)
	{
		pipelineAsset = asset;
		m_GlobalSettings = RenderPipelineGlobalSettings<UniversalRenderPipelineGlobalSettings, UniversalRenderPipeline>.instance;
		runtimeTextures = GraphicsSettings.GetRenderPipelineSettings<UniversalRenderPipelineRuntimeTextures>();
		UniversalRenderPipelineRuntimeShaders renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<UniversalRenderPipelineRuntimeShaders>();
		Blitter.Initialize(renderPipelineSettings.coreBlitPS, renderPipelineSettings.coreBlitColorAndDepthPS);
		SetSupportedRenderingFeatures(pipelineAsset);
		RTHandles.Initialize(Screen.width, Screen.height);
		ShaderGlobalKeywords.InitializeShaderGlobalKeywords();
		GraphicsSettings.useScriptableRenderPipelineBatching = asset.useSRPBatcher;
		if (((QualitySettings.antiAliasing <= 0) ? 1 : QualitySettings.antiAliasing) != asset.msaaSampleCount)
		{
			QualitySettings.antiAliasing = asset.msaaSampleCount;
		}
		URPDefaultVolumeProfileSettings renderPipelineSettings2 = GraphicsSettings.GetRenderPipelineSettings<URPDefaultVolumeProfileSettings>();
		VolumeManager.instance.Initialize(renderPipelineSettings2.volumeProfile, asset.volumeProfile);
		XRSystem.SetDisplayMSAASamples((MSAASamples)Mathf.Clamp(Mathf.NextPowerOfTwo(QualitySettings.antiAliasing), 1, 8));
		XRSystem.SetRenderScale(asset.renderScale);
		Lightmapping.SetDelegate(lightsDelegate);
		CameraCaptureBridge.enabled = true;
		RenderingUtils.ClearSystemInfoCache();
		DecalProjector.defaultMaterial = asset.decalMaterial;
		s_RenderGraph = new RenderGraph("URPRenderGraph");
		useRenderGraph = !GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>().enableRenderCompatibilityMode;
		Debug.Log("RenderGraph is now " + (useRenderGraph ? "enabled" : "disabled") + ".");
		s_RTHandlePool = new RTHandleResourcePool();
		DebugManager.instance.RefreshEditor();
		QualitySettings.enableLODCrossFade = asset.enableLODCrossFade;
		apvIsEnabled = asset != null && asset.lightProbeSystem == LightProbeSystem.ProbeVolumes;
		SupportedRenderingFeatures.active.overridesLightProbeSystem = apvIsEnabled;
		SupportedRenderingFeatures.active.skyOcclusion = apvIsEnabled;
		if (apvIsEnabled)
		{
			ProbeReferenceVolume instance = ProbeReferenceVolume.instance;
			ProbeVolumeSystemParameters parameters = new ProbeVolumeSystemParameters
			{
				memoryBudget = asset.probeVolumeMemoryBudget,
				blendingMemoryBudget = asset.probeVolumeBlendingMemoryBudget,
				shBands = asset.probeVolumeSHBands,
				supportGPUStreaming = asset.supportProbeVolumeGPUStreaming,
				supportDiskStreaming = asset.supportProbeVolumeDiskStreaming,
				supportScenarios = asset.supportProbeVolumeScenarios,
				supportScenarioBlending = asset.supportProbeVolumeScenarioBlending,
				sceneData = m_GlobalSettings.GetOrCreateAPVSceneData()
			};
			instance.Initialize(in parameters);
		}
		Vrs.InitializeResources();
	}

	protected override void Dispose(bool disposing)
	{
		Vrs.DisposeResources();
		if (apvIsEnabled)
		{
			ProbeReferenceVolume.instance.Cleanup();
		}
		Blitter.Cleanup();
		base.Dispose(disposing);
		pipelineAsset.DestroyRenderers();
		SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
		ShaderData.instance.Dispose();
		XRSystem.Dispose();
		s_RenderGraph.Cleanup();
		s_RenderGraph = null;
		s_RTHandlePool.Cleanup();
		s_RTHandlePool = null;
		Lightmapping.ResetDelegate();
		CameraCaptureBridge.enabled = false;
		ConstantBuffer.ReleaseAll();
		VolumeManager.instance.Deinitialize();
		DisposeAdditionalCameraData();
		AdditionalLightsShadowAtlasLayout.ClearStaticCaches();
	}

	private void DisposeAdditionalCameraData()
	{
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			if (allCameras[i].TryGetComponent<UniversalAdditionalCameraData>(out var component))
			{
				component.historyManager.Dispose();
			}
		}
	}

	protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
	{
		SetHDRState(cameras);
		int count = cameras.Count;
		AdjustUIOverlayOwnership(count);
		SetupScreenMSAASamplesState(count);
		GPUResidentDrawer.ReinitializeIfNeeded();
		using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.UniversalRenderTotal)))
		{
			using (new ContextRenderingScope(renderContext, cameras))
			{
				GraphicsSettings.lightsUseLinearIntensity = QualitySettings.activeColorSpace == ColorSpace.Linear;
				GraphicsSettings.lightsUseColorTemperature = true;
				SetupPerFrameShaderConstants();
				XRSystem.SetDisplayMSAASamples((MSAASamples)asset.msaaSampleCount);
				RTHandles.SetHardwareDynamicResolutionState(hwDynamicResRequested: true);
				SortCameras(cameras);
				int lastBaseCameraIndex = GetLastBaseCameraIndex(cameras);
				for (int i = 0; i < count; i++)
				{
					Camera camera = cameras[i];
					bool isLastBaseCamera = i == lastBaseCameraIndex;
					if (IsGameCamera(camera))
					{
						RenderCameraStack(renderContext, camera, isLastBaseCamera);
						continue;
					}
					using (new CameraRenderingScope(renderContext, camera))
					{
						VFXManager.PrepareCamera(camera);
						UpdateVolumeFramework(camera, null);
						RenderSingleCameraInternal(renderContext, camera, isLastBaseCamera);
					}
				}
				s_RenderGraph.EndFrame();
				s_RTHandlePool.PurgeUnusedResources(Time.frameCount);
			}
		}
	}

	protected override bool IsRenderRequestSupported<RequestData>(Camera camera, RequestData data)
	{
		if (data is StandardRequest)
		{
			return true;
		}
		if (data is SingleCameraRequest)
		{
			return true;
		}
		return false;
	}

	protected override void ProcessRenderRequests<RequestData>(ScriptableRenderContext context, Camera camera, RequestData renderRequest)
	{
		StandardRequest standardRequest = renderRequest as StandardRequest;
		SingleCameraRequest singleCameraRequest = renderRequest as SingleCameraRequest;
		if (standardRequest != null || singleCameraRequest != null)
		{
			RenderTexture renderTexture = ((standardRequest != null) ? standardRequest.destination : singleCameraRequest.destination);
			if (renderTexture == null)
			{
				Debug.LogError("RenderRequest has no destination texture, set one before sending request");
				return;
			}
			int num = standardRequest?.mipLevel ?? singleCameraRequest.mipLevel;
			int num2 = standardRequest?.slice ?? singleCameraRequest.slice;
			int num3 = (int)(standardRequest?.face ?? singleCameraRequest.face);
			RenderTexture targetTexture = camera.targetTexture;
			RenderTexture renderTexture2 = null;
			RenderTextureDescriptor desc = renderTexture.descriptor;
			if (renderTexture.dimension == TextureDimension.Cube)
			{
				desc = default(RenderTextureDescriptor);
			}
			desc.colorFormat = renderTexture.format;
			desc.volumeDepth = 1;
			desc.msaaSamples = renderTexture.descriptor.msaaSamples;
			desc.dimension = TextureDimension.Tex2D;
			desc.width = renderTexture.width / (int)Math.Pow(2.0, num);
			desc.height = renderTexture.height / (int)Math.Pow(2.0, num);
			desc.width = Mathf.Max(1, desc.width);
			desc.height = Mathf.Max(1, desc.height);
			if (renderTexture.dimension != TextureDimension.Tex2D || num != 0)
			{
				renderTexture2 = RenderTexture.GetTemporary(desc);
			}
			camera.targetTexture = (renderTexture2 ? renderTexture2 : renderTexture);
			if (standardRequest != null)
			{
				Render(context, new List<Camera> { camera });
			}
			else
			{
				List<Camera> value;
				using (ListPool<Camera>.Get(out value))
				{
					value.Add(camera);
					using (new ContextRenderingScope(context, value))
					{
						using (new CameraRenderingScope(context, camera))
						{
							camera.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out var component);
							RenderSingleCameraInternal(context, camera, ref component);
						}
					}
				}
			}
			if ((bool)renderTexture2)
			{
				bool flag = false;
				switch (renderTexture.dimension)
				{
				case TextureDimension.Tex2D:
					if ((SystemInfo.copyTextureSupport & CopyTextureSupport.Basic) != 0)
					{
						flag = true;
						Graphics.CopyTexture(renderTexture2, 0, 0, renderTexture, 0, num);
					}
					break;
				case TextureDimension.Tex2DArray:
					if ((SystemInfo.copyTextureSupport & CopyTextureSupport.DifferentTypes) != 0)
					{
						flag = true;
						Graphics.CopyTexture(renderTexture2, 0, 0, renderTexture, num2, num);
					}
					break;
				case TextureDimension.Tex3D:
					if ((SystemInfo.copyTextureSupport & CopyTextureSupport.DifferentTypes) != 0)
					{
						flag = true;
						Graphics.CopyTexture(renderTexture2, 0, 0, renderTexture, num2, num);
					}
					break;
				case TextureDimension.Cube:
					if ((SystemInfo.copyTextureSupport & CopyTextureSupport.DifferentTypes) != 0)
					{
						flag = true;
						Graphics.CopyTexture(renderTexture2, 0, 0, renderTexture, num3, num);
					}
					break;
				case TextureDimension.CubeArray:
					if ((SystemInfo.copyTextureSupport & CopyTextureSupport.DifferentTypes) != 0)
					{
						flag = true;
						Graphics.CopyTexture(renderTexture2, 0, 0, renderTexture, num3 + num2 * 6, num);
					}
					break;
				}
				if (!flag)
				{
					Debug.LogError("RenderRequest cannot have destination texture of this format: " + Enum.GetName(typeof(TextureDimension), renderTexture.dimension));
				}
			}
			camera.targetTexture = targetTexture;
			Graphics.SetRenderTarget(targetTexture);
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
		else
		{
			Debug.LogWarning("RenderRequest type: " + typeof(RequestData).FullName + " is either invalid or unsupported by the current pipeline");
		}
	}

	[Obsolete("RenderSingleCamera is obsolete, please use RenderPipeline.SubmitRenderRequest with UniversalRenderer.SingleCameraRequest as RequestData type")]
	public static void RenderSingleCamera(ScriptableRenderContext context, Camera camera)
	{
		RenderSingleCameraInternal(context, camera);
	}

	internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera, bool isLastBaseCamera = true)
	{
		UniversalAdditionalCameraData component = null;
		if (IsGameCamera(camera))
		{
			camera.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out component);
		}
		RenderSingleCameraInternal(context, camera, ref component, isLastBaseCamera);
	}

	internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera, ref UniversalAdditionalCameraData additionalCameraData, bool isLastBaseCamera = true)
	{
		if (additionalCameraData != null && additionalCameraData.renderType != 0)
		{
			Debug.LogWarning("Only Base cameras can be rendered with standalone RenderSingleCamera. Camera will be skipped.");
		}
		else if (camera.targetTexture.width == 0 || camera.targetTexture.height == 0 || camera.pixelWidth == 0 || camera.pixelHeight == 0)
		{
			Debug.LogWarning($"Camera '{camera.name}' has an invalid render target size (width: {camera.targetTexture.width}, height: {camera.targetTexture.height}) or pixel dimensions (width: {camera.pixelWidth}, height: {camera.pixelHeight}). Camera will be skipped.");
		}
		else
		{
			UniversalCameraData cameraData = CreateCameraData(GetRenderer(camera, additionalCameraData).frameData, camera, additionalCameraData);
			InitializeAdditionalCameraData(camera, additionalCameraData, resolveFinalTarget: true, isLastBaseCamera, cameraData);
			RenderSingleCamera(context, cameraData);
		}
	}

	private static bool TryGetCullingParameters(UniversalCameraData cameraData, out ScriptableCullingParameters cullingParams)
	{
		if (cameraData.xr.enabled)
		{
			cullingParams = cameraData.xr.cullingParams;
			if (!cameraData.camera.usePhysicalProperties && !XRGraphicsAutomatedTests.enabled)
			{
				cameraData.camera.fieldOfView = 57.29578f * Mathf.Atan(1f / cullingParams.stereoProjectionMatrix.m11) * 2f;
			}
			return true;
		}
		return cameraData.camera.TryGetCullingParameters(stereoAware: false, out cullingParams);
	}

	private static void RenderSingleCamera(ScriptableRenderContext context, UniversalCameraData cameraData)
	{
		Camera camera = cameraData.camera;
		ScriptableRenderer renderer = cameraData.renderer;
		if (renderer == null)
		{
			Debug.LogWarning($"Trying to render {camera.name} with an invalid renderer. Camera rendering will be skipped.");
			return;
		}
		using ContextContainer contextContainer = renderer.frameData;
		if (!TryGetCullingParameters(cameraData, out var cullingParams))
		{
			return;
		}
		ScriptableRenderer.current = renderer;
		s_RenderGraph.nativeRenderPassesEnabled = renderer.supportsNativeRenderPassRendergraphCompiler;
		_ = cameraData.isSceneViewCamera;
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		CommandBuffer cmd = (cameraData.xr.enabled ? null : commandBuffer);
		CameraMetadataCache.CameraMetadataCacheEntry cached = CameraMetadataCache.GetCached(camera);
		using (new ProfilingScope(cmd, cached.sampler))
		{
			renderer.Clear(cameraData.renderType);
			using (new ProfilingScope(Profiling.Pipeline.Renderer.setupCullingParameters))
			{
				CameraData cameraData2 = new CameraData(contextContainer);
				renderer.OnPreCullRenderPasses(in cameraData2);
				renderer.SetupCullingParameters(ref cullingParams, ref cameraData2);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			SetupPerCameraShaderConstants(commandBuffer);
			ProbeVolumesOptions options = null;
			if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var component))
			{
				options = component.volumeStack?.GetComponent<ProbeVolumesOptions>();
			}
			bool flag = asset != null && asset.lightProbeSystem == LightProbeSystem.ProbeVolumes;
			ProbeReferenceVolume.instance.SetEnableStateFromSRP(flag);
			ProbeReferenceVolume.instance.SetVertexSamplingEnabled(asset.shEvalMode == ShEvalMode.PerVertex || asset.shEvalMode == ShEvalMode.Mixed);
			if (flag && ProbeReferenceVolume.instance.isInitialized)
			{
				ProbeReferenceVolume.instance.PerformPendingOperations();
				if (camera.cameraType != CameraType.Reflection && camera.cameraType != CameraType.Preview)
				{
					ProbeReferenceVolume.instance.UpdateCellStreaming(commandBuffer, camera, options);
				}
			}
			if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
			{
				ScriptableRenderContext.EmitGeometryForCamera(camera);
			}
			if (flag)
			{
				ProbeReferenceVolume.instance.BindAPVRuntimeResources(commandBuffer, isProbeVolumeEnabled: true);
			}
			ProbeReferenceVolume.instance.RenderDebug(camera, options, Texture2D.whiteTexture);
			if (component != null)
			{
				component.motionVectorsPersistentData.Update(cameraData);
			}
			if (cameraData.taaHistory != null)
			{
				UpdateTemporalAATargets(cameraData);
			}
			RTHandles.SetReferenceSize(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
			UniversalRenderingData universalRenderingData = contextContainer.Create<UniversalRenderingData>();
			universalRenderingData.cullResults = context.Cull(ref cullingParams);
			RenderRequestBatcherContext context2 = default(RenderRequestBatcherContext);
			context2.commandBuffer = commandBuffer;
			GPUResidentDrawer.PostCullBeginCameraRendering(context2);
			RenderingMode? renderingMode = (cameraData.renderer as UniversalRenderer)?.renderingModeActual;
			UniversalLightData lightData;
			UniversalShadowData shadowData;
			using (new ProfilingScope(Profiling.Pipeline.initializeRenderingData))
			{
				CreateUniversalResourceData(contextContainer);
				lightData = CreateLightData(contextContainer, asset, universalRenderingData.cullResults.visibleLights, renderingMode);
				shadowData = CreateShadowData(contextContainer, asset, renderingMode);
				CreatePostProcessingData(contextContainer, asset);
				CreateRenderingData(contextContainer, asset, commandBuffer, renderingMode, cameraData.renderer);
				CreateCullContextData(contextContainer, context);
			}
			RenderingData renderingData = new RenderingData(contextContainer);
			CheckAndApplyDebugSettings(ref renderingData);
			CreateShadowAtlasAndCullShadowCasters(lightData, shadowData, cameraData, ref universalRenderingData.cullResults, ref context);
			renderer.AddRenderPasses(ref renderingData);
			if (useRenderGraph)
			{
				RecordAndExecuteRenderGraph(s_RenderGraph, context, renderer, commandBuffer, cameraData.camera, cached.name);
				renderer.FinishRenderGraphRendering(commandBuffer);
			}
			else
			{
				using (new ProfilingScope(Profiling.Pipeline.Renderer.setup))
				{
					renderer.Setup(context, ref renderingData);
				}
				renderer.Execute(context, ref renderingData);
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
		using (new ProfilingScope(Profiling.Pipeline.Context.submit))
		{
			if (!useRenderGraph && renderer.useRenderPassEnabled && !context.SubmitForRenderPassValidation())
			{
				renderer.useRenderPassEnabled = false;
				commandBuffer.SetKeyword(in ShaderGlobalKeywords.RenderPassEnabled, value: false);
				Debug.LogWarning("Rendering command not supported inside a native RenderPass found. Falling back to non-RenderPass rendering path");
			}
			context.Submit();
		}
		ScriptableRenderer.current = null;
	}

	private static void CreateShadowAtlasAndCullShadowCasters(UniversalLightData lightData, UniversalShadowData shadowData, UniversalCameraData cameraData, ref CullingResults cullResults, ref ScriptableRenderContext context)
	{
		if (shadowData.supportsMainLightShadows || shadowData.supportsAdditionalLightShadows)
		{
			if (shadowData.supportsMainLightShadows)
			{
				InitializeMainLightShadowResolution(shadowData);
			}
			if (shadowData.supportsAdditionalLightShadows)
			{
				shadowData.shadowAtlasLayout = BuildAdditionalLightsShadowAtlasLayout(lightData, shadowData, cameraData);
			}
			shadowData.visibleLightsShadowCullingInfos = ShadowCulling.CullShadowCasters(ref context, shadowData, ref shadowData.shadowAtlasLayout, ref cullResults);
		}
	}

	private static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera, bool isLastBaseCamera)
	{
		using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.RenderCameraStack)))
		{
			baseCamera.TryGetComponent<UniversalAdditionalCameraData>(out var component);
			if (component != null && component.renderType == CameraRenderType.Overlay)
			{
				return;
			}
			ScriptableRenderer renderer = GetRenderer(baseCamera, component);
			List<Camera> list = ((renderer == null || !renderer.SupportsCameraStackingType(CameraRenderType.Base)) ? null : component?.cameraStack);
			bool flag = component != null && component.renderPostProcessing;
			bool flag2 = HDROutputForMainDisplayIsActive();
			int num = -1;
			if (list != null)
			{
				Type type = renderer.GetType();
				bool flag3 = false;
				stackedOverlayCamerasRequireDepthForPostProcessing = false;
				for (int i = 0; i < list.Count; i++)
				{
					Camera camera = list[i];
					if (camera == null)
					{
						flag3 = true;
					}
					else if (camera.isActiveAndEnabled)
					{
						camera.TryGetComponent<UniversalAdditionalCameraData>(out var component2);
						ScriptableRenderer renderer2 = GetRenderer(camera, component2);
						Type type2 = renderer2.GetType();
						if (type2 != type)
						{
							Debug.LogWarning("Only cameras with compatible renderer types can be stacked. The camera: " + camera.name + " are using the renderer " + type2.Name + ", but the base camera: " + baseCamera.name + " are using " + type.Name + ". Will skip rendering");
						}
						else if ((renderer2.SupportedCameraStackingTypes() & 2) == 0)
						{
							Debug.LogWarning("The camera: " + camera.name + " is using a renderer of type " + renderer.GetType().Name + " which does not support Overlay cameras in it's current state.");
						}
						else if (component2 == null || component2.renderType != CameraRenderType.Overlay)
						{
							Debug.LogWarning("Stack can only contain Overlay cameras. The camera: " + camera.name + " " + $"has a type {component2.renderType} that is not supported. Will skip rendering.");
						}
						else
						{
							stackedOverlayCamerasRequireDepthForPostProcessing |= CheckPostProcessForDepth();
							flag |= component2.renderPostProcessing;
							num = i;
						}
					}
				}
				if (flag3)
				{
					component.UpdateCameraStack();
				}
			}
			bool flag4 = num != -1;
			bool flag5 = false;
			bool enableXR = component?.allowXRRendering ?? true;
			XRLayout xRLayout = XRSystem.NewLayout();
			xRLayout.AddCamera(baseCamera, enableXR);
			VFXCameraXRSettings camXRSettings = default(VFXCameraXRSettings);
			foreach (var activePass in xRLayout.GetActivePasses())
			{
				XRPass xr = activePass.Item2;
				XRPassUniversal xrPass = xr as XRPassUniversal;
				if (xr.enabled)
				{
					flag5 = true;
					UpdateCameraStereoMatrices(baseCamera, xr);
					float num2 = XRSystem.GetRenderViewportScale();
					if (XRSystem.GetDynamicResolutionScale() < 1f)
					{
						num2 = XRSystem.GetDynamicResolutionScale();
					}
					ScalableBufferManager.ResizeBuffers(num2, num2);
				}
				bool stackLastCameraOutputToHDR = false;
				using (new CameraRenderingScope(context, baseCamera))
				{
					UpdateVolumeFramework(baseCamera, component);
					UniversalCameraData universalCameraData = CreateCameraData(renderer.frameData, baseCamera, component);
					if (xr.enabled)
					{
						universalCameraData.xr = xr;
						UpdateCameraData(universalCameraData, in xr);
						xRLayout.ReconfigurePass(xr, baseCamera);
						XRSystemUniversal.BeginLateLatching(baseCamera, xrPass);
					}
					InitializeAdditionalCameraData(baseCamera, component, !flag4, isLastBaseCamera, universalCameraData);
					camXRSettings.viewTotal = ((!universalCameraData.xr.enabled) ? 1u : 2u);
					camXRSettings.viewCount = ((!universalCameraData.xr.enabled) ? 1u : ((uint)universalCameraData.xr.viewCount));
					camXRSettings.viewOffset = (uint)universalCameraData.xr.multipassId;
					VFXManager.PrepareCamera(baseCamera, camXRSettings);
					universalCameraData.postProcessingRequiresDepthTexture |= stackedOverlayCamerasRequireDepthForPostProcessing;
					bool flag6 = flag2;
					if (xr.enabled)
					{
						flag6 = xr.isHDRDisplayOutputActive;
					}
					stackLastCameraOutputToHDR = asset.supportsHDR && flag6 && baseCamera.targetTexture == null && (baseCamera.cameraType == CameraType.Game || baseCamera.cameraType == CameraType.VR) && universalCameraData.allowHDROutput;
					universalCameraData.stackAnyPostProcessingEnabled = flag;
					universalCameraData.stackLastCameraOutputToHDR = stackLastCameraOutputToHDR;
					RenderSingleCamera(context, universalCameraData);
				}
				if (xr.enabled)
				{
					XRSystemUniversal.EndLateLatching(baseCamera, xrPass);
				}
				if (!flag4)
				{
					continue;
				}
				for (int j = 0; j < list.Count; j++)
				{
					Camera camera2 = list[j];
					if (!camera2.isActiveAndEnabled)
					{
						continue;
					}
					camera2.TryGetComponent<UniversalAdditionalCameraData>(out var component3);
					if (component3 != null)
					{
						UniversalCameraData universalCameraData2 = CreateCameraData(GetRenderer(camera2, component3).frameData, baseCamera, component);
						if (xr.enabled)
						{
							universalCameraData2.xr = xr;
							UpdateCameraData(universalCameraData2, in xr);
						}
						InitializeAdditionalCameraData(camera2, component3, resolveFinalTarget: false, isLastBaseCamera, universalCameraData2);
						universalCameraData2.camera = camera2;
						universalCameraData2.baseCamera = baseCamera;
						UpdateCameraStereoMatrices(component3.camera, xr);
						using (new CameraRenderingScope(context, camera2))
						{
							VFXManager.PrepareCamera(camera2, camXRSettings);
							UpdateVolumeFramework(camera2, component3);
							bool resolveFinalTarget = j == num;
							InitializeAdditionalCameraData(camera2, component3, resolveFinalTarget, isLastBaseCamera, universalCameraData2);
							universalCameraData2.stackAnyPostProcessingEnabled = flag;
							universalCameraData2.stackLastCameraOutputToHDR = stackLastCameraOutputToHDR;
							xRLayout.ReconfigurePass(universalCameraData2.xr, camera2);
							RenderSingleCamera(context, universalCameraData2);
						}
					}
				}
			}
			if (flag5)
			{
				CommandBuffer commandBuffer = CommandBufferPool.Get();
				XRSystem.RenderMirrorView(commandBuffer, baseCamera);
				context.ExecuteCommandBuffer(commandBuffer);
				context.Submit();
				CommandBufferPool.Release(commandBuffer);
			}
			XRSystem.EndLayout();
		}
	}

	private static void UpdateCameraData(UniversalCameraData baseCameraData, in XRPass xr)
	{
		Rect rect = baseCameraData.camera.rect;
		Rect viewport = xr.GetViewport();
		baseCameraData.pixelRect = new Rect(rect.x * viewport.width + viewport.x, rect.y * viewport.height + viewport.y, rect.width * viewport.width, rect.height * viewport.height);
		Rect pixelRect = baseCameraData.pixelRect;
		baseCameraData.pixelWidth = (int)Math.Round(pixelRect.width + pixelRect.x) - (int)Math.Round(pixelRect.x);
		baseCameraData.pixelHeight = (int)Math.Round(pixelRect.height + pixelRect.y) - (int)Math.Round(pixelRect.y);
		baseCameraData.aspectRatio = (float)baseCameraData.pixelWidth / (float)baseCameraData.pixelHeight;
		RenderTextureDescriptor cameraTargetDescriptor = baseCameraData.cameraTargetDescriptor;
		baseCameraData.cameraTargetDescriptor = xr.renderTargetDesc;
		if (baseCameraData.isHdrEnabled)
		{
			baseCameraData.cameraTargetDescriptor.graphicsFormat = cameraTargetDescriptor.graphicsFormat;
		}
		baseCameraData.cameraTargetDescriptor.msaaSamples = cameraTargetDescriptor.msaaSamples;
		if (baseCameraData.isDefaultViewport)
		{
			baseCameraData.cameraTargetDescriptor.useDynamicScale = true;
			return;
		}
		baseCameraData.cameraTargetDescriptor.width = baseCameraData.pixelWidth;
		baseCameraData.cameraTargetDescriptor.height = baseCameraData.pixelHeight;
		baseCameraData.cameraTargetDescriptor.useDynamicScale = false;
	}

	private static void UpdateVolumeFramework(Camera camera, UniversalAdditionalCameraData additionalCameraData)
	{
		using (new ProfilingScope(ProfilingSampler.Get(URPProfileId.UpdateVolumeFramework)))
		{
			if (!((camera.cameraType == CameraType.SceneView) | (additionalCameraData != null && additionalCameraData.requiresVolumeFrameworkUpdate)) && (bool)additionalCameraData)
			{
				if (additionalCameraData.volumeStack != null && !additionalCameraData.volumeStack.isValid)
				{
					camera.DestroyVolumeStack(additionalCameraData);
				}
				if (additionalCameraData.volumeStack == null)
				{
					camera.UpdateVolumeStack(additionalCameraData);
				}
				VolumeManager.instance.stack = additionalCameraData.volumeStack;
			}
			else
			{
				if ((bool)additionalCameraData && additionalCameraData.volumeStack != null)
				{
					camera.DestroyVolumeStack(additionalCameraData);
				}
				camera.GetVolumeLayerMaskAndTrigger(additionalCameraData, out var layerMask, out var trigger);
				VolumeManager.instance.ResetMainStack();
				VolumeManager.instance.Update(trigger, layerMask);
			}
		}
	}

	private static bool CheckPostProcessForDepth(UniversalCameraData cameraData)
	{
		if (!cameraData.postProcessEnabled)
		{
			return false;
		}
		if (cameraData.IsTemporalAAEnabled() && cameraData.renderType == CameraRenderType.Base)
		{
			return true;
		}
		return CheckPostProcessForDepth();
	}

	private static bool CheckPostProcessForDepth()
	{
		VolumeStack stack = VolumeManager.instance.stack;
		if (stack.GetComponent<DepthOfField>().IsActive())
		{
			return true;
		}
		if (stack.GetComponent<MotionBlur>().IsActive())
		{
			return true;
		}
		return false;
	}

	private static void SetSupportedRenderingFeatures(UniversalRenderPipelineAsset pipelineAsset)
	{
		SupportedRenderingFeatures.active.supportsHDR = pipelineAsset.supportsHDR;
		SupportedRenderingFeatures.active.rendersUIOverlay = true;
	}

	private static ScriptableRenderer GetRenderer(Camera camera, UniversalAdditionalCameraData additionalCameraData)
	{
		ScriptableRenderer scriptableRenderer = ((additionalCameraData != null) ? additionalCameraData.scriptableRenderer : null);
		if (scriptableRenderer == null || camera.cameraType == CameraType.SceneView)
		{
			scriptableRenderer = asset.scriptableRenderer;
		}
		return scriptableRenderer;
	}

	private static UniversalCameraData CreateCameraData(ContextContainer frameData, Camera camera, UniversalAdditionalCameraData additionalCameraData)
	{
		using (new ProfilingScope(Profiling.Pipeline.initializeCameraData))
		{
			ScriptableRenderer renderer = GetRenderer(camera, additionalCameraData);
			UniversalCameraData universalCameraData = frameData.Create<UniversalCameraData>();
			InitializeStackedCameraData(camera, additionalCameraData, universalCameraData);
			universalCameraData.camera = camera;
			universalCameraData.historyManager = additionalCameraData?.historyManager;
			bool flag = renderer?.supportedRenderingFeatures.msaa ?? false;
			int msaaSamples = 1;
			if (camera.allowMSAA && asset.msaaSampleCount > 1 && flag)
			{
				msaaSamples = ((camera.targetTexture != null) ? camera.targetTexture.antiAliasing : asset.msaaSampleCount);
			}
			if (universalCameraData.xrRendering && flag && camera.targetTexture == null)
			{
				msaaSamples = (int)XRSystem.GetDisplayMSAASamples();
			}
			bool preserveFramebufferAlpha = Graphics.preserveFramebufferAlpha;
			universalCameraData.hdrColorBufferPrecision = (asset ? asset.hdrColorBufferPrecision : HDRColorBufferPrecision._32Bits);
			universalCameraData.cameraTargetDescriptor = CreateRenderTextureDescriptor(camera, universalCameraData, universalCameraData.isHdrEnabled, universalCameraData.hdrColorBufferPrecision, msaaSamples, preserveFramebufferAlpha, universalCameraData.requiresOpaqueTexture);
			GraphicsFormatUtility.GetAlphaComponentCount(universalCameraData.cameraTargetDescriptor.graphicsFormat);
			universalCameraData.isAlphaOutputEnabled = GraphicsFormatUtility.HasAlphaChannel(universalCameraData.cameraTargetDescriptor.graphicsFormat);
			if (universalCameraData.camera.cameraType == CameraType.SceneView && CoreUtils.IsSceneFilteringEnabled())
			{
				universalCameraData.isAlphaOutputEnabled = true;
			}
			return universalCameraData;
		}
	}

	private static void InitializeStackedCameraData(Camera baseCamera, UniversalAdditionalCameraData baseAdditionalCameraData, UniversalCameraData cameraData)
	{
		using (new ProfilingScope(Profiling.Pipeline.initializeStackedCameraData))
		{
			UniversalRenderPipelineAsset universalRenderPipelineAsset = asset;
			cameraData.targetTexture = baseCamera.targetTexture;
			cameraData.cameraType = baseCamera.cameraType;
			if (cameraData.isSceneViewCamera)
			{
				cameraData.volumeLayerMask = 1;
				cameraData.volumeTrigger = null;
				cameraData.isStopNaNEnabled = false;
				cameraData.isDitheringEnabled = false;
				cameraData.antialiasing = AntialiasingMode.None;
				cameraData.antialiasingQuality = AntialiasingQuality.High;
				cameraData.xrRendering = false;
				cameraData.allowHDROutput = false;
			}
			else if (baseAdditionalCameraData != null)
			{
				cameraData.volumeLayerMask = baseAdditionalCameraData.volumeLayerMask;
				cameraData.volumeTrigger = ((baseAdditionalCameraData.volumeTrigger == null) ? baseCamera.transform : baseAdditionalCameraData.volumeTrigger);
				cameraData.isStopNaNEnabled = baseAdditionalCameraData.stopNaN && SystemInfo.graphicsShaderLevel >= 35;
				cameraData.isDitheringEnabled = baseAdditionalCameraData.dithering;
				cameraData.antialiasing = baseAdditionalCameraData.antialiasing;
				cameraData.antialiasingQuality = baseAdditionalCameraData.antialiasingQuality;
				cameraData.xrRendering = baseAdditionalCameraData.allowXRRendering && XRSystem.displayActive;
				cameraData.allowHDROutput = baseAdditionalCameraData.allowHDROutput;
			}
			else
			{
				cameraData.volumeLayerMask = 1;
				cameraData.volumeTrigger = null;
				cameraData.isStopNaNEnabled = false;
				cameraData.isDitheringEnabled = false;
				cameraData.antialiasing = AntialiasingMode.None;
				cameraData.antialiasingQuality = AntialiasingQuality.High;
				cameraData.xrRendering = XRSystem.displayActive;
				cameraData.allowHDROutput = true;
			}
			cameraData.isHdrEnabled = baseCamera.allowHDR && universalRenderPipelineAsset.supportsHDR;
			cameraData.allowHDROutput &= universalRenderPipelineAsset.supportsHDR;
			Rect rect = baseCamera.rect;
			cameraData.pixelRect = baseCamera.pixelRect;
			cameraData.pixelWidth = baseCamera.pixelWidth;
			cameraData.pixelHeight = baseCamera.pixelHeight;
			cameraData.aspectRatio = (float)cameraData.pixelWidth / (float)cameraData.pixelHeight;
			cameraData.isDefaultViewport = !(Math.Abs(rect.x) > 0f) && !(Math.Abs(rect.y) > 0f) && !(Math.Abs(rect.width) < 1f) && !(Math.Abs(rect.height) < 1f);
			bool flag = cameraData.cameraType == CameraType.SceneView || cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection;
			bool flag2 = Mathf.Abs(1f - universalRenderPipelineAsset.renderScale) < 0.05f || flag;
			cameraData.renderScale = (flag2 ? 1f : universalRenderPipelineAsset.renderScale);
			RenderGraphSettings settings;
			bool enableRenderGraph = GraphicsSettings.TryGetRenderPipelineSettings<RenderGraphSettings>(out settings) && !settings.enableRenderCompatibilityMode;
			cameraData.upscalingFilter = ResolveUpscalingFilterSelection(new Vector2(cameraData.pixelWidth, cameraData.pixelHeight), cameraData.renderScale, universalRenderPipelineAsset.upscalingFilter, enableRenderGraph);
			if (cameraData.renderScale > 1f)
			{
				cameraData.imageScalingMode = ImageScalingMode.Downscaling;
			}
			else if (cameraData.renderScale < 1f || (!flag && (cameraData.upscalingFilter == ImageUpscalingFilter.FSR || cameraData.upscalingFilter == ImageUpscalingFilter.STP)))
			{
				cameraData.imageScalingMode = ImageScalingMode.Upscaling;
				if (cameraData.upscalingFilter == ImageUpscalingFilter.STP)
				{
					cameraData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
				}
			}
			else
			{
				cameraData.imageScalingMode = ImageScalingMode.None;
			}
			cameraData.fsrOverrideSharpness = universalRenderPipelineAsset.fsrOverrideSharpness;
			cameraData.fsrSharpness = universalRenderPipelineAsset.fsrSharpness;
			cameraData.xr = XRSystem.emptyPass;
			XRSystem.SetRenderScale(cameraData.renderScale);
			SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
			SortingCriteria sortingCriteria2 = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
			bool hasHiddenSurfaceRemovalOnGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
			bool flag3 = (baseCamera.opaqueSortMode == OpaqueSortMode.Default && hasHiddenSurfaceRemovalOnGPU) || baseCamera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
			cameraData.defaultOpaqueSortFlags = (flag3 ? sortingCriteria2 : sortingCriteria);
			cameraData.captureActions = Unity.RenderPipelines.Core.Runtime.Shared.CameraCaptureBridge.GetCachedCaptureActionsEnumerator(baseCamera);
		}
	}

	private static void InitializeAdditionalCameraData(Camera camera, UniversalAdditionalCameraData additionalCameraData, bool resolveFinalTarget, bool isLastBaseCamera, UniversalCameraData cameraData)
	{
		using (new ProfilingScope(Profiling.Pipeline.initializeAdditionalCameraData))
		{
			ScriptableRenderer renderer = GetRenderer(camera, additionalCameraData);
			UniversalRenderPipelineAsset universalRenderPipelineAsset = asset;
			bool flag = universalRenderPipelineAsset.supportsMainLightShadows || universalRenderPipelineAsset.supportsAdditionalLightShadows;
			cameraData.maxShadowDistance = Mathf.Min(universalRenderPipelineAsset.shadowDistance, camera.farClipPlane);
			cameraData.maxShadowDistance = ((flag && cameraData.maxShadowDistance >= camera.nearClipPlane) ? cameraData.maxShadowDistance : 0f);
			if (cameraData.isSceneViewCamera)
			{
				cameraData.renderType = CameraRenderType.Base;
				cameraData.clearDepth = true;
				cameraData.postProcessEnabled = CoreUtils.ArePostProcessesEnabled(camera);
				cameraData.requiresDepthTexture = universalRenderPipelineAsset.supportsCameraDepthTexture;
				cameraData.requiresOpaqueTexture = universalRenderPipelineAsset.supportsCameraOpaqueTexture;
				cameraData.useScreenCoordOverride = false;
				cameraData.screenSizeOverride = cameraData.pixelRect.size;
				cameraData.screenCoordScaleBias = Vector2.one;
			}
			else if (additionalCameraData != null)
			{
				cameraData.renderType = additionalCameraData.renderType;
				cameraData.clearDepth = additionalCameraData.renderType == CameraRenderType.Base || additionalCameraData.clearDepth;
				cameraData.postProcessEnabled = additionalCameraData.renderPostProcessing;
				cameraData.maxShadowDistance = (additionalCameraData.renderShadows ? cameraData.maxShadowDistance : 0f);
				cameraData.requiresDepthTexture = additionalCameraData.requiresDepthTexture;
				cameraData.requiresOpaqueTexture = additionalCameraData.requiresColorTexture;
				cameraData.useScreenCoordOverride = additionalCameraData.useScreenCoordOverride;
				cameraData.screenSizeOverride = additionalCameraData.screenSizeOverride;
				cameraData.screenCoordScaleBias = additionalCameraData.screenCoordScaleBias;
			}
			else
			{
				cameraData.renderType = CameraRenderType.Base;
				cameraData.clearDepth = true;
				cameraData.postProcessEnabled = false;
				cameraData.requiresDepthTexture = universalRenderPipelineAsset.supportsCameraDepthTexture;
				cameraData.requiresOpaqueTexture = universalRenderPipelineAsset.supportsCameraOpaqueTexture;
				cameraData.useScreenCoordOverride = false;
				cameraData.screenSizeOverride = cameraData.pixelRect.size;
				cameraData.screenCoordScaleBias = Vector2.one;
			}
			cameraData.renderer = renderer;
			cameraData.postProcessingRequiresDepthTexture = CheckPostProcessForDepth(cameraData);
			cameraData.resolveFinalTarget = resolveFinalTarget;
			cameraData.isLastBaseCamera = isLastBaseCamera;
			int useGPUOcclusionCulling;
			if (GPUResidentDrawer.IsInstanceOcclusionCullingEnabled() && renderer.supportsGPUOcclusion)
			{
				CameraType cameraType = camera.cameraType;
				useGPUOcclusionCulling = ((cameraType == CameraType.SceneView || cameraType == CameraType.Game || cameraType == CameraType.Preview) ? 1 : 0);
			}
			else
			{
				useGPUOcclusionCulling = 0;
			}
			cameraData.useGPUOcclusionCulling = (byte)useGPUOcclusionCulling != 0;
			cameraData.requiresDepthTexture |= cameraData.useGPUOcclusionCulling;
			bool num = cameraData.renderType == CameraRenderType.Overlay;
			if (num)
			{
				cameraData.requiresOpaqueTexture = false;
			}
			if (additionalCameraData != null)
			{
				UpdateTemporalAAData(cameraData, additionalCameraData);
			}
			Matrix4x4 projectionMatrix = camera.projectionMatrix;
			if (num && !camera.orthographic && cameraData.pixelRect != camera.pixelRect)
			{
				float m = camera.projectionMatrix.m00 * camera.aspect / cameraData.aspectRatio;
				projectionMatrix.m00 = m;
			}
			ApplyTaaRenderingDebugOverrides(ref cameraData.taaSettings);
			TemporalAA.JitterFunc jitterFunc = (cameraData.IsSTPEnabled() ? StpUtils.s_JitterFunc : TemporalAA.s_JitterFunc);
			Matrix4x4 jitterMatrix = TemporalAA.CalculateJitterMatrix(cameraData, jitterFunc);
			cameraData.SetViewProjectionAndJitterMatrix(camera.worldToCameraMatrix, projectionMatrix, jitterMatrix);
			cameraData.worldSpaceCameraPos = camera.transform.position;
			Color backgroundColor = camera.backgroundColor;
			cameraData.backgroundColor = CoreUtils.ConvertSRGBToActiveColorSpace(backgroundColor);
			cameraData.stackAnyPostProcessingEnabled = cameraData.postProcessEnabled;
			cameraData.stackLastCameraOutputToHDR = cameraData.isHDROutputActive;
			bool flag2 = !cameraData.postProcessEnabled || (cameraData.postProcessEnabled && universalRenderPipelineAsset.allowPostProcessAlphaOutput);
			cameraData.isAlphaOutputEnabled &= flag2;
		}
	}

	private static UniversalRenderingData CreateRenderingData(ContextContainer frameData, UniversalRenderPipelineAsset settings, CommandBuffer cmd, RenderingMode? renderingMode, ScriptableRenderer renderer)
	{
		UniversalLightData universalLightData = frameData.Get<UniversalLightData>();
		UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
		universalRenderingData.supportsDynamicBatching = settings.supportsDynamicBatching;
		universalRenderingData.perObjectData = GetPerObjectLightFlags(universalLightData, settings, renderingMode);
		if (useRenderGraph)
		{
			universalRenderingData.m_CommandBuffer = null;
		}
		else
		{
			universalRenderingData.m_CommandBuffer = cmd;
		}
		if (renderer is UniversalRenderer universalRenderer)
		{
			universalRenderingData.renderingMode = universalRenderer.renderingModeActual;
			universalRenderingData.prepassLayerMask = universalRenderer.prepassLayerMask;
			universalRenderingData.opaqueLayerMask = universalRenderer.opaqueLayerMask;
			universalRenderingData.transparentLayerMask = universalRenderer.transparentLayerMask;
		}
		universalRenderingData.stencilLodCrossFadeEnabled = settings.enableLODCrossFade && settings.lodCrossFadeDitheringType == LODCrossFadeDitheringType.Stencil;
		return universalRenderingData;
	}

	private static UniversalShadowData CreateShadowData(ContextContainer frameData, UniversalRenderPipelineAsset urpAsset, RenderingMode? renderingMode)
	{
		using (new ProfilingScope(Profiling.Pipeline.initializeShadowData))
		{
			UniversalShadowData universalShadowData = frameData.Create<UniversalShadowData>();
			UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
			UniversalLightData universalLightData = frameData.Get<UniversalLightData>();
			m_ShadowBiasData.Clear();
			m_ShadowResolutionData.Clear();
			universalShadowData.shadowmapDepthBufferBits = 16;
			universalShadowData.mainLightShadowCascadeBorder = urpAsset.cascadeBorder;
			universalShadowData.mainLightShadowCascadesCount = urpAsset.shadowCascadeCount;
			universalShadowData.mainLightShadowCascadesSplit = GetMainLightCascadeSplit(universalShadowData.mainLightShadowCascadesCount, urpAsset);
			universalShadowData.mainLightShadowmapWidth = urpAsset.mainLightShadowmapResolution;
			universalShadowData.mainLightShadowmapHeight = urpAsset.mainLightShadowmapResolution;
			universalShadowData.additionalLightsShadowmapWidth = (universalShadowData.additionalLightsShadowmapHeight = urpAsset.additionalLightsShadowmapResolution);
			universalShadowData.isKeywordAdditionalLightShadowsEnabled = false;
			universalShadowData.isKeywordSoftShadowsEnabled = false;
			universalShadowData.mainLightShadowResolution = 0;
			universalShadowData.mainLightRenderTargetWidth = 0;
			universalShadowData.mainLightRenderTargetHeight = 0;
			universalShadowData.shadowAtlasLayout = default(AdditionalLightsShadowAtlasLayout);
			universalShadowData.visibleLightsShadowCullingInfos = default(NativeArray<URPLightShadowCullingInfos>);
			int mainLightIndex = universalLightData.mainLightIndex;
			NativeArray<VisibleLight> visibleLights = universalLightData.visibleLights;
			bool flag = universalCameraData.maxShadowDistance > 0f;
			universalShadowData.mainLightShadowsEnabled = urpAsset.supportsMainLightShadows && urpAsset.mainLightRenderingMode == LightRenderingMode.PerPixel;
			universalShadowData.supportsMainLightShadows = SystemInfo.supportsShadows && universalShadowData.mainLightShadowsEnabled && flag;
			bool flag2 = renderingMode.HasValue && renderingMode.Value == RenderingMode.ForwardPlus;
			universalShadowData.additionalLightShadowsEnabled = urpAsset.supportsAdditionalLightShadows && (urpAsset.additionalLightsRenderingMode == LightRenderingMode.PerPixel || flag2);
			universalShadowData.supportsAdditionalLightShadows = SystemInfo.supportsShadows && universalShadowData.additionalLightShadowsEnabled && !universalLightData.shadeAdditionalLightsPerVertex && flag;
			if (!universalShadowData.supportsMainLightShadows && !universalShadowData.supportsAdditionalLightShadows)
			{
				return universalShadowData;
			}
			universalShadowData.supportsMainLightShadows &= mainLightIndex != -1 && visibleLights[mainLightIndex].light != null && visibleLights[mainLightIndex].light.shadows != LightShadows.None;
			if (universalShadowData.supportsAdditionalLightShadows)
			{
				bool flag3 = false;
				for (int i = 0; i < visibleLights.Length; i++)
				{
					if (i == mainLightIndex)
					{
						continue;
					}
					ref VisibleLight reference = ref visibleLights.UnsafeElementAtMutable(i);
					if (reference.lightType == LightType.Spot || reference.lightType == LightType.Point)
					{
						Light light = reference.light;
						if (!(light == null) && light.shadows != 0)
						{
							flag3 = true;
							break;
						}
					}
				}
				universalShadowData.supportsAdditionalLightShadows &= flag3;
			}
			if (!universalShadowData.supportsMainLightShadows && !universalShadowData.supportsAdditionalLightShadows)
			{
				return universalShadowData;
			}
			for (int j = 0; j < visibleLights.Length; j++)
			{
				if (!universalShadowData.supportsMainLightShadows && j == mainLightIndex)
				{
					m_ShadowBiasData.Add(Vector4.zero);
					m_ShadowResolutionData.Add(0);
					continue;
				}
				if (!universalShadowData.supportsAdditionalLightShadows && j != mainLightIndex)
				{
					m_ShadowBiasData.Add(Vector4.zero);
					m_ShadowResolutionData.Add(0);
					continue;
				}
				Light light2 = visibleLights.UnsafeElementAtMutable(j).light;
				UniversalAdditionalLightData component = null;
				if (light2 != null)
				{
					light2.gameObject.TryGetComponent<UniversalAdditionalLightData>(out component);
				}
				if ((bool)component && !component.usePipelineSettings)
				{
					m_ShadowBiasData.Add(new Vector4(light2.shadowBias, light2.shadowNormalBias, 0f, 0f));
				}
				else
				{
					m_ShadowBiasData.Add(new Vector4(urpAsset.shadowDepthBias, urpAsset.shadowNormalBias, 0f, 0f));
				}
				if ((bool)component && component.additionalLightsShadowResolutionTier == UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierCustom)
				{
					m_ShadowResolutionData.Add((int)light2.shadowResolution);
				}
				else if ((bool)component && component.additionalLightsShadowResolutionTier != UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierCustom)
				{
					int additionalLightsShadowResolutionTier = Mathf.Clamp(component.additionalLightsShadowResolutionTier, UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierLow, UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierHigh);
					m_ShadowResolutionData.Add(urpAsset.GetAdditionalLightsShadowResolution(additionalLightsShadowResolutionTier));
				}
				else
				{
					m_ShadowResolutionData.Add(urpAsset.GetAdditionalLightsShadowResolution(UniversalAdditionalLightData.AdditionalLightsShadowDefaultResolutionTier));
				}
			}
			universalShadowData.bias = m_ShadowBiasData;
			universalShadowData.resolution = m_ShadowResolutionData;
			universalShadowData.supportsSoftShadows = urpAsset.supportsSoftShadows && (universalShadowData.supportsMainLightShadows || universalShadowData.supportsAdditionalLightShadows);
			return universalShadowData;
		}
	}

	private static CullContextData CreateCullContextData(ContextContainer frameData, ScriptableRenderContext context)
	{
		CullContextData cullContextData = frameData.Create<CullContextData>();
		cullContextData.SetRenderContext(in context);
		return cullContextData;
	}

	private static Vector3 GetMainLightCascadeSplit(int mainLightShadowCascadesCount, UniversalRenderPipelineAsset urpAsset)
	{
		return mainLightShadowCascadesCount switch
		{
			1 => new Vector3(1f, 0f, 0f), 
			2 => new Vector3(urpAsset.cascade2Split, 1f, 0f), 
			3 => urpAsset.cascade3Split, 
			_ => urpAsset.cascade4Split, 
		};
	}

	private static void InitializeMainLightShadowResolution(UniversalShadowData shadowData)
	{
		shadowData.mainLightShadowResolution = ShadowUtils.GetMaxTileResolutionInAtlas(shadowData.mainLightShadowmapWidth, shadowData.mainLightShadowmapHeight, shadowData.mainLightShadowCascadesCount);
		shadowData.mainLightRenderTargetWidth = shadowData.mainLightShadowmapWidth;
		shadowData.mainLightRenderTargetHeight = ((shadowData.mainLightShadowCascadesCount == 2) ? (shadowData.mainLightShadowmapHeight >> 1) : shadowData.mainLightShadowmapHeight);
	}

	private static UniversalPostProcessingData CreatePostProcessingData(ContextContainer frameData, UniversalRenderPipelineAsset settings)
	{
		UniversalPostProcessingData universalPostProcessingData = frameData.Create<UniversalPostProcessingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		universalPostProcessingData.isEnabled = universalCameraData.stackAnyPostProcessingEnabled;
		universalPostProcessingData.gradingMode = (settings.supportsHDR ? settings.colorGradingMode : ColorGradingMode.LowDynamicRange);
		if (universalCameraData.stackLastCameraOutputToHDR)
		{
			universalPostProcessingData.gradingMode = ColorGradingMode.HighDynamicRange;
		}
		universalPostProcessingData.lutSize = settings.colorGradingLutSize;
		universalPostProcessingData.useFastSRGBLinearConversion = settings.useFastSRGBLinearConversion;
		universalPostProcessingData.supportScreenSpaceLensFlare = settings.supportScreenSpaceLensFlare;
		universalPostProcessingData.supportDataDrivenLensFlare = settings.supportDataDrivenLensFlare;
		return universalPostProcessingData;
	}

	private static UniversalResourceData CreateUniversalResourceData(ContextContainer frameData)
	{
		return frameData.Create<UniversalResourceData>();
	}

	private static UniversalLightData CreateLightData(ContextContainer frameData, UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights, RenderingMode? renderingMode)
	{
		using (new ProfilingScope(Profiling.Pipeline.initializeLightData))
		{
			UniversalLightData universalLightData = frameData.Create<UniversalLightData>();
			universalLightData.visibleLights = visibleLights;
			universalLightData.mainLightIndex = GetMainLightIndex(settings, visibleLights);
			if (settings.additionalLightsRenderingMode != 0)
			{
				universalLightData.additionalLightsCount = Math.Min((universalLightData.mainLightIndex != -1) ? (visibleLights.Length - 1) : visibleLights.Length, maxVisibleAdditionalLights);
				universalLightData.maxPerObjectAdditionalLightsCount = Math.Min(settings.maxAdditionalLightsCount, maxPerObjectLights);
			}
			else
			{
				universalLightData.additionalLightsCount = 0;
				universalLightData.maxPerObjectAdditionalLightsCount = 0;
			}
			if (settings.mainLightRenderingMode == LightRenderingMode.Disabled)
			{
				int brightestDirectionalLightIndex = GetBrightestDirectionalLightIndex(settings, visibleLights);
				if (brightestDirectionalLightIndex != -1)
				{
					universalLightData.additionalLightsCount--;
					universalLightData.mainLightIndex = brightestDirectionalLightIndex;
				}
			}
			universalLightData.supportsAdditionalLights = settings.additionalLightsRenderingMode != LightRenderingMode.Disabled;
			universalLightData.shadeAdditionalLightsPerVertex = settings.additionalLightsRenderingMode == LightRenderingMode.PerVertex;
			universalLightData.supportsMixedLighting = settings.supportsMixedLighting;
			universalLightData.reflectionProbeBoxProjection = settings.reflectionProbeBoxProjection;
			universalLightData.supportsLightLayers = RenderingUtils.SupportsLightLayers(SystemInfo.graphicsDeviceType) && settings.useRenderingLayers;
			universalLightData.reflectionProbeBlending = settings.ShouldUseReflectionProbeBlending();
			universalLightData.reflectionProbeAtlas = renderingMode.HasValue && settings.ShouldUseReflectionProbeAtlasBlending(renderingMode.Value);
			return universalLightData;
		}
	}

	private static void ApplyTaaRenderingDebugOverrides(ref TemporalAA.Settings taaSettings)
	{
		switch (DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance.renderingSettings.taaDebugMode)
		{
		case DebugDisplaySettingsRendering.TaaDebugMode.ShowClampedHistory:
			taaSettings.m_FrameInfluence = 0f;
			break;
		case DebugDisplaySettingsRendering.TaaDebugMode.ShowRawFrame:
			taaSettings.m_FrameInfluence = 1f;
			break;
		case DebugDisplaySettingsRendering.TaaDebugMode.ShowRawFrameNoJitter:
			taaSettings.m_FrameInfluence = 1f;
			taaSettings.jitterScale = 0f;
			break;
		}
	}

	private static void UpdateTemporalAAData(UniversalCameraData cameraData, UniversalAdditionalCameraData additionalCameraData)
	{
		additionalCameraData.historyManager.RequestAccess<TaaHistory>();
		cameraData.taaHistory = additionalCameraData.historyManager.GetHistoryForWrite<TaaHistory>();
		if (cameraData.IsSTPEnabled())
		{
			additionalCameraData.historyManager.RequestAccess<StpHistory>();
			cameraData.stpHistory = additionalCameraData.historyManager.GetHistoryForWrite<StpHistory>();
		}
		ref TemporalAA.Settings taaSettings = ref additionalCameraData.taaSettings;
		cameraData.taaSettings = taaSettings;
		taaSettings.resetHistoryFrames -= ((taaSettings.resetHistoryFrames > 0) ? 1 : 0);
	}

	private static void UpdateTemporalAATargets(UniversalCameraData cameraData)
	{
		if (cameraData.IsTemporalAAEnabled())
		{
			bool flag = false;
			flag = cameraData.xr.enabled && !cameraData.xr.singlePassEnabled;
			bool flag2;
			if (cameraData.IsSTPRequested())
			{
				cameraData.taaHistory.Reset();
				flag2 = cameraData.stpHistory.Update(cameraData);
			}
			else
			{
				flag2 = cameraData.taaHistory.Update(ref cameraData.cameraTargetDescriptor, flag);
			}
			if (flag2)
			{
				cameraData.taaSettings.resetHistoryFrames += ((!flag) ? 1 : 2);
			}
		}
		else
		{
			cameraData.taaHistory.Reset();
			if (cameraData.IsSTPRequested())
			{
				cameraData.stpHistory?.Reset();
			}
		}
	}

	private static void UpdateCameraStereoMatrices(Camera camera, XRPass xr)
	{
		if (!xr.enabled)
		{
			return;
		}
		if (xr.singlePassEnabled)
		{
			for (int i = 0; i < Mathf.Min(2, xr.viewCount); i++)
			{
				camera.SetStereoProjectionMatrix((Camera.StereoscopicEye)i, xr.GetProjMatrix(i));
				camera.SetStereoViewMatrix((Camera.StereoscopicEye)i, xr.GetViewMatrix(i));
			}
		}
		else
		{
			camera.SetStereoProjectionMatrix((Camera.StereoscopicEye)xr.multipassId, xr.GetProjMatrix());
			camera.SetStereoViewMatrix((Camera.StereoscopicEye)xr.multipassId, xr.GetViewMatrix());
		}
	}

	private static PerObjectData GetPerObjectLightFlags(UniversalLightData universalLightData, UniversalRenderPipelineAsset settings, RenderingMode? renderingMode)
	{
		using (new ProfilingScope(Profiling.Pipeline.getPerObjectLightFlags))
		{
			bool flag = settings.ShouldUseReflectionProbeBlending();
			bool flag2 = false;
			if (renderingMode.HasValue)
			{
				flag2 = renderingMode.Value == RenderingMode.ForwardPlus;
			}
			PerObjectData perObjectData = PerObjectData.LightProbe | PerObjectData.Lightmaps | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask;
			if (!flag2)
			{
				perObjectData |= PerObjectData.ReflectionProbes | PerObjectData.LightData;
			}
			else if (!flag)
			{
				perObjectData |= PerObjectData.ReflectionProbes;
			}
			if (universalLightData.additionalLightsCount > 0 && !flag2 && !RenderingUtils.useStructuredBuffer)
			{
				perObjectData |= PerObjectData.LightIndices;
			}
			return perObjectData;
		}
	}

	private static int GetBrightestDirectionalLightIndex(UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights)
	{
		Light sun = RenderSettings.sun;
		int result = -1;
		float num = 0f;
		int length = visibleLights.Length;
		for (int i = 0; i < length; i++)
		{
			ref VisibleLight reference = ref visibleLights.UnsafeElementAtMutable(i);
			Light light = reference.light;
			if (light == null)
			{
				break;
			}
			if (reference.lightType == LightType.Directional)
			{
				if (light == sun)
				{
					return i;
				}
				if (light.intensity > num)
				{
					num = light.intensity;
					result = i;
				}
			}
		}
		return result;
	}

	private static int GetMainLightIndex(UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights)
	{
		using (new ProfilingScope(Profiling.Pipeline.getMainLightIndex))
		{
			if (visibleLights.Length == 0 || settings.mainLightRenderingMode != LightRenderingMode.PerPixel)
			{
				return -1;
			}
			return GetBrightestDirectionalLightIndex(settings, visibleLights);
		}
	}

	private void SetupPerFrameShaderConstants()
	{
		using (new ProfilingScope(Profiling.Pipeline.setupPerFrameShaderConstants))
		{
			Shader.SetGlobalColor(ShaderPropertyId.rendererColor, Color.white);
			Texture2D texture2D = null;
			switch (asset.lodCrossFadeDitheringType)
			{
			case LODCrossFadeDitheringType.BayerMatrix:
				texture2D = runtimeTextures.bayerMatrixTex;
				break;
			case LODCrossFadeDitheringType.BlueNoise:
				texture2D = runtimeTextures.blueNoise64LTex;
				break;
			case LODCrossFadeDitheringType.Stencil:
				texture2D = runtimeTextures.stencilDitherTex;
				break;
			default:
				Debug.LogWarning($"This Lod Cross Fade Dithering Type is not supported: {asset.lodCrossFadeDitheringType}");
				break;
			}
			if (texture2D != null)
			{
				Shader.SetGlobalFloat(ShaderPropertyId.ditheringTextureInvSize, 1f / (float)texture2D.width);
				Shader.SetGlobalTexture(ShaderPropertyId.ditheringTexture, texture2D);
			}
		}
	}

	private static void SetupPerCameraShaderConstants(CommandBuffer cmd)
	{
		using (new ProfilingScope(Profiling.Pipeline.setupPerCameraShaderConstants))
		{
			SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
			Color color = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * RenderSettings.reflectionIntensity);
			cmd.SetGlobalVector(ShaderPropertyId.glossyEnvironmentColor, color);
			cmd.SetGlobalTexture(ShaderPropertyId.glossyEnvironmentCubeMap, ReflectionProbe.defaultTexture);
			cmd.SetGlobalVector(ShaderPropertyId.glossyEnvironmentCubeMapHDR, ReflectionProbe.defaultTextureHDRDecodeValues);
			cmd.SetGlobalVector(ShaderPropertyId.ambientSkyColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientSkyColor));
			cmd.SetGlobalVector(ShaderPropertyId.ambientEquatorColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientEquatorColor));
			cmd.SetGlobalVector(ShaderPropertyId.ambientGroundColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientGroundColor));
			cmd.SetGlobalVector(ShaderPropertyId.subtractiveShadowColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.subtractiveShadowColor));
		}
	}

	private static void CheckAndApplyDebugSettings(ref RenderingData renderingData)
	{
		UniversalRenderPipelineDebugDisplaySettings instance = DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance;
		ref CameraData cameraData = ref renderingData.cameraData;
		if (instance.AreAnySettingsActive && !cameraData.isPreviewCamera)
		{
			DebugDisplaySettingsRendering renderingSettings = instance.renderingSettings;
			int msaaSamples = cameraData.cameraTargetDescriptor.msaaSamples;
			if (!renderingSettings.enableMsaa)
			{
				msaaSamples = 1;
			}
			if (!renderingSettings.enableHDR)
			{
				cameraData.isHdrEnabled = false;
			}
			if (!instance.IsPostProcessingAllowed)
			{
				cameraData.postProcessEnabled = false;
			}
			cameraData.hdrColorBufferPrecision = (asset ? asset.hdrColorBufferPrecision : HDRColorBufferPrecision._32Bits);
			cameraData.cameraTargetDescriptor.graphicsFormat = MakeRenderTextureGraphicsFormat(cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, needsAlpha: true);
			cameraData.cameraTargetDescriptor.msaaSamples = msaaSamples;
		}
	}

	private static ImageUpscalingFilter ResolveUpscalingFilterSelection(Vector2 imageSize, float renderScale, UpscalingFilterSelection selection, bool enableRenderGraph)
	{
		ImageUpscalingFilter result = ImageUpscalingFilter.Linear;
		if ((selection == UpscalingFilterSelection.FSR && !FSRUtils.IsSupported()) || (selection == UpscalingFilterSelection.STP && (!STP.IsSupported() || !enableRenderGraph)))
		{
			selection = UpscalingFilterSelection.Auto;
		}
		switch (selection)
		{
		case UpscalingFilterSelection.Auto:
		{
			float num = 1f / renderScale;
			if (Mathf.Approximately(num - Mathf.Floor(num), 0f))
			{
				float num2 = imageSize.x / num;
				float num3 = imageSize.y / num;
				if (Mathf.Approximately(num2 - Mathf.Floor(num2), 0f) && Mathf.Approximately(num3 - Mathf.Floor(num3), 0f))
				{
					result = ImageUpscalingFilter.Point;
				}
			}
			break;
		}
		case UpscalingFilterSelection.Point:
			result = ImageUpscalingFilter.Point;
			break;
		case UpscalingFilterSelection.FSR:
			result = ImageUpscalingFilter.FSR;
			break;
		case UpscalingFilterSelection.STP:
			result = ImageUpscalingFilter.STP;
			break;
		}
		return result;
	}

	internal static bool HDROutputForMainDisplayIsActive()
	{
		bool num = SystemInfo.hdrDisplaySupportFlags.HasFlag(HDRDisplaySupportFlags.Supported) && asset.supportsHDR;
		bool flag = HDROutputSettings.main.available && HDROutputSettings.main.active;
		return num && flag;
	}

	internal static bool HDROutputForAnyDisplayIsActive()
	{
		bool flag = HDROutputForMainDisplayIsActive();
		if (XRSystem.displayActive)
		{
			flag |= XRSystem.isHDRDisplayOutputActive;
		}
		return flag;
	}

	private void SetHDRState(List<Camera> cameras)
	{
		bool flag = HDROutputSettings.main.available && HDROutputSettings.main.active;
		if (SystemInfo.hdrDisplaySupportFlags.HasFlag(HDRDisplaySupportFlags.RuntimeSwitchable) && !asset.supportsHDR && flag)
		{
			HDROutputSettings.main.RequestHDRModeChange(enabled: false);
		}
		if (flag)
		{
			HDROutputSettings.main.automaticHDRTonemapping = false;
		}
	}

	internal static void GetHDROutputLuminanceParameters(HDROutputUtils.HDRDisplayInformation hdrDisplayInformation, ColorGamut hdrDisplayColorGamut, Tonemapping tonemapping, out Vector4 hdrOutputParameters)
	{
		float x = hdrDisplayInformation.minToneMapLuminance;
		float y = hdrDisplayInformation.maxToneMapLuminance;
		float num = hdrDisplayInformation.paperWhiteNits;
		if (!tonemapping.detectPaperWhite.value)
		{
			num = tonemapping.paperWhite.value;
		}
		if (!tonemapping.detectBrightnessLimits.value)
		{
			x = tonemapping.minNits.value;
			y = tonemapping.maxNits.value;
		}
		hdrOutputParameters = new Vector4(x, y, num, 1f / num);
	}

	internal static void GetHDROutputGradingParameters(Tonemapping tonemapping, out Vector4 hdrOutputParameters)
	{
		int num = 0;
		float y = 0f;
		switch (tonemapping.mode.value)
		{
		case TonemappingMode.Neutral:
			num = (int)tonemapping.neutralHDRRangeReductionMode.value;
			y = tonemapping.hueShiftAmount.value;
			break;
		case TonemappingMode.ACES:
			num = (int)tonemapping.acesPreset.value;
			break;
		}
		hdrOutputParameters = new Vector4(num, y, 0f, 0f);
	}

	private static AdditionalLightsShadowAtlasLayout BuildAdditionalLightsShadowAtlasLayout(UniversalLightData lightData, UniversalShadowData shadowData, UniversalCameraData cameraData)
	{
		using (new ProfilingScope(Profiling.Pipeline.buildAdditionalLightsShadowAtlasLayout))
		{
			return new AdditionalLightsShadowAtlasLayout(lightData, shadowData, cameraData);
		}
	}

	private static void AdjustUIOverlayOwnership(int cameraCount)
	{
		if (XRSystem.displayActive || cameraCount == 0)
		{
			SupportedRenderingFeatures.active.rendersUIOverlay = false;
		}
		else
		{
			SupportedRenderingFeatures.active.rendersUIOverlay = true;
		}
	}

	private static void SetupScreenMSAASamplesState(int cameraCount)
	{
		canOptimizeScreenMSAASamples = cameraCount == 1;
		startFrameScreenMSAASamples = Screen.msaaSamples;
	}

	public static bool IsGameCamera(Camera camera)
	{
		if (camera == null)
		{
			throw new ArgumentNullException("camera");
		}
		if (camera.cameraType != CameraType.Game)
		{
			return camera.cameraType == CameraType.VR;
		}
		return true;
	}

	private void SortCameras(List<Camera> cameras)
	{
		if (cameras.Count > 1)
		{
			cameras.Sort(cameraComparison);
		}
	}

	private int GetLastBaseCameraIndex(List<Camera> cameras)
	{
		int result = 0;
		for (int i = 0; i < cameras.Count; i++)
		{
			cameras[i].TryGetComponent<UniversalAdditionalCameraData>(out var component);
			if ((object)component != null && component.renderType == CameraRenderType.Base)
			{
				result = i;
			}
		}
		return result;
	}

	internal static GraphicsFormat MakeRenderTextureGraphicsFormat(bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, bool needsAlpha)
	{
		if (isHdrEnabled)
		{
			if (!needsAlpha && requestHDRColorBufferPrecision != HDRColorBufferPrecision._64Bits && SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormatUsage.Blend))
			{
				return GraphicsFormat.B10G11R11_UFloatPack32;
			}
			if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, GraphicsFormatUsage.Blend))
			{
				return GraphicsFormat.R16G16B16A16_SFloat;
			}
			return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
		}
		return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
	}

	internal static GraphicsFormat MakeUnormRenderTextureGraphicsFormat()
	{
		if (SystemInfo.IsFormatSupported(GraphicsFormat.A2B10G10R10_UNormPack32, GraphicsFormatUsage.Blend))
		{
			return GraphicsFormat.A2B10G10R10_UNormPack32;
		}
		return GraphicsFormat.R8G8B8A8_UNorm;
	}

	internal static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, UniversalCameraData cameraData, bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, int msaaSamples, bool needsAlpha, bool requiresOpaqueTexture)
	{
		RenderTextureDescriptor renderTextureDescriptor;
		if (camera.targetTexture == null)
		{
			renderTextureDescriptor = new RenderTextureDescriptor(cameraData.scaledWidth, cameraData.scaledHeight);
			renderTextureDescriptor.graphicsFormat = MakeRenderTextureGraphicsFormat(isHdrEnabled, requestHDRColorBufferPrecision, needsAlpha);
			renderTextureDescriptor.depthBufferBits = (int)CoreUtils.GetDefaultDepthBufferBits();
			renderTextureDescriptor.depthStencilFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
			renderTextureDescriptor.msaaSamples = msaaSamples;
			renderTextureDescriptor.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
		}
		else
		{
			renderTextureDescriptor = camera.targetTexture.descriptor;
			renderTextureDescriptor.msaaSamples = msaaSamples;
			renderTextureDescriptor.width = cameraData.scaledWidth;
			renderTextureDescriptor.height = cameraData.scaledHeight;
			if (camera.cameraType == CameraType.SceneView && !isHdrEnabled)
			{
				renderTextureDescriptor.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
			}
		}
		renderTextureDescriptor.enableRandomWrite = false;
		renderTextureDescriptor.bindMS = false;
		renderTextureDescriptor.useDynamicScale = camera.allowDynamicResolution;
		renderTextureDescriptor.msaaSamples = SystemInfo.GetRenderTextureSupportedMSAASampleCount(renderTextureDescriptor);
		if (!SystemInfo.supportsStoreAndResolveAction)
		{
			renderTextureDescriptor.msaaSamples = 1;
		}
		return renderTextureDescriptor;
	}

	public static void GetLightAttenuationAndSpotDirection(LightType lightType, float lightRange, Matrix4x4 lightLocalToWorldMatrix, float spotAngle, float? innerSpotAngle, out Vector4 lightAttenuation, out Vector4 lightSpotDir)
	{
		lightAttenuation = k_DefaultLightAttenuation;
		lightSpotDir = k_DefaultLightSpotDirection;
		if (lightType != LightType.Directional)
		{
			GetPunctualLightDistanceAttenuation(lightRange, ref lightAttenuation);
			if (lightType == LightType.Spot)
			{
				GetSpotDirection(ref lightLocalToWorldMatrix, out lightSpotDir);
				GetSpotAngleAttenuation(spotAngle, innerSpotAngle, ref lightAttenuation);
			}
		}
	}

	internal static void GetPunctualLightDistanceAttenuation(float lightRange, ref Vector4 lightAttenuation)
	{
		float num = lightRange * lightRange;
		float num2 = 0.64000005f * num - num;
		float y = (0f - num) / num2;
		float x = 1f / Mathf.Max(0.0001f, num);
		lightAttenuation.x = x;
		lightAttenuation.y = y;
	}

	internal static void GetSpotAngleAttenuation(float spotAngle, float? innerSpotAngle, ref Vector4 lightAttenuation)
	{
		float num = Mathf.Cos(MathF.PI / 180f * spotAngle * 0.5f);
		float num2 = ((!innerSpotAngle.HasValue) ? Mathf.Cos(2f * Mathf.Atan(Mathf.Tan(spotAngle * 0.5f * (MathF.PI / 180f)) * 46f / 64f) * 0.5f) : Mathf.Cos(innerSpotAngle.Value * (MathF.PI / 180f) * 0.5f));
		float num3 = Mathf.Max(0.001f, num2 - num);
		float num4 = 1f / num3;
		float w = (0f - num) * num4;
		lightAttenuation.z = num4;
		lightAttenuation.w = w;
	}

	internal static void GetSpotDirection(ref Matrix4x4 lightLocalToWorldMatrix, out Vector4 lightSpotDir)
	{
		Vector4 column = lightLocalToWorldMatrix.GetColumn(2);
		lightSpotDir = new Vector4(0f - column.x, 0f - column.y, 0f - column.z, 0f);
	}

	public static void InitializeLightConstants_Common(NativeArray<VisibleLight> lights, int lightIndex, out Vector4 lightPos, out Vector4 lightColor, out Vector4 lightAttenuation, out Vector4 lightSpotDir, out Vector4 lightOcclusionProbeChannel)
	{
		lightPos = k_DefaultLightPosition;
		lightColor = k_DefaultLightColor;
		lightOcclusionProbeChannel = k_DefaultLightsProbeChannel;
		lightAttenuation = k_DefaultLightAttenuation;
		lightSpotDir = k_DefaultLightSpotDirection;
		if (lightIndex < 0)
		{
			return;
		}
		ref VisibleLight reference = ref lights.UnsafeElementAtMutable(lightIndex);
		Light light = reference.light;
		Matrix4x4 lightLocalToWorldMatrix = reference.localToWorldMatrix;
		LightType lightType = reference.lightType;
		if (lightType == LightType.Directional)
		{
			Vector4 vector = -lightLocalToWorldMatrix.GetColumn(2);
			lightPos = new Vector4(vector.x, vector.y, vector.z, 0f);
		}
		else
		{
			Vector4 column = lightLocalToWorldMatrix.GetColumn(3);
			lightPos = new Vector4(column.x, column.y, column.z, 1f);
			GetPunctualLightDistanceAttenuation(reference.range, ref lightAttenuation);
			if (lightType == LightType.Spot)
			{
				GetSpotAngleAttenuation(reference.spotAngle, light?.innerSpotAngle, ref lightAttenuation);
				GetSpotDirection(ref lightLocalToWorldMatrix, out lightSpotDir);
			}
		}
		lightColor = reference.finalColor;
		if (light != null && light.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed && 0 <= light.bakingOutput.occlusionMaskChannel && light.bakingOutput.occlusionMaskChannel < 4)
		{
			lightOcclusionProbeChannel[light.bakingOutput.occlusionMaskChannel] = 1f;
		}
	}

	private static void RecordAndExecuteRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context, ScriptableRenderer renderer, CommandBuffer cmd, Camera camera, string cameraName)
	{
		RenderGraphParameters renderGraphParameters = default(RenderGraphParameters);
		renderGraphParameters.executionName = cameraName;
		renderGraphParameters.commandBuffer = cmd;
		renderGraphParameters.scriptableRenderContext = context;
		renderGraphParameters.currentFrameIndex = Time.frameCount;
		RenderGraphParameters parameters = renderGraphParameters;
		try
		{
			renderGraph.BeginRecording(in parameters);
			renderer.RecordRenderGraph(renderGraph, context);
		}
		catch (Exception e)
		{
			if (renderGraph.ResetGraphAndLogException(e))
			{
				throw;
			}
			return;
		}
		renderGraph.EndRecordingAndExecute();
	}
}
