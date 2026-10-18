using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

internal static class Extensions
{
	public static void SetSizeOptimized(this RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax, Vector2 fixedDimensions, bool setAnchoredPosition)
	{
		Vector2 sizeDelta = rectTransform.sizeDelta;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		Vector2 pivot = rectTransform.pivot;
		Vector2 vector = offsetMin - (anchoredPosition - new Vector2(sizeDelta.x * pivot.x, sizeDelta.y * pivot.y));
		sizeDelta.x = vector.x + sizeDelta.x;
		sizeDelta.y = vector.y + sizeDelta.y;
		anchoredPosition = new Vector2(offsetMin.x + sizeDelta.x * pivot.x, offsetMin.y + sizeDelta.y * pivot.y);
		vector = offsetMax - (anchoredPosition + new Vector2(sizeDelta.x * (1f - pivot.x), sizeDelta.y * (1f - pivot.y)));
		sizeDelta.x = vector.x + sizeDelta.x;
		sizeDelta.y = vector.y + sizeDelta.y;
		anchoredPosition = new Vector2(offsetMax.x - sizeDelta.x * (1f - pivot.x), offsetMax.y - sizeDelta.y * (1f - pivot.y));
		sizeDelta.x = ((fixedDimensions.x != 0f) ? fixedDimensions.x : sizeDelta.x);
		sizeDelta.y = ((fixedDimensions.y != 0f) ? fixedDimensions.y : sizeDelta.y);
		rectTransform.sizeDelta = sizeDelta;
		if (setAnchoredPosition)
		{
			rectTransform.anchoredPosition = anchoredPosition;
		}
	}
}
