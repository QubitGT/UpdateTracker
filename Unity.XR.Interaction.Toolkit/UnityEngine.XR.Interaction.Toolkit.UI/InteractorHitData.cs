using UnityEngine.UIElements;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

internal struct InteractorHitData
{
	public Vector3 closestPoint;

	public Vector3 interactorOrigin;

	public Vector3 interactorDirection;

	public UIDocument hitDocument;
}
