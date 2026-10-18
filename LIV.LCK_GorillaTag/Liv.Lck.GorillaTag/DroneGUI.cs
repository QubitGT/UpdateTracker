using System;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class DroneGUI
{
	private enum UIMode
	{
		Settings,
		Help
	}

	private struct KeyData
	{
		public string Letter;

		public bool IsActive;

		public bool Show;
	}

	private GUISkin _guiSkin;

	private DroneDataModel _model;

	private const int PANEL_WIDTH = 368;

	private const int PANEL_HEIGHT = 1080;

	private const int STEPPER_SUBBUTTON_WIDTH = 156;

	private const int BUTTON_HEIGHT = 40;

	private const int PRIMARY_OFFSET = 24;

	private const int KEYS_GROUP_SECONDARY_OFFSET = 188;

	private const int PADDING_TOP = 8;

	private bool _isDroneModeActive;

	private GUIStyle _labelAlignRight;

	private GUIStyle _labelAlignLeft;

	private GUIStyle _stepperSubButtonStyle;

	private GUIStyle _activateDeactivateButtonStyle;

	private GUIStyle _recordButtonStyle;

	private GUIStyle _infoButtonStyle;

	private GUIStyle _secondaryButtonStyle;

	private GUIStyle _keyStyleNormal;

	private GUIStyle _keyStyleActive;

	private GUIStyle _keyStyleSpace;

	private const string HELP_WEBSITE = "https://gorillatag.fandom.com/wiki/LIV_Camera";

	private KeyData[] _movementKeysW = new KeyData[6];

	private KeyData[] _movementKeysS = new KeyData[6];

	private KeyData[] _movementKeysA = new KeyData[6];

	private KeyData[] _movementKeysD = new KeyData[6];

	private KeyData[] _movementKeysQ = new KeyData[6];

	private KeyData[] _movementKeysE = new KeyData[6];

	private KeyData[] _rotationKeysUp = new KeyData[6];

	private KeyData[] _rotationKeysDown = new KeyData[6];

	private KeyData[] _rotationKeysLeft = new KeyData[6];

	private KeyData[] _rotationKeysRight = new KeyData[6];

	private UIMode _currentUIMode;

	public DroneGUI(DroneDataModel model, GUISkin skin)
	{
		_model = model;
		_guiSkin = skin;
		_labelAlignRight = new GUIStyle(_guiSkin.label);
		_labelAlignRight.alignment = TextAnchor.MiddleRight;
		_labelAlignRight.padding = new RectOffset(0, 0, -16, 0);
		_labelAlignLeft = new GUIStyle(_guiSkin.label);
		_labelAlignLeft.alignment = TextAnchor.MiddleLeft;
		_labelAlignLeft.padding = new RectOffset(0, 0, -16, 0);
		_stepperSubButtonStyle = new GUIStyle(_guiSkin.button);
		_stepperSubButtonStyle.alignment = TextAnchor.MiddleCenter;
		_stepperSubButtonStyle.normal.textColor = Color.white;
		_stepperSubButtonStyle.normal.background = _guiSkin.toggle.normal.background;
		_stepperSubButtonStyle.hover.textColor = Color.white;
		_stepperSubButtonStyle.hover.background = _guiSkin.toggle.hover.background;
		_stepperSubButtonStyle.active.textColor = Color.white;
		_stepperSubButtonStyle.active.background = _guiSkin.toggle.active.background;
		_stepperSubButtonStyle.fontSize = 48;
		_stepperSubButtonStyle.fixedWidth = 156f;
		_stepperSubButtonStyle.contentOffset = new Vector2(4f, 0f);
		_activateDeactivateButtonStyle = new GUIStyle(_guiSkin.button);
		_activateDeactivateButtonStyle.fixedWidth = 272f;
		_activateDeactivateButtonStyle.contentOffset = new Vector2(8f, 0f);
		_infoButtonStyle = new GUIStyle(_stepperSubButtonStyle);
		_infoButtonStyle.fixedWidth = 40f;
		_infoButtonStyle.fixedHeight = 40f;
		_infoButtonStyle.fontSize = 32;
		_infoButtonStyle.contentOffset = new Vector2(2f, 0f);
		_secondaryButtonStyle = new GUIStyle(_infoButtonStyle);
		_secondaryButtonStyle.fixedWidth = 320f;
		_secondaryButtonStyle.contentOffset = new Vector2(8f, 0f);
		_keyStyleNormal = new GUIStyle(_guiSkin.label);
		_keyStyleNormal.alignment = TextAnchor.MiddleCenter;
		_keyStyleNormal.fontSize = 24;
		_keyStyleNormal.fixedWidth = 24f;
		_keyStyleNormal.fixedHeight = 24f;
		_keyStyleNormal.contentOffset = new Vector2(3f, 0f);
		_keyStyleNormal.normal.background = _secondaryButtonStyle.normal.background;
		_keyStyleActive = new GUIStyle(_keyStyleNormal);
		_keyStyleActive.normal.background = _activateDeactivateButtonStyle.normal.background;
		_keyStyleActive.normal.textColor = _activateDeactivateButtonStyle.normal.textColor;
		_keyStyleSpace = new GUIStyle(_keyStyleActive);
		_keyStyleSpace.fixedWidth = 144f;
		_recordButtonStyle = new GUIStyle(_activateDeactivateButtonStyle);
		_recordButtonStyle.fixedWidth = 320f;
		KeyData[] array = new KeyData[6]
		{
			new KeyData
			{
				Letter = "Q",
				IsActive = false,
				Show = true
			},
			new KeyData
			{
				Letter = "W",
				IsActive = false,
				Show = true
			},
			new KeyData
			{
				Letter = "E",
				IsActive = false,
				Show = true
			},
			new KeyData
			{
				Letter = "A",
				IsActive = false,
				Show = true
			},
			new KeyData
			{
				Letter = "S",
				IsActive = false,
				Show = true
			},
			new KeyData
			{
				Letter = "D",
				IsActive = false,
				Show = true
			}
		};
		KeyData[] array2 = new KeyData[6]
		{
			new KeyData
			{
				Letter = " ",
				IsActive = false,
				Show = false
			},
			new KeyData
			{
				Letter = "[",
				IsActive = false,
				Show = true
			},
			new KeyData
			{
				Letter = " ",
				IsActive = false,
				Show = false
			},
			new KeyData
			{
				Letter = "<",
				IsActive = false,
				Show = true
			},
			new KeyData
			{
				Letter = "]",
				IsActive = false,
				Show = true
			},
			new KeyData
			{
				Letter = ">",
				IsActive = false,
				Show = true
			}
		};
		Array.Copy(array, _movementKeysW, array.Length);
		_movementKeysW[1].IsActive = true;
		Array.Copy(array, _movementKeysS, array.Length);
		_movementKeysS[4].IsActive = true;
		Array.Copy(array, _movementKeysA, array.Length);
		_movementKeysA[3].IsActive = true;
		Array.Copy(array, _movementKeysD, array.Length);
		_movementKeysD[5].IsActive = true;
		Array.Copy(array, _movementKeysQ, array.Length);
		_movementKeysQ[0].IsActive = true;
		Array.Copy(array, _movementKeysE, array.Length);
		_movementKeysE[2].IsActive = true;
		Array.Copy(array2, _rotationKeysLeft, array2.Length);
		_rotationKeysLeft[3].IsActive = true;
		Array.Copy(array2, _rotationKeysRight, array2.Length);
		_rotationKeysRight[5].IsActive = true;
		Array.Copy(array2, _rotationKeysUp, array2.Length);
		_rotationKeysUp[1].IsActive = true;
		Array.Copy(array2, _rotationKeysDown, array2.Length);
		_rotationKeysDown[4].IsActive = true;
	}

	public void SetRecordButtonState(RecordingState state)
	{
		_recordButtonStyle.normal = ((state == RecordingState.Recording) ? _guiSkin.button.onNormal : _guiSkin.button.normal);
		_recordButtonStyle.hover = ((state == RecordingState.Recording) ? _guiSkin.button.onHover : _guiSkin.button.hover);
		_recordButtonStyle.active = ((state == RecordingState.Recording) ? _guiSkin.button.onActive : _guiSkin.button.active);
	}

	public void Run()
	{
		GUI.skin = _guiSkin;
		GUI.BeginGroup(new Rect(Screen.width - 368, 0f, 368f, 1080f));
		GUI.Box(new Rect(0f, 0f, 368f, 1080f), "");
		if (_currentUIMode == UIMode.Settings)
		{
			RenderSettingsUIMode();
		}
		else
		{
			RenderHelpUIMode();
		}
		GUI.Label(new Rect(24f, 1032f, 368f, 48f), "PRESS <color=#FFC23F>TAB</color> TO SHOW/HIDE UI");
		GUI.EndGroup();
	}

	private void RenderSettingsUIMode()
	{
		GUI.Label(new Rect(24f, 8f, 368f, 40f), "LIV FREE CAMERA");
		if (GUI.Button(new Rect(24f, 72f, 272f, 40f), _model.IsDroneModeActive ? "DEACTIVATE" : "ACTIVATE", _activateDeactivateButtonStyle))
		{
			ToggleDroneMode();
			bool isDroneModeActive = _model.IsDroneModeActive;
			_activateDeactivateButtonStyle.normal = (isDroneModeActive ? _guiSkin.button.onNormal : _guiSkin.button.normal);
			_activateDeactivateButtonStyle.hover = (isDroneModeActive ? _guiSkin.button.onHover : _guiSkin.button.hover);
			_activateDeactivateButtonStyle.active = (isDroneModeActive ? _guiSkin.button.onActive : _guiSkin.button.active);
		}
		if (GUI.Button(new Rect(304f, 72f, 40f, 40f), "?", _infoButtonStyle))
		{
			_currentUIMode = UIMode.Help;
		}
		_model.UseKeyboard = ToggleUI(_model.UseKeyboard, 136f, "KEYBOARD");
		_model.UseMouse = ToggleUI(_model.UseMouse, 176f, "MOUSE");
		_model.UseGamepad = ToggleUI(_model.UseGamepad, 216f, "GAMEPAD");
		_model.MoveSpeed = StepperUI(_model.MoveSpeed, _model.MoveSpeedStep, 280f, "MOVE SPEED", 0.1f, _model.MaxMoveSpeed);
		_model.MoveSmoothness = StepperUI(_model.MoveSmoothness, _model.MoveSmoothnessStep, 362f, "MOVE SMOOTHNESS", 0f, _model.MaxMoveSmoothness);
		_model.RotationSpeed = StepperUI(_model.RotationSpeed, _model.RotationSpeedStep, 460f, "ROTATION SPEED", 1f, _model.MaxRotationSpeed);
		_model.RotationSmoothness = StepperUI(_model.RotationSmoothness, _model.RotationSmoothnessStep, 542f, "ROTATION SMOOTHNESS", 0f, _model.MaxRotationSmoothness);
		_model.Fov = StepperUI(_model.Fov, _model.FovStep, 640f, "FOV", _model.MinFov, _model.MaxFov);
		_model.FovSmoothness = StepperUI(_model.FovSmoothness, _model.FovSmoothnessStep, 722f, "FOV SMOOTHNESS", 0f, _model.MaxFovSmoothness);
		_model.UseTiltAsDirection = ToggleUI(_model.UseTiltAsDirection, 820f, "USE TILT AS DIRECTION");
		_model.SnapAxis = ToggleUI(_model.SnapAxis, 860f, "SNAP AXIS");
		_model.IsMouseInverted = ToggleUI(_model.IsMouseInverted, 900f, "INVERT MOUSE");
		if (_model.IsDroneModeActive && GUI.Button(new Rect(24f, 964f, 272f, 40f), GetRecordingButtonText(_model.DroneRecordingStateData.State), _recordButtonStyle))
		{
			_model.RecordButtonPressed();
		}
	}

	private void RenderHelpUIMode()
	{
		if (GUI.Button(new Rect(24f, 24f, 320f, 40f), "LEARN MORE", _secondaryButtonStyle))
		{
			Application.OpenURL("https://gorillatag.fandom.com/wiki/LIV_Camera");
		}
		if (GUI.Button(new Rect(24f, 72f, 320f, 40f), "BACK TO SETTINGS", _secondaryButtonStyle))
		{
			_currentUIMode = UIMode.Settings;
		}
		GUI.Label(new Rect(24f, 136f, 320f, 48f), "MOVEMENT");
		KeysGroup(_movementKeysW, new Vector2(24f, 200f), "FORW");
		KeysGroup(_movementKeysS, new Vector2(188f, 200f), "BACK");
		KeysGroup(_movementKeysA, new Vector2(24f, 316f), "LEFT");
		KeysGroup(_movementKeysD, new Vector2(188f, 316f), "RIGHT");
		KeysGroup(_movementKeysQ, new Vector2(24f, 432f), "UP");
		KeysGroup(_movementKeysE, new Vector2(188f, 432f), "DOWN");
		GUI.Label(new Rect(24f, 564f, 320f, 48f), "SPACE", _keyStyleSpace);
		GUI.Label(new Rect(188f, 556f, 320f, 48f), "SPEED UP");
		GUI.Label(new Rect(24f, 628f, 320f, 48f), "ROTATION");
		KeysGroup(_rotationKeysLeft, new Vector2(24f, 692f), "LEFT");
		KeysGroup(_rotationKeysRight, new Vector2(188f, 692f), "RIGHT");
		KeysGroup(_rotationKeysUp, new Vector2(24f, 808f), "UP");
		KeysGroup(_rotationKeysDown, new Vector2(188f, 808f), "DOWN");
	}

	private void ToggleDroneMode()
	{
		_model.IsDroneModeActive = !_model.IsDroneModeActive;
	}

	private string GetRecordingButtonText(RecordingState state)
	{
		return state switch
		{
			RecordingState.Idle => "RECORD", 
			RecordingState.Recording => _model.DroneRecordingStateData.FormattedDuration, 
			RecordingState.Saving => "SAVING", 
			_ => "UNKNOWN", 
		};
	}

	private bool ToggleUI(bool value, float yOffset, string label)
	{
		value = GUI.Toggle(new Rect(24f, yOffset, 320f, 48f), value, label);
		return value;
	}

	private float StepperUI(float value, float step, float yOffset, string label, float min, float max)
	{
		GUI.BeginGroup(new Rect(24f, yOffset, 320f, 74f));
		GUI.Label(new Rect(0f, 0f, 320f, 32f), label, _labelAlignLeft);
		GUI.Label(new Rect(0f, 0f, 160f, 32f), value.ToString("F1"), _labelAlignRight);
		if (GUI.Button(new Rect(0f, 34f, 156f, 40f), "-", _stepperSubButtonStyle))
		{
			value -= step;
			value = Mathf.Max(value, min);
		}
		if (GUI.Button(new Rect(164f, 34f, 156f, 40f), "+", _stepperSubButtonStyle))
		{
			value += step;
			value = Mathf.Min(value, max);
		}
		GUI.EndGroup();
		return value;
	}

	private void Key(KeyData data, Vector2 position)
	{
		if (data.Show)
		{
			GUI.Label(new Rect(position.x, position.y, 24f, 24f), data.Letter, data.IsActive ? _keyStyleActive : _keyStyleNormal);
		}
	}

	private void KeysGroup(KeyData[] keys, Vector2 position, string label)
	{
		GUI.BeginGroup(new Rect(position.x, position.y, 80f, 100f));
		Key(keys[0], new Vector2(0f, 0f));
		Key(keys[1], new Vector2(28f, 0f));
		Key(keys[2], new Vector2(56f, 0f));
		Key(keys[3], new Vector2(0f, 28f));
		Key(keys[4], new Vector2(28f, 28f));
		Key(keys[5], new Vector2(56f, 28f));
		GUI.Label(new Rect(0f, 56f, 80f, 48f), label);
		GUI.EndGroup();
	}
}
