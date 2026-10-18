using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace UnityEngine.Experimental.Rendering;

public static class XRSystem
{
	private static XRLayoutStack s_Layout = new XRLayoutStack();

	private static Func<XRPassCreateInfo, XRPass> s_PassAllocator = null;

	private static List<XRDisplaySubsystem> s_DisplayList = new List<XRDisplaySubsystem>();

	private static XRDisplaySubsystem s_Display;

	private static MSAASamples s_MSAASamples = MSAASamples.None;

	private static float s_OcclusionMeshScaling = 1f;

	private static bool s_UseVisibilityMesh = true;

	private static Material s_OcclusionMeshMaterial;

	private static Material s_MirrorViewMaterial;

	private static Action<XRLayout, Camera> s_LayoutOverride = null;

	public static readonly XRPass emptyPass = new XRPass();

	public static bool displayActive
	{
		get
		{
			if (s_Display == null)
			{
				return false;
			}
			return s_Display.running;
		}
	}

	public static bool isHDRDisplayOutputActive => (s_Display?.hdrOutputSettings?.active).GetValueOrDefault();

	public static bool singlePassAllowed { get; set; } = true;


	public static FoveatedRenderingCaps foveatedRenderingCaps { get; set; }

	public static bool dumpDebugInfo { get; set; } = false;


	public static XRDisplaySubsystem GetActiveDisplay()
	{
		return s_Display;
	}

	public static void Initialize(Func<XRPassCreateInfo, XRPass> passAllocator, Shader occlusionMeshPS, Shader mirrorViewPS)
	{
		if (passAllocator == null)
		{
			throw new ArgumentNullException("passCreator");
		}
		s_PassAllocator = passAllocator;
		RefreshDeviceInfo();
		foveatedRenderingCaps = SystemInfo.foveatedRenderingCaps;
		if (occlusionMeshPS != null && s_OcclusionMeshMaterial == null)
		{
			s_OcclusionMeshMaterial = CoreUtils.CreateEngineMaterial(occlusionMeshPS);
		}
		if (mirrorViewPS != null && s_MirrorViewMaterial == null)
		{
			s_MirrorViewMaterial = CoreUtils.CreateEngineMaterial(mirrorViewPS);
		}
		if (XRGraphicsAutomatedTests.enabled)
		{
			SetLayoutOverride(XRGraphicsAutomatedTests.OverrideLayout);
		}
		SinglepassKeywords.STEREO_MULTIVIEW_ON = GlobalKeyword.Create("STEREO_MULTIVIEW_ON");
		SinglepassKeywords.STEREO_INSTANCING_ON = GlobalKeyword.Create("STEREO_INSTANCING_ON");
	}

	public static void SetDisplayMSAASamples(MSAASamples msaaSamples)
	{
		if (s_MSAASamples == msaaSamples)
		{
			return;
		}
		s_MSAASamples = msaaSamples;
		SubsystemManager.GetSubsystems(s_DisplayList);
		foreach (XRDisplaySubsystem s_Display in s_DisplayList)
		{
			s_Display.SetMSAALevel((int)s_MSAASamples);
		}
	}

	public static MSAASamples GetDisplayMSAASamples()
	{
		return s_MSAASamples;
	}

	internal static void SetOcclusionMeshScale(float occlusionMeshScale)
	{
		s_OcclusionMeshScaling = occlusionMeshScale;
	}

	internal static float GetOcclusionMeshScale()
	{
		return s_OcclusionMeshScaling;
	}

	internal static void SetUseVisibilityMesh(bool useVisibilityMesh)
	{
		s_UseVisibilityMesh = useVisibilityMesh;
	}

	internal static bool GetUseVisibilityMesh()
	{
		return s_UseVisibilityMesh;
	}

	internal static void SetMirrorViewMode(int mirrorBlitMode)
	{
		if (s_Display != null)
		{
			s_Display.SetPreferredMirrorBlitMode(mirrorBlitMode);
		}
	}

	internal static int GetMirrorViewMode()
	{
		if (s_Display == null)
		{
			return -6;
		}
		return s_Display.GetPreferredMirrorBlitMode();
	}

	public static void SetRenderScale(float renderScale)
	{
		SubsystemManager.GetSubsystems(s_DisplayList);
		foreach (XRDisplaySubsystem s_Display in s_DisplayList)
		{
			s_Display.scaleOfAllRenderTargets = renderScale;
		}
	}

	public static float GetRenderViewportScale()
	{
		return s_Display.scaleOfAllViewports;
	}

