using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Liv.Lck.Rendering;

public class LckHeadsetCaptureRenderPass : ScriptableRenderPass
{
	private class PassData
	{
		public TextureHandle Source;

		public TextureHandle Destination;

		public Material Material;
	}

	private const string PassName = "LCK Headset Capture";

	private const int ShaderPassIndex = 1;

	private const int LegacyBlitPassIndex = 0;

	private static readonly int BlitTextureId = Shader.PropertyToID("_BlitTexture");

	private LckHeadsetCamera _headsetCamera;

	private Camera _camera;

	private RTHandle _cachedTargetHandle;

	private RenderTexture _cachedTargetRT;

	public void Setup(LckHeadsetCamera headsetCamera, Camera camera)
	{
		_headsetCamera = headsetCamera;
		_camera = camera;
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		RenderTexture activeTargetTexture = _headsetCamera.ActiveTargetTexture;
		if (activeTargetTexture == null || _headsetCamera.MaterialInstance == null)
		{
			return;
		}
		_headsetCamera.UpdateMaterialForCapture(_camera, universalResourceData.isActiveTargetBackBuffer);
		if (_cachedTargetRT != activeTargetTexture)
		{
			_cachedTargetHandle?.Release();
			_cachedTargetHandle = RTHandles.Alloc(new RenderTargetIdentifier(activeTargetTexture));
			_cachedTargetRT = activeTargetTexture;
		}
		TextureHandle activeColorTexture = universalResourceData.activeColorTexture;
		RenderTargetInfo renderTargetInfo = default(RenderTargetInfo);
		renderTargetInfo.width = activeTargetTexture.width;
		renderTargetInfo.height = activeTargetTexture.height;
		renderTargetInfo.volumeDepth = activeTargetTexture.volumeDepth;
		renderTargetInfo.msaaSamples = activeTargetTexture.antiAliasing;
		renderTargetInfo.format = activeTargetTexture.graphicsFormat;
		RenderTargetInfo info = renderTargetInfo;
		TextureHandle destination = renderGraph.ImportTexture(_cachedTargetHandle, info);
		PassData passData;
		using (IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = renderGraph.AddUnsafePass<PassData>("LCK Headset Capture", out passData, ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Rendering\\LckHeadsetCaptureRenderPass.cs", 85))
		{
			passData.Source = activeColorTexture;
			passData.Destination = destination;
			passData.Material = _headsetCamera.MaterialInstance;
			unsafeRenderGraphBuilder.UseTexture(in activeColorTexture);
			unsafeRenderGraphBuilder.UseTexture(in destination, AccessFlags.Write);
			unsafeRenderGraphBuilder.AllowPassCulling(value: false);
			unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
			{
				context.cmd.SetRenderTarget(data.Destination);
				context.cmd.SetGlobalTexture(BlitTextureId, data.Source);
				CommandBufferHelpers.GetNativeCommandBuffer(context.cmd).DrawProcedural(Matrix4x4.identity, data.Material, 1, MeshTopology.Triangles, 3);
			});
		}
		_headsetCamera.MarkCapturedByRenderFeature();
	}

	[Obsolete("This pass uses RecordRenderGraph on Unity 6+.", false)]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
	}

	public void Dispose()
	{
		_cachedTargetHandle?.Release();
		_cachedTargetHandle = null;
		_cachedTargetRT = null;
	}
}
