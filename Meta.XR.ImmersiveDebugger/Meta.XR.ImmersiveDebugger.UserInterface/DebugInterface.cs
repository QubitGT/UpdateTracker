using System;
using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

public class DebugInterface : Interface
{
	private DebugBar _bar;

	private Toggle _showAllButton;

	private Toggle _followButton;

	private Toggle _rotateButton;

	private Toggle _opacityButton;

	private Toggle _distanceButton;

	private InspectorPanel _inspectorPanel;

	private Console _console;

	private int _distanceToggleIndex;

	private readonly int _distanceOptionSize = Enum.GetValues(typeof(RuntimeSettings.DistanceOption)).Length;

	protected override bool FollowOverride
	{
		get
		{
			return _followButton.State;
		}
		set
		{
			_followButton.State = value;
		}
	}

	protected override bool RotateOverride
	{
		get
		{
			return _rotateButton.State;
		}
		set
		{
			_rotateButton.State = value;
		}
	}

	public bool OpacityOverride
	{
		get
		{
			return _opacityButton.State;
		}
		set
		{
			_opacityButton.State = value;
			foreach (Controller child in base.Children)
			{
				if (child is InteractableController controller)
				{
					SetTransparencyRecursive(controller, !OpacityOverride);
				}
			}
		}
	}

	internal void SetTransparencyRecursive(Controller controller, bool transparent)
	{
		controller.Transparent = transparent;
		if (controller.Children == null)
		{
			return;
		}
		foreach (Controller child in controller.Children)
		{
			SetTransparencyRecursive(child, transparent);
		}
	}

	internal override void Awake()
	{
		base.Awake();
		Hide();
		_inspectorPanel = Append<InspectorPanel>("inspectors");
		_inspectorPanel.LayoutStyle = Style.Load<LayoutStyle>("InspectorPanel");
		_inspectorPanel.BackgroundStyle = Style.Load<ImageStyle>("PanelBackground");
		_inspectorPanel.Title = "Inspectors";
		_inspectorPanel.Icon = Resources.Load<Texture2D>("Textures/inspectors_icon");
		_inspectorPanel.SetPanelPosition(RuntimeSettings.Instance.PanelDistance, skipAnimation: true);
		_console = Append<Console>("console");
		_console.LayoutStyle = Style.Load<LayoutStyle>("ConsolePanel");
		_console.BackgroundStyle = Style.Load<ImageStyle>("PanelBackground");
		_console.Title = "Console";
		_console.Icon = Resources.Load<Texture2D>("Textures/console_icon");
		_console.SetPanelPosition(RuntimeSettings.Instance.PanelDistance, skipAnimation: true);
		_distanceToggleIndex = (int)RuntimeSettings.Instance.PanelDistance;
		_bar = Append<DebugBar>("bar");
		_bar.LayoutStyle = Style.Load<LayoutStyle>("Bar");
		_bar.BackgroundStyle = Style.Load<ImageStyle>("BarBackground");
		_bar.SphericalCoordinates = new Vector3(0.7f, 0f, -0.5f);
		_bar.RegisterPanel(_console);
		_bar.RegisterPanel(_inspectorPanel);
		_opacityButton = _bar.RegisterControl("opacity", Resources.Load<Texture2D>("Textures/opacity_icon"), delegate
		{
			OpacityOverride = !OpacityOverride;
		});
		_followButton = _bar.RegisterControl("followMove", Resources.Load<Texture2D>("Textures/move_icon"), ToggleFollowTranslation);
		_rotateButton = _bar.RegisterControl("followRotation", Resources.Load<Texture2D>("Textures/rotate_icon"), ToggleFollowRotation);
		_distanceButton = _bar.RegisterControl("setDistance", Resources.Load<Texture2D>("Textures/shift_icon"), ToggleDistances);
		_distanceButton.State = true;
		RuntimeSettings instance = RuntimeSettings.Instance;
		FollowOverride = instance.FollowOverride;
		RotateOverride = instance.RotateOverride;
		OpacityOverride = true;
		if (instance.ShowInspectors)
		{
			_inspectorPanel.Show();
		}
		if (instance.ShowConsole)
		{
			_console.Show();
		}
		if (instance.ImmersiveDebuggerDisplayAtStartup)
		{
			Show();
		}
		DebugManager instance2 = DebugManager.Instance;
		if (instance2 != null)
		{
			instance2.OnUpdateAction += UpdateVisibility;
			instance2.CustomShouldRetrieveInstanceCondition += IsInspectorPanelVisible;
		}
	}

	private void ToggleDistances()
	{
		_distanceToggleIndex = ++_distanceToggleIndex % _distanceOptionSize;
		_inspectorPanel.SetPanelPosition((RuntimeSettings.DistanceOption)_distanceToggleIndex);
		_console.SetPanelPosition((RuntimeSettings.DistanceOption)_distanceToggleIndex);
	}

	private void ToggleFollowTranslation()
	{
		FollowOverride = !FollowOverride;
	}

	private void ToggleFollowRotation()
	{
		RotateOverride = !RotateOverride;
	}

	private void UpdateVisibility()
	{
		if (OVRInput.GetDown(RuntimeSettings.Instance.ImmersiveDebuggerToggleDisplayButton))
		{
			ToggleVisibility();
		}
	}

	private void Update()
	{
		RuntimeSettings instance = RuntimeSettings.Instance;
		if (OVRInput.GetDown(instance.ToggleFollowTranslationButton))
		{
			ToggleFollowTranslation();
		}
		if (OVRInput.GetDown(instance.ToggleFollowRotationButton))
		{
			ToggleFollowRotation();
		}
	}

	private bool IsInspectorPanelVisible()
	{
		if (base.Visibility)
		{
			return _inspectorPanel?.Visibility ?? false;
		}
		return false;
	}
}
