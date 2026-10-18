using System;
using Unity.XR.CoreUtils;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Attachment;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit.Interaction")]
public class AttachPointVelocityTracker : IAttachPointVelocityTracker, IAttachPointVelocityProvider
{
	private const int k_BufferSize = 20;

	private const float k_MinimumDeltaTime = 1E-05f;

	private readonly CircularBuffer<(Vector3 position, float time)> m_PositionTimeBuffer = new CircularBuffer<(Vector3, float)>(20);

	private readonly CircularBuffer<(Quaternion rotation, float time)> m_RotationTimeBuffer = new CircularBuffer<(Quaternion, float)>(20);

	private Vector3 m_AttachPointVelocity;

	private Vector3 m_AttachPointAngularVelocity;

	public void UpdateAttachPointVelocityData(Transform attachTransform)
	{
		UpdateAttachPointVelocityData(attachTransform, useXROriginTransform: false);
	}

	public void UpdateAttachPointVelocityData(Transform attachTransform, Transform xrOriginTransform)
	{
		UpdateAttachPointVelocityData(attachTransform, useXROriginTransform: true, xrOriginTransform);
	}

	private void UpdateAttachPointVelocityData(Transform attachTransform, bool useXROriginTransform, Transform xrOriginTransform = null)
	{
		float unscaledTime = Time.unscaledTime;
		bool num = useXROriginTransform && xrOriginTransform != null;
		Pose worldPose = attachTransform.GetWorldPose();
		Vector3 item = (num ? xrOriginTransform.InverseTransformPoint(worldPose.position) : worldPose.position);
		Quaternion item2 = (num ? (Quaternion.Inverse(xrOriginTransform.rotation) * worldPose.rotation) : worldPose.rotation);
		m_PositionTimeBuffer.Add((item, unscaledTime));
		m_RotationTimeBuffer.Add((item2, unscaledTime));
		m_AttachPointVelocity = ((m_PositionTimeBuffer.count > 1) ? CalculateVelocityWithWeightedLinearRegression() : Vector3.zero);
		m_AttachPointAngularVelocity = ((m_RotationTimeBuffer.count > 1) ? CalculateAngularVelocityWithWeightedRegression() : Vector3.zero);
	}

	private Vector3 CalculateVelocityWithWeightedLinearRegression()
	{
		int count = m_PositionTimeBuffer.count;
		if (count < 2)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		float num = 0f;
		Vector3 zero2 = Vector3.zero;
		float num2 = 0f;
		float num3 = 0f;
		float item = m_PositionTimeBuffer[0].time;
		float num4 = m_PositionTimeBuffer[count - 1].time - item;
		if (num4 < 1E-05f)
		{
			if (!Mathf.Approximately(num4, 0f))
			{
				return (m_PositionTimeBuffer[count - 1].position - m_PositionTimeBuffer[0].position) / num4;
			}
			return Vector3.zero;
		}
		for (int i = 0; i < count; i++)
		{
			float num5 = m_PositionTimeBuffer[i].time - item;
			Vector3 item2 = m_PositionTimeBuffer[i].position;
			float num6 = 1f + num5 / num4;
			zero += item2 * num6;
			num += num5 * num6;
			zero2 += item2 * (num5 * num6);
			num2 += num5 * num5 * num6;
			num3 += num6;
		}
		float num7 = num3 * num2 - num * num;
		if (Mathf.Approximately(num7, 0f))
		{
			return Vector3.zero;
		}
		return (num3 * zero2 - zero * num) / num7;
	}

	private Vector3 CalculateAngularVelocityWithWeightedRegression()
	{
		int count = m_RotationTimeBuffer.count;
		if (count < 2)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		float num = 0f;
		Vector3 zero2 = Vector3.zero;
		float num2 = 0f;
		float num3 = 0f;
		float item = m_RotationTimeBuffer[0].time;
		float num4 = m_RotationTimeBuffer[count - 1].time - item;
		if (num4 < 1E-05f)
		{
			return Vector3.zero;
		}
		Quaternion item2 = m_RotationTimeBuffer[0].rotation;
		for (int i = 1; i < count; i++)
		{
			float num5 = m_RotationTimeBuffer[i].time - item;
			(m_RotationTimeBuffer[i].rotation * Quaternion.Inverse(item2)).ToAngleAxis(out var angle, out var axis);
			if (angle > 180f)
			{
				angle -= 360f;
			}
			Vector3 vector = axis * (angle * (MathF.PI / 180f));
			float num6 = 1f + num5 / num4;
			zero += vector * num6;
			num += num5 * num6;
			zero2 += vector * (num5 * num6);
			num2 += num5 * num5 * num6;
			num3 += num6;
		}
		float num7 = num3 * num2 - num * num;
		if (Mathf.Approximately(num7, 0f))
		{
			return Vector3.zero;
		}
		return (num3 * zero2 - zero * num) / num7;
	}

	public void ResetVelocityTracking()
	{
		m_PositionTimeBuffer.Clear();
		m_RotationTimeBuffer.Clear();
		m_AttachPointVelocity = Vector3.zero;
		m_AttachPointAngularVelocity = Vector3.zero;
	}

	public Vector3 GetAttachPointVelocity()
	{
		return m_AttachPointVelocity;
	}

	public Vector3 GetAttachPointAngularVelocity()
	{
		return m_AttachPointAngularVelocity;
	}
}
