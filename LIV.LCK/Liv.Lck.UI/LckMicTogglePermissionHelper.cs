using Liv.Lck.Settings;
using Liv.Lck.Tablet;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckMicTogglePermissionHelper : MonoBehaviour
{
	[SerializeField]
	private LCKCameraController _controller;

	[SerializeField]
	private LckButtonColors _noMicPermissionColors;

	[SerializeField]
	private Sprite _noMicPermissionIcon;

	[SerializeField]
	private LckToggle _micLckToggle;

	[SerializeField]
	private Toggle _micToggle;

	[SerializeField]
	private Image _micToggleIcon;

	private static int _permissionAskCount;

	private bool _hasMicPermission = true;

	private void Start()
	{
		if (Application.platform != RuntimePlatform.Android || Application.isEditor || LckSettings.Instance.MicPermissionType == LckSettings.MicPermissionAskType.NeverAskFromLck)
		{
			_controller.ToggleMicrophoneRecording(isMicOn: true);
		}
		else
		{
			if (Application.platform != RuntimePlatform.Android || Application.isEditor)
			{
				return;
			}
			if (!Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO"))
			{
				_hasMicPermission = false;
				SetMicPermissionOffVisuals();
				_controller.ToggleMicrophoneRecording(isMicOn: false);
				switch (LckSettings.Instance.MicPermissionType)
				{
				case LckSettings.MicPermissionAskType.OnAppStartup:
					_micLckToggle.SetDisabledState();
					break;
				case LckSettings.MicPermissionAskType.OnTabletSpawn:
					_micLckToggle.SetDisabledState();
					if (!UserSelectedDontShowAgain())
					{
						CheckForMicPermission();
					}
					break;
				case LckSettings.MicPermissionAskType.OnMicUnmute:
					_micToggle.onValueChanged.AddListener(CheckForMicPermission);
					break;
				}
			}
			else
			{
				_controller.ToggleMicrophoneRecording(isMicOn: true);
			}
		}
	}

	private void CheckForMicPermission(bool toggleValue = true)
	{
		if (Application.platform != RuntimePlatform.Android || Application.isEditor || Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO"))
		{
			return;
		}
		if (UserSelectedDontShowAgain())
		{
			if ((bool)_micToggle && LckSettings.Instance.MicPermissionType == LckSettings.MicPermissionAskType.OnMicUnmute)
			{
				_micLckToggle.SetDisabledState();
				LCKCameraController.ColliderButtonsInUse = false;
				_micToggle.onValueChanged.RemoveListener(CheckForMicPermission);
			}
		}
		else
		{
			_permissionAskCount++;
			PermissionCallbacks permissionCallbacks = new PermissionCallbacks();
			permissionCallbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
			permissionCallbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
			LckLog.Log("Requesting Microphone Permission", "CheckForMicPermission", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckMicTogglePermissionHelper.cs", 96);
			Permission.RequestUserPermission("android.permission.RECORD_AUDIO", permissionCallbacks);
		}
	}

	internal void PermissionCallbacks_PermissionGranted(string permissionName)
	{
		_hasMicPermission = true;
		LckLog.Log("Microphone Permission Granted", "PermissionCallbacks_PermissionGranted", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckMicTogglePermissionHelper.cs", 105);
		SetMicPermissionOnVisuals();
		_controller.ToggleMicrophoneRecording(isMicOn: true);
		_micLckToggle.RestoreToggleState();
		if ((bool)_micToggle && LckSettings.Instance.MicPermissionType == LckSettings.MicPermissionAskType.OnMicUnmute)
		{
			_micToggle.onValueChanged.RemoveListener(CheckForMicPermission);
		}
	}

	internal void PermissionCallbacks_PermissionDenied(string permissionName)
	{
		_hasMicPermission = false;
		LckLog.Log("Microphone Permission Denied", "PermissionCallbacks_PermissionDenied", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckMicTogglePermissionHelper.cs", 120);
		SetMicPermissionOffVisuals();
		_controller.ToggleMicrophoneRecording(isMicOn: false);
		if (LckSettings.Instance.MicPermissionType != LckSettings.MicPermissionAskType.OnMicUnmute)
		{
			_micLckToggle.SetDisabledState();
		}
	}

	private bool UserSelectedDontShowAgain()
	{
		if (_permissionAskCount >= 1)
		{
			return !Permission.ShouldShowRequestPermissionRationale("android.permission.RECORD_AUDIO");
		}
		return false;
	}

	public void SetMicPermissionOnVisuals()
	{
		_micLckToggle.RestoreDefaultColors();
		_micLckToggle.RestoreDefaultIcons();
		_micLckToggle.SetToggleVisualsOn();
		SetToggleIconAlpha(1f);
	}

	public void SetMicPermissionOffVisuals()
	{
		_micLckToggle.SetCustomColors(_noMicPermissionColors, _noMicPermissionColors);
		_micLckToggle.SetCustomIcons(_noMicPermissionIcon, _noMicPermissionIcon);
		SetToggleIconAlpha(0.2f);
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus && LckSettings.Instance.MicPermissionType != LckSettings.MicPermissionAskType.NeverAskFromLck)
		{
			if (!_hasMicPermission && Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO"))
			{
				LckLog.Log("User allowed mic permission from settings", "OnApplicationFocus", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckMicTogglePermissionHelper.cs", 161);
				_controller.ToggleMicrophoneRecording(isMicOn: true);
				SetMicPermissionOnVisuals();
				_micLckToggle.RestoreToggleState();
				_hasMicPermission = true;
			}
			else if (_hasMicPermission && !Permission.HasUserAuthorizedPermission("android.permission.RECORD_AUDIO"))
			{
				LckLog.Log("User disabled mic permission from settings", "OnApplicationFocus", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Tablet\\LckMicTogglePermissionHelper.cs", 169);
				_controller.ToggleMicrophoneRecording(isMicOn: false);
				SetMicPermissionOffVisuals();
				_micLckToggle.SetDisabledState();
				_hasMicPermission = false;
			}
		}
	}

	private void SetToggleIconAlpha(float alpha)
	{
		Color color = _micToggleIcon.color;
		color.a = alpha;
		_micToggleIcon.color = color;
	}
}
