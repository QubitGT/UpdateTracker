using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Liv.Lck.Rendering;

public class LckHeadsetCaptureRenderFeature : ScriptableRendererFeature
{
	private LckHeadsetCaptureRenderPass _pass;

	internal static bool IsConfigured { get; private set; }

	public override void Create()
	{
		IsConfigured = true;
		_pass = new LckHeadsetCaptureRenderPass
		{
			renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
		};
	}

	protected override void Dispose(bool disposing)
	{
		_pass?.Dispose();
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (LckHeadsetCamera._activeInstances.Count == 0)
		{
			return;
		}
		Camera camera = renderingData.cameraData.camera;
		foreach (LckHeadsetCamera activeInstance in LckHeadsetCamera._activeInstances)
		{
			if (activeInstance.IsTargetCamera(camera) && activeInstance.ShouldCaptureEye(camera))
			{
				_pass.Setup(activeInstance, camera);
				renderer.EnqueuePass(_pass);
				break;
			}
		}
	}
}
