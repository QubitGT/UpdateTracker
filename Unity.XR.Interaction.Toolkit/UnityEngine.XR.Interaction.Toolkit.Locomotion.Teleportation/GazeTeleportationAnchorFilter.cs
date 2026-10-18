using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[CreateAssetMenu(fileName = "GazeTeleportationAnchorFilter", menuName = "XR/Locomotion/Gaze Teleportation Anchor Filter")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.GazeTeleportationAnchorFilter.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class GazeTeleportationAnchorFilter : ScriptableObject, ITeleportationVolumeAnchorFilter
{
	[SerializeField]
	[Range(0f, 180f)]
	[Tooltip("The maximum angle (in degrees) between the camera forward and the direction from the camera to an anchor for the anchor to be considered a valid destination.")]
	private float m_MaxGazeAngle = 90f;

	[SerializeField]
	[Tooltip("The curve used to score an anchor by its angle from the camera forward. The X axis is the normalized angle, where 0 is 0 degrees and 1 is the Max Gaze Angle. The Y axis is the score, where a higher value means a better destination.")]
	private AnimationCurve m_GazeAngleScoreCurve;

	[SerializeField]
	[Tooltip("Whether to weight an anchor's score by its distance from the user.")]
	private bool m_EnableDistanceWeighting = true;

	[SerializeField]
	[Tooltip("The curve used to weight an anchor's score by its distance from the user. The X axis is the normalized distance, where 0 is the closest anchor and 1 is the furthest anchor. The Y axis is the weight.")]
	private AnimationCurve m_DistanceWeightCurve;

	private float[] m_AnchorWeights;

	public float maxGazeAngle
	{
		get
		{
			return m_MaxGazeAngle;
		}
		set
		{
			m_MaxGazeAngle = value;
		}
	}

	public AnimationCurve gazeAngleScoreCurve
	{
		get
		{
			return m_GazeAngleScoreCurve;
		}
		set
		{
			m_GazeAngleScoreCurve = value;
		}
	}

	public bool enableDistanceWeighting
	{
		get
		{
			return m_EnableDistanceWeighting;
		}
		set
		{
			m_EnableDistanceWeighting = value;
		}
	}

	public AnimationCurve distanceWeightCurve
	{
		get
		{
			return m_DistanceWeightCurve;
		}
		set
		{
			m_DistanceWeightCurve = value;
		}
	}

	protected void Reset()
	{
		m_GazeAngleScoreCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, 0f), new Keyframe(1f, 0f, -2f, -2f));
		m_DistanceWeightCurve = new AnimationCurve(new Keyframe(0f, 0.1f, 0f, 0f), new Keyframe(0.01f, 0.1f, 0f, 0f), new Keyframe(0.05f, 1f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f));
	}

	public int GetDestinationAnchorIndex(TeleportationMultiAnchorVolume teleportationVolume)
	{
		List<Transform> anchorTransforms = teleportationVolume.anchorTransforms;
		if (m_AnchorWeights == null || m_AnchorWeights.Length != anchorTransforms.Count)
		{
			m_AnchorWeights = new float[anchorTransforms.Count];
		}
		XROrigin xrOrigin = teleportationVolume.teleportationProvider.mediator.xrOrigin;
		if (m_EnableDistanceWeighting)
		{
			Vector3 cameraFloorWorldPosition = xrOrigin.GetCameraFloorWorldPosition();
			float num = -1f;
			float num2 = float.MaxValue;
			for (int i = 0; i < anchorTransforms.Count; i++)
			{
				float sqrMagnitude = (anchorTransforms[i].position - cameraFloorWorldPosition).sqrMagnitude;
				m_AnchorWeights[i] = sqrMagnitude;
				if (sqrMagnitude > num)
				{
					num = sqrMagnitude;
				}
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
				}
			}
			for (int j = 0; j < anchorTransforms.Count; j++)
			{
				m_AnchorWeights[j] = m_DistanceWeightCurve.Evaluate((m_AnchorWeights[j] - num2) / (num - num2));
			}
		}
		else
		{
			for (int k = 0; k < anchorTransforms.Count; k++)
			{
				m_AnchorWeights[k] = 1f;
			}
		}
		int result = -1;
		Transform transform = xrOrigin.Camera.transform;
		Vector3 position = transform.position;
		Vector3 a = transform.forward;
		float num3 = 0f;
		for (int l = 0; l < anchorTransforms.Count; l++)
		{
			Vector3 b = Vector3.Normalize(anchorTransforms[l].position - position);
			BurstMathUtility.Angle(in a, in b, out var angle);
			if (!(angle > m_MaxGazeAngle))
			{
				float time = angle / m_MaxGazeAngle;
				float num4 = m_GazeAngleScoreCurve.Evaluate(time) * m_AnchorWeights[l];
				if (num4 > num3)
				{
					result = l;
					num3 = num4;
				}
			}
		}
		return result;
	}
}
