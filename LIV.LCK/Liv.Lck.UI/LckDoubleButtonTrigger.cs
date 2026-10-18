using System;
using Liv.Lck.Settings;
using Liv.Lck.Tablet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckDoubleButtonTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[SerializeField]
	private bool _isUsingColliders;

	[SerializeField]
	private bool _isIncreaseButton;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Image _icon;

	private bool _hasCollided;

	public event Action<bool> OnDown;

	public event Action<bool> OnEnter;

	public event Action<bool, bool> OnUp;

	public event Action<bool> OnExit;

	public void SetBackgroundColor(Color color)
	{
		_background.color = color;
	}

	public void SetIconColor(Color color)
	{
		_icon.color = color;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!_isUsingColliders)
		{
			this.OnDown?.Invoke(_isIncreaseButton);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_isUsingColliders)
		{
			this.OnEnter?.Invoke(_isIncreaseButton);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!_isUsingColliders)
		{
			this.OnUp?.Invoke(_isIncreaseButton, arg2: false);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_isUsingColliders)
		{
			this.OnExit?.Invoke(_isIncreaseButton);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == LckSettings.Instance.TriggerEnterTag && IsValidTap(other.ClosestPoint(base.transform.position)) && !LCKCameraController.ColliderButtonsInUse)
		{
			LCKCameraController.ColliderButtonsInUse = true;
			_hasCollided = true;
			this.OnDown?.Invoke(_isIncreaseButton);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == LckSettings.Instance.TriggerEnterTag && _hasCollided)
		{
			this.OnUp?.Invoke(_isIncreaseButton, arg2: true);
			_hasCollided = false;
			LCKCameraController.ColliderButtonsInUse = false;
		}
	}

	private bool IsValidTap(Vector3 tapPosition)
	{
		Vector3 to = tapPosition - base.transform.position;
		return Vector3.Angle(-base.transform.forward, to) < 65f;
	}
}
