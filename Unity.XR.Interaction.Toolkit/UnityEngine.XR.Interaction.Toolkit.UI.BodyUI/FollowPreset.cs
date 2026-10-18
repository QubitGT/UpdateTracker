using System;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;

[Serializable]
public class FollowPreset
{
	[Header("Local Space Anchor Transform")]
	[Tooltip("Local space anchor position for the right hand.")]
	public Vector3 rightHandLocalPosition;

	[Tooltip("Local space anchor position for the left hand.")]
	public Vector3 leftHandLocalPosition;

	[Tooltip("Local space anchor rotation for the right hand.")]
	public Vector3 rightHandLocalRotation;

	[Tooltip("Local space anchor rotation for the left hand.")]
	public Vector3 leftHandLocalRotation;

	[Header("Hand anchor angle constraints")]
	[Tooltip("Reference axis equivalent used for comparisons with the user's gaze direction and the world up direction.")]
	public FollowReferenceAxis palmReferenceAxis = FollowReferenceAxis.Down;

	[Tooltip("Given that the default reference hand for menus is the left hand, it may be required to mirror the reference axis for the right hand.")]
	public bool invertAxisForRightHand;

	[Tooltip("Whether or not check if the palm reference axis is facing the user.")]
	public bool requirePalmFacingUser;

	[Tooltip("The angle threshold in degrees to check if the palm reference axis is facing the user.")]
	public float palmFacingUserDegreeAngleThreshold;

	private float m_PalmFacingUserDotThreshold;

	[Tooltip("Whether or not check if the palm reference axis is facing up.")]
	public bool requirePalmFacingUp;

	[Tooltip("The angle threshold in degrees to check if the palm reference axis is facing up.")]
	public float palmFacingUpDegreeAngleThreshold;

	private float m_PalmFacingUpDotThreshold;

	[Header("Snap To gaze config")]
	[Tooltip("Whether to snap the following element to the gaze direction.")]
	public bool snapToGaze;

	[Tooltip("The angle threshold in degrees to snap the following element to the gaze direction.")]
	public float snapToGazeAngleThreshold;

	private float m_SnapToGazeDotThreshold;

	[Header("Hide delay config")]
	[Tooltip("The amount of time in seconds to wait before hiding the following element after the hand is no longer tracked.")]
	public float hideDelaySeconds = 0.25f;

	[Header("Smoothing Config")]
	[Tooltip("Whether to allow smoothing of the following element position and rotation.")]
	public bool allowSmoothing = true;

	[Tooltip("The lower bound of smoothing to apply.")]
	public float followLowerSmoothingValue = 10f;

	[Tooltip("The upper bound of smoothing to apply.")]
	public float followUpperSmoothingValue = 16f;

	public float palmFacingUserDotThreshold => m_PalmFacingUserDotThreshold;

	public float palmFacingUpDotThreshold => m_PalmFacingUpDotThreshold;

	public float snapToGazeDotThreshold => m_SnapToGazeDotThreshold;

	public void ApplyPreset(Transform leftTrackingOffset, Transform rightTrackingOffset)
	{
		leftTrackingOffset.SetLocalPose(new Pose(leftHandLocalPosition, Quaternion.Euler(leftHandLocalRotation)));
		rightTrackingOffset.SetLocalPose(new Pose(rightHandLocalPosition, Quaternion.Euler(rightHandLocalRotation)));
		ComputeDotProductThresholds();
	}

	public void ComputeDotProductThresholds()
	{
		m_PalmFacingUserDotThreshold = AngleToDot(palmFacingUserDegreeAngleThreshold);
		m_PalmFacingUpDotThreshold = AngleToDot(palmFacingUpDegreeAngleThreshold);
		m_SnapToGazeDotThreshold = AngleToDot(snapToGazeAngleThreshold);
	}

	private static float AngleToDot(float angleDeg)
	{
		return Mathf.Cos(MathF.PI / 180f * angleDeg);
	}

	public Vector3 GetReferenceAxisForTrackingAnchor(Transform trackingRoot, bool isRightHand)
	{
		return trackingRoot.TransformDirection(GetLocalAxis(isRightHand));
	}

	private Vector3 GetLocalAxis(bool isRightHand)
	{
		Vector3 result = Vector3.zero;
		bool flag = isRightHand && invertAxisForRightHand;
		switch (palmReferenceAxis)
		{
		case FollowReferenceAxis.Right:
			result = (flag ? Vector3.left : Vector3.right);
			break;
		case FollowReferenceAxis.Up:
			result = (flag ? Vector3.down : Vector3.up);
			break;
		case FollowReferenceAxis.Forward:
			result = (flag ? Vector3.back : Vector3.forward);
			break;
		case FollowReferenceAxis.Left:
			result = (flag ? Vector3.right : Vector3.left);
			break;
		case FollowReferenceAxis.Down:
			result = (flag ? Vector3.up : Vector3.down);
			break;
		case FollowReferenceAxis.Back:
			result = (flag ? Vector3.forward : Vector3.back);
			break;
		}
		return result;
	}
}
