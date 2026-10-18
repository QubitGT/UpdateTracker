using Liv.Lck.DependencyInjection;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

[DefaultExecutionOrder(100)]
public class GtCoconutCameraController : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[SerializeField]
	private CoconutCamera _cocoCamera;

	[SerializeField]
	private bool _hideOnStart = true;

	private void OnEnable()
	{
		if (_lckService == null)
		{
			Debug.LogError("LckService is null");
			return;
		}
		_lckService.OnRecordingStarted += OnRecordingStarted;
		_lckService.OnRecordingStopped += OnRecordingStopped;
	}

	private void Start()
	{
		_cocoCamera.SetVisualsActive(!_hideOnStart);
	}

	private void OnDisable()
	{
		if (_lckService != null)
		{
			_lckService.OnRecordingStarted -= OnRecordingStarted;
			_lckService.OnRecordingStopped -= OnRecordingStopped;
		}
	}

	private void OnRecordingStarted(LckResult result)
	{
		_cocoCamera.SetRecordingState(isRecording: true);
	}

	private void OnRecordingStopped(LckResult result)
	{
		_cocoCamera.SetRecordingState(isRecording: false);
	}
}
