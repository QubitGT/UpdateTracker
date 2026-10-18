using System;
using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Position Control/Cinemachine Third Person Follow")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Body)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineThirdPersonFollow.html")]
public class CinemachineThirdPersonFollow : CinemachineComponentBase, CinemachineFreeLookModifier.IModifierValueSource, CinemachineFreeLookModifier.IModifiablePositionDamping, CinemachineFreeLookModifier.IModifiableDistance
{
	[Serializable]
	public struct ObstacleSettings
	{
		[Tooltip("If enabled, camera will be pulled in front of occluding obstacles")]
		public bool Enabled;

		[Tooltip("Camera will avoid obstacles on these layers")]
		public LayerMask CollisionFilter;

		[TagField]
		[Tooltip("Obstacles with this tag will be ignored.  It is a good idea to set this field to the target's tag")]
		public string IgnoreTag;

		[Tooltip("Specifies how close the camera can get to obstacles")]
		[Range(0f, 1f)]
		public float CameraRadius;

		[Range(0f, 10f)]
		[Tooltip("How gradually the camera moves to correct for occlusions.  Higher numbers will move the camera more gradually.")]
		public float DampingIntoCollision;

		[Range(0f, 10f)]
		[Tooltip("How gradually the camera returns to its normal position after having been corrected by the built-in collision resolution system.  Higher numbers will move the camera more gradually back to normal.")]
		public float DampingFromCollision;

		internal static ObstacleSettings Default
		{
			get
			{
				ObstacleSettings result = default(ObstacleSettings);
				result.Enabled = false;
				result.CollisionFilter = 1;
				result.IgnoreTag = string.Empty;
				result.CameraRadius = 0.2f;
				result.DampingIntoCollision = 0f;
				result.DampingFromCollision = 0.5f;
				return result;
			}
		}
	}

	[Tooltip("How responsively the camera tracks the target.  Each axis (camera-local) can have its own setting.  Value is the approximate time it takes the camera to catch up to the target's new position.  Smaller values give a more rigid effect, larger values give a squishier one")]
	public Vector3 Damping;

	[Header("Rig")]
	[Tooltip("Position of the shoulder pivot relative to the Follow target origin.  This offset is in target-local space")]
	public Vector3 ShoulderOffset;

	[Tooltip("Vertical offset of the hand in relation to the shoulder.  Arm length will affect the follow target's screen position when the camera rotates vertically")]
	public float VerticalArmLength;

	[Tooltip("Specifies which shoulder (left, right, or in-between) the camera is on")]
	[Range(0f, 1f)]
	public float CameraSide;

	[Tooltip("How far behind the hand the camera will be placed")]
	public float CameraDistance;

	[FoldoutWithEnabledButton("Enabled")]
	public ObstacleSettings AvoidObstacles = ObstacleSettings.Default;

	private Vector3 m_PreviousFollowTargetPosition;

	private Vector3 m_DampingCorrection;

	private float m_CamPosCollisionCorrection;

	public Collider CurrentObstacle { get; set; }

	float CinemachineFreeLookModifier.IModifierValueSource.NormalizedModifierValue
	{
		get
		{
			Vector3 referenceUp = base.VirtualCamera.State.ReferenceUp;
			Quaternion followTargetRotation = base.FollowTargetRotation;
			return Mathf.Clamp(Vector3.SignedAngle(followTargetRotation * Vector3.up, referenceUp, followTargetRotation * Vector3.right), -90f, 90f) / -90f;
		}
	}

	Vector3 CinemachineFreeLookModifier.IModifiablePositionDamping.PositionDamping
	{
		get
		{
			return Damping;
		}
		set
		{
			Damping = value;
		}
	}

	float CinemachineFreeLookModifier.IModifiableDistance.Distance
	{
		get
		{
			return CameraDistance;
		}
		set
		{
			CameraDistance = value;
		}
	}

	public override bool IsValid
	{
		get
		{
			if (base.enabled)
			{
				return base.FollowTarget != null;
			}
			return false;
		}
	}

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Body;

	private void OnValidate()
	{
		CameraSide = Mathf.Clamp(CameraSide, -1f, 1f);
		Damping.x = Mathf.Max(0f, Damping.x);
		Damping.y = Mathf.Max(0f, Damping.y);
		Damping.z = Mathf.Max(0f, Damping.z);
		AvoidObstacles.CameraRadius = Mathf.Max(0.001f, AvoidObstacles.CameraRadius);
		AvoidObstacles.DampingIntoCollision = Mathf.Max(0f, AvoidObstacles.DampingIntoCollision);
		AvoidObstacles.DampingFromCollision = Mathf.Max(0f, AvoidObstacles.DampingFromCollision);
	}

	private void Reset()
	{
		ShoulderOffset = new Vector3(0.5f, -0.4f, 0f);
		VerticalArmLength = 0.4f;
		CameraSide = 1f;
		CameraDistance = 2f;
		Damping = new Vector3(0.1f, 0.5f, 0.3f);
		AvoidObstacles = ObstacleSettings.Default;
	}

