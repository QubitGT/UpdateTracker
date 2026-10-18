using UnityEngine;

namespace Oculus.Interaction;

public class OneGrabSphereTransformer : MonoBehaviour, ITransformer
{
	[SerializeField]
	private Transform _sphereCenter;

	[SerializeField]
	[Range(-90f, 90f)]
	private float _minAngle = -90f;

	[SerializeField]
	[Range(-90f, 90f)]
	private float _maxAngle = 90f;

	[SerializeField]
	private bool _scaleWithRadius;

	[SerializeField]
	private Vector3 _radiusToScaleRatio = new Vector3(1f, 1f, 1f);

	private IGrabbable _grabbable;

	private Pose _localToTransform;

	public float MinAngle
	{
		get
		{
			return _minAngle;
		}
		set
		{
			_minAngle = value;
			ClampMinMax();
		}
	}

	public float MaxAngle
	{
		get
		{
			return _maxAngle;
		}
		set
		{
			_maxAngle = value;
			ClampMinMax();
		}
	}

	public bool ScaleWithRadius
	{
		get
		{
			return _scaleWithRadius;
		}
		set
		{
			_scaleWithRadius = value;
		}
	}

	public Vector3 RadiusToScaleRatio
	{
		get
		{
			return _radiusToScaleRatio;
		}
		set
		{
			_radiusToScaleRatio = value;
		}
	}

	private void ClampMinMax()
	{
		_minAngle = Mathf.Clamp(_minAngle, -90f, 90f);
		_maxAngle = Mathf.Clamp(_maxAngle, _minAngle, 90f);
	}

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
		ClampMinMax();
	}

	public void BeginTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		_localToTransform = new Pose(transform.InverseTransformPoint(pose.position), Quaternion.Inverse(transform.rotation) * pose.rotation);
	}

	public void UpdateTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		Vector3 vector = pose.position - _sphereCenter.position;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		if (_scaleWithRadius)
		{
			Vector3 vector2 = new Vector3(_localToTransform.position.x * _radiusToScaleRatio.x, _localToTransform.position.y * _radiusToScaleRatio.y, 0f);
			float num4 = 1f - (0f - _localToTransform.position.z) * _radiusToScaleRatio.z;
			float num5 = num4 * num4;
			float num6 = vector2.x * vector2.x / num5;
			float num7 = vector2.y * vector2.y / num5;
			num = sqrMagnitude / (1f + num6 + num7);
			num2 = Mathf.Sqrt(num);
			num3 = num2 / (1f - (0f - _localToTransform.position.z) * _radiusToScaleRatio.z);
			Vector3 vector3 = ((transform.parent != null) ? transform.parent.lossyScale : Vector3.one);
			Vector3 vector4 = num3 * _radiusToScaleRatio;
			transform.localScale = new Vector3(vector4.x / vector3.x, vector4.y / vector3.y, vector4.z / vector3.z);
		}
		else
		{
			float sqrMagnitude2 = transform.TransformVector(new Vector3(_localToTransform.position.x, _localToTransform.position.y, 0f)).sqrMagnitude;
			num = sqrMagnitude - sqrMagnitude2;
			if (num <= 0f)
			{
				return;
			}
			num2 = Mathf.Sqrt(num);
			float num8 = transform.TransformVector(new Vector3(0f, 0f, _localToTransform.position.z)).magnitude;
			if (_localToTransform.position.z < 0f)
			{
				num8 *= -1f;
			}
			num3 = num2 - num8;
		}
		float sqrMagnitude3 = transform.TransformVector(new Vector3(0f, _localToTransform.position.y, 0f)).sqrMagnitude;
		float num9 = Mathf.Sqrt(sqrMagnitude3 + num);
		float num10 = Mathf.Asin(Mathf.Clamp(vector.y / num9, -1f, 1f)) * 57.29578f;
		float num11 = Mathf.Sqrt(sqrMagnitude3);
		if (_localToTransform.position.y < 0f)
		{
			num11 *= -1f;
		}
		float num12 = Mathf.Atan2(0f - num11, num2) * 57.29578f;
		num10 += num12;
		num10 = Mathf.Clamp(num10, _minAngle, _maxAngle);
		Quaternion quaternion = Quaternion.AngleAxis(0f - num10, Vector3.right);
		Pose pose2 = new Pose(quaternion * (num3 * Vector3.forward), quaternion);
		Vector3 lossyScale = transform.lossyScale;
		Vector3 vector5 = new Vector3(lossyScale.x * _localToTransform.position.x, lossyScale.y * _localToTransform.position.y, lossyScale.z * _localToTransform.position.z);
		Vector3 vector6 = pose2.position + pose2.rotation * vector5;
		Vector3 from = new Vector3(vector6.x, 0f, vector6.z);
		Vector3 to = new Vector3(vector.x, 0f, vector.z);
		quaternion = Quaternion.AngleAxis(Vector3.SignedAngle(from, to, Vector3.up), Vector3.up) * quaternion;
		transform.position = _sphereCenter.position + quaternion * (num3 * Vector3.forward);
		transform.rotation = quaternion;
	}

	public void EndTransform()
	{
	}
}
