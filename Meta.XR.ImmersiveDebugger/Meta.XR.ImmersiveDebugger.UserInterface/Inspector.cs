using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class Inspector : Controller, IInspector
{
	private InstanceHandle _instanceHandle;

	private ToggleWithLabel _title;

	private Flex _flex;

	private Background _background;

	private readonly Dictionary<MemberInfo, Member> _registry = new Dictionary<MemberInfo, Member>();

	private ImageStyle _backgroundImageStyle;

	private Toggle _foldout;

	private bool _previousEnabledState;

	public ImageStyle BackgroundStyle
	{
		set
		{
			_background.Sprite = value.sprite;
			_background.Color = value.color;
			_background.PixelDensityMultiplier = value.pixelDensityMultiplier;
		}
	}

	public string Title
	{
		get
		{
			return _title.Label;
		}
		set
		{
			_title.Label = value;
		}
	}

	public InstanceHandle InstanceHandle
	{
		get
		{
			return _instanceHandle;
		}
		set
		{
			_instanceHandle = value;
			Object instance = _instanceHandle.Instance;
			string title = ((instance != null) ? (instance.name + " - " + _instanceHandle.Type.Name) : (_instanceHandle.Type.Name ?? ""));
			Title = title;
			UpdateInstanceState();
		}
	}

	public Toggle Foldout => _foldout;

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_background = Append<Background>("background");
		_background.LayoutStyle = Style.Load<LayoutStyle>("Fill");
		_backgroundImageStyle = Style.Load<ImageStyle>("InspectorBackground");
		BackgroundStyle = _backgroundImageStyle;
		_title = Append<ToggleWithLabel>("title");
		_title.LayoutStyle = Style.Load<LayoutStyle>("InspectorTitle");
		_title.Background.LayoutStyle = Style.Load<LayoutStyle>("InspectorTitleBackground");
		_title.BackgroundStyle = Style.Load<ImageStyle>("InspectorTitleBackground");
		_foldout = Append<Toggle>("foldout");
		_foldout.LayoutStyle = Style.Load<LayoutStyle>("InspectorFoldout");
		_foldout.Icon = Resources.Load<Texture2D>("Textures/caret_right_icon");
		_foldout.IconStyle = Style.Load<ImageStyle>("InspectorFoldoutIcon");
		_flex = Append<Flex>("list");
		_flex.LayoutStyle = Style.Load<LayoutStyle>("InspectorFlex");
		_foldout.StateChanged = OnStateChanged;
		_foldout.Callback = _foldout.ToggleState;
		_title.Callback = _foldout.ToggleState;
		_foldout.State = true;
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		_background.Color = (base.Transparent ? _backgroundImageStyle.colorOff : _backgroundImageStyle.color);
	}

	public void UpdateBackground(bool transparent)
	{
		base.Transparent = transparent;
		OnTransparencyChanged();
	}

	public IMember RegisterMember(MemberInfo memberInfo, DebugMember attribute)
	{
		if (!_registry.TryGetValue(memberInfo, out var value))
		{
			value = _flex.Append<Member>(memberInfo.Name);
			value.LayoutStyle = Style.Instantiate<LayoutStyle>("Member");
			value.Title = (string.IsNullOrEmpty(attribute.DisplayName) ? (memberInfo.Name ?? "") : attribute.DisplayName);
			if (!string.IsNullOrEmpty(attribute.Description))
			{
				value.RegisterDescriptor();
				value.Description = attribute.Description;
			}
			value.PillColor = attribute.Color;
			_registry.Add(memberInfo, value);
			if (!_foldout.State)
			{
				_flex.Forget(value);
			}
		}
		return value;
	}

	public IMember GetMember(MemberInfo memberInfo)
	{
		_registry.TryGetValue(memberInfo, out var value);
		return value;
	}

	private void OnStateChanged(bool state)
	{
		_foldout.Icon = Resources.Load<Texture2D>(state ? "Textures/caret_down_icon" : "Textures/caret_right_icon");
		if (state)
		{
			foreach (KeyValuePair<MemberInfo, Member> item in _registry)
			{
				_flex.Remember(item.Value);
			}
			_flex.LayoutStyle = Style.Load<LayoutStyle>("InspectorFlex");
		}
		else
		{
			_flex.ForgetAll();
			_flex.LayoutStyle = Style.Load<LayoutStyle>("InspectorFlexFold");
		}
	}

	private void Update()
	{
		UpdateInstanceState();
	}

	private void UpdateInstanceState(bool force = false)
	{
		if (InstanceHandle.Instance is Behaviour behaviour)
		{
			UpdateInstanceState(behaviour != null && behaviour.isActiveAndEnabled, force);
		}
		else
		{
			UpdateInstanceState(state: true, force);
		}
	}

	private void UpdateInstanceState(bool state, bool force = false)
	{
		if (_previousEnabledState != state || force)
		{
			_title.TextStyle = Style.Load<TextStyle>(state ? "InspectorTitle" : "InspectorTitleDeactivated");
			_previousEnabledState = state;
		}
	}
}
