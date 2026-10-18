using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

public class ConsoleLine : InteractableController
{
	private Label _label;

	private Flex _flex;

	private Background _background;

	private Background _pill;

	private LogEntry _entry;

	internal UnityEvent<LogEntry> OnClick = new UnityEvent<LogEntry>();

	private Label _counterLabel;

	private Background _counterBackground;

	private ImageStyle _backgroundImageStyle;

	private const int MaxLabelCharacterSize = 116;

	private const int DefaultCounterBackgroundWidth = 16;

	private const int MaxCounterBackgroundWidth = 64;

	internal LogEntry Entry
	{
		get
		{
			return _entry;
		}
		set
		{
			if (_entry != value)
			{
				_entry = value;
				Label = Utils.ClampText(value.Label, 116);
				PillStyle = value.Severity.PillStyle;
				RefreshLogCounter();
			}
		}
	}

	internal string Label
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

	internal ImageStyle BackgroundStyle
	{
		set
		{
			_background.Sprite = value.sprite;
			_background.Color = value.color;
			_background.PixelDensityMultiplier = value.pixelDensityMultiplier;
		}
	}

	internal ImageStyle PillStyle
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
		_background = Append<Background>("background");
		_background.LayoutStyle = Style.Load<LayoutStyle>("Fill");
		_backgroundImageStyle = Style.Load<ImageStyle>("ConsoleLineBackground");
		BackgroundStyle = _backgroundImageStyle;
		_background.RaycastTarget = true;
		_flex = Append<Flex>("line");
		_flex.LayoutStyle = Style.Load<LayoutStyle>("ConsoleLineFlex");
		_pill = _flex.Append<Background>("pill");
		_pill.LayoutStyle = Style.Load<LayoutStyle>("PillVertical");
		_label = _flex.Append<Label>("log");
		_label.LayoutStyle = Style.Load<LayoutStyle>("ConsoleLineLabel");
		_label.TextStyle = Style.Load<TextStyle>("ConsoleLineLabel");
		_label.Text.verticalOverflow = VerticalWrapMode.Truncate;
		_counterBackground = _flex.Append<Background>("counterbackground");
		_counterBackground.LayoutStyle = Object.Instantiate(Style.Load<LayoutStyle>("MiniCounter"));
		ImageStyle imageStyle = Style.Load<ImageStyle>("MiniCounter");
		_counterBackground.Sprite = imageStyle.sprite;
		_counterBackground.Color = imageStyle.color;
		_counterBackground.PixelDensityMultiplier = imageStyle.pixelDensityMultiplier;
		_counterLabel = _counterBackground.Append<Label>("counter");
		_counterLabel.LayoutStyle = Object.Instantiate(Style.Load<LayoutStyle>("MiniCounterValue"));
		_counterLabel.TextStyle = Style.Load<TextStyle>("ConsoleLogCounter");
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		_backgroundImageStyle.colorHover.a = (base.Transparent ? 0.6f : 1f);
		_background.Color = (base.Transparent ? _backgroundImageStyle.colorOff : _backgroundImageStyle.color);
	}

	private void RefreshLogCounter()
	{
		if (!(_counterBackground == null) && !(_counterLabel == null))
		{
			bool logCollapseMode = Entry.Severity.Owner.LogCollapseMode;
			bool flag = Entry.Count > 1 && logCollapseMode;
			ShowCounter(flag);
			if (flag)
			{
				_counterLabel.Content = Entry.Count.ToString();
				_counterBackground.LayoutStyle.size.x = Mathf.Clamp(_counterLabel.Text.preferredWidth + 8f, 16f, 64f);
				_counterBackground.RefreshLayout();
			}
			_label.RefreshLayout();
		}
	}

	private void ShowCounter(bool show = true)
	{
		if (show)
		{
			_counterBackground.Show();
			_counterLabel.Show();
		}
		else
		{
			_counterBackground.Hide();
			_counterLabel.Hide();
		}
	}

	public override void OnPointerClick()
	{
		Entry?.DisplayDetails();
	}

	protected override void OnHoverChanged()
	{
		base.OnHoverChanged();
		_background.Color = (base.Hover ? _backgroundImageStyle.colorHover : (base.Transparent ? _backgroundImageStyle.colorOff : _backgroundImageStyle.color));
	}
}
