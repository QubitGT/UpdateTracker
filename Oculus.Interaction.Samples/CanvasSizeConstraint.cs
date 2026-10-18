using UnityEngine;

public class CanvasSizeConstraint : MonoBehaviour
{
	public Transform horizontalAnchorA;

	public Transform horizontalAnchorB;

	public Transform verticalAnchorA;

	public Transform verticalAnchorB;

	public float horizontalSizeOffset;

	public float verticalSizeOffset;

	private Vector2 _initialSize;

	private Vector2 _initialRectSize;

	private RectTransform _rectTransform;

	private Vector3 _initialLocalScale;

	private void Start()
	{
		_rectTransform = GetComponent<RectTransform>();
		_initialRectSize = _rectTransform.sizeDelta;
		_initialSize = new Vector2(Vector3.Distance(horizontalAnchorA.position, horizontalAnchorB.position) - horizontalSizeOffset, Vector3.Distance(verticalAnchorA.position, verticalAnchorB.position) - verticalSizeOffset);
		_initialLocalScale = _rectTransform.localScale;
	}

	private void Update()
	{
		Vector2 vector = new Vector2(Vector3.Distance(horizontalAnchorA.position, horizontalAnchorB.position) - horizontalSizeOffset, Vector3.Distance(verticalAnchorA.position, verticalAnchorB.position) - verticalSizeOffset);
		Vector2 vector2 = new Vector2(vector.x / _initialSize.x, vector.y / _initialSize.y);
		_rectTransform.localScale = new Vector3(_initialLocalScale.x / vector2.x, _initialLocalScale.y / vector2.y, _initialLocalScale.z);
		_rectTransform.sizeDelta = new Vector2(_initialRectSize.x * vector2.x, _initialRectSize.y * vector2.y);
	}
}
