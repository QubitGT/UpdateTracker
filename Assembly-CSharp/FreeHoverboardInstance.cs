using UnityEngine;

public class FreeHoverboardInstance : MonoBehaviour
{
	public int ownerActorNumber;

	public int boardIndex;

	[SerializeField]
	private Vector3 sphereCastCenter;

	[SerializeField]
	private float sphereCastRadius;

	[SerializeField]
	private LayerMask hoverRaycastMask;

	[SerializeField]
	private float hoverHeight;

	[SerializeField]
	private float hoverCastDistance = 1f;

	[SerializeField]
	private float hoverRotationLerp;

	[SerializeField]
	private float avelocityDragWhileHovering;

	[SerializeField]
	private Transform visualSlot;

	[SerializeField]
	private GameObject defaultVisualPrefab;

	private BoardSkinVisual skinVisual;

	private bool hasHoverPoint;

	private Vector3 hoverPoint;

	private Vector3 hoverNormal;

	private const float MaxGrabScaleRatio = 1.1f;

	public Rigidbody Rigidbody { get; private set; }

	private float AdjustedSphereCastRadius => sphereCastRadius * base.transform.lossyScale.y;

	private float AdjustedHoverHeight => hoverHeight * base.transform.lossyScale.y;

	private float AdjustedHoverCastDistance => hoverCastDistance * base.transform.lossyScale.y;

	public Color boardColor => GetOrCreateSkinVisual().Color;

	public int skinActorNumber { get; private set; }

	public BoardSkinVisual GetOrCreateSkinVisual()
	{
		if (skinVisual == null)
		{
			skinVisual = new BoardSkinVisual(base.gameObject, visualSlot, defaultVisualPrefab);
		}
		return skinVisual;
	}

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		Rigidbody.useGravity = false;
		GetOrCreateSkinVisual();
	}

	public void SetColor(Color col)
	{
		GetOrCreateSkinVisual().SetColor(col);
	}

	public bool IsGrabbableAtScale(float playerScale)
	{
		float x = base.transform.lossyScale.x;
		if (x <= 0f || playerScale <= 0f)
		{
			return false;
		}
		float num = x / playerScale;
		if (num <= 1.1f)
		{
			return num >= 0.9090909f;
		}
		return false;
	}

	public void SetSkinActorNumber(int actorNumber)
	{
		skinActorNumber = actorNumber;
		GetOrCreateSkinVisual().ApplySkin(FreeHoverboardManager.GetSkinDisplayName(actorNumber));
	}

	public void RevertSkinIfOwnedBy(int actorNumber)
	{
		if (skinActorNumber == actorNumber)
		{
			SetSkinActorNumber(0);
		}
	}

	private void OnDisable()
	{
		skinVisual?.Clear();
	}

	private void Update()
	{
		if (Physics.SphereCast(new Ray(base.transform.TransformPoint(sphereCastCenter), base.transform.TransformVector(Vector3.down)), AdjustedSphereCastRadius, out var hitInfo, AdjustedHoverCastDistance, hoverRaycastMask.value))
		{
			hasHoverPoint = true;
			hoverPoint = hitInfo.point;
			hoverNormal = hitInfo.normal;
		}
		else
		{
			hasHoverPoint = false;
		}
	}

	private void FixedUpdate()
	{
		Rigidbody.AddForce(Physics.gravity * base.transform.lossyScale.y, ForceMode.Acceleration);
		if (hasHoverPoint)
		{
			float num = Vector3.Dot(base.transform.TransformPoint(sphereCastCenter) - hoverPoint, hoverNormal);
			if (num < AdjustedHoverHeight)
			{
				Rigidbody.position += hoverNormal * (AdjustedHoverHeight - num);
				Rigidbody.linearVelocity = Vector3.ProjectOnPlane(Rigidbody.linearVelocity, hoverNormal);
				Vector3 vector = Quaternion.Inverse(Rigidbody.rotation) * Rigidbody.angularVelocity;
				vector.x *= avelocityDragWhileHovering;
				vector.z *= avelocityDragWhileHovering;
				Rigidbody.angularVelocity = Rigidbody.rotation * vector;
				Rigidbody.rotation = Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(Rigidbody.rotation * Vector3.forward, hoverNormal), hoverNormal), hoverRotationLerp);
			}
		}
	}
}
