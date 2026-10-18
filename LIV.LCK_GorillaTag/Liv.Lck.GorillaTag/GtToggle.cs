using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtToggle : MonoBehaviour
{
	[SerializeField]
	private bool _isFirstSelected = true;

	[SerializeField]
	private string _name;

	[SerializeField]
	private string _firstLabelValue;

	[SerializeField]
	private string _secondLabelValue;

	[SerializeField]
	private GtUiSettings _settings;

	[Space(10f)]
	[Header("Elements")]
	[SerializeField]
	private TextMeshPro _label;

	[SerializeField]
	private SpriteRenderer _firstButtonRenderer;

	[SerializeField]
	private SpriteRenderer _secondButtonRenderer;

	[SerializeField]
	private TextMeshPro _firstButtonLabel;

	[SerializeField]
	private TextMeshPro _secondButtonLabel;

	[SerializeField]
	private Transform _visualsTrans;

	[Space(10f)]
	[Header("Sounds")]
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	[Space(10f)]
	[Header("Events")]
	public UnityEvent<bool> onValueChanged = new UnityEvent<bool>();

	public bool IsFirstSelected
	{
		get
		{
			return _isFirstSelected;
		}
		set
		{
			_isFirstSelected = value;
			UpdateToggle(_isFirstSelected);
			onValueChanged.Invoke(_isFirstSelected);
		}
	}

	private void OnValidate()
	{
		SetUp();
	}

	private void Start()
	{
		SetUp();
	}

	public void FirstButtonPressed()
	{
		IsFirstSelected = true;
		_visualsTrans.localRotation = Quaternion.Euler(0f, _settings.CounterAngleOffset, 0f);
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
	}

	public void SecondButtonPressed()
	{
		IsFirstSelected = false;
		_visualsTrans.localRotation = Quaternion.Euler(0f, 0f - _settings.CounterAngleOffset, 0f);
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
	}

	public void TapEnded()
	{
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
	}

	public void Reset()
	{
		_visualsTrans.localRotation = Quaternion.Euler(0f, 0f, 0f);
	}

	private void UpdateToggle(bool isFirstSelected)
	{
		_firstButtonRenderer.color = (isFirstSelected ? _settings.PrimaryCounterButtonActiveColor : _settings.PrimaryCounterButtonDefaultColor);
		_secondButtonRenderer.color = (isFirstSelected ? _settings.PrimaryCounterButtonDefaultColor : _settings.PrimaryCounterButtonActiveColor);
	}

	private void SetUp()
	{
		_label.text = _name.ToUpper();
		UpdateToggle(_isFirstSelected);
		_firstButtonLabel.text = _firstLabelValue.ToUpper();
		_secondButtonLabel.text = _secondLabelValue.ToUpper();
	}
}
