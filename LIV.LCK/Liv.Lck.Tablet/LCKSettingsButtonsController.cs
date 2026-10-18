using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Liv.Lck.Tablet;

public class LCKSettingsButtonsController : MonoBehaviour
{
	[Header("Camera Mode Settings Groups")]
	[SerializeField]
	private GameObject _selfieSettings;

	[SerializeField]
	private GameObject _firstPersonSettings;

	[SerializeField]
	private GameObject _thirdPersonSettings;

	[SerializeField]
	private GameObject _headsetViewSettings;

	[Header("Toggle References")]
	[SerializeField]
	private ToggleGroup _toggleGroup;

	[SerializeField]
	private Toggle _selfieToggle;

	[SerializeField]
	private Toggle _firstPersonToggle;

	[SerializeField]
	private Toggle _thirdPersonToggle;

	[SerializeField]
	private Toggle _headsetViewToggle;

	private Dictionary<CameraMode, GameObject> _settingsDictionary;

	public Action<CameraMode> OnCameraModeChanged { get; set; }

	private void Awake()
	{
		_settingsDictionary = new Dictionary<CameraMode, GameObject>
		{
			{
				CameraMode.Selfie,
				_selfieSettings
			},
			{
				CameraMode.FirstPerson,
				_firstPersonSettings
			},
			{
				CameraMode.ThirdPerson,
				_thirdPersonSettings
			},
			{
				CameraMode.Headset,
				_headsetViewSettings
			}
		};
	}

	private void OnEnable()
	{
		_selfieToggle.group = _toggleGroup;
		_firstPersonToggle.group = _toggleGroup;
		_thirdPersonToggle.group = _toggleGroup;
		_headsetViewToggle.group = _toggleGroup;
	}

	public void SwitchCameraModes(CameraMode mode)
	{
		foreach (KeyValuePair<CameraMode, GameObject> item in _settingsDictionary)
		{
			if (item.Key.Equals(mode))
			{
				item.Value.SetActive(value: true);
				OnCameraModeChanged?.Invoke(mode);
			}
			else
			{
				item.Value.SetActive(value: false);
			}
		}
	}
}
