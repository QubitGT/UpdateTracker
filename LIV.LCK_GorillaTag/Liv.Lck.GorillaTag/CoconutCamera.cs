using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class CoconutCamera : MonoBehaviour, IGtCameraVisuals
{
	[SerializeField]
	private GameObject _visuals;

	[SerializeField]
	private MeshRenderer _bodyRenderer;

	[SerializeField]
	private bool _isNetworkedVersion;

	private MaterialPropertyBlock _propertyBlock;

	private string IS_RECORDING = "_Is_Recording";

	private int _isRecordingID;

	private bool _isRecording;

	private void Awake()
	{
		_propertyBlock = new MaterialPropertyBlock();
		_isRecordingID = Shader.PropertyToID(IS_RECORDING);
	}

	public void SetVisualsActive(bool active)
	{
		if (!_isNetworkedVersion)
		{
			_visuals.SetActive(active);
			SetRecordingState(_isRecording);
		}
	}

	public void SetNetworkedVisualsActive(bool active)
	{
		_visuals.SetActive(active);
		SetRecordingState(_isRecording);
	}

	public void SetRecordingState(bool isRecording)
	{
		if (_propertyBlock == null)
		{
			_propertyBlock = new MaterialPropertyBlock();
			_isRecordingID = Shader.PropertyToID(IS_RECORDING);
		}
		_propertyBlock.SetInt(_isRecordingID, isRecording ? 1 : 0);
		if (_visuals.gameObject.activeInHierarchy && !_bodyRenderer.gameObject.activeInHierarchy)
		{
			Transform[] componentsInChildren = _visuals.transform.GetComponentsInChildren<Transform>(includeInactive: false);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name == "Body")
				{
					_bodyRenderer = transform.GetComponent<MeshRenderer>();
					if (_bodyRenderer != null)
					{
						break;
					}
				}
			}
		}
		_bodyRenderer.SetPropertyBlock(_propertyBlock);
		_isRecording = isRecording;
	}
}
