using UnityEngine;

namespace Valve.VR.InteractionSystem;

public class HandPhysics : MonoBehaviour
{
	[Tooltip("Hand collider prefab to instantiate")]
	public HandCollider handColliderPrefab;

	[HideInInspector]
	public HandCollider handCollider;

	[Tooltip("Layers to consider when checking if an area is clear")]
	public LayerMask clearanceCheckMask;

	[HideInInspector]
	public Hand hand;

	private const float handResetDistance = 0.6f;

	private const float collisionReenableClearanceRadius = 0.1f;

	private bool initialized;

	private bool collisionsEnabled = true;

	private Matrix4x4 wristToRoot;

	private Matrix4x4 rootToArmature;

	private Matrix4x4 wristToArmature;

	private Vector3 targetPosition = Vector3.zero;

	private Quaternion targetRotation = Quaternion.identity;

	private const int wristBone = 1;

	private const int rootBone = 0;

	private Collider[] clearanceBuffer = new Collider[1];

	private Transform wrist;

	private const int thumbBone = 4;

	private const int indexBone = 9;

	private const int middleBone = 14;

	private const int ringBone = 19;

	private const int pinkyBone = 24;

	private void Start()
	{
		hand = GetComponent<Hand>();
		handCollider = Object.Instantiate(handColliderPrefab.gameObject).GetComponent<HandCollider>();
		Vector3 localPosition = handCollider.transform.localPosition;
		Quaternion localRotation = handCollider.transform.localRotation;
		handCollider.transform.parent = Player.instance.transform;
		handCollider.transform.localPosition = localPosition;
		handCollider.transform.localRotation = localRotation;
		handCollider.hand = this;
		GetComponent<SteamVR_Behaviour_Pose>().onTransformUpdated.AddListener(UpdateHand);
	}

	private void FixedUpdate()
	{
		if (!(hand.skeleton == null))
		{
			initialized = true;
			UpdateCenterPoint();
			handCollider.MoveTo(targetPosition, targetRotation);
			if ((handCollider.transform.position - targetPosition).sqrMagnitude > 0.36f)
			{
				handCollider.TeleportTo(targetPosition, targetRotation);
			}
			UpdateFingertips();
		}
	}

	private void UpdateCenterPoint()
	{
		Vector3 vector = hand.skeleton.GetBonePosition(12) - hand.skeleton.GetBonePosition(0);
		if (hand.HasSkeleton())
		{
			handCollider.SetCenterPoint(hand.skeleton.transform.position + vector);
		}
	}

	private void UpdatePositions()
	{
		if (hand.currentAttachedObject != null)
		{
			collisionsEnabled = false;
		}
		else if (!collisionsEnabled)
		{
			clearanceBuffer[0] = null;
			Physics.OverlapSphereNonAlloc(hand.objectAttachmentPoint.position, 0.1f, clearanceBuffer);
			if (clearanceBuffer[0] == null)
			{
				collisionsEnabled = true;
			}
		}
		handCollider.SetCollisionDetectionEnabled(collisionsEnabled);
		if (!(hand.skeleton == null))
		{
			initialized = true;
			wristToRoot = Matrix4x4.TRS(ProcessPos(1, hand.skeleton.GetBone(1).localPosition), ProcessRot(1, hand.skeleton.GetBone(1).localRotation), Vector3.one).inverse;
			rootToArmature = Matrix4x4.TRS(ProcessPos(0, hand.skeleton.GetBone(0).localPosition), ProcessRot(0, hand.skeleton.GetBone(0).localRotation), Vector3.one).inverse;
			wristToArmature = (wristToRoot * rootToArmature).inverse;
			targetPosition = base.transform.TransformPoint(wristToArmature.MultiplyPoint3x4(Vector3.zero));
			targetRotation = base.transform.rotation * wristToArmature.GetRotation();
			if (Time.timeScale == 0f)
			{
				handCollider.TeleportTo(targetPosition, targetRotation);
			}
		}
	}

	private void UpdateFingertips()
	{
		wrist = hand.skeleton.GetBone(1);
		for (int i = 0; i < 5; i++)
		{
			int boneForFingerTip = SteamVR_Skeleton_JointIndexes.GetBoneForFingerTip(i);
			int num = boneForFingerTip;
			for (int j = 0; j < handCollider.fingerColliders[i].Length; j++)
			{
				num = boneForFingerTip - 1 - j;
				if (handCollider.fingerColliders[i][j] != null)
				{
					handCollider.fingerColliders[i][j].localPosition = wrist.InverseTransformPoint(hand.skeleton.GetBone(num).position);
				}
			}
		}
	}

	private void UpdateHand(SteamVR_Behaviour_Pose pose, SteamVR_Input_Sources inputSource)
	{
		if (initialized)
		{
			UpdateCenterPoint();
			UpdatePositions();
			Quaternion rotation = handCollider.transform.rotation * wristToArmature.inverse.GetRotation();
			hand.mainRenderModel.transform.rotation = rotation;
			Vector3 position = handCollider.transform.TransformPoint(wristToArmature.inverse.MultiplyPoint3x4(Vector3.zero));
			hand.mainRenderModel.transform.position = position;
		}
	}

	private Vector3 ProcessPos(int boneIndex, Vector3 pos)
	{
		if (hand.skeleton.mirroring != 0)
		{
			return SteamVR_Behaviour_Skeleton.MirrorPosition(boneIndex, pos);
		}
		return pos;
	}

	private Quaternion ProcessRot(int boneIndex, Quaternion rot)
	{
		if (hand.skeleton.mirroring != 0)
		{
			return SteamVR_Behaviour_Skeleton.MirrorRotation(boneIndex, rot);
		}
		return rot;
	}
}
