using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public abstract class BaseTeleportationInteractable : XRBaseInteractable, IXRReticleDirectionProvider
{
	public enum TeleportTrigger
	{
		OnSelectExited = 0,
		OnSelectEntered = 1,
		OnActivated = 2,
		OnDeactivated = 3,
		[Obsolete("OnSelectExit has been deprecated. Use OnSelectExited instead. (UnityUpgradable) -> OnSelectExited", true)]
		OnSelectExit = 0,
		[Obsolete("OnSelectEnter has been deprecated. Use OnSelectEntered instead. (UnityUpgradable) -> OnSelectEntered", true)]
		OnSelectEnter = 1,
		[Obsolete("OnActivate has been deprecated. Use OnActivated instead. (UnityUpgradable) -> OnActivated", true)]
		OnActivate = 2,
		[Obsolete("OnDeactivate has been deprecated. Use OnDeactivated instead. (UnityUpgradable) -> OnDeactivated", true)]
		OnDeactivate = 3
	}

	private const float k_DefaultNormalToleranceDegrees = 30f;

	[SerializeField]
	[Tooltip("The teleportation provider that this teleportation interactable will communicate teleport requests to. If no teleportation provider is configured, will attempt to find a teleportation provider.")]
	private TeleportationProvider m_TeleportationProvider;

	[SerializeField]
	[Tooltip("How to orient the rig after teleportation.\nSet to:\n\nWorld Space Up to stay oriented according to the world space up vector.\n\nSet to Target Up to orient according to the target BaseTeleportationInteractable Transform's up vector.\n\nSet to Target Up And Forward to orient according to the target BaseTeleportationInteractable Transform's rotation.\n\nSet to None to maintain the same orientation before and after teleporting.")]
	private MatchOrientation m_MatchOrientation;

	[SerializeField]
	[Tooltip("Whether or not to rotate the rig to match the forward direction of the attach transform of the selecting interactor.")]
	private bool m_MatchDirectionalInput;

	[SerializeField]
	[Tooltip("Specify when the teleportation will be triggered. Options map to when the trigger is pressed or when it is released.")]
	private TeleportTrigger m_TeleportTrigger;

	[SerializeField]
	[Tooltip("When enabled, this teleportation interactable will only be selectable by a ray interactor if its current hit normal is aligned with this object's up vector.")]
	private bool m_FilterSelectionByHitNormal;

	[SerializeField]
	[Tooltip("Sets the tolerance in degrees from this object's up vector for a hit normal to be considered aligned with the up vector.")]
	private float m_UpNormalToleranceDegrees = 30f;

	[SerializeField]
	private TeleportingEvent m_Teleporting = new TeleportingEvent();

	private readonly LinkedPool<TeleportingEventArgs> m_TeleportingEventArgs = new LinkedPool<TeleportingEventArgs>(() => new TeleportingEventArgs(), null, null, null, collectionCheck: false);

	private readonly Dictionary<IXRInteractor, Vector3> m_TeleportForwardPerInteractor = new Dictionary<IXRInteractor, Vector3>();

	private const string k_GenerateTeleportRequestDeprecated = "GenerateTeleportRequest(XRBaseInteractor, RaycastHit, ref TeleportRequest) has been deprecated. Use GenerateTeleportRequest(IXRInteractor, RaycastHit, ref TeleportRequest) instead.";

	public TeleportationProvider teleportationProvider
	{
		get
		{
			return m_TeleportationProvider;
		}
		set
		{
			m_TeleportationProvider = value;
		}
	}

	public MatchOrientation matchOrientation
	{
		get
		{
			return m_MatchOrientation;
		}
		set
		{
			m_MatchOrientation = value;
		}
	}

	public bool matchDirectionalInput
	{
		get
		{
			return m_MatchDirectionalInput;
		}
		set
		{
			m_MatchDirectionalInput = value;
		}
	}

	public TeleportTrigger teleportTrigger
	{
		get
		{
			return m_TeleportTrigger;
		}
		set
		{
			m_TeleportTrigger = value;
		}
	}

	public bool filterSelectionByHitNormal
	{
		get
		{
			return m_FilterSelectionByHitNormal;
		}
		set
		{
			m_FilterSelectionByHitNormal = value;
		}
	}

	public float upNormalToleranceDegrees
	{
		get
		{
			return m_UpNormalToleranceDegrees;
		}
		set
		{
			m_UpNormalToleranceDegrees = value;
		}
	}

	public TeleportingEvent teleporting
	{
		get
		{
			return m_Teleporting;
		}
		set
		{
			m_Teleporting = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_TeleportationProvider == null)
		{
			ComponentLocatorUtility<TeleportationProvider>.TryFindComponent(out m_TeleportationProvider, limitTryFindPerFrame: true);
		}
	}

	protected override void Reset()
	{
		base.selectMode = InteractableSelectMode.Multiple;
	}

	protected virtual bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
	{
		return false;
	}

	protected bool SendTeleportRequest(IXRInteractor interactor)
	{
		RaycastHit raycastHit = default(RaycastHit);
		if (interactor is XRRayInteractor xRRayInteractor && xRRayInteractor != null && (!xRRayInteractor.TryGetCurrent3DRaycastHit(out raycastHit) || !base.interactionManager.TryGetInteractableForCollider(raycastHit.collider, out var interactable) || interactable != this || (m_FilterSelectionByHitNormal && Vector3.Angle(base.transform.up, raycastHit.normal) > m_UpNormalToleranceDegrees)))
		{
			return false;
		}
		if (m_TeleportationProvider == null && !ComponentLocatorUtility<TeleportationProvider>.TryFindComponent(out m_TeleportationProvider))
		{
			Debug.LogWarning("Teleportation Provider was null and one could not be found in the scene: Teleport request failed.", this);
			return false;
		}
		TeleportRequest teleportRequest = default(TeleportRequest);
		teleportRequest.matchOrientation = m_MatchOrientation;
		teleportRequest.requestTime = Time.time;
		TeleportRequest teleportRequest2 = teleportRequest;
		bool flag = GenerateTeleportRequest(interactor, raycastHit, ref teleportRequest2);
		if (flag)
		{
			UpdateTeleportRequestRotation(interactor, ref teleportRequest2);
			flag = m_TeleportationProvider.QueueTeleportRequest(teleportRequest2);
			if (flag && m_Teleporting != null)
			{
				TeleportingEventArgs v;
				using (m_TeleportingEventArgs.Get(out v))
				{
					v.interactorObject = interactor;
					v.interactableObject = this;
					v.teleportRequest = teleportRequest2;
					m_Teleporting.Invoke(v);
				}
			}
		}
		return flag;
	}

	private void UpdateTeleportRequestRotation(IXRInteractor interactor, ref TeleportRequest teleportRequest)
	{
		if (m_MatchDirectionalInput && interactor != null && m_TeleportForwardPerInteractor.TryGetValue(interactor, out var value))
		{
			switch (teleportRequest.matchOrientation)
			{
			case MatchOrientation.WorldSpaceUp:
				teleportRequest.destinationRotation = Quaternion.LookRotation(value, Vector3.up);
				teleportRequest.matchOrientation = MatchOrientation.TargetUpAndForward;
				break;
			case MatchOrientation.TargetUp:
				teleportRequest.destinationRotation = Quaternion.LookRotation(value, base.transform.up);
				teleportRequest.matchOrientation = MatchOrientation.TargetUpAndForward;
				break;
			}
		}
	}

	public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractable(updatePhase);
		if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic || !m_MatchDirectionalInput)
		{
			return;
		}
		int i = 0;
		for (int count = base.interactorsHovering.Count; i < count; i++)
		{
			IXRHoverInteractor interactor2 = base.interactorsHovering[i];
			CalculateTeleportForward(interactor2);
		}
		int j = 0;
		for (int count2 = base.interactorsSelecting.Count; j < count2; j++)
		{
			IXRSelectInteractor interactor3 = base.interactorsSelecting[j];
			if (!IsHovered(interactor3))
			{
				CalculateTeleportForward(interactor3);
			}
		}
		void CalculateTeleportForward(IXRInteractor interactor)
		{
			Transform attachTransform = interactor.GetAttachTransform(this);
			switch (matchOrientation)
			{
			case MatchOrientation.WorldSpaceUp:
				m_TeleportForwardPerInteractor[interactor] = Vector3.ProjectOnPlane(attachTransform.forward, Vector3.up).normalized;
				break;
			case MatchOrientation.TargetUp:
				m_TeleportForwardPerInteractor[interactor] = Vector3.ProjectOnPlane(attachTransform.forward, base.transform.up).normalized;
				break;
			}
		}
	}

	protected override void OnSelectEntered(SelectEnterEventArgs args)
	{
		if (m_TeleportTrigger == TeleportTrigger.OnSelectEntered)
		{
			SendTeleportRequest(args.interactorObject);
		}
		base.OnSelectEntered(args);
	}

	protected override void OnSelectExited(SelectExitEventArgs args)
	{
		if (m_TeleportTrigger == TeleportTrigger.OnSelectExited && !args.isCanceled)
		{
			SendTeleportRequest(args.interactorObject);
		}
		base.OnSelectExited(args);
	}

	protected override void OnActivated(ActivateEventArgs args)
	{
		if (m_TeleportTrigger == TeleportTrigger.OnActivated)
		{
			SendTeleportRequest(args.interactorObject);
		}
		base.OnActivated(args);
	}

	protected override void OnDeactivated(DeactivateEventArgs args)
	{
		if (m_TeleportTrigger == TeleportTrigger.OnDeactivated)
		{
			SendTeleportRequest(args.interactorObject);
		}
		base.OnDeactivated(args);
	}

	public override bool IsSelectableBy(IXRSelectInteractor interactor)
	{
		bool flag = base.IsSelectableBy(interactor);
		if (flag && m_FilterSelectionByHitNormal && interactor is XRRayInteractor xRRayInteractor && xRRayInteractor != null && xRRayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit) && base.interactionManager.TryGetInteractableForCollider(raycastHit.collider, out var interactable) && interactable == this)
		{
			flag &= Vector3.Angle(base.transform.up, raycastHit.normal) <= m_UpNormalToleranceDegrees;
		}
		return flag;
	}

	public void GetReticleDirection(IXRInteractor interactor, Vector3 hitNormal, out Vector3 reticleUp, out Vector3? optionalReticleForward)
	{
		optionalReticleForward = null;
		reticleUp = hitNormal;
		XROrigin xrOrigin = teleportationProvider.mediator.xrOrigin;
		Vector3 value;
		switch (matchOrientation)
		{
		case MatchOrientation.WorldSpaceUp:
			reticleUp = Vector3.up;
			if (m_MatchDirectionalInput && interactor != null && m_TeleportForwardPerInteractor.TryGetValue(interactor, out value))
			{
				optionalReticleForward = value;
			}
			else if (xrOrigin != null)
			{
				optionalReticleForward = xrOrigin.Camera.transform.forward;
			}
			break;
		case MatchOrientation.TargetUp:
			reticleUp = base.transform.up;
			if (m_MatchDirectionalInput && interactor != null && m_TeleportForwardPerInteractor.TryGetValue(interactor, out value))
			{
				optionalReticleForward = value;
			}
			else if (xrOrigin != null)
			{
				optionalReticleForward = xrOrigin.Camera.transform.forward;
			}
			break;
		case MatchOrientation.TargetUpAndForward:
			reticleUp = base.transform.up;
			optionalReticleForward = base.transform.forward;
			break;
		case MatchOrientation.None:
			if (xrOrigin != null)
			{
				reticleUp = xrOrigin.Origin.transform.up;
				optionalReticleForward = xrOrigin.Camera.transform.forward;
			}
			break;
		}
	}

	[Obsolete("GenerateTeleportRequest(XRBaseInteractor, RaycastHit, ref TeleportRequest) has been deprecated. Use GenerateTeleportRequest(IXRInteractor, RaycastHit, ref TeleportRequest) instead.", true)]
	protected virtual bool GenerateTeleportRequest(XRBaseInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
	{
		Debug.LogError("GenerateTeleportRequest(XRBaseInteractor, RaycastHit, ref TeleportRequest) has been deprecated. Use GenerateTeleportRequest(IXRInteractor, RaycastHit, ref TeleportRequest) instead.", this);
		throw new NotSupportedException("GenerateTeleportRequest(XRBaseInteractor, RaycastHit, ref TeleportRequest) has been deprecated. Use GenerateTeleportRequest(IXRInteractor, RaycastHit, ref TeleportRequest) instead.");
	}
}
