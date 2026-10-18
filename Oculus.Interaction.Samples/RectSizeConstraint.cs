using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class RectSizeConstraint : UIBehaviour
{
	public RectTransform target;

	protected virtual void LateUpdate()
	{
		if (target != null)
		{
			RectTransform obj = (RectTransform)base.transform;
			obj.sizeDelta = new Vector2(target.rect.width, target.rect.height);
			obj.ForceUpdateRectTransforms();
		}
	}
}
