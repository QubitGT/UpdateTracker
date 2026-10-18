namespace UnityEngine.EventSystems;

public static class PointerEventDataExtension
{
	public static bool IsVRPointer(this PointerEventData pointerEventData)
	{
		return pointerEventData is OVRPointerEventData;
	}

	public static Ray GetRay(this PointerEventData pointerEventData)
	{
		return (pointerEventData as OVRPointerEventData).worldSpaceRay;
	}

	public static Vector2 GetSwipeStart(this PointerEventData pointerEventData)
	{
		return (pointerEventData as OVRPointerEventData).swipeStart;
	}

	public static void SetSwipeStart(this PointerEventData pointerEventData, Vector2 start)
	{
		(pointerEventData as OVRPointerEventData).swipeStart = start;
	}
}
