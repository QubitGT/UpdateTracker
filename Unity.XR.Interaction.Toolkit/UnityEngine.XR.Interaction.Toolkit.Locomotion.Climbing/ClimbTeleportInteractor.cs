using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

[AddComponentMenu("XR/Locomotion/Climb Teleport Interactor", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbTeleportInteractor.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class ClimbTeleportInteractor : XRBaseInteractor, IXRActivateInteractor, IXRInteractor
{
	[SerializeField]
	[Tooltip("The climb locomotion provider to query for active locomotion and climbed interactable.")]
	private ClimbProvider m_ClimbProvider;

	[SerializeField]
	[Tooltip("Optional settings for how the hovered teleport volume evaluates a destination anchor. Applies as an override to the teleport volume's settings if set to Use Value or if the asset reference is set.")]
	private TeleportVolumeDestinationSettingsDatumProperty m_DestinationEvaluationSettings = new TeleportVolumeDestinationSettingsDatumProperty(new TeleportVolumeDestinationSettings
	{
		enableDestinationEvaluationDelay = false,
		pollForDestinationChange = true
	});

	private readonly LinkedPool<ActivateEventArgs> m_ActivateEventArgs = new LinkedPool<ActivateEventArgs>(() => new ActivateEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<DeactivateEventArgs> m_DeactivateEventArgs = new LinkedPool<DeactivateEventArgs>(() => new DeactivateEventArgs(), null, null, null, collectionCheck: false);

	private TeleportationMultiAnchorVolume m_TargetTeleportVolume;

	private TeleportVolumeDestinationSettingsDatumProperty m_PreservedTeleportVolumeSettings;

	public ClimbProvider climbProvider
	{
		get
		{
			return m_ClimbProvider;
		}
		set
		{
			m_ClimbProvider = value;
		}
	}

	public TeleportVolumeDestinationSettingsDatumProperty destinationEvaluationSettings
	{
		get
		{
			return m_DestinationEvaluationSettings;
		}
		set
		{
			m_DestinationEvaluationSettings = value;
		}
	}

	public override bool isSelectActive
	{
		get
		{
			if (base.isSelectActive)
			{
				return base.isPerformingManualInteraction;
			}
			return false;
		}
	}

	public bool shouldActivate => false;

	public bool shouldDeactivate => false;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!(m_ClimbProvider == null) || ComponentLocatorUtility<ClimbProvider>.TryFindComponent(out m_ClimbProvider))
		{
			m_ClimbProvider.locomotionStateChanged += OnLocomotionStateChanged;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ReleaseTargetTeleportVolume();
		if (!(m_ClimbProvider == null))
		{
			m_ClimbProvider.locomotionStateChanged -= OnLocomotionStateChanged;
			m_ClimbProvider.climbAnchorUpdated -= OnClimbAnchorUpdated;
		}
	}

	private void OnLocomotionStateChanged(LocomotionProvider provider, LocomotionState state)
	{
		switch (state)
		{
		case LocomotionState.Moving:
			OnClimbBegin();
			break;
		case LocomotionState.Ended:
			OnClimbEnd();
			break;
		}
	}

	private void OnClimbBegin()
	{
		SetTargetTeleportVolume(m_ClimbProvider.climbAnchorInteractable);
		m_ClimbProvider.climbAnchorUpdated += OnClimbAnchorUpdated;
	}

	private void OnClimbEnd()
	{
		m_ClimbProvider.climbAnchorUpdated -= OnClimbAnchorUpdated;
		if (m_TargetTeleportVolume == null)
		{
			return;
		}
		switch (m_TargetTeleportVolume.teleportTrigger)
		{
		case BaseTeleportationInteractable.TeleportTrigger.OnSelectExited:
		case BaseTeleportationInteractable.TeleportTrigger.OnSelectEntered:
			StartManualInteraction((IXRSelectInteractable)m_TargetTeleportVolume);
			EndManualInteraction();
			break;
		case BaseTeleportationInteractable.TeleportTrigger.OnActivated:
		case BaseTeleportationInteractable.TeleportTrigger.OnDeactivated:
		{
			ActivateEventArgs v;
			using (m_ActivateEventArgs.Get(out v))
			{
				v.interactorObject = this;
				v.interactableObject = m_TargetTeleportVolume;
				((IXRActivateInteractable)m_TargetTeleportVolume).OnActivated(v);
			}
			DeactivateEventArgs v2;
			using (m_DeactivateEventArgs.Get(out v2))
			{
				v2.interactorObject = this;
				v2.interactableObject = m_TargetTeleportVolume;
				((IXRActivateInteractable)m_TargetTeleportVolume).OnDeactivated(v2);
			}
			break;
		}
		}
		ReleaseTargetTeleportVolume();
	}

	private void OnClimbAnchorUpdated(ClimbProvider provider)
	{
		SetTargetTeleportVolume(provider.climbAnchorInteractable);
	}

	private void SetTargetTeleportVolume(ClimbInteractable activeClimbInteractable)
	{
		TeleportationMultiAnchorVolume climbAssistanceTeleportVolume = activeClimbInteractable.climbAssistanceTeleportVolume;
		if (m_TargetTeleportVolume == climbAssistanceTeleportVolume)
		{
			return;
		}
		ReleaseTargetTeleportVolume();
		m_TargetTeleportVolume = climbAssistanceTeleportVolume;
		if (!(m_TargetTeleportVolume == null))
		{
			m_PreservedTeleportVolumeSettings = m_TargetTeleportVolume.destinationEvaluationSettings;
			if (destinationEvaluationSettings.Value != null)
			{
				m_TargetTeleportVolume.destinationEvaluationSettings = destinationEvaluationSettings;
			}
		}
	}

	private void ReleaseTargetTeleportVolume()
	{
		if (m_TargetTeleportVolume != null)
		{
			m_TargetTeleportVolume.destinationEvaluationSettings = m_PreservedTeleportVolumeSettings;
		}
		m_PreservedTeleportVolumeSettings = null;
		m_TargetTeleportVolume = null;
	}

	public override void GetValidTargets(List<IXRInteractable> targets)
	{
		targets.Clear();
		if (m_TargetTeleportVolume != null)
		{
			targets.Add(m_TargetTeleportVolume);
		}
	}

	public void GetActivateTargets(List<IXRActivateInteractable> targets)
	{
		targets.Clear();
		if (m_TargetTeleportVolume != null)
		{
			targets.Add(m_TargetTeleportVolume);
		}
	}

	Transform IXRInteractor.get_transform()
	{
		return base.transform;
	}
}
