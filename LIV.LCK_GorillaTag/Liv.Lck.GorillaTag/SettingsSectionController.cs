using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class SettingsSectionController : MonoBehaviour
{
	[SerializeField]
	private CameraMode _mode;

	[SerializeField]
	private GameObject _ui;

	public void EvaluateMode(CameraMode mode)
	{
		bool active = mode == _mode;
		_ui.SetActive(active);
	}
}
