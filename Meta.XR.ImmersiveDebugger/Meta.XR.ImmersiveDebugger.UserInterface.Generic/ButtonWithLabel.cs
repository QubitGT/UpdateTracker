namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class ButtonWithLabel : Button
{
	protected Label _label;

	protected Background _background;

	protected ImageStyle _backgroundStyle;

	public Background Background => _background;

	public ImageStyle BackgroundStyle
	{
		set
		{
			_backgroundStyle = value;
			_background.Sprite = value.sprite;
			_background.PixelDensityMultiplier = value.pixelDensityMultiplier;
			RefreshStyle();
		}
	}

	public TextStyle TextStyle
	{
		set
		{
			_label.TextStyle = value;
		}
	}

	public LayoutStyle LabelLayoutStyle
	{
		set
		{
			_label.LayoutStyle = value;
		}
	}

	public string Label
	{
		get
		{
			return _label.Content;
		}
		set
		{
			_label.Content = value;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_background = Append<Background>("background");
		_background.LayoutStyle = Style.Load<LayoutStyle>("Fill");
		_label = Append<Label>("label");
		_label.LayoutStyle = Style.Load<LayoutStyle>("Fill");
	}

	protected override void OnHoverChanged()
	{
		base.OnHoverChanged();
		RefreshStyle();
	}

	protected void RefreshStyle()
	{
		UpdateBackground();
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		_backgroundStyle.colorHover.a = (base.Transparent ? 0.6f : 1f);
		_background.Color = (base.Transparent ? _backgroundStyle.colorOff : _backgroundStyle.color);
	}

	protected virtual void UpdateBackground()
	{
		if (_backgroundStyle != null && _backgroundStyle.enabled)
		{
			_background.Show();
			_background.Color = (base.Hover ? _backgroundStyle.colorHover : (base.Transparent ? _backgroundStyle.colorOff : _backgroundStyle.color));
			_background.RaycastTarget = true;
		}
		else
		{
			_background.Hide();
		}
	}
}
