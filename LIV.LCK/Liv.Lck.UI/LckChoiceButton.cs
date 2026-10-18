using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckChoiceButton : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private string _title;

	[SerializeField]
	private string _leftLabel;

	[SerializeField]
	private string _rightLabel;

	[SerializeField]
	private int _selectedIndex;

	[SerializeField]
	private LckButtonColors _colors;

	[SerializeField]
	private LckButtonColors _colorsSelected;

	[Header("References")]
	[SerializeField]
	private TMP_Text _titleText;

	[SerializeField]
	private TMP_Text _leftText;

	[SerializeField]
	private TMP_Text _rightText;

	[SerializeField]
	private Image _leftBackground;

	[SerializeField]
	private Image _rightBackground;

	[SerializeField]
	private LckDoubleButtonTrigger _leftTrigger;

	[SerializeField]
	private LckDoubleButtonTrigger _rightTrigger;

	[Header("Audio")]
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	private static readonly Color TextColor = Color.white;

	public int SelectedIndex
	{
		get
		{
			return _selectedIndex;
		}
		set
		{
			SetSelectedIndex(value);
		}
	}

	public event Action<int> OnSelectionChanged;

	private void OnEnable()
	{
		_leftTrigger.OnEnter += OnEnter;
		_leftTrigger.OnDown += OnPressDown;
		_leftTrigger.OnUp += OnPressUp;
		_leftTrigger.OnExit += OnExit;
		_rightTrigger.OnEnter += OnEnter;
		_rightTrigger.OnDown += OnPressDown;
		_rightTrigger.OnUp += OnPressUp;
		_rightTrigger.OnExit += OnExit;
		UpdateVisuals();
	}

	private void OnDisable()
	{
		_leftTrigger.OnEnter -= OnEnter;
		_leftTrigger.OnDown -= OnPressDown;
		_leftTrigger.OnUp -= OnPressUp;
		_leftTrigger.OnExit -= OnExit;
		_rightTrigger.OnEnter -= OnEnter;
		_rightTrigger.OnDown -= OnPressDown;
		_rightTrigger.OnUp -= OnPressUp;
		_rightTrigger.OnExit -= OnExit;
	}

	public void SelectLeft()
	{
		if (_selectedIndex != 0)
		{
			_selectedIndex = 0;
			UpdateVisuals();
			this.OnSelectionChanged?.Invoke(0);
			_audioController?.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
		}
	}

	public void SelectRight()
	{
		if (_selectedIndex != 1)
		{
			_selectedIndex = 1;
			UpdateVisuals();
			this.OnSelectionChanged?.Invoke(1);
			_audioController?.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
		}
	}

	public void SetSelectedIndex(int index)
	{
		_selectedIndex = Mathf.Clamp(index, 0, 1);
		UpdateVisuals();
	}

	private void OnEnter(bool isRight)
	{
		Image obj = (isRight ? _rightBackground : _leftBackground);
		LckButtonColors colorsForSide = GetColorsForSide(isRight);
		obj.color = colorsForSide.HighlightedColor;
		_audioController?.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.HoverSound);
	}

	private void OnPressDown(bool isRight)
	{
		Image obj = (isRight ? _rightBackground : _leftBackground);
		int num = (isRight ? 1 : 0);
		if (_selectedIndex != num)
		{
			_selectedIndex = num;
			UpdateVisuals();
			this.OnSelectionChanged?.Invoke(_selectedIndex);
		}
		obj.color = GetColorsForSide(isRight).PressedColor;
		_audioController?.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
	}

	private void OnPressUp(bool isRight, bool usingCollider)
	{
		Image obj = (isRight ? _rightBackground : _leftBackground);
		LckButtonColors colorsForSide = GetColorsForSide(isRight);
		obj.color = (usingCollider ? colorsForSide.NormalColor : colorsForSide.HighlightedColor);
		_audioController?.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
	}

	private void OnExit(bool isRight)
	{
		UpdateVisuals();
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			UpdateVisuals();
		}
	}

	private LckButtonColors GetColorsForSide(bool isRight)
	{
		if (!(isRight ? (_selectedIndex == 1) : (_selectedIndex == 0)))
		{
			return _colors;
		}
		return _colorsSelected;
	}

	private void UpdateVisuals()
	{
		if (_leftText != null)
		{
			_leftText.color = TextColor;
		}
		if (_rightText != null)
		{
			_rightText.color = TextColor;
		}
		if (_leftBackground != null)
		{
			_leftBackground.color = GetColorsForSide(isRight: false).NormalColor;
		}
		if (_rightBackground != null)
		{
			_rightBackground.color = GetColorsForSide(isRight: true).NormalColor;
		}
	}

	private void OnValidate()
	{
		if (_titleText != null)
		{
			_titleText.text = _title;
		}
		if (_leftText != null)
		{
			_leftText.text = _leftLabel;
		}
		if (_rightText != null)
		{
			_rightText.text = _rightLabel;
		}
		UpdateVisuals();
	}
}
