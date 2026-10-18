using Liv.Lck.UI;
using UnityEngine;

namespace Liv.Lck.Tablet;

public class LckTopButtonsHelper : MonoBehaviour, ILckTopButtons
{
	[SerializeField]
	private LckToggle _cameraToggle;

	[SerializeField]
	private LckToggle _streamToggle;

	[SerializeField]
	private LckToggle _echoToggle;

	public void HideButtons()
	{
		_cameraToggle.SetDisabledState(usePressedPosition: true);
		_streamToggle.SetDisabledState(usePressedPosition: true);
		_echoToggle.SetDisabledState(usePressedPosition: true);
	}

	public void ShowButtons()
	{
		_cameraToggle.RestoreToggleState();
		_streamToggle.RestoreToggleState();
		_echoToggle.RestoreToggleState();
	}

	public void SetCameraPageVisualsManually()
	{
		_cameraToggle.SetToggleVisualsOn();
		_streamToggle.SetToggleVisualsOff();
		_echoToggle.SetToggleVisualsOff();
	}
}
