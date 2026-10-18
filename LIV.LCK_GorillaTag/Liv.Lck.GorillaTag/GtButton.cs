using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtButton : MonoBehaviour
{
	[SerializeField]
	private GtUiSettings _settings;

	[Space(10f)]
	[Header("Settings")]
	[SerializeField]
	private string _name;

	[SerializeField]
	private bool _doFlipping = true;

	[SerializeField]
	private ButtonInitializeType _initializeType;

	[Space(10f)]
	[Header("UI Elements")]
	[SerializeField]
	private TextMeshPro _label;

	[SerializeField]
	private MeshRenderer _bodyRenderer;

	[SerializeField]
	private Transform _visualsTrans;

	[SerializeField]
	private SpriteRenderer _iconImage;

	[Space(10f)]
	[Header("Sounds")]
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	[Space(10f)]
	[Header("Events")]
	public UnityEvent onTap;

	private Vector3 _defaultLocalPosition;

	private bool _isFlipped;

	private bool _isDisabled;

	private void Awake()
	{
		if (_initializeType == ButtonInitializeType.Awake)
		{
			InitSetUp();
		}
	}

	private void Start()
	{
		if (_initializeType == ButtonInitializeType.Start)
		{
			InitSetUp();
		}
	}

	public void SetDisabled(bool isDisabled)
	{
		_isDisabled = isDisabled;
		if ((bool)_iconImage)
		{
			_iconImage.color = (isDisabled ? _settings.InactiveIconColor : _settings.PrimaryIconColor);
		}
		if ((bool)_label)
		{
			_label.color = (isDisabled ? _settings.DisabledTextColor : _settings.PrimaryTextColor);
		}
	}

	public void TapStarted()
	{
		if (!_isDisabled)
		{
			_bodyRenderer.material = _settings.SelectedBodyMaterial;
			_visualsTrans.localPosition = _defaultLocalPosition + Vector3.forward * (0f - _settings.ActiveButtonOffset);
			FlipVisuals();
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			onTap.Invoke();
		}
	}

	public void TapEnded()
	{
		if (!_isDisabled)
		{
			_bodyRenderer.material = _settings.DefaultBodyMaterial;
			_visualsTrans.localPosition = _defaultLocalPosition;
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
		}
	}

	public void TapEndedNoAudio()
	{
		_bodyRenderer.material = _settings.DefaultBodyMaterial;
		_visualsTrans.localPosition = _defaultLocalPosition;
	}

	public void SetLabelText(string text)
	{
		_label.text = text;
	}

	private void InitSetUp()
	{
		_defaultLocalPosition = _visualsTrans.localPosition;
		_label.color = _settings.PrimaryTextColor;
		if ((bool)_iconImage)
		{
			_iconImage.color = _settings.PrimaryIconColor;
		}
		_label.text = _name.ToUpper();
	}

	private void FlipVisuals()
	{
		if (_doFlipping)
		{
			_isFlipped = !_isFlipped;
			_visualsTrans.localScale = new Vector3((!_isFlipped) ? 1 : (-1), 1f, 1f);
		}
	}
}
