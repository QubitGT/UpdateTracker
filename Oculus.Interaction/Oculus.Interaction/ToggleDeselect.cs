using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Oculus.Interaction;

public class ToggleDeselect : Toggle
{
	[SerializeField]
	private bool _clearStateOnDrag;

	public bool ClearStateOnDrag
	{
		get
		{
			return _clearStateOnDrag;
		}
		set
		{
			_clearStateOnDrag = value;
		}
	}

	public void OnBeginDrag(PointerEventData pointerEventData)
	{
		if (_clearStateOnDrag)
		{
			InstantClearState();
			DoStateTransition(SelectionState.Normal, instant: true);
			ExecuteEvents.ExecuteHierarchy(base.transform.parent.gameObject, pointerEventData, ExecuteEvents.beginDragHandler);
		}
	}
}
