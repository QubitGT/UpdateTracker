using System;
using Liv.Lck.Settings;
using Liv.Lck.Tablet;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckToggle : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[Header("Settings")]
	[SerializeField]
	private string _name;

	[SerializeField]
	private Sprite _icon;

	[SerializeField]
	private Sprite _iconOn;

	private Tuple<Sprite, Sprite> _defaultIcons;

	[SerializeField]
	private LckButtonColors _colors;

	[SerializeField]
	private LckButtonColors _colorsOn;

	private Tuple<LckButtonColors, LckButtonColors> _defaultColors;

	[SerializeField]
	private Vector3 _togglePressedPosition = new Vector3(0f, 0f, 40f);

	[Header("Toggle Group Settings")]
	[SerializeField]
	private bool _stayPressedDownWhenToggled;

	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI _labelText;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private RectTransform _visuals;

	[SerializeField]
	private Image _iconImage;

	[SerializeField]
	private Toggle _toggle;

	[Header("Audio")]
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	private bool _collided;

	private GameObject _clickedObject;

	private MaterialPropertyBlock _propertyBlock;

	private int _colorId;

	public bool IsDisabled { get; private set; }

	private void Awake()
	{
		_defaultColors = new Tuple<LckButtonColors, LckButtonColors>(_colors, _colorsOn);
		_defaultIcons = new Tuple<Sprite, Sprite>(_icon, _iconOn);
	}

	private void Start()
	{
		ValidateMeshColors();
		_toggle.onValueChanged.AddListener(OnToggleValueChanged);
	}

	public void OnToggleValueChanged(bool value)
	{
		ValidateIcon();
		ValidateColors();
		ValidateMeshColors();
		if (_toggle.group != null && !value)
		{
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!IsDisabled)
		{
			SetMeshColor((_toggle.isOn && (bool)_colorsOn) ? _colorsOn.HighlightedColor : _colors.HighlightedColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.HoverSound);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!IsDisabled)
		{
			_visuals.anchoredPosition3D = _togglePressedPosition;
			SetMeshColor((_toggle.isOn && (bool)_colorsOn) ? _colorsOn.PressedColor : _colors.PressedColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			if (eventData != null)
			{
				_clickedObject = eventData.pointerEnter;
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!IsDisabled)
		{
			if (_toggle.group == null || !_stayPressedDownWhenToggled)
			{
				_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
				_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
			}
			if (eventData != null && _clickedObject != eventData.pointerEnter)
			{
				_toggle.OnPointerClick(eventData);
				SetMeshColor((_toggle.isOn && (bool)_colorsOn) ? _colorsOn.NormalColor : _colors.NormalColor);
			}
			else
			{
				SetMeshColor((!_toggle.isOn && (bool)_colorsOn) ? _colorsOn.HighlightedColor : _colors.HighlightedColor);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!IsDisabled)
		{
			SetMeshColor((_toggle.isOn && (bool)_colorsOn) ? _colorsOn.NormalColor : _colors.NormalColor);
			if (_toggle.group == null || !_stayPressedDownWhenToggled)
			{
				_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IsDisabled && other.gameObject.CompareTag(LckSettings.Instance.TriggerEnterTag) && IsValidTap(other.ClosestPoint(base.transform.position)) && !LCKCameraController.ColliderButtonsInUse)
		{
			LCKCameraController.ColliderButtonsInUse = true;
			_collided = true;
			OnPointerDown(null);
			_toggle.isOn = !_toggle.isOn;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!IsDisabled && _collided)
		{
			OnPointerUp(null);
			OnPointerExit(null);
			_collided = false;
			LCKCameraController.ColliderButtonsInUse = false;
		}
	}

	private bool IsValidTap(Vector3 tapPosition)
	{
		Vector3 to = tapPosition - base.transform.position;
		return Vector3.Angle(-base.transform.forward, to) < 90f;
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			_collided = false;
			if (_stayPressedDownWhenToggled && _toggle.isOn)
			{
				_visuals.anchoredPosition3D = _togglePressedPosition;
			}
			else
			{
				_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
			}
			SetMeshColor((_toggle.isOn && (bool)_colorsOn) ? _colorsOn.NormalColor : _colors.NormalColor);
		}
	}

	public void SetDisabledState(bool usePressedPosition = false)
	{
		IsDisabled = true;
		if (usePressedPosition)
		{
			_visuals.anchoredPosition3D = _togglePressedPosition;
		}
		else
		{
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
		}
		_toggle.enabled = false;
	}

	public void RestoreToggleState()
	{
		IsDisabled = false;
		_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
		_toggle.enabled = true;
	}

	public void SetToggleVisualsOff()
	{
		_toggle.SetIsOnWithoutNotify(value: false);
		SetMeshColor(_colors.NormalColor);
		ValidateIcon();
	}

	public void SetToggleVisualsOn()
	{
		_toggle.SetIsOnWithoutNotify(value: true);
		SetMeshColor((_toggle.isOn && (bool)_colorsOn) ? _colorsOn.NormalColor : _colors.NormalColor);
		ValidateIcon();
	}

	public void SetCustomColors(LckButtonColors colors, LckButtonColors colorsOn)
	{
		_colors = colors;
		_colorsOn = colorsOn;
		ValidateMeshColors();
	}

	public void RestoreDefaultColors()
	{
		_colors = _defaultColors.Item1;
		_colorsOn = _defaultColors.Item2;
		ValidateMeshColors();
	}

	public void SetCustomIcons(Sprite icon, Sprite iconOn)
	{
		_icon = icon;
		_iconOn = iconOn;
		ValidateIcon();
	}

	public void RestoreDefaultIcons()
	{
		_icon = _defaultIcons.Item1;
		_iconOn = _defaultIcons.Item2;
		ValidateIcon();
	}

	private void ValidateColors()
	{
		if (!_colors)
		{
			return;
		}
		if (_toggle.isOn && (bool)_colorsOn)
		{
			ColorBlock colors = _toggle.colors;
			colors.normalColor = _colorsOn.NormalColor;
			colors.highlightedColor = _colorsOn.HighlightedColor;
			colors.pressedColor = _colorsOn.PressedColor;
			colors.selectedColor = _colorsOn.SelectedColor;
			colors.disabledColor = _colorsOn.DisabledColor;
			if (_toggle.colors != colors)
			{
				_toggle.colors = colors;
			}
		}
		else
		{
			ColorBlock colors2 = _toggle.colors;
			colors2.normalColor = _colors.NormalColor;
			colors2.highlightedColor = _colors.HighlightedColor;
			colors2.pressedColor = _colors.PressedColor;
			colors2.selectedColor = _colors.SelectedColor;
			colors2.disabledColor = _colors.DisabledColor;
			if (_toggle.colors != colors2)
			{
				_toggle.colors = colors2;
			}
		}
	}

	private void ValidateIcon()
	{
		if ((bool)_iconImage && (bool)_icon)
		{
			if (_toggle.isOn && _iconOn != null)
			{
				_iconImage.sprite = _iconOn;
			}
			else
			{
				_iconImage.sprite = _icon;
			}
			if (!_iconImage.gameObject.activeSelf)
			{
				_iconImage.gameObject.SetActive(value: true);
			}
			if ((bool)_labelText && _labelText.gameObject.activeSelf)
			{
				_labelText.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if ((bool)_iconImage && _iconImage.gameObject.activeSelf)
			{
				_iconImage.gameObject.SetActive(value: false);
			}
			if ((bool)_labelText && !_labelText.gameObject.activeSelf)
			{
				_labelText.gameObject.SetActive(value: true);
			}
		}
	}

	private void ValidateMeshColors()
	{
		if ((bool)_renderer)
		{
			if (_propertyBlock == null)
			{
				_propertyBlock = new MaterialPropertyBlock();
			}
			if (_colorId == 0)
			{
				_colorId = Shader.PropertyToID("_Color");
			}
			SetMeshColor((_toggle.isOn && (bool)_colorsOn) ? _colorsOn.NormalColor : _colors.NormalColor);
		}
	}

	private void OnValidate()
	{
		if ((bool)_labelText)
		{
			_labelText.text = _name;
		}
		if (_toggle.group != null && _stayPressedDownWhenToggled && _toggle.isOn)
		{
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 40f);
		}
		ValidateIcon();
		ValidateColors();
		ValidateMeshColors();
	}

	private void SetMeshColor(Color color)
	{
		if (_propertyBlock == null)
		{
			_propertyBlock = new MaterialPropertyBlock();
		}
		_propertyBlock.SetColor(_colorId, color);
		_renderer.SetPropertyBlock(_propertyBlock);
	}
}
