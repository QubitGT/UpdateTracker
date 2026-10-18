using Liv.Lck.GorillaTag;
using UnityEngine;

namespace Liv.Lck.Tablet;

public class GtTopButtonsHelper : MonoBehaviour, ILckTopButtons
{
	[SerializeField]
	private GtTopButton _cameraButton;

	[SerializeField]
	private GtTopButton _streamButton;

	[SerializeField]
	private GtTopButton _echoButton;

	private void Start()
	{
		_cameraButton.OnTap.AddListener(delegate
		{
			SelectButton(_cameraButton);
		});
		_streamButton.OnTap.AddListener(delegate
		{
			SelectButton(_streamButton);
		});
		_echoButton.OnTap.AddListener(delegate
		{
			SelectButton(_echoButton);
		});
	}

	private void SelectButton(GtTopButton selected)
	{
		if (_cameraButton != selected)
		{
			_cameraButton.SetDefaultState();
		}
		if (_streamButton != selected)
		{
			_streamButton.SetDefaultState();
		}
		if (_echoButton != selected)
		{
			_echoButton.SetDefaultState();
		}
	}

	public void HideButtons()
	{
		_cameraButton.SetDisabledState();
		_streamButton.SetDisabledState();
		_echoButton.SetDisabledState();
	}

	public void ShowButtons()
	{
		_cameraButton.RestoreButtonState();
		_streamButton.RestoreButtonState();
		_echoButton.RestoreButtonState();
	}

	public void SetCameraPageVisualsManually()
	{
		_cameraButton.SetSelectedState();
		_streamButton.SetDefaultState();
		_echoButton.SetDefaultState();
	}
}
