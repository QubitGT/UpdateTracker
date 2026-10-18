using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class ScrollViewport : Controller
{
	private RawImage _image;

	private Mask _mask;

	private Flex _flex;

	internal Flex Flex => _flex;

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		ScrollView scrollView = owner as ScrollView;
		if (!(scrollView == null))
		{
			_image = base.GameObject.AddComponent<RawImage>();
			_image.raycastTarget = true;
			_mask = base.GameObject.AddComponent<Mask>();
			_mask.showMaskGraphic = false;
			_flex = Append<Flex>("content");
			scrollView.ScrollRect.content = _flex.RectTransform;
			_flex.ScrollViewport = this;
		}
	}
}
