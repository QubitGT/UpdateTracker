using UnityEngine;
using UnityEngine.EventSystems;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

[RequireComponent(typeof(Canvas))]
internal class PanelRaycaster : OVRRaycaster
{
	public bool IsValid => eventCamera != null;

	public override void OnPointerEnter(PointerEventData e)
	{
	}

	public override bool IsFocussed()
	{
		return false;
	}

	protected override void OnEnable()
	{
		PanelInputModule.RegisterRaycaster(this);
	}

	protected override void OnDisable()
	{
		PanelInputModule.UnregisterRaycaster(this);
	}
}
