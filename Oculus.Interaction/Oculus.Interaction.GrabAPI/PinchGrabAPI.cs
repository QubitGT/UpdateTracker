using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

public class PinchGrabAPI : IFingerAPI
{
	private class FingerPinchData
	{
		private readonly HandJointId _tipId;

		private float _minPinchDistance;

		public float PinchStrength;

		public bool IsPinching;

		public Vector3 TipPosition { get; private set; }

		public bool IsPinchingChanged { get; private set; }

		public FingerPinchData(HandFinger fingerId)
		{
			_tipId = HandJointUtils.GetHandFingerTip(fingerId);
		}

		public void UpdateTipPosition(ShadowHand hand)
		{
			TipPosition = hand.GetWorldPose(_tipId).position;
		}

		public void UpdateIsPinching(float distance, float start, float stopOffset, float stopMax)
		{
			if (!IsPinching)
			{
				if (distance < start)
				{
					IsPinching = true;
					IsPinchingChanged = true;
					_minPinchDistance = distance;
				}
				return;
			}
			_minPinchDistance = Mathf.Min(_minPinchDistance, distance);
			if (distance > stopMax || distance > _minPinchDistance + stopOffset)
			{
				IsPinching = false;
				IsPinchingChanged = true;
				_minPinchDistance = float.MaxValue;
			}
		}

		public void ClearState()
		{
			IsPinchingChanged = false;
		}
	}

	private bool _isPinchVisibilityGood;

	private const float PINCH_DISTANCE_START = 0.02f;

	private const float PINCH_DISTANCE_STOP_MAX = 0.1f;

	private const float PINCH_DISTANCE_STOP_OFFSET = 0.04f;

	private const float PINCH_HQ_DISTANCE_START = 0.016f;

	private const float PINCH_HQ_DISTANCE_STOP_MAX = 0.1f;

	private const float PINCH_HQ_DISTANCE_STOP_OFFSET = 0.016f;

	private const float THUMB_DISTANCE_START = 0.03f;

	private const float THUMB_DISTANCE_STOP_MAX = 0.05f;

	private const float THUMB_DISTANCE_STOP_OFFSET = 0.04f;

	private const float THUMB_MAX_DOT = 0.5f;

	private const float PINCH_HQ_VIEW_ANGLE_THRESHOLD = 40f;

	private readonly HandJointId[] THUMB_JOINTS_SELECT = new HandJointId[2]
	{
		HandJointId.HandThumb3,
		HandJointId.HandThumbTip
	};

	private readonly HandJointId[] THUMB_JOINTS_MAINTAIN = new HandJointId[3]
	{
		HandJointId.HandThumb2,
		HandJointId.HandThumb3,
		HandJointId.HandThumbTip
	};

	private readonly HandJointId[] INDEX_JOINTS = new HandJointId[4]
	{
		HandJointId.HandIndex1,
		HandJointId.HandIndex2,
		HandJointId.HandIndex3,
		HandJointId.HandIndexTip
	};

	private readonly FingerPinchData[] _fingersPinchData = new FingerPinchData[5]
	{
		new FingerPinchData(HandFinger.Thumb),
		new FingerPinchData(HandFinger.Index),
		new FingerPinchData(HandFinger.Middle),
		new FingerPinchData(HandFinger.Ring),
		new FingerPinchData(HandFinger.Pinky)
	};

	private IHmd _hmd;

	private readonly ShadowHand _shadowHand = new ShadowHand();

	private float _handScale;

	private Pose _rootPose;

	private float DistanceStart
	{
		get
		{
			if (!_isPinchVisibilityGood)
			{
				return 0.02f;
			}
			return 0.016f;
		}
	}

	private float DistanceStopMax
	{
		get
		{
			if (!_isPinchVisibilityGood)
			{
				return 0.1f;
			}
			return 0.1f;
		}
	}

	private float DistanceStopOffset
	{
		get
		{
			if (!_isPinchVisibilityGood)
			{
				return 0.04f;
			}
			return 0.016f;
		}
	}

	public PinchGrabAPI(IHmd hmd = null)
	{
		_hmd = hmd;
	}

	public bool GetFingerIsGrabbing(HandFinger finger)
	{
		return _fingersPinchData[(int)finger].IsPinching;
	}

