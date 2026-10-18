using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtDroneModeTabletUIAppearance : MonoBehaviour
{
	[Header("Tablet UI Elements")]
	[SerializeField]
	private GameObject _selectorsGroup;

	[SerializeField]
	private GameObject _orientationButton;

	[SerializeField]
	private GameObject _settingsGroup;

	[SerializeField]
	private GtDisplay _display;

	private bool _isDroneModeActive;

	public bool IsDroneModeActive
	{
		get
		{
			return _isDroneModeActive;
		}
		set
		{
			_isDroneModeActive = value;
			EvaluateMode(_isDroneModeActive);
		}
	}

	private void EvaluateMode(bool isDroneMode)
	{
		_selectorsGroup.SetActive(!isDroneMode);
		_settingsGroup.SetActive(!isDroneMode);
		if (isDroneMode)
		{
			_display.Maximize();
		}
		else
		{
			_display.Minimize();
		}
	}
}
