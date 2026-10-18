using Liv.Lck.Settings;
using Liv.Lck.Tablet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckScreenButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[Header("References")]
	[SerializeField]
	private LckButtonColors _colors;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _icon;

	[Header("Audio")]
	[SerializeField]
	private LckDiscreetAudioController _audioController;

	private bool _isDisabled;

	private bool _hasCollided;

	private GameObject _clickedObject;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_isDisabled)
		{
			SetIconColor(_colors.HighlightedColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.HoverSound);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!_isDisabled)
		{
			SetIconColor(_colors.PressedColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			_clickedObject = eventData.pointerEnter;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!_isDisabled)
		{
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
			SetIconColor(_colors.HighlightedColor);
			if (_clickedObject != eventData.pointerEnter)
			{
				SetDefaultButtonColors();
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_isDisabled)
		{
			SetIconColor(_colors.NormalColor);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!_isDisabled && other.gameObject.tag == LckSettings.Instance.TriggerEnterTag && IsValidTap(other.ClosestPoint(base.transform.position)) && !LCKCameraController.ColliderButtonsInUse)
		{
			LCKCameraController.ColliderButtonsInUse = true;
			_hasCollided = true;
			SetIconColor(_colors.PressedColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
			_button.onClick.Invoke();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!_isDisabled && other.gameObject.tag == LckSettings.Instance.TriggerEnterTag && _hasCollided)
		{
			SetIconColor(_colors.NormalColor);
			_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
			_hasCollided = false;
			LCKCameraController.ColliderButtonsInUse = false;
		}
	}

	private bool IsValidTap(Vector3 tapPosition)
	{
		Vector3 to = tapPosition - base.transform.position;
		return Vector3.Angle(-base.transform.forward, to) < 90f;
	}

	public void DisableForDuration(float duration)
	{
		_isDisabled = true;
		SetIconColor(_colors.NormalColor);
		_icon.gameObject.SetActive(value: false);
		Invoke("ReEnableButton", duration);
	}

	private void ReEnableButton()
	{
		_icon.gameObject.SetActive(value: true);
		_isDisabled = false;
	}

	private void SetIconColor(Color color)
	{
		if (_icon != null)
		{
			_icon.color = color;
		}
	}

	public void SetDefaultButtonColors()
	{
		SetIconColor(_colors.NormalColor);
	}

	private void OnValidate()
	{
		if ((bool)_icon && (bool)_colors)
		{
			SetDefaultButtonColors();
		}
	}
}