	public static float GetDynamicResolutionScale()
	{
		return s_Display.globalDynamicScale;
	}

	public static int ScaleTextureWidthForXR(RenderTexture texture)
	{
		return s_Display.ScaledTextureWidth(texture);
	}

	public static int ScaleTextureHeightForXR(RenderTexture texture)
	{
		return s_Display.ScaledTextureHeight(texture);
	}

	public static XRLayout NewLayout()
	{
		RefreshDeviceInfo();
		return s_Layout.New();
	}

	public static void EndLayout()
	{
		if (dumpDebugInfo)
		{
			s_Layout.top.LogDebugInfo();
		}
		s_Layout.Release();
	}

	public static void RenderMirrorView(CommandBuffer cmd, Camera camera)
	{
		XRMirrorView.RenderMirrorView(cmd, camera, s_MirrorViewMaterial, s_Display);
	}

	public static void Dispose()
	{
		if (s_OcclusionMeshMaterial != null)
		{
			CoreUtils.Destroy(s_OcclusionMeshMaterial);
			s_OcclusionMeshMaterial = null;
		}
		if (s_MirrorViewMaterial != null)
		{
			CoreUtils.Destroy(s_MirrorViewMaterial);
			s_MirrorViewMaterial = null;
		}
	}

	internal static void SetDisplayZRange(float zNear, float zFar)
	{
		if (s_Display != null)
		{
			s_Display.zNear = zNear;
			s_Display.zFar = zFar;
		}
	}

