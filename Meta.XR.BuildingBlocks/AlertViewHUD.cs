using UnityEngine;
using UnityEngine.UI;

public class AlertViewHUD : MonoBehaviour
{
	public enum MessageType
	{
		Info,
		Warning,
		Error
	}

	[Tooltip("Set -1 to show always.")]
	[SerializeField]
	internal int _hideAfterSec = 20;

	[SerializeField]
	internal bool _centerInCamera = true;

	[SerializeField]
	private GameObject _panel;

	[SerializeField]
	private Sprite _warningIcon;

	[SerializeField]
	private Sprite _errorIcon;

	[SerializeField]
	private Sprite _infoIcon;

	[SerializeField]
	private Text _messageTextField;

	[SerializeField]
	private Text _messageTypeTextField;

	[SerializeField]
	private Image _messageTypeIconField;

	private Transform _centerEyeTransform;

	private float _initialTime;

	private Vector3 _initialPosition;

	private Quaternion _initialRotation;

	private float _speed = 7f;

	private static AlertViewHUD Instance { get; set; }

	public int HideAfterSec
	{
		get
		{
			return _hideAfterSec;
		}
		set
		{
			_hideAfterSec = value;
		}
	}

	public bool CenterInCamera
	{
		get
		{
			return _centerInCamera;
		}
		set
		{
			_centerInCamera = value;
		}
	}

	private bool Hidden => !_panel.activeSelf;

	private void Awake()
	{
		Instance = this;
		_centerEyeTransform = Object.FindObjectOfType<OVRCameraRig>()?.centerEyeAnchor;
		_initialTime = Time.time;
		_initialPosition = base.transform.position;
		_initialRotation = base.transform.rotation;
		Hide();
	}

	public static void PostMessage(string message, MessageType messageType = MessageType.Warning)
	{
		if (!(Instance == null))
		{
			Instance.Post(message, messageType);
		}
	}

	private void Post(string message, MessageType type)
	{
		switch (type)
		{
		case MessageType.Info:
			_messageTypeIconField.sprite = _infoIcon;
			_messageTypeTextField.text = "Info";
			break;
		case MessageType.Warning:
			_messageTypeIconField.sprite = _warningIcon;
			_messageTypeTextField.text = "Warning";
			break;
		case MessageType.Error:
			_messageTypeIconField.sprite = _errorIcon;
			_messageTypeTextField.text = "Error";
			break;
		}
		_messageTextField.text = message + "\n";
		Reset();
	}

	private void ClearMessage()
	{
		_messageTextField.text = "";
	}

	private void Update()
	{
		CalculateHideAfterMessage();
		FollowCamera();
	}

	private void CalculateHideAfterMessage()
	{
		if (HideAfterSec != -1 && !Hidden && Time.time - _initialTime >= (float)HideAfterSec)
		{
			Hide();
		}
	}

	private void Reset()
	{
		_initialTime = Time.time;
		_panel.SetActive(value: true);
	}

	private void Hide()
	{
		_panel.SetActive(value: false);
	}

	private void FollowCamera()
	{
		if (!(_centerEyeTransform == null) && !Hidden && CenterInCamera)
		{
			Vector3 b = _centerEyeTransform.TransformPoint(_initialPosition);
			Quaternion b2 = _centerEyeTransform.rotation * _initialRotation;
			Vector3 position = Vector3.Lerp(base.transform.position, b, Time.deltaTime * _speed);
			Quaternion rotation = Quaternion.Lerp(base.transform.rotation, b2, Time.deltaTime * _speed);
			base.transform.SetPositionAndRotation(position, rotation);
		}
	}
}
