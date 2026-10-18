using System;
using Meta.XR.ImmersiveDebugger.Manager;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Switch : ButtonWithIcon
{
	private Texture2D _toggleIconOn;

	private Texture2D _toggleIconOff;

	internal Tweak Tweak { get; set; }

	public bool State
	{
		get
		{
			if (Tweak != null)
			{
				return Math.Abs(Tweak.Tween - 1f) < Mathf.Epsilon;
			}
			return false;
		}
		set
		{
			Tweak.Tween = (value ? 1f : 0f);
			OnStateChanged();
		}
	}

	public Action<bool> StateChanged { get; set; }

	private void OnStateChanged()
	{
		StateChanged?.Invoke(State);
		RefreshStyle();
	}

	private void Start()
	{
		State = Tweak.Tween > 0f;
		UpdateIcon();
	}

	internal void SetToggleIcons(Texture2D onState, Texture2D offState)
	{
		_toggleIconOn = onState;
		_toggleIconOff = offState;
	}

	protected override void UpdateIcon()
	{
		base.Icon = (State ? _toggleIconOn : _toggleIconOff);
		_icon.Color = (base.Hover ? _iconStyle.colorHover : (State ? _iconStyle.color : _iconStyle.colorOff));
		_icon.RaycastTarget = _backgroundStyle == null || !_backgroundStyle.enabled;
	}
}
