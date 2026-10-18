using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckSlider : MonoBehaviour
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private Slider _slider;

	[SerializeField]
	private float _defaultValue;

	[SerializeField]
	private float _minValue;

	[SerializeField]
	private float _maxValue = 1f;

	[SerializeField]
	private bool _isInt;

	[SerializeField]
	private int _precision = 2;

	[SerializeField]
	private float _valueMultiplier = 1f;

	[SerializeField]
	private TextMeshProUGUI _valueText;

	[SerializeField]
	private TextMeshProUGUI _typeText;

	public float Value => GetValue();

	public event Action<float> OnValueChanged;

	private void Start()
	{
		_slider.value = _defaultValue;
		_slider.minValue = _minValue;
		_slider.maxValue = _maxValue;
		_slider.wholeNumbers = _isInt;
		_slider.onValueChanged.AddListener(ChangeValue);
		_valueText.text = _slider.value.ToString();
		_typeText.text = _name;
	}

	private void OnValidate()
	{
		if ((bool)_typeText)
		{
			_typeText.text = _name;
		}
		if ((bool)_slider)
		{
			_slider.minValue = _minValue;
			_slider.maxValue = _maxValue;
			_slider.wholeNumbers = _isInt;
			_slider.value = _defaultValue;
		}
		UpdateValueText();
	}

	private void UpdateValueText()
	{
		if ((bool)_slider && (bool)_valueText)
		{
			if (_isInt)
			{
				_valueText.text = ((int)GetValue()).ToString();
			}
			else
			{
				_valueText.text = GetValue().ToString($"N{_precision}");
			}
		}
	}

	private float GetValue()
	{
		return _slider.value * _valueMultiplier;
	}

	public void ChangeValue(float value)
	{
		UpdateValueText();
		this.OnValueChanged?.Invoke(value);
	}
}
