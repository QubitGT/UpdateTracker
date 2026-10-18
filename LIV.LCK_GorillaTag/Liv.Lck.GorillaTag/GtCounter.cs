using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtCounter : MonoBehaviour
{
	[SerializeField]
	private GtUiSettings _settings;

	[Space(10f)]
	[SerializeField]
	private string _name;

	[SerializeField]
	private int _value;

	[SerializeField]
	private int _step;

	[SerializeField]
	private int _minValue;

	[SerializeField]
	private int _maxValue;

	[SerializeField]
	private bool _showOffInsteadOfZero;

	[SerializeField]
	private bool _showMaxInsteadOfNumber;

	[SerializeField]
	private TextMeshPro _label;

	[SerializeField]
	private TextMeshPro _valueLabel;

	[SerializeField]
	private SpriteRenderer _decrementButtonRenderer;

	[SerializeField]
	private SpriteRenderer _incrementButtonRenderer;

	[SerializeField]
	private SpriteRenderer _minusRenderer;

	[SerializeField]
	private SpriteRenderer _plusRenderer;

	[SerializeField]
	private Transform _visualsTrans;

	[SerializeField]
	private LckDiscreetAudioController _audioController;

	public UnityEvent<int> onValueChanged = new UnityEvent<int>();

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = ((value <= _minValue) ? _minValue : value);
			_value = ((_value >= _maxValue) ? _maxValue : _value);
			UpdateCounter(_value);
			onValueChanged.Invoke(_value);
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

	private void SetUp()
	{
		_label.text = _name.ToUpper();
		_label.color = _settings.PrimaryTextColor;
		_valueLabel.color = _settings.PrimaryTextColor;
		Value = _value;
		_decrementButtonRenderer.color = _settings.PrimaryCounterButtonDefaultColor;
		_incrementButtonRenderer.color = _settings.PrimaryCounterButtonDefaultColor;
		UpdateCounter(_value);
	}

	public void Increase()
	{
		_visualsTrans.localRotation = Quaternion.Euler(0f, 0f - _settings.CounterAngleOffset, 0f);
		_incrementButtonRenderer.color = _settings.PrimaryCounterButtonActiveColor;
		Value += _step;
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
	}

	public void Decrease()
	{
		_visualsTrans.localRotation = Quaternion.Euler(0f, _settings.CounterAngleOffset, 0f);
		_decrementButtonRenderer.color = ((Value <= 0) ? _settings.PrimaryCounterButtonDefaultColor : _settings.PrimaryCounterButtonActiveColor);
		Value -= _step;
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
	}

	public void TapEnded()
	{
		_visualsTrans.localRotation = Quaternion.identity;
		_incrementButtonRenderer.color = _settings.PrimaryCounterButtonDefaultColor;
		_decrementButtonRenderer.color = _settings.PrimaryCounterButtonDefaultColor;
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
	}

	private void UpdateCounter(int num)
	{
		if (num <= _minValue)
		{
			_valueLabel.text = (_showOffInsteadOfZero ? "OFF" : _minValue.ToString());
			_minusRenderer.color = _settings.InactiveIconColor;
			_plusRenderer.color = _settings.PrimaryIconColor;
		}
		else if (num >= _maxValue)
		{
			_valueLabel.text = (_showMaxInsteadOfNumber ? "MAX" : _maxValue.ToString());
			_plusRenderer.color = _settings.InactiveIconColor;
			_minusRenderer.color = _settings.PrimaryIconColor;
		}
		else
		{
			_valueLabel.text = num.ToString();
			_minusRenderer.color = _settings.PrimaryIconColor;
			_plusRenderer.color = _settings.PrimaryIconColor;
		}
	}
}