	private static void SetLayoutOverride(Action<XRLayout, Camera> action)
	{
		s_LayoutOverride = action;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void XRSystemInit()
	{
		if (GraphicsSettings.currentRenderPipeline != null)
		{
			RefreshDeviceInfo();
		}
	}

	private static void RefreshDeviceInfo()
	{
		SubsystemManager.GetSubsystems(s_DisplayList);
		if (s_DisplayList.Count > 0)
		{
			if (s_DisplayList.Count > 1)
			{
				throw new NotImplementedException("Only one XR display is supported!");
			}
			s_Display = s_DisplayList[0];
			s_Display.disableLegacyRenderer = true;
			s_Display.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
			s_Display.textureLayout = XRDisplaySubsystem.TextureLayout.Texture2DArray;
			TextureXR.maxViews = Math.Max(TextureXR.slices, 2);
		}
		else
		{
			s_Display = null;
		}
	}

	internal static void CreateDefaultLayout(Camera camera, XRLayout layout)
	{
		if (s_Display == null)
		{
			throw new NullReferenceException("s_Display");
		}
		for (int i = 0; i < s_Display.GetRenderPassCount(); i++)
		{
			s_Display.GetRenderPass(i, out var renderPass2);
			s_Display.GetCullingParameters(camera, renderPass2.cullingPassIndex, out var scriptableCullingParameters);
			int renderParameterCount = renderPass2.GetRenderParameterCount();
			if (CanUseSinglePass(camera, renderPass2))
			{
				XRPassCreateInfo arg = BuildPass(renderPass2, scriptableCullingParameters, layout);
				XRPass xrPass2 = s_PassAllocator(arg);
				for (int j = 0; j < renderParameterCount; j++)
				{
					AddViewToPass(xrPass2, renderPass2, j);
				}
				layout.AddPass(camera, xrPass2);
			}
			else
			{
				for (int k = 0; k < renderParameterCount; k++)
				{
					XRPassCreateInfo arg2 = BuildPass(renderPass2, scriptableCullingParameters, layout);
					XRPass xrPass3 = s_PassAllocator(arg2);
					AddViewToPass(xrPass3, renderPass2, k);
					layout.AddPass(camera, xrPass3);
				}
			}
		}
		s_LayoutOverride?.Invoke(layout, camera);
		void AddViewToPass(XRPass xrPass, XRDisplaySubsystem.XRRenderPass renderPass, int renderParamIndex)
		{
			renderPass.GetRenderParameter(camera, renderParamIndex, out var renderParameter);
			xrPass.AddView(BuildView(renderPass, renderParameter));
		}
	}

	internal static void ReconfigurePass(XRPass xrPass, Camera camera)
	{
		if (xrPass.enabled && s_Display != null)
		{
			s_Display.GetRenderPass(xrPass.multipassId, out var renderPass);
			s_Display.GetCullingParameters(camera, renderPass.cullingPassIndex, out var scriptableCullingParameters);
			xrPass.AssignCullingParams(renderPass.cullingPassIndex, scriptableCullingParameters);
			for (int i = 0; i < renderPass.GetRenderParameterCount(); i++)
			{
				renderPass.GetRenderParameter(camera, i, out var renderParameter);
				xrPass.AssignView(i, BuildView(renderPass, renderParameter));
			}
			s_LayoutOverride?.Invoke(s_Layout.top, camera);
		}
	}

	private static bool CanUseSinglePass(Camera camera, XRDisplaySubsystem.XRRenderPass renderPass)
	{
		if (!singlePassAllowed)
		{
			return false;
		}
		if (renderPass.renderTargetDesc.dimension != TextureDimension.Tex2DArray)
		{
			return false;
		}
		if (renderPass.GetRenderParameterCount() != 2 || renderPass.renderTargetDesc.volumeDepth != 2)
		{
			return false;
		}
		renderPass.GetRenderParameter(camera, 0, out var renderParameter);
		renderPass.GetRenderParameter(camera, 1, out var renderParameter2);
		if (renderParameter.textureArraySlice != 0 || renderParameter2.textureArraySlice != 1)
		{
			return false;
		}
		if (renderParameter.viewport != renderParameter2.viewport)
		{
			return false;
		}
		return true;
	}

	private static XRView BuildView(XRDisplaySubsystem.XRRenderPass renderPass, XRDisplaySubsystem.XRRenderParameter renderParameter)
	{
		Rect viewport = renderParameter.viewport;
		viewport.x *= renderPass.renderTargetScaledWidth;
		viewport.width *= renderPass.renderTargetScaledWidth;
		viewport.y *= renderPass.renderTargetScaledHeight;
		viewport.height *= renderPass.renderTargetScaledHeight;
		Mesh occlusionMesh = (XRGraphicsAutomatedTests.running ? null : renderParameter.occlusionMesh);
		Mesh visibleMesh = (XRGraphicsAutomatedTests.running ? null : renderParameter.visibleMesh);
		return new XRView(renderParameter.projection, renderParameter.view, renderParameter.previousView, renderParameter.isPreviousViewValid, viewport, occlusionMesh, visibleMesh, renderParameter.textureArraySlice);
	}

	private static RenderTextureDescriptor XrRenderTextureDescToUnityRenderTextureDesc(RenderTextureDescriptor xrDesc)
	{
		RenderTextureDescriptor result = new RenderTextureDescriptor(xrDesc.width, xrDesc.height, xrDesc.graphicsFormat, xrDesc.depthStencilFormat, xrDesc.mipCount);
		result.dimension = xrDesc.dimension;
		result.msaaSamples = xrDesc.msaaSamples;
		result.volumeDepth = xrDesc.volumeDepth;
		result.vrUsage = xrDesc.vrUsage;
		result.sRGB = xrDesc.sRGB;
		result.shadowSamplingMode = xrDesc.shadowSamplingMode;
		return result;
	}

	private static XRPassCreateInfo BuildPass(XRDisplaySubsystem.XRRenderPass xrRenderPass, ScriptableCullingParameters cullingParameters, XRLayout layout)
	{
		XRPassCreateInfo result = default(XRPassCreateInfo);
		result.renderTarget = xrRenderPass.renderTarget;
		result.renderTargetDesc = XrRenderTextureDescToUnityRenderTextureDesc(xrRenderPass.renderTargetDesc);
		result.renderTargetScaledWidth = xrRenderPass.renderTargetScaledWidth;
		result.renderTargetScaledHeight = xrRenderPass.renderTargetScaledHeight;
		result.hasMotionVectorPass = xrRenderPass.hasMotionVectorPass;
		result.motionVectorRenderTarget = xrRenderPass.motionVectorRenderTarget;
		result.motionVectorRenderTargetDesc = XrRenderTextureDescToUnityRenderTextureDesc(xrRenderPass.motionVectorRenderTargetDesc);
		result.cullingParameters = cullingParameters;
		result.occlusionMeshMaterial = s_OcclusionMeshMaterial;
		result.occlusionMeshScale = GetOcclusionMeshScale();
		result.foveatedRenderingInfo = xrRenderPass.foveatedRenderingInfo;
		result.multipassId = layout.GetActivePasses().Count;
		result.cullingPassId = xrRenderPass.cullingPassIndex;
		result.copyDepth = xrRenderPass.shouldFillOutDepth;
		result.spaceWarpRightHandedNDC = xrRenderPass.spaceWarpRightHandedNDC;
		result.xrSdkRenderPass = xrRenderPass;
		return result;
	}
}
