using System.Text;

namespace UnityEngine.EventSystems;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-isdk-pointer-events/")]
public class OVRPointerEventData : PointerEventData
{
	public Ray worldSpaceRay;

	public Vector2 swipeStart;

	public OVRPointerEventData(EventSystem eventSystem)
		: base(eventSystem)
	{
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("<b>Position</b>: " + base.position.ToString());
		stringBuilder.AppendLine("<b>delta</b>: " + base.delta.ToString());
		stringBuilder.AppendLine("<b>eligibleForClick</b>: " + base.eligibleForClick);
		stringBuilder.AppendLine("<b>pointerEnter</b>: " + base.pointerEnter);
		stringBuilder.AppendLine("<b>pointerPress</b>: " + base.pointerPress);
		stringBuilder.AppendLine("<b>lastPointerPress</b>: " + base.lastPress);
		stringBuilder.AppendLine("<b>pointerDrag</b>: " + base.pointerDrag);
		Ray ray = worldSpaceRay;
		stringBuilder.AppendLine("<b>worldSpaceRay</b>: " + ray.ToString());
		Vector2 vector = swipeStart;
		stringBuilder.AppendLine("<b>swipeStart</b>: " + vector.ToString());
		stringBuilder.AppendLine("<b>Use Drag Threshold</b>: " + base.useDragThreshold);
		return stringBuilder.ToString();
	}
}
