using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OVR.Input;

public class UIButtonSquishHelper : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerExitHandler, IPointerUpHandler, IPointerEnterHandler
{
	private const float _squishAmount = 1.1f;

	private const float _highlightAmount = 1.05f;

	private Vector3 _originalScale;

	private Button _button;

	private void Start()
	{
		_originalScale = base.transform.localScale;
		_button = GetComponent<Button>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_button || _button.interactable)
		{
			base.transform.localScale = _originalScale * 1.05f;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!_button || _button.interactable)
		{
			base.transform.localScale = _originalScale * 1.1f;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_button || _button.interactable)
		{
			base.transform.localScale = _originalScale;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!_button || _button.interactable)
		{
			base.transform.localScale = _originalScale;
		}
	}
}
