using Liv.Lck.GorillaTag;
using Liv.Lck.Smoothing;
using UnityEngine;

public class GtThirdPersonCameraBehaviour : MonoBehaviour
{
	public bool front = true;

	public float distance = 2f;

	public float heightOffsetAngle;

	public float shoulderOffset = 0.25f;

	public float positionalSmoothness;

	public float rotationalSmoothness;

	public LayerMask cameraCollisionMask;

	public float cameraRadius = 0.1f;

	private KalmanFilterVector3 _positionFilter;

	private KalmanFilterQuaternion _rotationFilter;

	private const float DECAY = 16f;

	private void OnEnable()
	{
		if (GtTag.TryGetTransform(GtTagType.HMD, out var transform))
		{
			_positionFilter = new KalmanFilterVector3(transform.position);
			_rotationFilter = new KalmanFilterQuaternion(transform.rotation);
		}
	}

	private void LateUpdate()
	{
		UpdateCamera(useLerp: true);
	}

	private void UpdateCamera(bool useLerp)
	{
		if (GtTag.TryGetTransform(GtTagType.HMD, out var transform))
		{
			Quaternion quaternion = _rotationFilter.Update(transform.rotation, Time.deltaTime, useLerp ? rotationalSmoothness : 0f);
			Vector3 vector = transform.TransformPoint(Vector3.right * shoulderOffset);
			Vector3 eulerAngles = quaternion.eulerAngles;
			eulerAngles.x += (front ? (0f - heightOffsetAngle) : heightOffsetAngle);
			Vector3 vector2 = (front ? Vector3.forward : Vector3.back);
			Vector3 vector3 = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0f) * vector2;
			Vector3 lossyScale = transform.lossyScale;
			float num = (lossyScale.x + lossyScale.y + lossyScale.z) * 0.333333f;
			float num2 = distance * num;
			float radius = cameraRadius * num;
			float num3 = (useLerp ? Lerp(Vector3.Distance(vector, base.transform.position), num2, Time.deltaTime * 0.1f) : num2);
			if (Physics.SphereCast(new Ray(vector, vector3), radius, out var hitInfo, 10f, cameraCollisionMask, QueryTriggerInteraction.Ignore))
			{
				num3 = Mathf.Min(num2, hitInfo.distance);
			}
			Vector3 vector4 = vector + vector3 * num3;
			Quaternion rotation = Quaternion.LookRotation((vector - vector4).normalized, Vector3.up);
			base.transform.SetPositionAndRotation(vector4, rotation);
		}
	}

	public void UpdateCameraWithoutSmoothing()
	{
		UpdateCamera(useLerp: false);
	}

	public static float Lerp(float a, float b, float t)
	{
		return b + (a - b) * Mathf.Exp(-16f * t);
	}

	public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
	{
		return b + (a - b) * Mathf.Exp(-16f * t);
	}

	public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
	{
		return b + (a - b) * Mathf.Exp(-16f * t);
	}

	public static Vector4 Lerp(Vector4 a, Vector4 b, float t)
	{
		return b + (a - b) * Mathf.Exp(-16f * t);
	}
}
