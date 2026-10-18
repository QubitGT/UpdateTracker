using UnityEngine;
using UnityEngine.Animations;

public class UpdateRoundedBoxAnchorConstraint : MonoBehaviour
{
	[SerializeField]
	private PositionConstraint _topLeft;

	[SerializeField]
	private PositionConstraint _topRight;

	[SerializeField]
	private PositionConstraint _bottomLeft;

	[SerializeField]
	private PositionConstraint _bottomRight;

	[SerializeField]
	private float _interactableLength;

	[SerializeField]
	private Vector2 _offset;

	private static void UpdateOffset(PositionConstraint constraint, Vector2 direction, Vector2 offset, float interactableLength)
	{
		constraint.translationOffset = direction * offset + direction * interactableLength * 0.5f;
	}

	public static void UpdateAnchors(PositionConstraint topLeft, PositionConstraint topRight, PositionConstraint bottomLeft, PositionConstraint bottomRight, Vector2 offset, float interactableLength)
	{
		UpdateOffset(topLeft, new Vector2(1f, -1f), offset, interactableLength);
		UpdateOffset(topRight, new Vector2(-1f, -1f), offset, interactableLength);
		UpdateOffset(bottomLeft, new Vector2(1f, 1f), offset, interactableLength);
		UpdateOffset(bottomRight, new Vector2(-1f, 1f), offset, interactableLength);
	}

	[ContextMenu("Update Anchors")]
	public void UpdateAnchorsMenu()
	{
		UpdateAnchors(_topLeft, _topRight, _bottomLeft, _bottomRight, _offset, _interactableLength);
	}
}