	public Vector3 GetWristOffsetLocal()
	{
		float num = _fingersPinchData[0].PinchStrength;
		Vector3 tipPosition = _fingersPinchData[0].TipPosition;
		Vector3 result = tipPosition;
		for (int i = 1; i < 5; i++)
		{
			float pinchStrength = _fingersPinchData[i].PinchStrength;
			if (pinchStrength > num)
			{
				num = pinchStrength;
				Vector3 tipPosition2 = _fingersPinchData[i].TipPosition;
				result = (tipPosition + tipPosition2) * 0.5f;
			}
		}
		return result;
	}

	public bool GetFingerIsGrabbingChanged(HandFinger finger, bool targetPinchState)
	{
		if (_fingersPinchData[(int)finger].IsPinchingChanged)
		{
			return _fingersPinchData[(int)finger].IsPinching == targetPinchState;
		}
		return false;
	}

	public float GetFingerGrabScore(HandFinger finger)
	{
		return _fingersPinchData[(int)finger].PinchStrength;
	}

	public void Update(IHand hand)
	{
		hand.GetRootPose(out var pose);
		hand.GetJointPosesLocal(out var localJointPoses);
		Update(localJointPoses, hand.Handedness, pose, hand.Scale);
	}

	internal void Update(IReadOnlyList<Pose> handPoses, Handedness handedness, Pose rootPose, float handScale)
	{
		ClearState();
		_shadowHand.SetRoot(Pose.identity);
		_shadowHand.FromJoints(handPoses, flipHandedness: false);
		_rootPose = rootPose;
		_handScale = handScale;
		_isPinchVisibilityGood = PinchHasGoodVisibility(handedness);
		UpdateThumb(handedness);
		UpdateFinger(HandFinger.Index);
		UpdateFinger(HandFinger.Middle);
		UpdateFinger(HandFinger.Ring);
		UpdateFinger(HandFinger.Pinky);
	}

	private void UpdateThumb(Handedness handedness)
	{
		int num = 0;
		_fingersPinchData[num].UpdateTipPosition(_shadowHand);
		float distance = float.PositiveInfinity;
		if (IsThumbNearIndex(handedness))
		{
			Vector3 position = _shadowHand.GetWorldPose(HandJointId.HandThumb3).position;
			Vector3 tipPosition = _fingersPinchData[num].TipPosition;
			distance = GetClosestDistanceToJoints(position, tipPosition, INDEX_JOINTS, 0.5f);
		}
		UpdatePinchData(distance, num, 0.03f, 0.04f, 0.05f);
	}

	private bool IsThumbNearIndex(Handedness handedness)
	{
		Pose worldPose = _shadowHand.GetWorldPose(HandJointId.HandThumbTip);
		Pose worldPose2 = _shadowHand.GetWorldPose(HandJointId.HandIndex2);
		Vector3 inNormal = worldPose2.rotation * ((handedness == Handedness.Left) ? Constants.LeftThumbSide : Constants.RightThumbSide);
		float num = Mathf.Abs(new Plane(inNormal, worldPose2.position).GetDistanceToPoint(worldPose.position));
		if (num > 0f)
		{
			return num < 0.05f;
		}
		return false;
	}

	private void UpdateFinger(HandFinger finger)
	{
		_fingersPinchData[(int)finger].UpdateTipPosition(_shadowHand);
		float distance = float.PositiveInfinity;
		if (_fingersPinchData[(int)finger].IsPinching)
		{
			distance = GetClosestDistanceToJoints(_fingersPinchData[(int)finger].TipPosition, THUMB_JOINTS_MAINTAIN);
		}
		if (IsPointNearThumb(_fingersPinchData[(int)finger].TipPosition, THUMB_JOINTS_SELECT))
		{
			distance = GetClosestDistanceToJoints(_fingersPinchData[(int)finger].TipPosition, THUMB_JOINTS_SELECT);
		}
		UpdatePinchData(distance, (int)finger, DistanceStart, DistanceStopOffset, DistanceStopMax);
	}

	private void UpdatePinchData(float distance, int fingerIndex, float distanceStart, float distanceStopOffset, float distanceStopMax)
	{
		_fingersPinchData[fingerIndex].UpdateIsPinching(distance, distanceStart, distanceStopOffset, distanceStopMax);
		float value = (distance - distanceStart) / (distanceStopMax - distanceStart);
		float pinchStrength = 1f - Mathf.Clamp01(value);
		_fingersPinchData[fingerIndex].PinchStrength = pinchStrength;
	}

