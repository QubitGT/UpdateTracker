using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class Member : Controller, IMember
{
	private Label _title;

	private TextArea _description;

	private Flex _flex;

	private Flex _valueFlex;

	private Flex _verticalFlex;

	private Values _values;

	private ButtonForAction _action;

	private Slider _slider;

	private Switch _switch;

	private ToggleForGizmo _gizmo;

	private Background _pill;

	private ImageStyle _pillBackgroundStyle;

	private Color _defaultPillColor;

	private Color _transparentPillColor;

	public string Title
	{
		get
		{
			return _title.Content;
		}
		set
		{
			_title.Content = value.ToDisplayText();
		}
	}

	public string Description
	{
		get
		{
			return _description.Content;
		}
		set
		{
			_description.Content = value;
		}
	}

	public Color PillColor
	{
		set
		{
			_defaultPillColor = value;
			_transparentPillColor = value;
			_transparentPillColor.a = 0.8f;
			_pill.Color = (base.Transparent ? _transparentPillColor : _defaultPillColor);
		}
	}

	public ImageStyle PillStyle
	{
		set
		{
			_pill.Sprite = value.sprite;
			_pill.Color = value.color;
			_pill.PixelDensityMultiplier = value.pixelDensityMultiplier;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_flex = Append<Flex>("list");
		_flex.LayoutStyle = Style.Load<LayoutStyle>("MemberFlex");
		_pill = _flex.Append<Background>("pill");
		_pill.LayoutStyle = Style.Load<LayoutStyle>("PillVertical");
		_pillBackgroundStyle = Style.Load<ImageStyle>("PillInfo");
		PillStyle = _pillBackgroundStyle;
		_title = _flex.Append<Label>("title");
		_title.LayoutStyle = Style.Load<LayoutStyle>("MemberTitle");
		_title.TextStyle = Style.Load<TextStyle>("MemberTitle");
		_verticalFlex = Append<Flex>("vertical");
		_verticalFlex.LayoutStyle = Style.Load<LayoutStyle>("VerticalValueFlex");
		_valueFlex = _verticalFlex.Append<Flex>("values");
		_valueFlex.LayoutStyle = Style.Instantiate<LayoutStyle>("MemberValueFlex");
	}

	public void RegisterDescriptor()
	{
		_description = _verticalFlex.Append<TextArea>("description");
		_description.Label.LayoutStyle.margin = new Vector2(4f, 4f);
		_description.Background.LayoutStyle.margin = new Vector2(0f, 0f);
		_description.LayoutStyle = Style.Instantiate<LayoutStyle>("MemberDescriptor");
		_description.TextStyle = Style.Load<TextStyle>("MemberDescriptorValue");
		_description.BackgroundStyle = Style.Load<ImageStyle>("MemberDescriptionBackground");
		RefreshLayout();
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		_pill.Color = (base.Transparent ? _transparentPillColor : _defaultPillColor);
	}

	public ActionHook GetAction()
	{
		if (!(_action != null))
		{
			return null;
		}
		return _action.Action;
	}

	public void RegisterAction(ActionHook action)
	{
		if (_action == null)
		{
			_action = _valueFlex.Append<ButtonForAction>("action");
			_action.LayoutStyle = Style.Load<LayoutStyle>("MemberAction");
			_action.TextStyle = Style.Load<TextStyle>("MemberValue");
			_action.BackgroundStyle = Style.Load<ImageStyle>("MemberActionBackground");
			string input = (string.IsNullOrEmpty(action.Attribute.DisplayName) ? (action.MemberInfo.Name ?? "") : action.Attribute.DisplayName);
			_action.Label = input.ToDisplayText(64);
			_flex.Hide();
		}
		_action.Action = action;
	}

	public GizmoHook GetGizmo()
	{
		if (!(_gizmo != null))
		{
			return null;
		}
		return _gizmo.Hook;
	}

	public void RegisterGizmo(GizmoHook gizmo)
	{
		if (_gizmo == null)
		{
			_gizmo = _valueFlex.Append<ToggleForGizmo>("gizmo");
			_gizmo.LayoutStyle = Style.Load<LayoutStyle>("MemberButton");
			_gizmo.Icon = Resources.Load<Texture2D>("Textures/eye_icon");
			_gizmo.IconStyle = Style.Load<ImageStyle>("MiniButtonIcon");
		}
		_gizmo.Hook = gizmo;
	}

	public Watch GetWatch()
	{
		if (!(_values != null))
		{
			return null;
		}
		return _values.Watch;
	}

	public void RegisterWatch(Watch watch)
	{
		if (_values == null)
		{
			_values = _valueFlex.Append<Values>("watch");
		}
		_values.Setup(watch);
	}

	public void RegisterEnum(TweakEnum tweak)
	{
		Dropdown dropdown = _valueFlex.Append<Dropdown>("dropdown");
		dropdown.LayoutStyle = Style.Instantiate<LayoutStyle>("DropdownMemberValue");
		dropdown.SetupMenu(tweak);
	}

	public void RegisterTexture(WatchTexture watchTexture)
	{
		Image image = _valueFlex.Append<Image>("texture");
		image.LayoutStyle = Style.Instantiate<LayoutStyle>("TextureValue");
		image.Setup(watchTexture);
		RefreshLayout();
	}

	public Tweak GetTweak()
	{
		if (!(_slider != null))
		{
			return null;
		}
		return _slider.Tweak;
	}

	public void RegisterTweak(Tweak tweak)
	{
		if (!(tweak is Tweak<float>) && !(tweak is Tweak<int>))
		{
			if (tweak is Tweak<bool>)
			{
				AddToggle(tweak);
			}
		}
		else
		{
			AddSlider(tweak);
		}
	}

	private void AddToggle(Tweak tweak)
	{
		if (_switch == null)
		{
			_switch = _valueFlex.Prepend<Switch>("switch");
			_switch.LayoutStyle = Style.Load<LayoutStyle>("MemberButtonToggle");
			_switch.SetToggleIcons(Resources.Load<Texture2D>("Textures/toggle_on"), Resources.Load<Texture2D>("Textures/toggle_off"));
			_switch.IconStyle = Style.Load<ImageStyle>("ToggleButtonIcon");
			_switch.Callback = delegate
			{
				_switch.State = !_switch.State;
			};
		}
		_switch.Tweak = tweak;
	}

	private void AddSlider(Tweak tweak)
	{
		if (_slider == null)
		{
			_slider = _valueFlex.Append<Slider>("slider");
			_slider.LayoutStyle = Style.Load<LayoutStyle>("MemberSlider");
			_slider.EmptyBackgroundStyle = Style.Load<ImageStyle>("MemberValueBackground");
			_slider.FillBackgroundStyle = Style.Load<ImageStyle>("MemberActionBackground");
		}
		_slider.Tweak = tweak;
	}
}
