using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class PanelScrollRect : ScrollRect
{
	public override void OnScroll(PointerEventData eventData)
	{
		if (PanelInputModule.Processing)
		{
			base.OnScroll(eventData);
		}
	}

	public override void OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (PanelInputModule.Processing)
		{
			base.OnInitializePotentialDrag(eventData);
		}
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (PanelInputModule.Processing)
		{
			base.OnBeginDrag(eventData);
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (PanelInputModule.Processing)
		{
			base.OnEndDrag(eventData);
		}
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (PanelInputModule.Processing)
		{
			base.OnDrag(eventData);
		}
	}
}
