using System;
using Meta.XR.ImmersiveDebugger.Hierarchy;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class HierarchyItemButton : Flex
{
	private Item _item;

	private int _counter;

	private ToggleWithLabel _label;

	private Toggle _foldout;

	private bool _previousEnabledState;

	internal Item Item
	{
		get
		{
			return _item;
		}
		set
		{
			_item = value;
			_label.Label = _item.Label;
			if (_item.ComputeNumberOfChildren() > 0)
			{
				_foldout.IconStyle = Style.Load<ImageStyle>("FoldoutIcon");
			}
			else
			{
				_foldout.IconStyle = Style.Load<ImageStyle>("None");
			}
			UpdateGameObjectState(force: true);
		}
	}

	internal int Counter
	{
		get
		{
			return _counter;
		}
		set
		{
			_counter = value;
			_counter = Math.Max(0, _counter);
		}
	}

	internal Toggle Foldout => _foldout;

	internal ToggleWithLabel Label => _label;

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_foldout = Append<Toggle>("foldout");
		_foldout.LayoutStyle = Style.Load<LayoutStyle>("Foldout");
		_foldout.Icon = Resources.Load<Texture2D>("Textures/caret_right_icon");
		_foldout.IconStyle = Style.Load<ImageStyle>("FoldoutIcon");
		_foldout.StateChanged = OnStateChanged;
		_label = Append<ToggleWithLabel>("label");
		_label.LayoutStyle = Style.Load<LayoutStyle>("HierarchyItemLabel");
		_label.TextStyle = Style.Load<TextStyle>("HierarchyItemLabel");
		_label.BackgroundStyle = Style.Instantiate<ImageStyle>("HierarchyItemBackground");
		_label.LabelLayoutStyle = Style.Load<LayoutStyle>("HierarchyItemLabelInner");
	}

	private void OnStateChanged(bool state)
	{
		_foldout.Icon = Resources.Load<Texture2D>(state ? "Textures/caret_down_icon" : "Textures/caret_right_icon");
		if (state)
		{
			Item.BuildChildren();
		}
		else
		{
			Item.ClearChildren();
		}
	}

	private void Update()
	{
		if (!Item.Valid)
		{
			Item.Clear();
			return;
		}
		UpdateGameObjectState();
		if (_foldout.State && Item.ComputeNeedsRefresh())
		{
			Item.BuildChildren();
		}
	}

	private void UpdateGameObjectState(bool force = false)
	{
		if (Item.Owner is GameObject gameObject)
		{
			UpdateGameObjectState(gameObject.activeSelf, force);
		}
		else
		{
			UpdateGameObjectState(state: true, force);
		}
	}

	private void UpdateGameObjectState(bool state, bool force = false)
	{
		if (_previousEnabledState != state || force)
		{
			_label.TextStyle = Style.Load<TextStyle>(state ? "HierarchyItemLabel" : "HierarchyItemLabelDeactivated");
			_previousEnabledState = state;
		}
	}
}
