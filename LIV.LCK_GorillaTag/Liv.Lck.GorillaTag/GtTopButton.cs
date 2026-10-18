using System;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtTopButton : MonoBehaviour
{
	private enum TopButtonState
	{
		Default,
		Selected,
		Disabled
	}

	[Header("Global Settings")]
	[SerializeField]
	private GtUiSettings _settings;

	[Space(10f)]
	[Header("Parameters")]
	[SerializeField]
	private TopButtonState _currentState;

	[SerializeField]
	private TopButtonState _previousState;

	[Header("Elements")]
	[SerializeField]
	private MeshRenderer _bodyRenderer;

	[SerializeField]
	private Transform _visualsTrans;

	[SerializeField]
	private LckDiscreetAudioController _audioController;

	[Space(10f)]
	[Header("Events")]
	public UnityEvent OnTap;

	private Vector3 _defaultLocalPosition = Vector3.zero;

	private Vector3 _disabledOrPressedLocalPosition = new Vector3(0f, -0.045f, 0f);

	public void TapStarted()
	{
		if (_currentState != TopButtonState.Disabled)
		{
			_visualsTrans.localPosition = _disabledOrPressedLocalPosition;
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			EvaluateState(TopButtonState.Selected);
			OnTap.Invoke();
		}
	}

	public void TapEnded()
	{
		if (_currentState != TopButtonState.Disabled)
		{
			_visualsTrans.localPosition = _defaultLocalPosition;
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
		}
	}

	private void EvaluateState(TopButtonState state)
	{
		switch (state)
		{
		case TopButtonState.Default:
			SetDefaultState();
			break;
		case TopButtonState.Selected:
			SetSelectedState();
			break;
		case TopButtonState.Disabled:
			SetDisabledState();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetDefaultState()
	{
		_bodyRenderer.material = _settings.DefaultBodyMaterial;
		_currentState = TopButtonState.Default;
	}

	public void SetSelectedState()
	{
		_bodyRenderer.material = _settings.SelectedBodyMaterial;
		_currentState = TopButtonState.Selected;
	}

	public void SetDisabledState()
	{
		if (_currentState != TopButtonState.Disabled)
		{
			_visualsTrans.localPosition = _disabledOrPressedLocalPosition;
			_previousState = _currentState;
			_currentState = TopButtonState.Disabled;
		}
	}

	public void RestoreButtonState()
	{
		if (_previousState == TopButtonState.Selected)
		{
			SetSelectedState();
			_visualsTrans.localPosition = _defaultLocalPosition;
		}
		else if (_previousState == TopButtonState.Default)
		{
			SetDefaultState();
			_visualsTrans.localPosition = _defaultLocalPosition;
		}
	}

	private void OnValidate()
	{
		if (_currentState == TopButtonState.Default)
		{
			SetDefaultState();
			_visualsTrans.localPosition = _defaultLocalPosition;
		}
		else if (_currentState == TopButtonState.Selected)
		{
			SetSelectedState();
			_visualsTrans.localPosition = _defaultLocalPosition;
		}
		else if (_currentState == TopButtonState.Disabled)
		{
			SetDisabledState();
		}
	}
}
