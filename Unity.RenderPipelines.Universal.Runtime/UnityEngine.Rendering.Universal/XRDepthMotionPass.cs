using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public class XRDepthMotionPass : ScriptableRenderPass
{
	private class PassData
	{
		internal RendererListHandle objMotionRendererList;

		internal Matrix4x4[] previousViewProjectionStereo = new Matrix4x4[2];

		internal Matrix4x4[] viewProjectionStereo = new Matrix4x4[2];

		internal Material xrMotionVector;
	}

	public const string k_MotionOnlyShaderTagIdName = "XRMotionVectors";

	private static readonly ShaderTagId k_MotionOnlyShaderTagId = new ShaderTagId("XRMotionVectors");

	private static readonly int k_SpaceWarpNDCModifier = Shader.PropertyToID("_SpaceWarpNDCModifier");

	private PassData m_PassData;

	private RTHandle m_XRMotionVectorColor;

	private TextureHandle xrMotionVectorColor;

	private RTHandle m_XRMotionVectorDepth;

	private TextureHandle xrMotionVectorDepth;

	private bool m_XRSpaceWarpRightHandedNDC;

	private const int k_XRViewCount = 2;

	private Matrix4x4[] m_ViewProjection = new Matrix4x4[2];

	private Matrix4x4[] m_PreviousViewProjection = new Matrix4x4[2];

	private int m_LastFrameIndex;

	private Material m_XRMotionVectorMaterial;

	public XRDepthMotionPass(RenderPassEvent evt, Shader xrMotionVector)
	{
		base.profilingSampler = new ProfilingSampler("XRDepthMotionPass");
		m_PassData = new PassData();
		base.renderPassEvent = evt;
		ResetMotionData();
		m_XRMotionVectorMaterial = CoreUtils.CreateEngineMaterial(xrMotionVector);
		xrMotionVectorColor = TextureHandle.nullHandle;
		m_XRMotionVectorColor = null;
		xrMotionVectorDepth = TextureHandle.nullHandle;
		m_XRMotionVectorDepth = null;
	}

	private static DrawingSettings GetObjectMotionDrawingSettings(Camera camera)
	{
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = SortingCriteria.CommonOpaque;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(k_MotionOnlyShaderTagId, sortingSettings2);
		drawingSettings.perObjectData = PerObjectData.MotionVectors;
		drawingSettings.enableDynamicBatching = false;
		drawingSettings.enableInstancing = true;
		DrawingSettings result = drawingSettings;
		result.SetShaderPassName(0, k_MotionOnlyShaderTagId);
		return result;
	}

	private void InitObjectMotionRendererLists(ref PassData passData, ref CullingResults cullResults, RenderGraph renderGraph, Camera camera)
	{
		DrawingSettings objectMotionDrawingSettings = GetObjectMotionDrawingSettings(camera);
		FilteringSettings fs = new FilteringSettings(RenderQueueRange.opaque, camera.cullingMask);
		fs.forceAllMotionVectorObjects = true;
		RenderingUtils.CreateRendererListWithRenderStateBlock(rsb: new RenderStateBlock(RenderStateMask.Nothing), renderGraph: renderGraph, cullResults: ref cullResults, ds: objectMotionDrawingSettings, fs: fs, rl: ref passData.objMotionRendererList);
	}

	private void InitPassData(ref PassData passData, UniversalCameraData cameraData)
	{
		passData.previousViewProjectionStereo = m_PreviousViewProjection;
		passData.viewProjectionStereo = m_ViewProjection;
		passData.xrMotionVector = m_XRMotionVectorMaterial;
	}

	private void ImportXRMotionColorAndDepth(RenderGraph renderGraph, UniversalCameraData cameraData)
	{
		RenderTargetIdentifier motionVectorRenderTarget = cameraData.xr.motionVectorRenderTarget;
		if (m_XRMotionVectorColor == null)
		{
			m_XRMotionVectorColor = RTHandles.Alloc(motionVectorRenderTarget);
		}
		else if (m_XRMotionVectorColor.nameID != motionVectorRenderTarget)
		{
			RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_XRMotionVectorColor, motionVectorRenderTarget);
		}
		RenderTargetIdentifier motionVectorRenderTarget2 = cameraData.xr.motionVectorRenderTarget;
		if (m_XRMotionVectorDepth == null)
		{
			m_XRMotionVectorDepth = RTHandles.Alloc(motionVectorRenderTarget2);
		}
		else if (m_XRMotionVectorDepth.nameID != motionVectorRenderTarget2)
		{
			RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_XRMotionVectorDepth, motionVectorRenderTarget2);
		}
		RenderTargetInfo renderTargetInfo = default(RenderTargetInfo);
		renderTargetInfo.width = cameraData.xr.motionVectorRenderTargetDesc.width;
		renderTargetInfo.height = cameraData.xr.motionVectorRenderTargetDesc.height;
		renderTargetInfo.volumeDepth = cameraData.xr.motionVectorRenderTargetDesc.volumeDepth;
		renderTargetInfo.msaaSamples = cameraData.xr.motionVectorRenderTargetDesc.msaaSamples;
		renderTargetInfo.format = cameraData.xr.motionVectorRenderTargetDesc.graphicsFormat;
		RenderTargetInfo renderTargetInfo2 = default(RenderTargetInfo);
		renderTargetInfo2 = renderTargetInfo;
		renderTargetInfo2.format = cameraData.xr.motionVectorRenderTargetDesc.depthStencilFormat;
		ImportResourceParams importParams = default(ImportResourceParams);
		importParams.clearOnFirstUse = true;
		importParams.clearColor = Color.black;
		importParams.discardOnLastUse = false;
		ImportResourceParams importParams2 = default(ImportResourceParams);
		importParams2.clearOnFirstUse = true;
		importParams2.clearColor = Color.black;
		importParams2.discardOnLastUse = false;
		xrMotionVectorColor = renderGraph.ImportTexture(m_XRMotionVectorColor, renderTargetInfo, importParams);
		xrMotionVectorDepth = renderGraph.ImportTexture(m_XRMotionVectorDepth, renderTargetInfo2, importParams2);
		m_XRSpaceWarpRightHandedNDC = cameraData.xr.spaceWarpRightHandedNDC;
	}

	internal void Render(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
		UniversalCameraData universalCameraData = frameData.Get<UniversalCameraData>();
		if (!universalCameraData.xr.enabled || !universalCameraData.xr.singlePassEnabled)
		{
			Debug.LogWarning("XRDepthMotionPass::Render is skipped because either XR is not enabled or singlepass rendering is not enabled.");
			return;
		}
		if (!universalCameraData.xr.hasMotionVectorPass)
		{
			Debug.LogWarning("XRDepthMotionPass::Render is skipped because XR motion vector is not enabled for the current XRPass.");
			return;
		}
		ImportXRMotionColorAndDepth(renderGraph, universalCameraData);
		universalCameraData.camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>("XR Motion Pass", out passData, base.profilingSampler, ".\\Library\\PackageCache\\com.unity.render-pipelines.universal@bc6f352be672\\Runtime\\Passes\\XRDepthMotionPass.cs", 194);
		rasterRenderGraphBuilder.EnableFoveatedRasterization(universalCameraData.xr.supportsFoveatedRendering);
		rasterRenderGraphBuilder.SetRenderAttachment(xrMotionVectorColor, 0);
		rasterRenderGraphBuilder.SetRenderAttachmentDepth(xrMotionVectorDepth);
		InitObjectMotionRendererLists(ref passData, ref universalRenderingData.cullResults, renderGraph, universalCameraData.camera);
		rasterRenderGraphBuilder.UseRendererList(in passData.objMotionRendererList);
		rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
		InitPassData(ref passData, universalCameraData);
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			context.cmd.SetGlobalMatrixArray(ShaderPropertyId.previousViewProjectionNoJitterStereo, data.previousViewProjectionStereo);
			context.cmd.SetGlobalMatrixArray(ShaderPropertyId.viewProjectionNoJitterStereo, data.viewProjectionStereo);
			context.cmd.SetGlobalFloat(k_SpaceWarpNDCModifier, m_XRSpaceWarpRightHandedNDC ? (-1f) : 1f);
			context.cmd.DrawRendererList(passData.objMotionRendererList);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.xrMotionVector, 0, MeshTopology.Triangles, 3, 1);
		});
	}

	private void ResetMotionData()
	{
		for (int i = 0; i < 2; i++)
		{
			m_ViewProjection[i] = Matrix4x4.identity;
			m_PreviousViewProjection[i] = Matrix4x4.identity;
		}
		m_LastFrameIndex = -1;
	}

	public void Update(ref UniversalCameraData cameraData)
	{
		if (!cameraData.xr.enabled || !cameraData.xr.singlePassEnabled)
		{
			Debug.LogWarning("XRDepthMotionPass::Update is skipped because either XR is not enabled or singlepass rendering is not enabled.");
		}
		else if (m_LastFrameIndex != Time.frameCount)
		{
			Matrix4x4 matrix4x = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(), renderIntoTexture: false) * cameraData.GetViewMatrix();
			Matrix4x4 matrix4x2 = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrixNoJitter(1), renderIntoTexture: false) * cameraData.GetViewMatrix(1);
			m_PreviousViewProjection[0] = m_ViewProjection[0];
			m_PreviousViewProjection[1] = m_ViewProjection[1];
			m_ViewProjection[0] = matrix4x;
			m_ViewProjection[1] = matrix4x2;
			m_LastFrameIndex = Time.frameCount;
		}
	}

	public void Dispose()
	{
		m_XRMotionVectorColor?.Release();
		m_XRMotionVectorDepth?.Release();
		CoreUtils.Destroy(m_XRMotionVectorMaterial);
	}
}
