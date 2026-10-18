using System;
using System.Collections;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Dropdown : Controller
{
	private Flex _flex;

	private TweakEnum _tweak;

	private ButtonWithLabel _baseLabel;

	private Background _background;

	private bool _requestBackgroundUpdate;

	private LayoutStyle _rootLayoutStyle;

	private InspectorPanel _inspectorPanel;

	private float _previousScrollPosition;

	private ImageStyle _backgroundImageStyle;

	private bool IsMenuVisible => _flex.Visibility;

	private float DefaultHeight => _baseLabel.RectTransform.rect.size.y;

	public string Label
	{
		get
		{
			return _baseLabel.Label;
		}
		set
		{
			_baseLabel.Label = value;
			_tweak.Value = value;
		}
	}

	private ImageStyle BackgroundStyle
	{
		set
		{
			_backgroundImageStyle = value;
			_background.Sprite = value.sprite;
			_background.Color = value.color;
			_background.PixelDensityMultiplier = value.pixelDensityMultiplier;
		}
	}

	internal void SetupMenu(TweakEnum tweak)
	{
		_tweak = tweak;
		Label = _tweak.Value;
		SetupDropdownList();
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_baseLabel = Append<ButtonWithLabel>("label");
		_baseLabel.LayoutStyle = Style.Instantiate<LayoutStyle>("DropdownValueItem");
		_baseLabel.TextStyle = Style.Load<TextStyle>("MemberValue");
		_baseLabel.BackgroundStyle = Style.Instantiate<ImageStyle>("DropdownValueBackgroundRoot");
		ButtonWithLabel baseLabel = _baseLabel;
		baseLabel.Callback = (Action)Delegate.Combine(baseLabel.Callback, new Action(OnDropdownClick));
		Icon icon = _baseLabel.Append<Icon>("icon");
		icon.LayoutStyle = Style.Load<LayoutStyle>("DropdownArrowIcon");
		ImageStyle imageStyle = Style.Load<ImageStyle>("DownArrowIcon");
		icon.Texture = imageStyle.icon;
		icon.Color = imageStyle.color;
		_rootLayoutStyle = base.Owner.LayoutStyle;
		_inspectorPanel = base.gameObject.GetComponentInParent<InspectorPanel>();
	}

	private void OnDropdownClick()
	{
		SetDropdownMenuVisibility(!IsMenuVisible);
	}

	internal void OnMenuItemClick(DropdownMenuItem menuItem)
	{
		Label = menuItem.Label;
		SetDropdownMenuVisibility(visible: false);
	}

	private void SetDropdownMenuVisibility(bool visible)
	{
		if (visible)
		{
			_flex.Show();
		}
		else
		{
			_flex.Hide();
		}
		_requestBackgroundUpdate = true;
	}

	private void Update()
	{
		if (_requestBackgroundUpdate)
		{
			_requestBackgroundUpdate = false;
			float num = DefaultHeight + _flex.RectTransform.rect.size.y;
			_rootLayoutStyle.size.y = (_flex.Visibility ? num : DefaultHeight);
			float num2 = DefaultHeight - 2f;
			_background.RectTransform.sizeDelta = new Vector2(_background.RectTransform.sizeDelta.x, _rootLayoutStyle.size.y - num2);
			RefreshLayout();
			StartCoroutine(UpdateScrollPosition(_flex.Visibility));
		}
	}

	private IEnumerator UpdateScrollPosition(bool dropdownIsShowing)
	{
		if (!dropdownIsShowing)
		{
			yield return new WaitForEndOfFrame();
			_inspectorPanel.ScrollView.Progress = _previousScrollPosition;
			yield break;
		}
		_previousScrollPosition = _inspectorPanel.ScrollView.Progress;
		ScrollRect scrollRect = _inspectorPanel.ScrollView.ScrollRect;
		float menuHeight = _flex.RectTransform.rect.size.y;
		yield return new WaitForEndOfFrame();
		float num = Mathf.Abs(scrollRect.content.rect.size.y - _inspectorPanel.ScrollView.RectTransform.rect.size.y);
		float num2 = menuHeight / num;
		_inspectorPanel.ScrollView.Progress = Mathf.Clamp01(_inspectorPanel.ScrollView.Progress + num2);
	}

	private void HideDropdownItems()
	{
		_flex.Hide();
	}

	private void SetupDropdownList()
	{
		_flex = Append<Flex>("list");
		_flex.LayoutStyle = Style.Load<LayoutStyle>("DropdownValuesFlex");
		_background = _flex.Append<Background>("background");
		_background.LayoutStyle = Style.Instantiate<LayoutStyle>("DropdownBackground");
		BackgroundStyle = Style.Load<ImageStyle>("DropdownBackground");
		Array array = null;
		Type type = (_tweak.Member as FieldInfo)?.FieldType;
		Type type2 = (_tweak.Member as PropertyInfo)?.PropertyType;
		if (type != null)
		{
			array = Enum.GetValues(type);
		}
		else if (type2 != null)
		{
			array = Enum.GetValues(type2);
		}
		foreach (object item in array)
		{
			AppendValue(item.ToString());
		}
		HideDropdownItems();
	}

	private void AppendValue(string data)
	{
		DropdownMenuItem dropdownMenuItem = _flex.Append<DropdownMenuItem>("menu_item_" + data);
		dropdownMenuItem.Label = data;
		dropdownMenuItem.RegisterDropdownSourceMenu(this);
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		_backgroundImageStyle.colorHover.a = (base.Transparent ? 0.6f : 1f);
		_background.Color = (base.Transparent ? _backgroundImageStyle.colorOff : _backgroundImageStyle.color);
	}
}
