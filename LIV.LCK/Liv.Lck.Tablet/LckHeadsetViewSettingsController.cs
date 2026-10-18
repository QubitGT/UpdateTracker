using Liv.Lck.UI;
using UnityEngine;

namespace Liv.Lck.Tablet;

public class LckHeadsetViewSettingsController : MonoBehaviour
{
	[SerializeField]
	private LckHeadsetCamera _headsetCamera;

	[Header("Choice Buttons")]
	[SerializeField]
	private LckChoiceButton _eyeChoice;

	[SerializeField]
	private LckChoiceButton _cropModeChoice;

	private void OnEnable()
	{
		if (_eyeChoice != null)
		{
			_eyeChoice.OnSelectionChanged += OnEyeChanged;
		}
		if (_cropModeChoice != null)
		{
			_cropModeChoice.OnSelectionChanged += OnCropModeChanged;
		}
		SyncVisuals();
	}

	private void OnDisable()
	{
		if (_eyeChoice != null)
		{
			_eyeChoice.OnSelectionChanged -= OnEyeChanged;
		}
		if (_cropModeChoice != null)
		{
			_cropModeChoice.OnSelectionChanged -= OnCropModeChanged;
		}
	}

	private void OnEyeChanged(int index)
	{
		if (!(_headsetCamera == null))
		{
			_headsetCamera.Eye = ((index != 0) ? EyeSelection.Right : EyeSelection.Left);
		}
	}

	private void OnCropModeChanged(int index)
	{
		if (!(_headsetCamera == null))
		{
			_headsetCamera.CropMode = ((index != 0) ? HeadsetCropMode.ZoomFill : HeadsetCropMode.Fit);
		}
	}

	private void SyncVisuals()
	{
		if (!(_headsetCamera == null))
		{
			_eyeChoice?.SetSelectedIndex((_headsetCamera.Eye != 0) ? 1 : 0);
			_cropModeChoice?.SetSelectedIndex((_headsetCamera.CropMode != 0) ? 1 : 0);
		}
	}
}
