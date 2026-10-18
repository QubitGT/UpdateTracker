using Liv.Lck.Settings;
using Liv.Lck.Tablet;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[Header("Settings")]
	[SerializeField]
	private string _name;

	[SerializeField]
	private LckButtonColors _colors;

	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI _labelText;

	[SerializeField]
	private Image _iconImage;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private RectTransform _visuals;

	[SerializeField]
	private Button _button;

	[Header("Audio")]
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	private GameObject _clickedObject;

	private bool _hasCollided;

	private MaterialPropertyBlock _propertyBlock;

	private int _colorId;

	private bool _isDisabled;

	private void Awake()
	{
		_propertyBlock = new MaterialPropertyBlock();
		_colorId = Shader.PropertyToID("_Color");
	}

	public void SetLabelText(string text)
	{
		_labelText.text = text;
	}

	public void SetIsDisabled(bool isDisabled)
	{
		_isDisabled = isDisabled;
		_iconImage.color = (isDisabled ? _colors.HighlightedColor : Color.white);
		_labelText.color = (isDisabled ? _colors.HighlightedColor : Color.white);
		_button.interactable = !isDisabled;
		SetMeshColor(isDisabled ? _colors.DisabledColor : _colors.NormalColor);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_isDisabled)
		{
			SetMeshColor(_colors.HighlightedColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.HoverSound);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!_isDisabled)
		{
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 40f);
			SetMeshColor(_colors.PressedColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			_clickedObject = eventData.pointerEnter;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!_isDisabled)
		{
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
			if (_clickedObject != eventData.pointerEnter)
			{
				_button.OnPointerClick(eventData);
			}
			else
			{
				SetMeshColor(_colors.HighlightedColor);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_isDisabled)
		{
			SetMeshColor(_colors.NormalColor);
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!_isDisabled && other.gameObject.tag == LckSettings.Instance.TriggerEnterTag && IsValidTap(other.ClosestPoint(base.transform.position)) && !LCKCameraController.ColliderButtonsInUse)
		{
			LCKCameraController.ColliderButtonsInUse = true;
			_hasCollided = true;
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 40f);
			SetMeshColor(_colors.PressedColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			_button.onClick.Invoke();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!_isDisabled && other.gameObject.tag == LckSettings.Instance.TriggerEnterTag && _hasCollided)
		{
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
			SetMeshColor(_colors.NormalColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
			_hasCollided = false;
			LCKCameraController.ColliderButtonsInUse = false;
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			_hasCollided = false;
			_visuals.anchoredPosition3D = new Vector3(0f, 0f, 0f);
			SetMeshColor(_colors.NormalColor);
			if (_isDisabled)
			{
				SetIsDisabled(isDisabled: true);
			}
		}
	}

	private bool IsValidTap(Vector3 tapPosition)
	{
		Vector3 to = tapPosition - base.transform.position;
		return Vector3.Angle(-base.transform.forward, to) < 65f;
	}

	private void OnValidate()
	{
		if ((bool)_labelText)
		{
			_labelText.text = _name;
		}
		if ((bool)_colors && (bool)_button)
		{
			ColorBlock colors = _button.colors;
			colors.normalColor = _colors.NormalColor;
			colors.highlightedColor = _colors.HighlightedColor;
			colors.pressedColor = _colors.PressedColor;
			colors.selectedColor = _colors.SelectedColor;
			colors.disabledColor = _colors.DisabledColor;
			if (_button.colors != colors)
			{
				_button.colors = colors;
			}
		}
		if ((bool)_renderer)
		{
			_propertyBlock = new MaterialPropertyBlock();
			SetMeshColor(_colors.NormalColor);
		}
	}

	private void SetMeshColor(Color color)
	{
		_propertyBlock.SetColor(_colorId, color);
		_renderer.SetPropertyBlock(_propertyBlock);
	}
}
