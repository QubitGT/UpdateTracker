using System;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class ToggleWithLabel : ButtonWithLabel
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
}
