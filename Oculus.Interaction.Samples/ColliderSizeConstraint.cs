using UnityEngine;

public class ColliderSizeConstraint : MonoBehaviour
{
	public Vector3 size;

	public int expandingAxis;

	public Transform pointA;

	public Transform pointB;

	public float wideSideOffset;

	private void Update()
	{
		float num = Vector3.Distance(pointA.position, pointB.position);
		num -= wideSideOffset;
		Vector3 lossyScale = base.transform.parent.lossyScale;
		Vector3 vector = size;
		vector[expandingAxis] = num;
		Vector3 localScale = new Vector3(vector.x / lossyScale.x, vector.y / lossyScale.y, vector.z / lossyScale.z);
		base.transform.localScale = localScale;
	}
}
