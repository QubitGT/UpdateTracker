using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[Serializable]
public class XRDistanceEvaluator : XRTargetEvaluator
{
	[Tooltip("The maximum distance from the Interactor. Any target from this distance will receive a 0 normalized score.")]
	[SerializeField]
	private float m_MaxDistance = 1f;

	public float maxDistance
	{
		get
		{
			return m_MaxDistance;
		}
		set
		{
			m_MaxDistance = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		base.weight = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0.5f), new Keyframe(1f, 1f, 2f, 2f));
	}

	protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
	{
		if (Mathf.Approximately(m_MaxDistance, 0f))
		{
			return 0f;
		}
		using (new XRInteractableUtility.AllowTriggerCollidersScope(newAllowTriggerColliders: true))
		{
			return 1f - Mathf.Clamp01(target.GetDistanceSqrToInteractor(interactor) / (m_MaxDistance * m_MaxDistance));
		}
	}
}
