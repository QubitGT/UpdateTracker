using Oculus.Interaction.Surfaces;
using UnityEngine;
using UnityEngine.Animations;

public class PanelSetup : MonoBehaviour
{
	public float InteractableLength;

	public float InteractableDepth;

	public bool AddVerticalRotation;

	public bool AddHorizontalRotation;

	public RectTransform panelTransform;

	public UnionClippedPlaneSurface panelClippedPlaneSurface;

	public BoundsClipper boundsClipper;

	public Transform topLeftCornerAnchor;

	[Header("Anchors")]
	public Transform AnchorTopLeft;

	public Transform AnchorTopRight;

	public Transform AnchorBottomLeft;

	public Transform AnchorBottomRight;

	[Header("SideCollider")]
	public GameObject PanelInteractable;

	[Header("Scaler")]
	public GameObject ScalerTopLeft;

	public GameObject ScalerTopRight;

	public GameObject ScalerBottomLeft;

	public GameObject ScalerBottomRight;

	[Header("Rotator")]
	public GameObject RotatorVerticalTop;

	public GameObject RotatorVerticalBottom;

	public GameObject RotatorHorizontalLeft;

	public GameObject RotatorHorizontalRight;

	[ContextMenu("Update Panel")]
	public void UpdatePanelProperties()
	{
		Debug.Log("Update Function");
		Vector2 vector = panelTransform.sizeDelta * panelTransform.lossyScale;
		Vector3[] rectCorners = GetRectCorners(Vector3.zero, vector);
		float num = InteractableLength * 0.5f;
		AnchorTopLeft.localPosition = rectCorners[0] + num * (Vector3)Vec2Sign(rectCorners[0]);
		AnchorTopRight.localPosition = rectCorners[1] + num * (Vector3)Vec2Sign(rectCorners[1]);
		AnchorBottomLeft.localPosition = rectCorners[2] + num * (Vector3)Vec2Sign(rectCorners[2]);
		AnchorBottomRight.localPosition = rectCorners[3] + num * (Vector3)Vec2Sign(rectCorners[3]);
		Vector3 size = new Vector3(InteractableLength, InteractableLength, InteractableDepth);
		SetColliderSize(ScalerTopLeft, size);
		SetColliderSize(ScalerTopRight, size);
		SetColliderSize(ScalerBottomLeft, size);
		SetColliderSize(ScalerBottomRight, size);
		if (AddVerticalRotation)
		{
			SetColliderSize(RotatorVerticalTop, size);
			SetColliderSize(RotatorVerticalBottom, size);
			RotatorVerticalTop.gameObject.SetActive(value: true);
			RotatorVerticalBottom.gameObject.SetActive(value: true);
		}
		else
		{
			RotatorVerticalTop.gameObject.SetActive(value: false);
			RotatorVerticalBottom.gameObject.SetActive(value: false);
		}
		if (AddHorizontalRotation)
		{
			SetColliderSize(RotatorHorizontalLeft, size);
			SetColliderSize(RotatorHorizontalRight, size);
			RotatorHorizontalLeft.gameObject.SetActive(value: true);
			RotatorHorizontalRight.gameObject.SetActive(value: true);
		}
		else
		{
			RotatorHorizontalLeft.gameObject.SetActive(value: false);
			RotatorHorizontalRight.gameObject.SetActive(value: false);
		}
		Vector3[] rectSides = GetRectSides(Vector3.zero, vector);
		if (AddVerticalRotation)
		{
			CreateCollider("ColliderUpLeft", vector, rectSides[0], Vector3.up, Vector3.left, fullSize: false, 0, 1, RotatorVerticalTop.transform, ScalerTopLeft.transform);
			CreateCollider("ColliderUpRight", vector, rectSides[0], Vector3.up, Vector3.right, fullSize: false, 0, 1, RotatorVerticalTop.transform, ScalerTopRight.transform);
			CreateCollider("ColliderDownLeft", vector, rectSides[1], Vector3.down, Vector3.left, fullSize: false, 0, 1, RotatorVerticalBottom.transform, ScalerBottomLeft.transform);
			CreateCollider("ColliderDownRight", vector, rectSides[1], Vector3.down, Vector3.right, fullSize: false, 0, 1, RotatorVerticalBottom.transform, ScalerBottomRight.transform);
		}
		else
		{
			CreateCollider("ColliderUp", vector, rectSides[0], Vector3.up, Vector3.zero, fullSize: true, 0, 1, ScalerTopLeft.transform, ScalerTopRight.transform);
			CreateCollider("ColliderDown", vector, rectSides[1], Vector3.down, Vector3.zero, fullSize: true, 0, 1, ScalerBottomLeft.transform, ScalerBottomRight.transform);
		}
		if (AddHorizontalRotation)
		{
			CreateCollider("ColliderLeftUp", vector, rectSides[2], Vector3.left, Vector3.up, fullSize: false, 1, 0, RotatorHorizontalLeft.transform, ScalerTopLeft.transform);
			CreateCollider("ColliderLeftDown", vector, rectSides[2], Vector3.left, Vector3.down, fullSize: false, 1, 0, RotatorHorizontalLeft.transform, ScalerBottomLeft.transform);
			CreateCollider("ColliderRightUp", vector, rectSides[3], Vector3.right, Vector3.up, fullSize: false, 1, 0, RotatorHorizontalRight.transform, ScalerTopRight.transform);
			CreateCollider("ColliderRightDown", vector, rectSides[3], Vector3.right, Vector3.down, fullSize: false, 1, 0, RotatorHorizontalRight.transform, ScalerBottomRight.transform);
		}
		else
		{
			CreateCollider("ColliderLeft", vector, rectSides[2], Vector3.left, Vector3.zero, fullSize: true, 1, 0, ScalerBottomLeft.transform, ScalerTopLeft.transform);
			CreateCollider("ColliderRight", vector, rectSides[3], Vector3.right, Vector3.zero, fullSize: true, 1, 0, ScalerBottomRight.transform, ScalerTopRight.transform);
		}
		boundsClipper.Size = new Vector3(vector.x, vector.y, InteractableDepth);
		topLeftCornerAnchor.localPosition = new Vector3((0f - vector.x) * 0.5f, vector.y * 0.5f, 0f);
	}

