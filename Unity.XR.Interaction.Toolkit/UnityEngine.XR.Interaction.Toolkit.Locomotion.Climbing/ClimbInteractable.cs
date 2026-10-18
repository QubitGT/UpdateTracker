using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

[SelectionBase]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("XR/Climb Interactable", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbInteractable.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class ClimbInteractable : XRBaseInteractable
{
	private const float k_DefaultMaxInteractionDistance = 0.1f;

	[SerializeField]
	[Tooltip("The climb provider that performs locomotion while this interactable is selected. If no climb provider is configured, will attempt to find one.")]
	private ClimbProvider m_ClimbProvider;

	[SerializeField]
	[Tooltip("Transform that defines the coordinate space for climb locomotion. Will use this GameObject's Transform by default.")]
	private Transform m_ClimbTransform;

	[SerializeField]
	[Tooltip("Controls whether to apply a distance check when validating hover and select interaction.")]
	private bool m_FilterInteractionByDistance = true;

	[SerializeField]
	[Tooltip("The maximum distance that an interactor can be from this interactable to begin hover or select.")]
	private float m_MaxInteractionDistance = 0.1f;

	[SerializeField]
	[Tooltip("The teleport volume used to assist with movement to a specific destination after ending a climb (optional, may be None). Only used if there is a Climb Teleport Interactor in the scene.")]
	private TeleportationMultiAnchorVolume m_ClimbAssistanceTeleportVolume;

	[SerializeField]
	[Tooltip("Optional override of locomotion settings specified in the climb provider. Only applies as an override if set to Use Value or if the asset reference is set.")]
	private ClimbSettingsDatumProperty m_ClimbSettingsOverride;

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

	public Transform climbTransform
	{
		get
		{
			if (m_ClimbTransform == null)
			{
				m_ClimbTransform = base.transform;
			}
			return m_ClimbTransform;
		}
		set
		{
			m_ClimbTransform = value;
		}
	}

	public bool filterInteractionByDistance
	{
		get
		{
			return m_FilterInteractionByDistance;
		}
		set
		{
			m_FilterInteractionByDistance = value;
		}
	}

	public float maxInteractionDistance
	{
		get
		{
			return m_MaxInteractionDistance;
		}
		set
		{
			m_MaxInteractionDistance = value;
		}
	}

	public TeleportationMultiAnchorVolume climbAssistanceTeleportVolume
	{
		get
		{
			return m_ClimbAssistanceTeleportVolume;
		}
		set
		{
			m_ClimbAssistanceTeleportVolume = value;
		}
	}

	public ClimbSettingsDatumProperty climbSettingsOverride
	{
		get
		{
			return m_ClimbSettingsOverride;
		}
		set
		{
			m_ClimbSettingsOverride = value;
		}
	}

	protected virtual void OnValidate()
	{
		if (m_ClimbTransform == null)
		{
			m_ClimbTransform = base.transform;
		}
	}

	protected override void Reset()
	{
		base.selectMode = InteractableSelectMode.Multiple;
		m_ClimbTransform = base.transform;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_ClimbProvider == null)
		{
			ComponentLocatorUtility<ClimbProvider>.TryFindComponent(out m_ClimbProvider, limitTryFindPerFrame: true);
		}
	}

	public override bool IsHoverableBy(IXRHoverInteractor interactor)
	{
		if (base.IsHoverableBy(interactor))
		{
			if (m_FilterInteractionByDistance)
			{
				return GetDistanceSqrToInteractor(interactor) <= m_MaxInteractionDistance * m_MaxInteractionDistance;
			}
			return true;
		}
		return false;
	}

	public override bool IsSelectableBy(IXRSelectInteractor interactor)
	{
		if (base.IsSelectableBy(interactor))
		{
			if (!IsSelected(interactor) && m_FilterInteractionByDistance)
			{
				return GetDistanceSqrToInteractor(interactor) <= m_MaxInteractionDistance * m_MaxInteractionDistance;
			}
			return true;
		}
		return false;
	}

	protected override void OnSelectEntered(SelectEnterEventArgs args)
	{
		base.OnSelectEntered(args);
		if (m_ClimbProvider != null || ComponentLocatorUtility<ClimbProvider>.TryFindComponent(out m_ClimbProvider))
		{
			m_ClimbProvider.StartClimbGrab(this, args.interactorObject);
		}
	}

	protected override void OnSelectExited(SelectExitEventArgs args)
	{
		base.OnSelectExited(args);
		if (m_ClimbProvider != null)
		{
			m_ClimbProvider.FinishClimbGrab(args.interactorObject);
		}
	}
}
