using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[AddComponentMenu("GorillaTag/KID/Age Slider With Progress Bar")]
public class AgeSliderWithProgressBar : MonoBehaviourTick
{
	[Serializable]
	public class SliderHeldEvent : UnityEvent<int>
	{
	}

	private const int MIN_AGE = 13;

	[SerializeField]
	private SliderHeldEvent m_OnHoldComplete = new SliderHeldEvent();

	[SerializeField]
	private bool _adjustAge;

	[SerializeField]
	private int _maxAge = 25;

	[SerializeField]
	private TMP_Text _ageValueTxt;

	[Tooltip("Optional game object that should hold the Progress Bar Fill. Disables Hold functionality if null.")]
	[SerializeField]
	private GameObject _progressBarContainer;

	[SerializeField]
	private float holdTime = 2.5f;

	[SerializeField]
	private Image progressBarFill;

	[SerializeField]
	private TMP_Text _messageText;

	[SerializeField]
	private float _stickVibrationStrength = 0.1f;

	[SerializeField]
	private float _stickVibrationDuration = 0.05f;

	[SerializeField]
	private KIDUIButton _confirmButton;

	[Header("Thumbstick")]
	[Tooltip("Stick X deflection needed to register a left/right press.")]
	[SerializeField]
	[Range(0.1f, 1f)]
	private float _stickPressThreshold = 0.6f;

	[Tooltip("Stick X deflection below which a held press is released (hysteresis, keep below the press threshold).")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _stickReleaseThreshold = 0.35f;

	[Tooltip("Seconds a direction must be held before the age starts repeating.")]
	[SerializeField]
	private float _stickRepeatDelay = 0.4f;

	[Tooltip("Seconds between repeats while the stick stays held.")]
	[SerializeField]
	private float _stickRepeatInterval = 0.12f;

	private int _heldStickDirection;

	private float _nextStickRepeatTime;

	private bool _ageSlidable = true;

	private bool _incrementButtonsLockingSlider;

	private bool controllerActive;

	[SerializeField]
	private string _lockMessage;

	private string _originalText;

	private int _currentAge;

	private float progress;

	public SliderHeldEvent onHoldComplete
	{
		get
		{
			return m_OnHoldComplete;
		}
		set
		{
			m_OnHoldComplete = value;
		}
	}

	public bool AdjustAge => _adjustAge;

	public bool ControllerActive
	{
		get
		{
			return controllerActive;
		}
		set
		{
			if (value)
			{
				ControllerBehaviour.Instance.OnAction += PostUpdate;
			}
			else
			{
				ControllerBehaviour.Instance.OnAction -= PostUpdate;
			}
			controllerActive = value;
		}
	}

	public string LockMessage
	{
		get
		{
			return _lockMessage;
		}
		set
		{
			_lockMessage = value;
		}
	}

	public int CurrentAge => _currentAge;

	private void Awake()
	{
		if ((bool)_messageText)
		{
			_originalText = _messageText.text;
		}
	}

	public void SetOriginalText(string text)
	{
		_originalText = text;
	}

	private new void OnEnable()
	{
		base.OnEnable();
		if (_progressBarContainer != null && progressBarFill != null)
		{
			progressBarFill.rectTransform.localScale = new Vector3(0f, 1f, 1f);
		}
		if ((bool)_ageValueTxt)
		{
			_ageValueTxt.text = ((_currentAge > 0) ? _currentAge.ToString() : "?");
		}
	}

	public override void Tick()
	{
		if (!ControllerActive)
		{
			_heldStickDirection = 0;
			return;
		}
		HandleStickInput();
		RefreshAgeDisplay();
		if (!_progressBarContainer)
		{
			return;
		}
		if (!_lockMessage.IsNullOrEmpty())
		{
			progress = 0f;
			if ((bool)_messageText)
			{
				_messageText.text = LockMessage;
			}
		}
		else
		{
			if ((bool)_messageText)
			{
				_messageText.text = _originalText;
			}
			if ((double)progress == 1.0)
			{
				m_OnHoldComplete.Invoke(_currentAge);
				progress = 0f;
			}
			if (ControllerBehaviour.Instance.ButtonDown && _progressBarContainer != null && (_currentAge > 0 || !AdjustAge))
			{
				progress += Time.deltaTime / holdTime;
				progress = Mathf.Clamp01(progress);
			}
			else
			{
				progress = 0f;
			}
		}
		if (_progressBarContainer != null)
		{
			progressBarFill.rectTransform.localScale = new Vector3(progress, 1f, 1f);
		}
	}

	private void PostUpdate()
	{
		RefreshAgeDisplay();
	}

	private void HandleStickInput()
	{
		if (!_ageValueTxt || !_ageSlidable || _incrementButtonsLockingSlider || !ControllerBehaviour.Instance)
		{
			_heldStickDirection = 0;
			return;
		}
		float dominantStickX = ControllerBehaviour.Instance.DominantStickX;
		int num = _heldStickDirection;
		if (num == 0)
		{
			if (dominantStickX <= 0f - _stickPressThreshold)
			{
				num = -1;
			}
			else if (dominantStickX >= _stickPressThreshold)
			{
				num = 1;
			}
		}
		else if (Mathf.Abs(dominantStickX) < _stickReleaseThreshold || (int)Mathf.Sign(dominantStickX) != num)
		{
			num = 0;
		}
		if (num == 0)
		{
			_heldStickDirection = 0;
		}
		else if (num != _heldStickDirection)
		{
			_heldStickDirection = num;
			StepAge(num);
			_nextStickRepeatTime = Time.time + _stickRepeatDelay;
		}
		else if (Time.time >= _nextStickRepeatTime)
		{
			StepAge(num);
			_nextStickRepeatTime = Time.time + _stickRepeatInterval;
		}
	}

	private void StepAge(int direction)
	{
		int currentAge = _currentAge;
		_currentAge = Mathf.Clamp(_currentAge + direction, 0, _maxAge);
		if (_currentAge != currentAge && _currentAge > 0 && _currentAge < _maxAge)
		{
			HandRayController.Instance?.PulseActiveHandray(_stickVibrationStrength, _stickVibrationDuration);
		}
	}

	private void RefreshAgeDisplay()
	{
		if ((bool)_ageValueTxt)
		{
			string ageString = GetAgeString();
			if (_ageValueTxt.text != ageString)
			{
				_ageValueTxt.text = ageString;
			}
			if (_progressBarContainer != null)
			{
				_progressBarContainer.SetActive(_currentAge > 0);
			}
		}
	}

	public void EnableEditing()
	{
		_ageSlidable = true;
	}

	public void DisableEditing()
	{
		_ageSlidable = false;
	}

	public string GetAgeString()
	{
		if ((bool)_confirmButton)
		{
			_confirmButton.interactable = true;
		}
		if (_currentAge == 0)
		{
			if ((bool)_confirmButton)
			{
				_confirmButton.interactable = false;
			}
			return "?";
		}
		if (_currentAge == _maxAge)
		{
			return _maxAge + "+";
		}
		return _currentAge.ToString();
	}

	public void ForceAddAge(int number)
	{
		_incrementButtonsLockingSlider = true;
		_currentAge = Math.Min(_currentAge + number, _maxAge);
	}

	public void ForceSubtractAge(int number)
	{
		_incrementButtonsLockingSlider = true;
		_currentAge = Math.Max(_currentAge - number, 1);
	}
}