	private void CreateCollider(string name, Vector2 rectSize, Vector3 sidePosition, Vector3 sideDirection, Vector3 offsetDirection, bool fullSize, int wideAxis, int normalAxis, Transform anchorA, Transform anchorB)
	{
		float num = InteractableLength * 0.5f;
		GameObject obj = new GameObject(name);
		obj.transform.SetParent(PanelInteractable.transform, worldPositionStays: false);
		obj.AddComponent<BoxCollider>().isTrigger = true;
		obj.AddComponent<BoundsClipper>();
		PositionConstraint positionConstraint = obj.AddComponent<PositionConstraint>();
		positionConstraint.AddSource(new ConstraintSource
		{
			sourceTransform = anchorA,
			weight = 1f
		});
		positionConstraint.AddSource(new ConstraintSource
		{
			sourceTransform = anchorB,
			weight = 1f
		});
		positionConstraint.constraintActive = true;
		obj.transform.localPosition = sidePosition + sideDirection * num;
		float num2 = (fullSize ? rectSize[wideAxis] : (Mathf.Abs(rectSize[wideAxis] - InteractableLength) / 2f));
		obj.transform.localPosition += offsetDirection * (num + num2 * 0.5f);
		Vector3 vector = new Vector3(0f, 0f, InteractableDepth)
		{
			[wideAxis] = num2,
			[normalAxis] = InteractableLength
		};
		ColliderSizeConstraint colliderSizeConstraint = obj.AddComponent<ColliderSizeConstraint>();
		colliderSizeConstraint.pointA = anchorA;
		colliderSizeConstraint.pointB = anchorB;
		colliderSizeConstraint.size = vector;
		colliderSizeConstraint.wideSideOffset = InteractableLength;
		colliderSizeConstraint.expandingAxis = wideAxis;
		obj.transform.localScale = vector;
	}

	private void SetColliderSize(GameObject colliderGO, Vector3 size)
	{
		if (!(colliderGO == null))
		{
			Vector3 lossyScale = colliderGO.transform.lossyScale;
			Vector3 size2 = new Vector3(size.x / lossyScale.x, size.y / lossyScale.y, size.z / lossyScale.z);
			BoxCollider component = colliderGO.GetComponent<BoxCollider>();
			if (component != null)
			{
				component.size = size2;
			}
			BoundsClipper component2 = colliderGO.GetComponent<BoundsClipper>();
			if (component2 != null)
			{
				component2.Size = size2;
			}
		}
	}

	private Vector2 Vec2Sign(Vector2 value)
	{
		return new Vector2(Mathf.Sign(value.x), Mathf.Sign(value.y));
	}

	private Vector3[] GetRectCorners(Vector3 position, Vector2 size)
	{
		return new Vector3[4]
		{
			position + new Vector3((0f - size.x) * 0.5f, size.y * 0.5f, 0f),
			position + new Vector3(size.x * 0.5f, size.y * 0.5f, 0f),
			position + new Vector3((0f - size.x) * 0.5f, (0f - size.y) * 0.5f, 0f),
			position + new Vector3(size.x * 0.5f, (0f - size.y) * 0.5f, 0f)
		};
	}

	private Vector3[] GetRectSides(Vector3 position, Vector2 size)
	{
		return new Vector3[4]
		{
			position + new Vector3(0f, size.y * 0.5f, 0f),
			position + new Vector3(0f, (0f - size.y) * 0.5f, 0f),
			position + new Vector3((0f - size.x) * 0.5f, 0f, 0f),
			position + new Vector3(size.x * 0.5f, 0f, 0f)
		};
	}
}
