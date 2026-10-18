using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Liv.Lck.Rendering;

public class LckCompositionRenderFeature : ScriptableRendererFeature
{
	public static readonly int OverlayTexID = Shader.PropertyToID("_OverlayTex");

	[Tooltip("The LCK Composition Profile to source layers from.")]
	[SerializeField]
	private LckCompositionProfile _compositionProfile;

	[Tooltip("The material used when making the blit operation.")]
	[SerializeField]
	private Material _material;

	[Tooltip("The event where to inject the pass.")]
	[SerializeField]
	private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

	[Tooltip("Display the pass on the Game preview windows in editor.")]
	[SerializeField]
	private bool _previewInGameWindow;

	private LckCompositionRenderPass m_Pass;

	public override void Create()
	{
		m_Pass = new LckCompositionRenderPass();
		m_Pass.renderPassEvent = _renderPassEvent;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if ((!renderingData.cameraData.camera.TryGetComponent<LckCamera>(out var _) && !_previewInGameWindow) || _material == null || _compositionProfile == null || _compositionProfile.Layers == null)
		{
			return;
		}
		List<ILckCompositionLayer> activeLayers = _compositionProfile.GetActiveLayers();
		if (activeLayers.Count == 0)
		{
			return;
		}
		ILckCompositionLayer lckCompositionLayer = activeLayers[0];
		if (lckCompositionLayer != null)
		{
			lckCompositionLayer.BlendMaterial.SetTexture(OverlayTexID, lckCompositionLayer.CurrentTexture);
			Texture texture = lckCompositionLayer?.CurrentTexture;
			if (!(texture == null))
			{
				m_Pass.Setup(_material, texture);
				renderer.EnqueuePass(m_Pass);
			}
		}
	}
}
