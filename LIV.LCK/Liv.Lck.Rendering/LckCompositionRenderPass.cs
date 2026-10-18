using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Liv.Lck.Rendering;

public class LckCompositionRenderPass : ScriptableRenderPass
{
	private const string PassName = "LckCompositionRenderPass";

	private static readonly int OverlayTexID = Shader.PropertyToID("_OverlayTex");

	private Material _blitMaterial;

	private Texture _overlayTexture;

	public void Setup(Material mat, Texture overlayTexture)
	{
		_blitMaterial = mat;
		_overlayTexture = overlayTexture;
		base.requiresIntermediateTexture = true;
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
	{
		UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
		if (!universalResourceData.isActiveTargetBackBuffer && !(_overlayTexture == null))
		{
			_blitMaterial.SetTexture(OverlayTexID, _overlayTexture);
			TextureHandle texture = universalResourceData.activeColorTexture;
			TextureDesc desc = renderGraph.GetTextureDesc(in texture);
			desc.name = "CameraColor-LckCompositionRenderPass";
			desc.clearBuffer = false;
			TextureHandle textureHandle = renderGraph.CreateTexture(in desc);
			RenderGraphUtils.BlitMaterialParameters blitParameters = new RenderGraphUtils.BlitMaterialParameters(texture, textureHandle, _blitMaterial, 0);
			renderGraph.AddBlitPass(blitParameters, "LckCompositionRenderPass", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Rendering\\LckCompositionRenderPass.cs", 48);
			universalResourceData.cameraColor = textureHandle;
		}
	}
}
