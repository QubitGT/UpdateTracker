using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal static class StpUtils
{
	internal static TemporalAA.JitterFunc s_JitterFunc = CalculateJitter;

	private static void CalculateJitter(int frameIndex, out Vector2 jitter, out bool allowScaling)
	{
		jitter = -STP.Jit16(frameIndex);
		allowScaling = false;
	}

	private static void PopulateStpConfig(UniversalCameraData cameraData, TextureHandle inputColor, TextureHandle inputDepth, TextureHandle inputMotion, int debugViewIndex, TextureHandle debugView, TextureHandle destination, Texture2D noiseTexture, out STP.Config config)
	{
		cameraData.camera.TryGetComponent<UniversalAdditionalCameraData>(out var component);
		MotionVectorsPersistentData motionVectorsPersistentData = component.motionVectorsPersistentData;
		config.enableHwDrs = false;
		config.enableTexArray = cameraData.xr.enabled && cameraData.xr.singlePassEnabled;
		config.enableMotionScaling = true;
		config.noiseTexture = noiseTexture;
		config.inputColor = inputColor;
		config.inputDepth = inputDepth;
		config.inputMotion = inputMotion;
		config.inputStencil = TextureHandle.nullHandle;
		config.stencilMask = 0;
		config.debugView = debugView;
		config.destination = destination;
		StpHistory stpHistory = cameraData.stpHistory;
		int num = ((cameraData.xr.enabled && !cameraData.xr.singlePassEnabled) ? cameraData.xr.multipassId : 0);
		config.historyContext = stpHistory.GetHistoryContext(num);
		config.nearPlane = cameraData.camera.nearClipPlane;
		config.farPlane = cameraData.camera.farClipPlane;
		config.frameIndex = TemporalAA.CalculateTaaFrameIndex(ref cameraData.taaSettings);
		config.hasValidHistory = !cameraData.resetHistory;
		config.debugViewIndex = debugViewIndex;
		config.deltaTime = motionVectorsPersistentData.deltaTime;
		config.lastDeltaTime = motionVectorsPersistentData.lastDeltaTime;
		config.currentImageSize = new Vector2Int(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
		config.priorImageSize = config.currentImageSize;
		config.outputImageSize = new Vector2Int(cameraData.pixelWidth, cameraData.pixelHeight);
		int num2 = ((!cameraData.xr.enabled) ? 1 : cameraData.xr.viewCount);
		STP.PerViewConfig perViewConfig = default(STP.PerViewConfig);
		for (int i = 0; i < num2; i++)
		{
			int num3 = i + num;
			perViewConfig.currentProj = motionVectorsPersistentData.projectionStereo[num3];
			perViewConfig.lastProj = motionVectorsPersistentData.previousProjectionStereo[num3];
			perViewConfig.lastLastProj = motionVectorsPersistentData.previousPreviousProjectionStereo[num3];
			perViewConfig.currentView = motionVectorsPersistentData.viewStereo[num3];
			perViewConfig.lastView = motionVectorsPersistentData.previousViewStereo[num3];
			perViewConfig.lastLastView = motionVectorsPersistentData.previousPreviousViewStereo[num3];
			Vector3 worldSpaceCameraPos = motionVectorsPersistentData.worldSpaceCameraPos;
			Vector3 previousWorldSpaceCameraPos = motionVectorsPersistentData.previousWorldSpaceCameraPos;
			Vector3 previousPreviousWorldSpaceCameraPos = motionVectorsPersistentData.previousPreviousWorldSpaceCameraPos;
			perViewConfig.currentView.SetColumn(3, new Vector4(0f - worldSpaceCameraPos.x, 0f - worldSpaceCameraPos.y, 0f - worldSpaceCameraPos.z, 1f));
			perViewConfig.lastView.SetColumn(3, new Vector4(0f - previousWorldSpaceCameraPos.x, 0f - previousWorldSpaceCameraPos.y, 0f - previousWorldSpaceCameraPos.z, 1f));
			perViewConfig.lastLastView.SetColumn(3, new Vector4(0f - previousPreviousWorldSpaceCameraPos.x, 0f - previousPreviousWorldSpaceCameraPos.y, 0f - previousPreviousWorldSpaceCameraPos.z, 1f));
			STP.perViewConfigs[i] = perViewConfig;
		}
		config.numActiveViews = num2;
		config.perViewConfigs = STP.perViewConfigs;
	}

	internal static void Execute(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, TextureHandle inputColor, TextureHandle inputDepth, TextureHandle inputMotion, TextureHandle destination, Texture2D noiseTexture)
	{
		TextureHandle textureHandle = TextureHandle.nullHandle;
		int debugViewIndex = 0;
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(cameraData);
		if (activeDebugHandler != null && activeDebugHandler.TryGetFullscreenDebugMode(out var debugFullScreenMode) && debugFullScreenMode == DebugFullScreenMode.STP)
		{
			TextureDesc desc = new TextureDesc(cameraData.pixelWidth, cameraData.pixelHeight, dynamicResolution: false, cameraData.xr.enabled && cameraData.xr.singlePassEnabled)
			{
				name = "STP Debug View",
				format = GraphicsFormat.R8G8B8A8_UNorm,
				clearBuffer = true,
				enableRandomWrite = true
			};
			textureHandle = renderGraph.CreateTexture(in desc);
			debugViewIndex = activeDebugHandler.stpDebugViewIndex;
			resourceData.stpDebugView = textureHandle;
		}
		PopulateStpConfig(cameraData, inputColor, inputDepth, inputMotion, debugViewIndex, textureHandle, destination, noiseTexture, out var config);
		STP.Execute(renderGraph, ref config);
	}
}
