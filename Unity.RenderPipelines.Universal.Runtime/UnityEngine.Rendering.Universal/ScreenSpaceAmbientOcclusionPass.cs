using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class ScreenSpaceAmbientOcclusionPass : ScriptableRenderPass
{
	private enum BlurTypes
	{
		Bilateral,
		Gaussian,
		Kawase
	}

	private enum ShaderPasses
	{
		AmbientOcclusion,
		BilateralBlurHorizontal,
		BilateralBlurVertical,
		BilateralBlurFinal,
		BilateralAfterOpaque,
		GaussianBlurHorizontal,
		GaussianBlurVertical,
		GaussianAfterOpaque,
		KawaseBlur,
		KawaseAfterOpaque
	}

	private struct SSAOMaterialParams
	{
		internal bool orthographicCamera;

		internal bool aoBlueNoise;

		internal bool aoInterleavedGradient;

		internal bool sampleCountHigh;

		internal bool sampleCountMedium;

		internal bool sampleCountLow;

		internal bool sourceDepthNormals;

		internal bool sourceDepthHigh;

		internal bool sourceDepthMedium;

		internal bool sourceDepthLow;

		internal Vector4 ssaoParams;

		internal SSAOMaterialParams(ref ScreenSpaceAmbientOcclusionSettings settings, bool isOrthographic)
		{
			bool flag = settings.Source == ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals;
			float num = ((settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.BlueNoise) ? 1.5f : 1f);
			orthographicCamera = isOrthographic;
			aoBlueNoise = settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.BlueNoise;
			aoInterleavedGradient = settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.InterleavedGradient;
			sampleCountHigh = settings.Samples == ScreenSpaceAmbientOcclusionSettings.AOSampleOption.High;
			sampleCountMedium = settings.Samples == ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Medium;
			sampleCountLow = settings.Samples == ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Low;
			sourceDepthNormals = settings.Source == ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals;
			sourceDepthHigh = !flag && settings.NormalSamples == ScreenSpaceAmbientOcclusionSettings.NormalQuality.High;
			sourceDepthMedium = !flag && settings.NormalSamples == ScreenSpaceAmbientOcclusionSettings.NormalQuality.Medium;
			sourceDepthLow = !flag && settings.NormalSamples == ScreenSpaceAmbientOcclusionSettings.NormalQuality.Low;
			ssaoParams = new Vector4(settings.Intensity, settings.Radius * num, 1f / (float)((!settings.Downsample) ? 1 : 2), settings.Falloff);
		}

		internal bool Equals(ref SSAOMaterialParams other)
		{
			if (orthographicCamera == other.orthographicCamera && aoBlueNoise == other.aoBlueNoise && aoInterleavedGradient == other.aoInterleavedGradient && sampleCountHigh == other.sampleCountHigh && sampleCountMedium == other.sampleCountMedium && sampleCountLow == other.sampleCountLow && sourceDepthNormals == other.sourceDepthNormals && sourceDepthHigh == other.sourceDepthHigh && sourceDepthMedium == other.sourceDepthMedium && sourceDepthLow == other.sourceDepthLow)
			{
				return ssaoParams == other.ssaoParams;
			}
			return false;
		}
	}

	private class SSAOPassData
	{
		internal bool afterOpaque;

		internal ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions BlurQuality;

		internal Material material;

		internal float directLightingStrength;

		internal TextureHandle cameraColor;

		internal TextureHandle AOTexture;

		internal TextureHandle finalTexture;

		internal TextureHandle blurTexture;

		internal TextureHandle cameraNormalsTexture;
	}

	private readonly bool m_SupportsR8RenderTextureFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8);

	private int m_BlueNoiseTextureIndex;

	private Material m_Material;

	private SSAOPassData m_PassData;

	private Texture2D[] m_BlueNoiseTextures;

	private Vector4[] m_CameraTopLeftCorner = new Vector4[2];

	private Vector4[] m_CameraXExtent = new Vector4[2];

	private Vector4[] m_CameraYExtent = new Vector4[2];

	private Vector4[] m_CameraZExtent = new Vector4[2];

	private RTHandle[] m_SSAOTextures = new RTHandle[4];

	private BlurTypes m_BlurType;

	private Matrix4x4[] m_CameraViewProjections = new Matrix4x4[2];

	private ProfilingSampler m_ProfilingSampler = ProfilingSampler.Get(URPProfileId.SSAO);

	private ScriptableRenderer m_Renderer;

	private RenderTextureDescriptor m_AOPassDescriptor;

	private ScreenSpaceAmbientOcclusionSettings m_CurrentSettings;

	private const string k_SSAOTextureName = "_ScreenSpaceOcclusionTexture";

	private const string k_AmbientOcclusionParamName = "_AmbientOcclusionParam";

	internal static readonly int s_AmbientOcclusionParamID = Shader.PropertyToID("_AmbientOcclusionParam");

	private static readonly int s_SSAOParamsID = Shader.PropertyToID("_SSAOParams");

	private static readonly int s_SSAOBlueNoiseParamsID = Shader.PropertyToID("_SSAOBlueNoiseParams");

	private static readonly int s_BlueNoiseTextureID = Shader.PropertyToID("_BlueNoiseTexture");

	private static readonly int s_SSAOFinalTextureID = Shader.PropertyToID("_ScreenSpaceOcclusionTexture");

	private static readonly int s_CameraViewXExtentID = Shader.PropertyToID("_CameraViewXExtent");

	private static readonly int s_CameraViewYExtentID = Shader.PropertyToID("_CameraViewYExtent");

	private static readonly int s_CameraViewZExtentID = Shader.PropertyToID("_CameraViewZExtent");

	private static readonly int s_ProjectionParams2ID = Shader.PropertyToID("_ProjectionParams2");

	private static readonly int s_CameraViewProjectionsID = Shader.PropertyToID("_CameraViewProjections");

	private static readonly int s_CameraViewTopLeftCornerID = Shader.PropertyToID("_CameraViewTopLeftCorner");

	private static readonly int s_CameraDepthTextureID = Shader.PropertyToID("_CameraDepthTexture");

	private static readonly int s_CameraNormalsTextureID = Shader.PropertyToID("_CameraNormalsTexture");

	private static readonly int[] m_BilateralTexturesIndices = new int[4] { 0, 1, 2, 3 };

	private static readonly ShaderPasses[] m_BilateralPasses = new ShaderPasses[3]
	{
		ShaderPasses.BilateralBlurHorizontal,
		ShaderPasses.BilateralBlurVertical,
		ShaderPasses.BilateralBlurFinal
	};

	private static readonly ShaderPasses[] m_BilateralAfterOpaquePasses = new ShaderPasses[3]
	{
		ShaderPasses.BilateralBlurHorizontal,
		ShaderPasses.BilateralBlurVertical,
		ShaderPasses.BilateralAfterOpaque
	};

	private static readonly int[] m_GaussianTexturesIndices = new int[4] { 0, 1, 3, 3 };

	private static readonly ShaderPasses[] m_GaussianPasses = new ShaderPasses[2]
	{
		ShaderPasses.GaussianBlurHorizontal,
		ShaderPasses.GaussianBlurVertical
	};

	private static readonly ShaderPasses[] m_GaussianAfterOpaquePasses = new ShaderPasses[2]
	{
		ShaderPasses.GaussianBlurHorizontal,
		ShaderPasses.GaussianAfterOpaque
	};

	private static readonly int[] m_KawaseTexturesIndices = new int[2] { 0, 3 };

	private static readonly ShaderPasses[] m_KawasePasses = new ShaderPasses[1] { ShaderPasses.KawaseBlur };

	private static readonly ShaderPasses[] m_KawaseAfterOpaquePasses = new ShaderPasses[1] { ShaderPasses.KawaseAfterOpaque };

	private SSAOMaterialParams m_SSAOParamsPrev;

	internal ScreenSpaceAmbientOcclusionPass()
	{
		m_CurrentSettings = new ScreenSpaceAmbientOcclusionSettings();
		m_PassData = new SSAOPassData();
	}

	internal bool Setup(ref ScreenSpaceAmbientOcclusionSettings featureSettings, ref ScriptableRenderer renderer, ref Material material, ref Texture2D[] blueNoiseTextures)
	{
		m_BlueNoiseTextures = blueNoiseTextures;
		m_Material = material;
		m_Renderer = renderer;
		m_CurrentSettings = featureSettings;
		if (renderer is UniversalRenderer { usesDeferredLighting: not false })
		{
			base.renderPassEvent = (m_CurrentSettings.AfterOpaque ? RenderPassEvent.AfterRenderingOpaques : RenderPassEvent.AfterRenderingGbuffer);
			m_CurrentSettings.Source = ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals;
		}
		else
		{
			base.renderPassEvent = (m_CurrentSettings.AfterOpaque ? RenderPassEvent.BeforeRenderingTransparents : ((RenderPassEvent)201));
		}
		switch (m_CurrentSettings.Source)
		{
		case ScreenSpaceAmbientOcclusionSettings.DepthSource.Depth:
			ConfigureInput(ScriptableRenderPassInput.Depth);
			break;
		case ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals:
			ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		switch (m_CurrentSettings.BlurQuality)
		{
		case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.High:
			m_BlurType = BlurTypes.Bilateral;
			break;
		case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Medium:
			m_BlurType = BlurTypes.Gaussian;
			break;
		case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low:
			m_BlurType = BlurTypes.Kawase;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (m_Material != null && m_CurrentSettings.Intensity > 0f && m_CurrentSettings.Radius > 0f)
		{
			return m_CurrentSettings.Falloff > 0f;
		}
		return false;
	}

	private static bool IsAfterOpaquePass(ref ShaderPasses pass)
	{
		if (pass != ShaderPasses.BilateralAfterOpaque && pass != ShaderPasses.GaussianAfterOpaque)
		{
			return pass == ShaderPasses.KawaseAfterOpaque;
		}
		return true;
	}

	private void SetupKeywordsAndParameters(ref ScreenSpaceAmbientOcclusionSettings settings, ref UniversalCameraData cameraData)
	{
		int num = ((!cameraData.xr.enabled || !cameraData.xr.singlePassEnabled) ? 1 : 2);
		for (int i = 0; i < num; i++)
		{
			Matrix4x4 viewMatrix = cameraData.GetViewMatrix(i);
			Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(i);
			m_CameraViewProjections[i] = projectionMatrix * viewMatrix;
			Matrix4x4 matrix4x = viewMatrix;
			matrix4x.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
			Matrix4x4 inverse = (projectionMatrix * matrix4x).inverse;
			Vector4 vector = inverse.MultiplyPoint(new Vector4(-1f, 1f, -1f, 1f));
			Vector4 vector2 = inverse.MultiplyPoint(new Vector4(1f, 1f, -1f, 1f));
			Vector4 vector3 = inverse.MultiplyPoint(new Vector4(-1f, -1f, -1f, 1f));
			Vector4 vector4 = inverse.MultiplyPoint(new Vector4(0f, 0f, 1f, 1f));
			m_CameraTopLeftCorner[i] = vector;
			m_CameraXExtent[i] = vector2 - vector;
			m_CameraYExtent[i] = vector3 - vector;
			m_CameraZExtent[i] = vector4;
		}
		m_Material.SetVector(s_ProjectionParams2ID, new Vector4(1f / cameraData.camera.nearClipPlane, 0f, 0f, 0f));
		m_Material.SetMatrixArray(s_CameraViewProjectionsID, m_CameraViewProjections);
		m_Material.SetVectorArray(s_CameraViewTopLeftCornerID, m_CameraTopLeftCorner);
		m_Material.SetVectorArray(s_CameraViewXExtentID, m_CameraXExtent);
		m_Material.SetVectorArray(s_CameraViewYExtentID, m_CameraYExtent);
		m_Material.SetVectorArray(s_CameraViewZExtentID, m_CameraZExtent);
		if (settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.BlueNoise)
		{
			m_BlueNoiseTextureIndex = (m_BlueNoiseTextureIndex + 1) % m_BlueNoiseTextures.Length;
			Texture2D value = m_BlueNoiseTextures[m_BlueNoiseTextureIndex];
			Vector4 value2 = new Vector4((float)cameraData.pixelWidth / (float)m_BlueNoiseTextures[m_BlueNoiseTextureIndex].width, (float)cameraData.pixelHeight / (float)m_BlueNoiseTextures[m_BlueNoiseTextureIndex].height, Random.value, Random.value);
			m_Material.SetTexture(s_BlueNoiseTextureID, value);
			m_Material.SetVector(s_SSAOBlueNoiseParamsID, value2);
		}
		SSAOMaterialParams other = new SSAOMaterialParams(ref settings, cameraData.camera.orthographic);
		bool num2 = !m_SSAOParamsPrev.Equals(ref other);
		bool flag = m_Material.HasProperty(s_SSAOParamsID);
		if (!(!num2 && flag))
		{
			m_SSAOParamsPrev = other;
			CoreUtils.SetKeyword(m_Material, "_ORTHOGRAPHIC", other.orthographicCamera);
			CoreUtils.SetKeyword(m_Material, "_BLUE_NOISE", other.aoBlueNoise);
			CoreUtils.SetKeyword(m_Material, "_INTERLEAVED_GRADIENT", other.aoInterleavedGradient);
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_HIGH", other.sampleCountHigh);
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_MEDIUM", other.sampleCountMedium);
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_LOW", other.sampleCountLow);
			CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_NORMALS", other.sourceDepthNormals);
			CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_HIGH", other.sourceDepthHigh);
			CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_MEDIUM", other.sourceDepthMedium);
			CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_LOW", other.sourceDepthLow);
			m_Material.SetVector(s_SSAOParamsID, other.ssaoParams);
		}
	}

	private void InitSSAOPassData(ref SSAOPassData data)
	{
		data.material = m_Material;
		data.BlurQuality = m_CurrentSettings.BlurQuality;
		data.afterOpaque = m_CurrentSettings.AfterOpaque;
		data.directLightingStrength = m_CurrentSettings.DirectLightingStrength;
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		CreateRenderTextureHandles(renderGraph, universalResourceData, cameraData, out var aoTexture, out var blurTexture, out var finalTexture);
		TextureHandle cameraDepthTexture = universalResourceData.cameraDepthTexture;
		TextureHandle cameraNormalsTexture = universalResourceData.cameraNormalsTexture;
		SetupKeywordsAndParameters(ref m_CurrentSettings, ref cameraData);
		SSAOPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<SSAOPassData>("Blit SSAO", out passData, m_ProfilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\ScreenSpaceAmbientOcclusionPass.cs", 335);
		unsafeRenderGraphBuilder.AllowGlobalStateModification(value: true);
		InitSSAOPassData(ref passData);
		passData.cameraColor = universalResourceData.cameraColor;
		passData.AOTexture = aoTexture;
		passData.finalTexture = finalTexture;
		passData.blurTexture = blurTexture;
		unsafeRenderGraphBuilder.UseTexture(in passData.AOTexture, AccessFlags.ReadWrite);
		if (universalResourceData.cameraColor.IsValid())
		{
			TextureHandle cameraColor = universalResourceData.cameraColor;
			unsafeRenderGraphBuilder.UseTexture(in cameraColor);
		}
		if (passData.BlurQuality != ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low)
		{
			unsafeRenderGraphBuilder.UseTexture(in passData.blurTexture, AccessFlags.ReadWrite);
		}
		if (cameraDepthTexture.IsValid())
		{
			unsafeRenderGraphBuilder.UseTexture(in cameraDepthTexture);
		}
		if (m_CurrentSettings.Source == ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals && cameraNormalsTexture.IsValid())
		{
			unsafeRenderGraphBuilder.UseTexture(in cameraNormalsTexture);
			passData.cameraNormalsTexture = cameraNormalsTexture;
		}
		if (!passData.afterOpaque && finalTexture.IsValid())
		{
			unsafeRenderGraphBuilder.UseTexture(in passData.finalTexture, AccessFlags.ReadWrite);
			unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in finalTexture, s_SSAOFinalTextureID);
		}
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(SSAOPassData data, UnsafeGraphContext rgContext)
		{
			CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);
			RenderBufferLoadAction loadAction = ((!data.afterOpaque) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
			if (data.cameraColor.IsValid())
			{
				PostProcessUtils.SetSourceSize(nativeCommandBuffer, data.cameraColor);
			}
			if (data.cameraNormalsTexture.IsValid())
			{
				data.material.SetTexture(s_CameraNormalsTextureID, data.cameraNormalsTexture);
			}
			Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.AOTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, data.material, 0);
			switch (data.BlurQuality)
			{
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.High:
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.blurTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, data.material, 1);
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.blurTexture, data.AOTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, data.material, 2);
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.finalTexture, loadAction, RenderBufferStoreAction.Store, data.material, data.afterOpaque ? 4 : 3);
				break;
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Medium:
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.blurTexture, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, data.material, 5);
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.blurTexture, data.finalTexture, loadAction, RenderBufferStoreAction.Store, data.material, data.afterOpaque ? 7 : 6);
				break;
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low:
				Blitter.BlitCameraTexture(nativeCommandBuffer, data.AOTexture, data.finalTexture, loadAction, RenderBufferStoreAction.Store, data.material, data.afterOpaque ? 9 : 8);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (!data.afterOpaque)
			{
				rgContext.cmd.SetKeyword(in ShaderGlobalKeywords.ScreenSpaceOcclusion, value: true);
				rgContext.cmd.SetGlobalVector(s_AmbientOcclusionParamID, new Vector4(1f, 0f, 0f, data.directLightingStrength));
			}
		});
	}

	private void CreateRenderTextureHandles(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, out TextureHandle aoTexture, out TextureHandle blurTexture, out TextureHandle finalTexture)
	{
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.colorFormat = (m_SupportsR8RenderTextureFormat ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
		cameraTargetDescriptor.depthStencilFormat = GraphicsFormat.None;
		cameraTargetDescriptor.msaaSamples = 1;
		int num = ((!m_CurrentSettings.Downsample) ? 1 : 2);
		bool flag = m_SupportsR8RenderTextureFormat && m_BlurType > BlurTypes.Bilateral;
		RenderTextureDescriptor desc = cameraTargetDescriptor;
		desc.colorFormat = (flag ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
		desc.width /= num;
		desc.height /= num;
		aoTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSAO_OcclusionTexture0", clear: false, FilterMode.Bilinear);
		finalTexture = (m_CurrentSettings.AfterOpaque ? resourceData.activeColorTexture : UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_ScreenSpaceOcclusionTexture", clear: false, FilterMode.Bilinear));
		if (m_CurrentSettings.BlurQuality != ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low)
		{
			blurTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_SSAO_OcclusionTexture1", clear: false, FilterMode.Bilinear);
		}
		else
		{
			blurTexture = TextureHandle.nullHandle;
		}
		if (!m_CurrentSettings.AfterOpaque)
		{
			resourceData.ssaoTexture = finalTexture;
		}
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		UniversalCameraData cameraData = renderingData.frameData.Get<UniversalCameraData>();
		InitSSAOPassData(ref m_PassData);
		SetupKeywordsAndParameters(ref m_CurrentSettings, ref cameraData);
		int num = ((!m_CurrentSettings.Downsample) ? 1 : 2);
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.msaaSamples = 1;
		cameraTargetDescriptor.depthStencilFormat = GraphicsFormat.None;
		m_AOPassDescriptor = cameraTargetDescriptor;
		m_AOPassDescriptor.width /= num;
		m_AOPassDescriptor.height /= num;
		bool flag = m_SupportsR8RenderTextureFormat && m_BlurType > BlurTypes.Bilateral;
		m_AOPassDescriptor.colorFormat = (flag ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
		RenderingUtils.ReAllocateHandleIfNeeded(ref m_SSAOTextures[0], in m_AOPassDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_SSAO_OcclusionTexture0");
		RenderingUtils.ReAllocateHandleIfNeeded(ref m_SSAOTextures[1], in m_AOPassDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_SSAO_OcclusionTexture1");
		RenderingUtils.ReAllocateHandleIfNeeded(ref m_SSAOTextures[2], in m_AOPassDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_SSAO_OcclusionTexture2");
		m_AOPassDescriptor.width *= num;
		m_AOPassDescriptor.height *= num;
		m_AOPassDescriptor.colorFormat = (m_SupportsR8RenderTextureFormat ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
		RenderingUtils.ReAllocateHandleIfNeeded(ref m_SSAOTextures[3], in m_AOPassDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, 1, 0f, "_SSAO_OcclusionTexture");
		PostProcessUtils.SetSourceSize(cmd, m_SSAOTextures[3]);
		ConfigureTarget(m_CurrentSettings.AfterOpaque ? m_Renderer.cameraColorTargetHandle : m_SSAOTextures[3]);
		ConfigureClear(ClearFlag.None, Color.white);
	}

	[Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_Material == null)
		{
			Debug.LogErrorFormat("{0}.Execute(): Missing material. ScreenSpaceAmbientOcclusion pass will not execute. Check for missing reference in the renderer resources.", GetType().Name);
			return;
		}
		CommandBuffer cmd = renderingData.commandBuffer;
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.SSAO)))
		{
			if (!m_CurrentSettings.AfterOpaque)
			{
				cmd.SetKeyword(in ShaderGlobalKeywords.ScreenSpaceOcclusion, value: true);
			}
			cmd.SetGlobalTexture("_ScreenSpaceOcclusionTexture", m_SSAOTextures[3]);
			bool flag = false;
			if (renderingData.cameraData.xr.supportsFoveatedRendering)
			{
				if (m_CurrentSettings.Downsample || SystemInfo.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster) || (SystemInfo.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.FoveationImage) && m_CurrentSettings.Source == ScreenSpaceAmbientOcclusionSettings.DepthSource.Depth))
				{
					cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
				}
				else if (SystemInfo.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.FoveationImage))
				{
					cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Enabled);
					flag = true;
				}
			}
			GetPassOrder(m_BlurType, m_CurrentSettings.AfterOpaque, out var textureIndices, out var shaderPasses);
			RTHandle baseMap = renderingData.cameraData.renderer.cameraDepthTargetHandle;
			RenderAndSetBaseMap(ref cmd, ref renderingData, ref renderingData.cameraData.renderer, ref m_Material, ref baseMap, ref m_SSAOTextures[0], ShaderPasses.AmbientOcclusion);
			for (int i = 0; i < shaderPasses.Length; i++)
			{
				int num = textureIndices[i];
				int num2 = textureIndices[i + 1];
				RenderAndSetBaseMap(ref cmd, ref renderingData, ref renderingData.cameraData.renderer, ref m_Material, ref m_SSAOTextures[num], ref m_SSAOTextures[num2], shaderPasses[i]);
			}
			cmd.SetGlobalVector(s_AmbientOcclusionParamID, new Vector4(1f, 0f, 0f, m_CurrentSettings.DirectLightingStrength));
			if (flag)
			{
				cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
			}
		}
	}

	private static void RenderAndSetBaseMap(ref CommandBuffer cmd, ref RenderingData renderingData, ref ScriptableRenderer renderer, ref Material mat, ref RTHandle baseMap, ref RTHandle target, ShaderPasses pass)
	{
		if (IsAfterOpaquePass(ref pass))
		{
			Blitter.BlitCameraTexture(cmd, baseMap, renderer.cameraColorTargetHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, mat, (int)pass);
		}
		else if (baseMap.rt == null)
		{
			Vector2 vector = (baseMap.useScaling ? new Vector2(baseMap.rtHandleProperties.rtHandleScale.x, baseMap.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			CoreUtils.SetRenderTarget(cmd, target);
			Blitter.BlitTexture(cmd, baseMap.nameID, vector, mat, (int)pass);
		}
		else
		{
			Blitter.BlitCameraTexture(cmd, baseMap, target, mat, (int)pass);
		}
	}

	private static void GetPassOrder(BlurTypes blurType, bool isAfterOpaque, out int[] textureIndices, out ShaderPasses[] shaderPasses)
	{
		switch (blurType)
		{
		case BlurTypes.Bilateral:
			textureIndices = m_BilateralTexturesIndices;
			shaderPasses = (isAfterOpaque ? m_BilateralAfterOpaquePasses : m_BilateralPasses);
			break;
		case BlurTypes.Gaussian:
			textureIndices = m_GaussianTexturesIndices;
			shaderPasses = (isAfterOpaque ? m_GaussianAfterOpaquePasses : m_GaussianPasses);
			break;
		case BlurTypes.Kawase:
			textureIndices = m_KawaseTexturesIndices;
			shaderPasses = (isAfterOpaque ? m_KawaseAfterOpaquePasses : m_KawasePasses);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		if (!m_CurrentSettings.AfterOpaque)
		{
			cmd.SetKeyword(in ShaderGlobalKeywords.ScreenSpaceOcclusion, value: false);
		}
	}

	public void Dispose()
	{
		m_SSAOTextures[0]?.Release();
		m_SSAOTextures[1]?.Release();
		m_SSAOTextures[2]?.Release();
		m_SSAOTextures[3]?.Release();
		m_SSAOParamsPrev = default(SSAOMaterialParams);
	}
}
