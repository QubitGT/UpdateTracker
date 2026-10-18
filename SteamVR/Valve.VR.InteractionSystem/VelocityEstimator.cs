using System;
using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem;

public class VelocityEstimator : MonoBehaviour
{
	[Tooltip("How many frames to average over for computing velocity")]
	public int velocityAverageFrames = 5;

	[Tooltip("How many frames to average over for computing angular velocity")]
	public int angularVelocityAverageFrames = 11;

	public bool estimateOnAwake;

	private Coroutine routine;

	private int sampleCount;

	private Vector3[] velocitySamples;

	private Vector3[] angularVelocitySamples;

	public void BeginEstimatingVelocity()
	{
		FinishEstimatingVelocity();
		routine = StartCoroutine(EstimateVelocityCoroutine());
	}

	public void FinishEstimatingVelocity()
	{
		if (routine != null)
		{
			StopCoroutine(routine);
			routine = null;
		}
	}

	public Vector3 GetVelocityEstimate()
	{
		Vector3 zero = Vector3.zero;
		int num = Mathf.Min(sampleCount, velocitySamples.Length);
		if (num != 0)
		{
			for (int i = 0; i < num; i++)
			{
				zero += velocitySamples[i];
			}
			zero *= 1f / (float)num;
		}
		return zero;
	}

	public Vector3 GetAngularVelocityEstimate()
	{
		Vector3 zero = Vector3.zero;
		int num = Mathf.Min(sampleCount, angularVelocitySamples.Length);
		if (num != 0)
		{
			for (int i = 0; i < num; i++)
			{
				zero += angularVelocitySamples[i];
			}
			zero *= 1f / (float)num;
		}
		return zero;
	}

	public Vector3 GetAccelerationEstimate()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 2 + sampleCount - velocitySamples.Length; i < sampleCount; i++)
		{
			if (i >= 2)
			{
				int num = i - 2;
				int num2 = i - 1;
				Vector3 vector = velocitySamples[num % velocitySamples.Length];
				Vector3 vector2 = velocitySamples[num2 % velocitySamples.Length];
				zero += vector2 - vector;
			}
		}
		return zero * (1f / Time.deltaTime);
	}

	private void Awake()
	{
		velocitySamples = new Vector3[velocityAverageFrames];
		angularVelocitySamples = new Vector3[angularVelocityAverageFrames];
		if (estimateOnAwake)
		{
			BeginEstimatingVelocity();
		}
	}

	private IEnumerator EstimateVelocityCoroutine()
	{
		sampleCount = 0;
		Vector3 previousPosition = base.transform.position;
		Quaternion previousRotation = base.transform.rotation;
		while (true)
		{
			yield return new WaitForEndOfFrame();
			float num = 1f / Time.deltaTime;
			int num2 = sampleCount % velocitySamples.Length;
			int num3 = sampleCount % angularVelocitySamples.Length;
			sampleCount++;
			velocitySamples[num2] = num * (base.transform.position - previousPosition);
			Quaternion quaternion = base.transform.rotation * Quaternion.Inverse(previousRotation);
			float num4 = 2f * Mathf.Acos(Mathf.Clamp(quaternion.w, -1f, 1f));
			if (num4 > MathF.PI)
			{
				num4 -= MathF.PI * 2f;
			}
			Vector3 vector = new Vector3(quaternion.x, quaternion.y, quaternion.z);
			if (vector.sqrMagnitude > 0f)
			{
				vector = num4 * num * vector.normalized;
			}
			angularVelocitySamples[num3] = vector;
			previousPosition = base.transform.position;
			previousRotation = base.transform.rotation;
		}
	}
}
