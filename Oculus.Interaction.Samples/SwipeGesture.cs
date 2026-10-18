using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SwipeGesture : UIBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler
{
	public enum Axis
	{
		Horizontal,
		Vertical
	}

	public float gestureMaxDuration = 1f;

	public float gestureMinDistanceNormalized = 0.15f;

	[Space(10f)]
	public bool invertScroll;

	public Axis swipeAxis;

	private float startTime;

	private Vector2 startLocalPosition;

	[Space(10f)]
	[SerializeField]
	private UnityEvent<int> swipeExecuted;

	public void OnBeginDrag(PointerEventData eventData)
	{
		startTime = Time.time;
		RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)base.transform, eventData.position, eventData.pressEventCamera, out var localPoint);
		startLocalPosition = localPoint;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		float num = Time.time - startTime;
		RectTransform rectTransform = (RectTransform)base.transform;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint);
		Vector2 vector = localPoint - startLocalPosition;
		bool flag = Mathf.Abs(vector.normalized[(int)swipeAxis]) > 0.5f;
		bool flag2 = num < gestureMaxDuration;
		float num2 = ((swipeAxis == Axis.Horizontal) ? rectTransform.rect.width : rectTransform.rect.height);
		float num3 = Mathf.Abs(vector[(int)swipeAxis]);
		float num4 = num2 * gestureMinDistanceNormalized;
		bool flag3 = num3 > num4;
		if (flag && flag3 && flag2)
		{
			int num5 = (int)Mathf.Sign(vector[(int)swipeAxis]);
			num5 *= ((!invertScroll) ? 1 : (-1));
			swipeExecuted.Invoke(num5);
		}
	}
}
