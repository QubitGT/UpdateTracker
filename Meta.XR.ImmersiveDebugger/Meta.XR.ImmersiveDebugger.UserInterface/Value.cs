using Meta.XR.ImmersiveDebugger.UserInterface.Generic;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

public class Value : Controller
{
	protected Label _label;

	protected Background _background;

	protected ImageStyle _backgroundStyle;

	internal Background Background => _background;

	internal Label Label => _label;

	internal ImageStyle BackgroundStyle
	{
		set
		{
			_backgroundStyle = value;
			_background.Sprite = value.sprite;
			_background.PixelDensityMultiplier = value.pixelDensityMultiplier;
			RefreshStyle();
		}
	}

	internal TextStyle TextStyle
	{
		get
		{
			return _label.TextStyle;
		}
		set
		{
			_label.TextStyle = value;
		}
	}

	internal virtual string Content
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
		_background.LayoutStyle = Style.Instantiate<LayoutStyle>("Fill");
		_label = Append<Label>("label");
		_label.LayoutStyle = Style.Instantiate<LayoutStyle>("Fill");
	}

	protected void RefreshStyle()
	{
		UpdateBackground();
	}

	protected virtual void UpdateBackground()
	{
		if (_backgroundStyle != null && _backgroundStyle.enabled)
		{
			_background.Show();
			_background.Color = (base.Transparent ? _backgroundStyle.colorOff : _backgroundStyle.color);
		}
		else
		{
			_background.Hide();
		}
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		RefreshStyle();
	}
}
