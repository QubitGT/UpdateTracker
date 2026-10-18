using UnityEngine;
using UnityEngine.EventSystems;

namespace Modio.Unity.UI.Navigation;

public class ModioViewportRestraintChild : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	[SerializeField]
	private RectTransform _overrideFocusTo;

	private ModioViewportRestraint _viewportRestraint;

	private void Awake()
	{
		_viewportRestraint = GetComponentInParent<ModioViewportRestraint>();
		if (_viewportRestraint == null)
		{
			base.enabled = false;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (!(eventData is PointerEventData))
		{
			MoveToSelected();
		}
	}

	private void MoveToSelected()
	{
		if (_overrideFocusTo != null)
		{
			_viewportRestraint.ChildSelected(_overrideFocusTo);
		}
		RectTransform rectTransform = base.transform as RectTransform;
		if (rectTransform != null)
		{
			_viewportRestraint.ChildSelected(rectTransform);
		}
	}
}
