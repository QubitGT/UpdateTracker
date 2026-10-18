using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Liv.Lck.UI;

public class LckToggleHelper : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[SerializeField]
	private LckToggle _lckToggle;

	[SerializeField]
	private Toggle _toggle;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_lckToggle.IsDisabled)
		{
			_lckToggle.OnPointerEnter(eventData);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!_lckToggle.IsDisabled)
		{
			_lckToggle.OnPointerDown(eventData);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!_lckToggle.IsDisabled)
		{
			_lckToggle.OnPointerUp(eventData);
			_toggle.isOn = true;
			_lckToggle.SetToggleVisualsOn();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_lckToggle.IsDisabled)
		{
			_lckToggle.OnPointerExit(eventData);
		}
	}
}
