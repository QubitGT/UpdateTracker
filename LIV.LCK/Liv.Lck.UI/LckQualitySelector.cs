using System;
using System.Collections.Generic;
using Liv.Lck.DependencyInjection;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.UI;

public class LckQualitySelector : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	private QualityOption _currentQualityOption;

	private int _currentQualityIndex;

	private List<QualityOption> _qualityOptions = new List<QualityOption>();

	[Obsolete("Only provides recording parameters for the selected quality option, and does not affect  streaming - Use OnQualityOptionChanged instead")]
	public Action<CameraTrackDescriptor> OnQualityOptionSelected;

	public Action<QualityOption> OnQualityOptionChanged;

	[SerializeField]
	private UnityEvent<string> _onQualityOptionChanged;

	[SerializeField]
	private UnityEvent<bool> _onSetQualityButtonIsDisabledState;

	public void InitializeOptions(List<QualityOption> qualityOptions)
	{
		_qualityOptions = qualityOptions;
		int num = _qualityOptions.FindIndex((QualityOption x) => x.IsDefault);
		if (num != -1)
		{
			_currentQualityIndex = num;
		}
		else
		{
			_currentQualityIndex = 0;
		}
		UpdateCurrentTrackDescriptor(_currentQualityIndex);
	}

	public void GoToNextOption()
	{
		if (_currentQualityIndex == _qualityOptions.Count - 1)
		{
			_currentQualityIndex = 0;
		}
		else
		{
			_currentQualityIndex++;
		}
		UpdateCurrentTrackDescriptor(_currentQualityIndex);
	}

	private void UpdateCurrentTrackDescriptor(int index)
	{
		if (_qualityOptions.Count > index)
		{
			_currentQualityOption = _qualityOptions[_currentQualityIndex];
			OnQualityOptionSelected?.Invoke(_currentQualityOption.RecordingCameraTrackDescriptor);
			OnQualityOptionChanged?.Invoke(_currentQualityOption);
			_onQualityOptionChanged.Invoke(_currentQualityOption.Name);
		}
	}

	public void SetQualityButtonIsDisabledState(bool state)
	{
		_onSetQualityButtonIsDisabledState.Invoke(state);
	}
}
