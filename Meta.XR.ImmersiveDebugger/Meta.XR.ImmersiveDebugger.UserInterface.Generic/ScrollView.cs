using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class ScrollView : InteractableController
{
	private ScrollRect _scrollRect;

	private ScrollViewport _viewport;

	private Mask _mask;

	private float _previousProgress;

	internal ScrollRect ScrollRect => _scrollRect;

	internal Flex Flex => _viewport.Flex;

	public float Progress
	{
		get
		{
			return _scrollRect.verticalNormalizedPosition;
		}
		set
		{
			_scrollRect.verticalNormalizedPosition = value;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_scrollRect = base.GameObject.AddComponent<PanelScrollRect>();
		_scrollRect.horizontal = false;
		_scrollRect.vertical = true;
		_scrollRect.inertia = true;
		_viewport = Append<ScrollViewport>("viewport");
		_viewport.LayoutStyle = Style.Load<LayoutStyle>("Fill");
		_scrollRect.content = _viewport.Flex.RectTransform;
	}

	protected override void RefreshLayoutPreChildren()
	{
		_previousProgress = Progress;
		base.RefreshLayoutPreChildren();
	}

	protected override void RefreshLayoutPostChildren()
	{
		Progress = _previousProgress;
		base.RefreshLayoutPostChildren();
	}
}
