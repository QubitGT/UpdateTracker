using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

[AddComponentMenu("XR/Locomotion/Climb Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbProvider.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class ClimbProvider : LocomotionProvider, IGravityController
{
	[SerializeField]
	[Tooltip("List of providers to disable while climb locomotion is active. If empty, no providers will be disabled by this component while climbing.")]
	private List<LocomotionProvider> m_ProvidersToDisable = new List<LocomotionProvider>();

	[SerializeField]
	[Tooltip("Whether to allow falling when climb locomotion ends. Disable to pause gravity when releasing, keeping the user from falling.")]
	private bool m_EnableGravityOnClimbEnd = true;

	[SerializeField]
	[Tooltip("Climb locomotion settings. Can be overridden by the Climb Interactable used for locomotion.")]
	private ClimbSettingsDatumProperty m_ClimbSettings = new ClimbSettingsDatumProperty(new ClimbSettings());

	private GravityProvider m_GravityProvider;

	private readonly List<IXRSelectInteractor> m_GrabbingInteractors = new List<IXRSelectInteractor>();

	private readonly List<ClimbInteractable> m_GrabbedClimbables = new List<ClimbInteractable>();

	private Vector3 m_InteractorAnchorWorldPosition;

	private Vector3 m_InteractorAnchorClimbSpacePosition;

	private List<LocomotionProvider> m_EnabledProvidersToDisable = new List<LocomotionProvider>();

	public List<LocomotionProvider> providersToDisable
	{
		get
		{
			return m_ProvidersToDisable;
		}
		set
		{
			m_ProvidersToDisable = value;
		}
	}

	public bool enableGravityOnClimbEnd
	{
		get
		{
			return m_EnableGravityOnClimbEnd;
		}
		set
		{
			m_EnableGravityOnClimbEnd = value;
		}
	}

	public ClimbSettingsDatumProperty climbSettings
	{
		get
		{
			return m_ClimbSettings;
		}
		set
		{
			m_ClimbSettings = value;
		}
	}

	public ClimbInteractable climbAnchorInteractable
	{
		get
		{
			if (m_GrabbedClimbables.Count > 0)
			{
				return m_GrabbedClimbables[m_GrabbedClimbables.Count - 1];
			}
			return null;
		}
	}

	public IXRSelectInteractor climbAnchorInteractor
	{
		get
		{
			if (m_GrabbingInteractors.Count > 0)
			{
				return m_GrabbingInteractors[m_GrabbingInteractors.Count - 1];
			}
			return null;
		}
	}

	public XROriginMovement transformation { get; set; } = new XROriginMovement();


	public bool canProcess => base.isActiveAndEnabled;

	public bool gravityPaused { get; protected set; }

	public event Action<ClimbProvider> climbAnchorUpdated;

	protected override void Awake()
	{
		base.Awake();
		if (m_ClimbSettings == null || m_ClimbSettings.Value == null)
		{
			m_ClimbSettings = new ClimbSettingsDatumProperty(new ClimbSettings());
		}
		ComponentLocatorUtility<GravityProvider>.TryFindComponent(out m_GravityProvider);
	}

	protected virtual void Update()
	{
		if (!base.isLocomotionActive)
		{
			return;
		}
		if (m_GrabbingInteractors.Count > 0)
		{
			if (base.locomotionState == LocomotionState.Preparing)
			{
				TryStartLocomotionImmediately();
			}
			int index = m_GrabbingInteractors.Count - 1;
			IXRSelectInteractor iXRSelectInteractor = m_GrabbingInteractors[index];
			ClimbInteractable climbInteractable = m_GrabbedClimbables[index];
			if (iXRSelectInteractor == null || climbInteractable == null)
			{
				FinishLocomotion();
			}
			else
			{
				StepClimbMovement(climbInteractable, iXRSelectInteractor);
			}
		}
		else
		{
			FinishLocomotion();
		}
	}

	public void StartClimbGrab(ClimbInteractable climbInteractable, IXRSelectInteractor interactor)
	{
		if (base.mediator.xrOrigin?.Origin == null)
		{
			return;
		}
		bool num = base.locomotionState == LocomotionState.Moving || base.locomotionState == LocomotionState.Preparing;
		m_GrabbingInteractors.Add(interactor);
		m_GrabbedClimbables.Add(climbInteractable);
		UpdateClimbAnchor(climbInteractable, interactor);
		TryPrepareLocomotion();
		if (!num)
		{
			TryLockGravity(GravityOverride.ForcedOff);
		}
		foreach (LocomotionProvider item in m_ProvidersToDisable)
		{
			if (!(item == null) && item.enabled)
			{
				item.enabled = false;
				m_EnabledProvidersToDisable.Add(item);
			}
		}
	}

	public void FinishClimbGrab(IXRSelectInteractor interactor)
	{
		int num = m_GrabbingInteractors.IndexOf(interactor);
		if (num >= 0)
		{
			if (num > 0 && num == m_GrabbingInteractors.Count - 1)
			{
				int index = num - 1;
				UpdateClimbAnchor(m_GrabbedClimbables[index], m_GrabbingInteractors[index]);
			}
			m_GrabbingInteractors.RemoveAt(num);
			m_GrabbedClimbables.RemoveAt(num);
		}
	}

	private void UpdateClimbAnchor(ClimbInteractable climbInteractable, IXRInteractor interactor)
	{
		Transform climbTransform = climbInteractable.climbTransform;
		m_InteractorAnchorWorldPosition = interactor.transform.position;
		m_InteractorAnchorClimbSpacePosition = climbTransform.InverseTransformPoint(m_InteractorAnchorWorldPosition);
		this.climbAnchorUpdated?.Invoke(this);
	}

	private void StepClimbMovement(ClimbInteractable currentClimbInteractable, IXRSelectInteractor currentInteractor)
	{
		ClimbSettings activeClimbSettings = GetActiveClimbSettings(currentClimbInteractable);
		bool allowFreeXMovement = activeClimbSettings.allowFreeXMovement;
		bool allowFreeYMovement = activeClimbSettings.allowFreeYMovement;
		bool allowFreeZMovement = activeClimbSettings.allowFreeZMovement;
		Vector3 position = currentInteractor.transform.position;
		Vector3 motion;
		if (allowFreeXMovement && allowFreeYMovement && allowFreeZMovement)
		{
			motion = m_InteractorAnchorWorldPosition - position;
		}
		else
		{
			Transform climbTransform = currentClimbInteractable.climbTransform;
			Vector3 vector = climbTransform.InverseTransformPoint(position);
			Vector3 vector2 = m_InteractorAnchorClimbSpacePosition - vector;
			if (!allowFreeXMovement)
			{
				vector2.x = 0f;
			}
			if (!allowFreeYMovement)
			{
				vector2.y = 0f;
			}
			if (!allowFreeZMovement)
			{
				vector2.z = 0f;
			}
			motion = climbTransform.TransformVector(vector2);
		}
		transformation.motion = motion;
		TryQueueTransformation(transformation);
	}

	private void FinishLocomotion()
	{
		TryEndLocomotion();
		m_GrabbingInteractors.Clear();
		m_GrabbedClimbables.Clear();
		RemoveGravityLock();
		gravityPaused = !m_EnableGravityOnClimbEnd;
		foreach (LocomotionProvider item in m_EnabledProvidersToDisable)
		{
			if (!(item == null))
			{
				item.enabled = true;
			}
		}
		m_EnabledProvidersToDisable.Clear();
	}

	private ClimbSettings GetActiveClimbSettings(ClimbInteractable climbInteractable)
	{
		if (climbInteractable.climbSettingsOverride.Value != null)
		{
			return climbInteractable.climbSettingsOverride;
		}
		return m_ClimbSettings;
	}

	public bool TryLockGravity(GravityOverride gravityOverride)
	{
		if (m_GravityProvider != null)
		{
			return m_GravityProvider.TryLockGravity(this, gravityOverride);
		}
		return false;
	}

	public void RemoveGravityLock()
	{
		if (m_GravityProvider != null)
		{
			m_GravityProvider.UnlockGravity(this);
		}
	}

	void IGravityController.OnGroundedChanged(bool isGrounded)
	{
		OnGroundedChanged(isGrounded);
	}

	void IGravityController.OnGravityLockChanged(GravityOverride gravityOverride)
	{
		OnGravityLockChanged(gravityOverride);
	}

	protected virtual void OnGroundedChanged(bool isGrounded)
	{
		gravityPaused = false;
	}

	protected virtual void OnGravityLockChanged(GravityOverride gravityOverride)
	{
		if (gravityOverride == GravityOverride.ForcedOn)
		{
			gravityPaused = false;
		}
	}
}
