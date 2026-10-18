using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[Serializable]
public class XRAngleGazeEvaluator : XRTargetEvaluator
{
	[Tooltip("The Transform whose forward direction is used to evaluate the target Interactable angle. If none is specified, during OnEnable this property is initialized with the XROrigin Camera.")]
	[SerializeField]
	private Transform m_GazeTransform;

	[Tooltip("The maximum value an angle can be evaluated as before the Interactor receives a normalized score of 0. Think of it as a field-of-view angle.")]
	[SerializeField]
	[Range(0f, 180f)]
	private float m_MaxAngle = 60f;

	public Transform gazeTransform
	{
		get
		{
			return m_GazeTransform;
		}
		set
		{
			m_GazeTransform = value;
		}
	}

	public float maxAngle
	{
		get
		{
			return m_MaxAngle;
		}
		set
		{
			m_MaxAngle = Mathf.Clamp(value, 0f, 180f);
		}
	}

	private static Camera GetXROriginCamera()
	{
		if (!ComponentLocatorUtility<XROrigin>.TryFindComponent(out var component))
		{
			return null;
		}
		return component.Camera;
	}

	private void InitializeGazeTransform()
	{
		Camera xROriginCamera = GetXROriginCamera();
		if (xROriginCamera != null && xROriginCamera.enabled && xROriginCamera.gameObject.activeInHierarchy)
		{
			m_GazeTransform = xROriginCamera.transform;
		}
		else
		{
			Debug.LogWarning("Couldn't find an active XROrigin Camera for XRAngleGazeEvaluator", base.filter);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_GazeTransform == null)
		{
			InitializeGazeTransform();
		}
	}

	public override void Reset()
	{
		base.Reset();
		InitializeGazeTransform();
	}

	protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
	{
		Transform transform = gazeTransform;
		if (transform == null || m_MaxAngle <= 0f)
		{
			return 0f;
		}
		Vector3 vector;
		if (target is XRBaseInteractable xRBaseInteractable)
		{
			vector = xRBaseInteractable.GetDistance(transform.position).point;
		}
		else
		{
			Transform transform2 = target.transform;
			if (transform2 == null)
			{
				return 0f;
			}
			vector = transform2.position;
		}
		float num = Vector3.Angle(vector - transform.position, transform.forward) * 2f / m_MaxAngle;
		return 1f - num;
	}
}
