using UnityEngine;

namespace Oculus.Interaction.Demo;

public class WaterSprayNozzleTransformer : MonoBehaviour, ITransformer
{
	[SerializeField]
	private float _factor = 3f;

	[SerializeField]
	private float _snapAngle = 90f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _snappiness = 0.8f;

	[SerializeField]
	private int _maxSteps = 1;

	private float _relativeAngle;

	private int _stepsCount;

	private IGrabbable _grabbable;

	private Pose _previousGrabPose;

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
	}

	public void BeginTransform()
	{
		_previousGrabPose = _grabbable.GrabPoints[0];
		_relativeAngle = 0f;
		_stepsCount = 0;
	}

	public void UpdateTransform()
	{
		Pose previousGrabPose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		Vector3 forward = Vector3.forward;
		Vector3 vector = transform.TransformDirection(forward);
		Vector3 normalized = Vector3.ProjectOnPlane(_previousGrabPose.right, vector).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(previousGrabPose.right, vector).normalized;
		float num = Vector3.SignedAngle(normalized, normalized2, vector) * _factor;
		_relativeAngle += num;
		if (Mathf.Abs(_relativeAngle) > _snapAngle * (1f - _snappiness) && Mathf.Abs((float)_stepsCount + Mathf.Sign(_relativeAngle)) <= (float)_maxSteps)
		{
			int num2 = Mathf.FloorToInt((transform.localEulerAngles.z + _snappiness * 0.5f) / _snapAngle);
			float num3 = Mathf.Sign(_relativeAngle);
			float z = ((num3 > 0f) ? (_snapAngle * (float)(num2 + 1)) : (_snapAngle * (float)num2));
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.z = z;
			transform.localEulerAngles = localEulerAngles;
			_relativeAngle = 0f;
			_stepsCount += (int)num3;
		}
		else
		{
			transform.Rotate(vector, num, Space.World);
		}
		_previousGrabPose = previousGrabPose;
	}

	public void EndTransform()
	{
	}
}
