using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Photon.Pun.UtilityScripts;

[RequireComponent(typeof(Graphic))]
public class GraphicToggleIsOnTransition : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Toggle toggle;

	private Graphic _graphic;

	public Color NormalOnColor = Color.white;

	public Color NormalOffColor = Color.black;

	public Color HoverOnColor = Color.black;

	public Color HoverOffColor = Color.black;

	private bool isHover;

	public void OnPointerEnter(PointerEventData eventData)
	{
		isHover = true;
		_graphic.color = (toggle.isOn ? HoverOnColor : HoverOffColor);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isHover = false;
		_graphic.color = (toggle.isOn ? NormalOnColor : NormalOffColor);
	}

	public void OnEnable()
	{
		_graphic = GetComponent<Graphic>();
		OnValueChanged(toggle.isOn);
		toggle.onValueChanged.AddListener(OnValueChanged);
	}

	public void OnDisable()
	{
		toggle.onValueChanged.RemoveListener(OnValueChanged);
	}

	public void OnValueChanged(bool isOn)
	{
		_graphic.color = ((!isOn) ? (isHover ? NormalOffColor : NormalOffColor) : (isHover ? HoverOnColor : HoverOnColor));
	}
}
