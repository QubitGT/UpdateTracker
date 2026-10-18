using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class SeverityEntry
{
	private readonly Console _owner;

	private readonly Toggle _button;

	private readonly Label _countLabel;

	private int _count = -1;

	internal Console Owner => _owner;

	public ImageStyle PillStyle { get; }

	public bool ShouldShow
	{
		get
		{
			return _button.State;
		}
		set
		{
			if (_button.State != value)
			{
				_button.State = value;
				_owner.Dirty = true;
			}
		}
	}

	public int Count
	{
		get
		{
			return _count;
		}
		set
		{
			if (_count != value)
			{
				_count = value;
				_countLabel.Content = _count.ToString();
			}
		}
	}

	public SeverityEntry(Console owner, string label, Texture2D icon, ImageStyle imageStyle, ImageStyle pillStyle)
	{
		SeverityEntry severityEntry = this;
		_owner = owner;
		_countLabel = owner.RegisterCount();
		_button = owner.RegisterControl(label, icon, imageStyle, delegate
		{
			severityEntry.ShouldShow = !severityEntry.ShouldShow;
			owner.Dirty = true;
		});
		Count = 0;
		PillStyle = pillStyle;
	}

	public void Reset()
	{
		Count = 0;
	}
}
