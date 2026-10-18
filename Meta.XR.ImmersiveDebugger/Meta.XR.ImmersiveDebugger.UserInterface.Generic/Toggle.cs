using System;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Toggle : ButtonWithIcon
{
	private bool _state;

	public bool State
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				_state = value;
				OnStateChanged();
			}
		}
	}

	public Action<bool> StateChanged { get; set; }

	public void ToggleState()
	{
		State = !State;
	}

	private void OnStateChanged()
	{
		StateChanged?.Invoke(State);
		RefreshStyle();
	}

	protected override void UpdateBackground()
	{
		if (_backgroundStyle != null && _backgroundStyle.enabled)
		{
			_background.Show();
			_background.Color = (base.Hover ? _backgroundStyle.colorHover : (State ? _backgroundStyle.colorHover : _backgroundStyle.color));
			_background.RaycastTarget = true;
		}
		else
		{
			_background.Hide();
		}
	}

	protected override void UpdateIcon()
	{
		if (_iconStyle != null && _iconStyle.enabled)
		{
			_icon.Show();
			_icon.Color = (base.Hover ? _iconStyle.colorHover : (State ? _iconStyle.color : _iconStyle.colorOff));
			_icon.RaycastTarget = _backgroundStyle == null || !_backgroundStyle.enabled;
		}
		else
		{
			_icon.Hide();
		}
	}
}
