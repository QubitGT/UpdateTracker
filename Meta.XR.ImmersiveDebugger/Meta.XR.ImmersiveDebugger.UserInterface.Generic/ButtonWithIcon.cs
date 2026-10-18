using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class ButtonWithIcon : Button
{
	protected Icon _icon;

	protected Background _background;

	protected ImageStyle _backgroundStyle;

	protected ImageStyle _iconStyle;

	public ImageStyle BackgroundStyle
	{
		get
		{
			return _backgroundStyle;
		}
		set
		{
			if (!(_backgroundStyle == value))
			{
				_backgroundStyle = value;
				_background.Sprite = _backgroundStyle.sprite;
				_background.PixelDensityMultiplier = _backgroundStyle.pixelDensityMultiplier;
				RefreshStyle();
			}
		}
	}

	public ImageStyle IconStyle
	{
		set
		{
			_iconStyle = value;
			RefreshStyle();
		}
	}

	public Texture2D Icon
	{
		set
		{
			_icon.Texture = value;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_background = Append<Background>("background");
		_background.LayoutStyle = Style.Load<LayoutStyle>("Fill");
		_icon = Append<Icon>("icon");
		_icon.LayoutStyle = Style.Load<LayoutStyle>("Fill");
	}

	protected override void OnHoverChanged()
	{
		base.OnHoverChanged();
		RefreshStyle();
	}

	protected void RefreshStyle()
	{
		UpdateBackground();
		UpdateIcon();
	}

	protected virtual void UpdateBackground()
	{
		if (_backgroundStyle != null && _backgroundStyle.enabled)
		{
			_background.Show();
			_background.Color = (base.Hover ? _backgroundStyle.colorHover : _backgroundStyle.color);
			_background.RaycastTarget = true;
		}
		else
		{
			_background.Hide();
		}
	}

	protected virtual void UpdateIcon()
	{
		if (_iconStyle != null && _iconStyle.enabled)
		{
			_icon.Show();
			_icon.Color = (base.Hover ? _iconStyle.colorHover : _iconStyle.color);
			_icon.RaycastTarget = _backgroundStyle == null || !_backgroundStyle.enabled;
		}
		else
		{
			_icon.Hide();
		}
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		if (!(BackgroundStyle == null) && BackgroundStyle.enabled)
		{
			BackgroundStyle.color.a = (base.Transparent ? 0.25f : 1f);
			BackgroundStyle.colorHover.a = (base.Transparent ? 0.5f : 1f);
			RefreshStyle();
		}
	}
}
