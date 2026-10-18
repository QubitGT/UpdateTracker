using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtSelectorsGroup : MonoBehaviour
{
	[SerializeField]
	private List<GtSelector> _selectors;

	public UnityEvent<CameraMode> onCameraModeChanged;

	private CameraMode _currentMode;

	public CameraMode CurrentMode
	{
		get
		{
			return _currentMode;
		}
		set
		{
			_currentMode = value;
			onCameraModeChanged.Invoke(_currentMode);
		}
	}

	public void Select(CameraMode mode)
	{
		_currentMode = mode;
		onCameraModeChanged.Invoke(_currentMode);
	}

	private void Awake()
	{
		foreach (GtSelector selector in _selectors)
		{
			selector.onCameraModeUpdate.AddListener(UpdateCurrentMode);
		}
	}

	private void Start()
	{
		foreach (GtSelector selector in _selectors)
		{
			onCameraModeChanged.AddListener(selector.ListenToCameraModeChanged);
		}
		onCameraModeChanged.Invoke(_currentMode);
	}

	private void OnDestroy()
	{
		foreach (GtSelector selector in _selectors)
		{
			onCameraModeChanged.RemoveListener(selector.ListenToCameraModeChanged);
		}
	}

	private void UpdateCurrentMode(CameraMode mode)
	{
		CurrentMode = mode;
	}
}
