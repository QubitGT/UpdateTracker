using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

public class DebugPanel : OverlayCanvasPanel
{
	private Label _title;

	private ButtonWithIcon _closeIcon;

	private const float DynamicPixelsPerUnit = 10f;

	public Texture2D Icon { get; set; }

	public string Title
	{
		get
		{
			return _title.Content;
		}
		set
		{
			_title.Content = value;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_title = Append<Label>("title");
		_title.LayoutStyle = Style.Load<LayoutStyle>("PanelTitle");
		_title.TextStyle = Style.Load<TextStyle>("PanelTitle");
		_closeIcon = Append<ButtonWithIcon>("CloseButton");
		_closeIcon.LayoutStyle = Style.Load<LayoutStyle>("CloseButton");
		_closeIcon.BackgroundStyle = Style.Load<ImageStyle>("CloseButtonBackground");
		_closeIcon.Icon = Resources.Load<Texture2D>("Textures/minimize_icon");
		_closeIcon.IconStyle = Style.Load<ImageStyle>("CloseButtonIcon");
		_closeIcon.Callback = base.Hide;
		SetExpectedPixelsPerUnit(1000f, 10f, 2.24f);
	}
}
