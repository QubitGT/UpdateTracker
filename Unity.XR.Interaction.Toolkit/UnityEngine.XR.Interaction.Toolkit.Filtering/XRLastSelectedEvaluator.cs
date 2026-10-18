using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[Serializable]
public class XRLastSelectedEvaluator : XRTargetEvaluator, IXRTargetEvaluatorLinkable
{
	private readonly Dictionary<IXRInteractable, float> m_InteractableSelectionTimeMap = new Dictionary<IXRInteractable, float>();

	[Tooltip("Any Interactable which was last selected over Max Time seconds ago will receive a normalized score of 0.")]
	[SerializeField]
	private float m_MaxTime = 10f;

	public float maxTime
	{
		get
		{
			return m_MaxTime;
		}
		set
		{
			m_MaxTime = value;
		}
	}

	private void OnSelect(SelectEnterEventArgs args)
	{
		if (base.enabled)
		{
			IXRInteractable interactableObject = args.interactableObject;
			if (interactableObject != null)
			{
				m_InteractableSelectionTimeMap[interactableObject] = Time.time;
			}
		}
	}

	public virtual void OnLink(IXRInteractor interactor)
	{
		if (interactor is IXRSelectInteractor iXRSelectInteractor)
		{
			iXRSelectInteractor.selectEntered.AddListener(OnSelect);
		}
	}

	public virtual void OnUnlink(IXRInteractor interactor)
	{
		if (interactor is IXRSelectInteractor iXRSelectInteractor)
		{
			iXRSelectInteractor.selectEntered.RemoveListener(OnSelect);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_InteractableSelectionTimeMap.Clear();
	}

	protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
	{
		if (!m_InteractableSelectionTimeMap.TryGetValue(target, out var value) || m_MaxTime <= 0f)
		{
			return 0.5f;
		}
		return (1f - Mathf.Clamp01((Time.time - value) / m_MaxTime)) * 0.5f + 0.5f;
	}
}
