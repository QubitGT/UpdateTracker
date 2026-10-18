using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class OverlayCanvasPanel : Panel
{
	private OverlayCanvas _overlayCanvas;

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		if (RuntimeSettings.Instance.ShouldUseOverlay)
		{
			_canvas.sortingOrder = -100;
			_canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
			_overlayCanvas = base.GameObject.AddComponent<OverlayCanvas>();
			_overlayCanvas.Panel = this;
		}
	}
}
