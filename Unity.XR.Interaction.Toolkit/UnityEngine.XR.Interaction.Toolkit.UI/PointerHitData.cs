using UnityEngine.UIElements;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

internal struct PointerHitData
{
	public Vector3 worldPosition;

	public Quaternion worldOrientation;

	public float hitDistance;

	public Collider hitCollider;

	public UIDocument hitDocument;

	public VisualElement hitElement;
}