	public override float GetMaxDampTime()
	{
		return Mathf.Max(AvoidObstacles.Enabled ? Mathf.Max(AvoidObstacles.DampingIntoCollision, AvoidObstacles.DampingFromCollision) : 0f, Mathf.Max(Damping.x, Mathf.Max(Damping.y, Damping.z)));
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (IsValid)
		{
			if (!base.VirtualCamera.PreviousStateIsValid)
			{
				deltaTime = -1f;
			}
			PositionCamera(ref curState, deltaTime);
		}
	}

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		base.OnTargetObjectWarped(target, positionDelta);
		if (target == base.FollowTarget)
		{
			m_PreviousFollowTargetPosition += positionDelta;
		}
	}

	private void PositionCamera(ref CameraState curState, float deltaTime)
	{
		Vector3 referenceUp = curState.ReferenceUp;
		Vector3 followTargetPosition = base.FollowTargetPosition;
		Quaternion followTargetRotation = base.FollowTargetRotation;
		Vector3 vector = followTargetRotation * Vector3.forward;
		Quaternion heading = GetHeading(followTargetRotation, referenceUp);
		if (deltaTime < 0f)
		{
			m_DampingCorrection = Vector3.zero;
			m_CamPosCollisionCorrection = 0f;
		}
		else
		{
			m_DampingCorrection += Quaternion.Inverse(heading) * (m_PreviousFollowTargetPosition - followTargetPosition);
			m_DampingCorrection -= base.VirtualCamera.DetachedFollowTargetDamp(m_DampingCorrection, Damping, deltaTime);
		}
		m_PreviousFollowTargetPosition = followTargetPosition;
		Vector3 root = followTargetPosition;
		GetRawRigPositions(root, followTargetRotation, heading, out var _, out var hand);
		Vector3 vector2 = hand - vector * (CameraDistance - m_DampingCorrection.z);
		CurrentObstacle = null;
		if (AvoidObstacles.Enabled)
		{
			float collisionCorrection = 0f;
			Vector3 root2 = ResolveCollisions(root, hand, -1f, AvoidObstacles.CameraRadius * 1.05f, ref collisionCorrection);
			vector2 = ResolveCollisions(root2, vector2, deltaTime, AvoidObstacles.CameraRadius, ref m_CamPosCollisionCorrection);
		}
		curState.RawPosition = vector2;
		curState.RawOrientation = followTargetRotation;
		if (!curState.HasLookAt() || curState.ReferenceLookAt.Equals(followTargetPosition))
		{
			curState.ReferenceLookAt = followTargetPosition + vector * 0.01f;
		}
	}

	public void GetRigPositions(out Vector3 root, out Vector3 shoulder, out Vector3 hand)
	{
		Vector3 referenceUp = base.VirtualCamera.State.ReferenceUp;
		Quaternion followTargetRotation = base.FollowTargetRotation;
		Quaternion heading = GetHeading(followTargetRotation, referenceUp);
		root = m_PreviousFollowTargetPosition;
		GetRawRigPositions(root, followTargetRotation, heading, out shoulder, out hand);
		if (AvoidObstacles.Enabled)
		{
			float collisionCorrection = 0f;
			hand = ResolveCollisions(root, hand, -1f, AvoidObstacles.CameraRadius * 1.05f, ref collisionCorrection);
		}
	}

	internal static Quaternion GetHeading(Quaternion targetRot, Vector3 up)
	{
		Vector3 vector = targetRot * Vector3.forward;
		Vector3 vector2 = Vector3.Cross(up, Vector3.Cross(vector.ProjectOntoPlane(up), up));
		if (vector2.AlmostZero())
		{
			vector2 = Vector3.Cross(targetRot * Vector3.right, up);
		}
		return Quaternion.LookRotation(vector2, up);
	}

	private void GetRawRigPositions(Vector3 root, Quaternion targetRot, Quaternion heading, out Vector3 shoulder, out Vector3 hand)
	{
		Vector3 shoulderOffset = ShoulderOffset;
		shoulderOffset.x = Mathf.Lerp(0f - shoulderOffset.x, shoulderOffset.x, CameraSide);
		shoulderOffset.x += m_DampingCorrection.x;
		shoulderOffset.y += m_DampingCorrection.y;
		shoulder = root + heading * shoulderOffset;
		hand = shoulder + targetRot * new Vector3(0f, VerticalArmLength, 0f);
	}

	private Vector3 ResolveCollisions(Vector3 root, Vector3 tip, float deltaTime, float cameraRadius, ref float collisionCorrection)
	{
		if (AvoidObstacles.CollisionFilter.value == 0)
		{
			return tip;
		}
		Vector3 vector = tip - root;
		float magnitude = vector.magnitude;
		if (magnitude < 0.0001f)
		{
			return tip;
		}
		vector /= magnitude;
		Vector3 result = tip;
		float num = 0f;
		if (RuntimeUtility.SphereCastIgnoreTag(new Ray(root, vector), cameraRadius, out var hitInfo, magnitude, AvoidObstacles.CollisionFilter, in AvoidObstacles.IgnoreTag))
		{
			CurrentObstacle = hitInfo.collider;
			num = (hitInfo.point + hitInfo.normal * cameraRadius - tip).magnitude;
		}
		collisionCorrection += ((deltaTime < 0f) ? (num - collisionCorrection) : Damper.Damp(num - collisionCorrection, (num > collisionCorrection) ? AvoidObstacles.DampingIntoCollision : AvoidObstacles.DampingFromCollision, deltaTime));
		if (collisionCorrection > 0.0001f)
		{
			result -= vector * collisionCorrection;
		}
		return result;
	}
}