	private void ClearState()
	{
		for (int i = 0; i < 5; i++)
		{
			_fingersPinchData[i].ClearState();
		}
	}

	private bool IsPointNearThumb(Vector3 position, HandJointId[] thumbJoints)
	{
		Pose worldPose = _shadowHand.GetWorldPose(thumbJoints[0]);
		Pose worldPose2 = _shadowHand.GetWorldPose(thumbJoints[1]);
		Vector3 position2 = worldPose.position;
		Vector3 rhs = worldPose2.position - position2;
		return Vector3.Dot(Vector3.Project(position - position2, rhs.normalized), rhs) > 0f;
	}

	private float GetClosestDistanceToJoints(Vector3 edgeStart, Vector3 edgeEnd, HandJointId[] targetJoints, float maximumDotAllowed = 1f)
	{
		float num = float.PositiveInfinity;
		for (int i = 0; i < targetJoints.Length - 1; i++)
		{
			Pose worldPose = _shadowHand.GetWorldPose(targetJoints[i]);
			Pose worldPose2 = _shadowHand.GetWorldPose(targetJoints[i + 1]);
			if (!(maximumDotAllowed < 1f) || !(Vector3.Dot((edgeEnd - edgeStart).normalized, (worldPose2.position - worldPose.position).normalized) >= maximumDotAllowed))
			{
				float b = DistanceSegmentToSegment(edgeStart, edgeEnd, worldPose.position, worldPose2.position);
				num = Mathf.Min(num, b);
			}
		}
		return num;
	}

	private float GetClosestDistanceToJoints(Vector3 position, HandJointId[] targetJoints)
	{
		float num = float.PositiveInfinity;
		for (int i = 0; i < targetJoints.Length - 1; i++)
		{
			Pose worldPose = _shadowHand.GetWorldPose(targetJoints[i]);
			Pose worldPose2 = _shadowHand.GetWorldPose(targetJoints[i + 1]);
			num = Mathf.Min(num, DistancePointToSegment(position, worldPose.position, worldPose2.position));
		}
		return num;
	}

	private float DistancePointToSegment(Vector3 point, Vector3 a0, Vector3 a1)
	{
		Vector3 vector = a1 - a0;
		float num = Mathf.Clamp01(Vector3.Dot(point - a0, vector) / Vector3.Dot(vector, vector));
		return (a0 + num * vector - point).magnitude;
	}

	private float DistanceSegmentToSegment(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1)
	{
		Vector3 vector = a1 - a0;
		Vector3 vector2 = b1 - b0;
		Vector3 planeNormal = Vector3.Cross(vector, vector2);
		Vector3 vector3 = Vector3.ProjectOnPlane(a0, planeNormal);
		Vector3 vector4 = Vector3.ProjectOnPlane(b0, planeNormal);
		Vector3 vector5 = Vector3.ProjectOnPlane(vector, planeNormal);
		Vector3 onNormal = Vector3.ProjectOnPlane(vector2, planeNormal);
		Vector3 vector6 = vector4 + Vector3.Project(vector3 - vector4, onNormal) - vector3;
		float num = Vector3.Dot(vector5.normalized, vector6.normalized);
		float num2 = vector6.magnitude / num;
		Vector3 vector7 = a0 + vector * Mathf.Clamp01(num2 / vector5.magnitude);
		Vector3 vector8 = Vector3.Project(vector7 - b0, vector2);
		if (Vector3.Dot(vector8, vector2) < 0f)
		{
			vector8 = Vector3.zero;
		}
		else if (vector8.sqrMagnitude > vector2.sqrMagnitude)
		{
			vector8 = vector2;
		}
		Vector3 b2 = b0 + vector8;
		return Vector3.Distance(vector7, b2);
	}

	private bool PinchHasGoodVisibility(Handedness handedness)
	{
		if (_hmd == null || !_hmd.TryGetRootPose(out var pose))
		{
			return false;
		}
		Vector3 from = _rootPose.rotation * ((handedness == Handedness.Left) ? Constants.LeftPinkySide : Constants.RightPinkySide);
		Vector3 forward = pose.forward;
		return Vector3.Angle(from, forward) <= 40f;
	}
}
