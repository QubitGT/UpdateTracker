using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtTabletSocialCameraVisuals : MonoBehaviour, IGtCameraVisuals
{
	[SerializeField]
	private GameObject _visuals;

	[SerializeField]
	private GameObject _recordingIndicatorRoot;

	private bool _isRecording;

	public void SetVisualsActive(bool active)
	{
		_visuals.SetActive(active);
		SetRecordingState(_isRecording);
	}

	public void SetNetworkedVisualsActive(bool active)
	{
		_visuals.SetActive(active);
	}

	public void SetRecordingState(bool isRecording)
	{
		_recordingIndicatorRoot.SetActive(isRecording);
		_isRecording = isRecording;
	}
}
