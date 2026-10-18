using System;
using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

public class DebugBar : OverlayCanvasPanel
{
	private List<DebugPanel> _panels = new List<DebugPanel>();

	private Dictionary<DebugPanel, Toggle> _panelToggles = new Dictionary<DebugPanel, Toggle>();

	private Flex _buttonsAnchor;

	private Flex _miniButtonsAnchor;

	private Label _time;

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_buttonsAnchor = Append<Flex>("buttons");
		_buttonsAnchor.LayoutStyle = Style.Load<LayoutStyle>("Buttons");
		Controller controller = Append<Controller>("leftbuttons");
		controller.LayoutStyle = Style.Load<LayoutStyle>("FillWithMargin");
		_time = controller.Append<Label>("time");
		_time.LayoutStyle = Style.Load<LayoutStyle>("BarTime");
		_time.TextStyle = Style.Load<TextStyle>("BarTime");
		_miniButtonsAnchor = controller.Append<Flex>("miniButtons");
		_miniButtonsAnchor.LayoutStyle = Style.Load<LayoutStyle>("MiniButtons");
		SetExpectedPixelsPerUnit(1000f, 10f, 2.24f);
		Show();
	}

	public void RegisterPanel(DebugPanel panel)
	{
		if (!(panel == null))
		{
			panel.OnVisibilityChangedEvent += OnPanelVisibilityChanged;
			_panels.Add(panel);
			Toggle toggle = _buttonsAnchor.Append<Toggle>("PanelButton");
			toggle.Icon = panel.Icon;
			toggle.LayoutStyle = Style.Load<LayoutStyle>("PanelButton");
			toggle.BackgroundStyle = Style.Load<ImageStyle>("PanelButtonBackground");
			toggle.IconStyle = Style.Load<ImageStyle>("PanelButtonIcon");
			toggle.Callback = panel.ToggleVisibility;
			_panelToggles.Add(panel, toggle);
		}
	}

	public Toggle RegisterControl(string buttonName, Texture2D icon, Action callback)
	{
		if (buttonName == null)
		{
			throw new ArgumentNullException("buttonName");
		}
		if (icon == null)
		{
			throw new ArgumentNullException("icon");
		}
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		Toggle toggle = _miniButtonsAnchor.Append<Toggle>(buttonName);
		toggle.LayoutStyle = Style.Load<LayoutStyle>("MiniButton");
		toggle.Icon = icon;
		toggle.IconStyle = Style.Load<ImageStyle>("MiniButtonIcon");
		toggle.Callback = callback;
		return toggle;
	}

	private void OnPanelVisibilityChanged(Controller controller)
	{
		if (controller is DebugPanel debugPanel && _panelToggles.TryGetValue(debugPanel, out var value))
		{
			value.State = debugPanel.Visibility;
		}
	}

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		int num = (int)(realtimeSinceStartup / 60f);
		int num2 = (int)(realtimeSinceStartup % 60f);
		string content = $"{num:00}:{num2:00}";
		_time.Content = content;
	}
}
