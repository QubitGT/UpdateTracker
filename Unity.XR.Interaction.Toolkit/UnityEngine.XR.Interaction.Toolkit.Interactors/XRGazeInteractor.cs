using Unity.XR.CoreUtils;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Gaze;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[AddComponentMenu("XR/Interactors/XR Gaze Interactor", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRGazeInteractor.html")]
public class XRGazeInteractor : XRRayInteractor
{
	public enum GazeAssistanceCalculation
	{
		FixedSize,
		ColliderSize
	}

	[SerializeField]
	private GazeAssistanceCalculation m_GazeAssistanceCalculation;

	[SerializeField]
	private float m_GazeAssistanceColliderFixedSize = 1f;

	[SerializeField]
	private float m_GazeAssistanceColliderScale = 1f;

	[SerializeField]
	private XRInteractableSnapVolume m_GazeAssistanceSnapVolume;

	[SerializeField]
	private bool m_GazeAssistanceDistanceScaling;

	[SerializeField]
	private bool m_ClampGazeAssistanceDistanceScaling;

	[SerializeField]
	private float m_GazeAssistanceDistanceScalingClampValue = 1f;

	public GazeAssistanceCalculation gazeAssistanceCalculation
	{
		get
		{
			return m_GazeAssistanceCalculation;
		}
		set
		{
			m_GazeAssistanceCalculation = value;
		}
	}

	public float gazeAssistanceColliderFixedSize
	{
		get
		{
			return m_GazeAssistanceColliderFixedSize;
		}
		set
		{
			m_GazeAssistanceColliderFixedSize = value;
		}
	}

	public float gazeAssistanceColliderScale
	{
		get
		{
			return m_GazeAssistanceColliderScale;
		}
		set
		{
			m_GazeAssistanceColliderScale = value;
		}
	}

	public XRInteractableSnapVolume gazeAssistanceSnapVolume
	{
		get
		{
			return m_GazeAssistanceSnapVolume;
		}
		set
		{
			m_GazeAssistanceSnapVolume = value;
		}
	}

	public bool gazeAssistanceDistanceScaling
	{
		get
		{
			return m_GazeAssistanceDistanceScaling;
		}
		set
		{
			m_GazeAssistanceDistanceScaling = value;
		}
	}

	public bool clampGazeAssistanceDistanceScaling
	{
		get
		{
			return m_ClampGazeAssistanceDistanceScaling;
		}
		set
		{
			m_ClampGazeAssistanceDistanceScaling = value;
		}
	}

	public float gazeAssistanceDistanceScalingClampValue
	{
		get
		{
			return m_GazeAssistanceDistanceScalingClampValue;
		}
		set
		{
			m_GazeAssistanceDistanceScalingClampValue = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		CreateGazeAssistanceSnapVolume();
	}

	private void CreateGazeAssistanceSnapVolume()
	{
		if (m_GazeAssistanceSnapVolume == null)
		{
			GameObject gameObject = new GameObject("Gaze Snap Volume");
			gameObject.AddComponent<SphereCollider>().isTrigger = true;
			m_GazeAssistanceSnapVolume = gameObject.AddComponent<XRInteractableSnapVolume>();
		}
		else if (m_GazeAssistanceSnapVolume.snapCollider != null && !(m_GazeAssistanceSnapVolume.snapCollider is SphereCollider) && !(m_GazeAssistanceSnapVolume.snapCollider is BoxCollider))
		{
			Debug.LogWarning("The Gaze Assistance Snap Volume is using a Snap Collider which does not support automatic dynamic scaling by the XR Gaze Interactor. It must be a Sphere Collider or Box Collider.", this);
		}
	}

	public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.PreprocessInteractor(updatePhase);
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			IXRInteractable interactable = (CanInteract(base.currentNearestValidTarget) ? base.currentNearestValidTarget : null);
			UpdateSnapVolumeInteractable(interactable);
		}
	}

	protected virtual void UpdateSnapVolumeInteractable(IXRInteractable interactable)
	{
		if (m_GazeAssistanceSnapVolume == null)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		float num = m_GazeAssistanceColliderScale;
		float num2 = 0f;
		IXRInteractable interactable2 = null;
		Collider collider = null;
		if (interactable is XRBaseInteractable xRBaseInteractable && xRBaseInteractable != null && xRBaseInteractable.allowGazeAssistance)
		{
			interactable2 = interactable;
			vector = interactable.transform.position;
			if (TryGetHitInfo(out var position, out var _, out var _, out var _) && XRInteractableUtility.TryGetClosestCollider(interactable, position, out var distanceInfo))
			{
				collider = distanceInfo.collider;
				vector = distanceInfo.collider.bounds.center;
			}
			num2 = CalculateSnapColliderSize(collider);
		}
		if (m_GazeAssistanceDistanceScaling)
		{
			num *= Vector3.Distance(base.transform.position, vector);
			if (m_ClampGazeAssistanceDistanceScaling)
			{
				num = Mathf.Clamp(num, 0f, m_GazeAssistanceDistanceScalingClampValue);
			}
		}
		Transform obj = m_GazeAssistanceSnapVolume.transform;
		obj.position = vector;
		obj.localScale = new Vector3(num, num, num);
		Collider snapCollider = m_GazeAssistanceSnapVolume.snapCollider;
		if (!(snapCollider is SphereCollider sphereCollider))
		{
			if (snapCollider is BoxCollider boxCollider)
			{
				boxCollider.size = new Vector3(num2, num2, num2);
			}
		}
		else
		{
			sphereCollider.radius = num2;
		}
		m_GazeAssistanceSnapVolume.interactable = interactable2;
		m_GazeAssistanceSnapVolume.snapToCollider = collider;
	}

	private float CalculateSnapColliderSize(Collider interactableCollider)
	{
		switch (m_GazeAssistanceCalculation)
		{
		case GazeAssistanceCalculation.FixedSize:
			return m_GazeAssistanceColliderFixedSize;
		case GazeAssistanceCalculation.ColliderSize:
			if (interactableCollider != null)
			{
				return interactableCollider.bounds.size.MaxComponent();
			}
			break;
		}
		return 0f;
	}

	private bool CanInteract(IXRInteractable interactable)
	{
		if (!(interactable is IXRHoverInteractable interactable2) || !base.interactionManager.CanHover(this, interactable2))
		{
			if (interactable is IXRSelectInteractable interactable3)
			{
				return base.interactionManager.CanSelect(this, interactable3);
			}
			return false;
		}
		return true;
	}

	protected override float GetHoverTimeToSelect(IXRInteractable interactable)
	{
		if (interactable is IXROverridesGazeAutoSelect { overrideGazeTimeToSelect: not false } iXROverridesGazeAutoSelect)
		{
			return iXROverridesGazeAutoSelect.gazeTimeToSelect;
		}
		return base.GetHoverTimeToSelect(interactable);
	}

	protected override float GetTimeToAutoDeselect(IXRInteractable interactable)
	{
		if (interactable is IXROverridesGazeAutoSelect { overrideTimeToAutoDeselectGaze: not false } iXROverridesGazeAutoSelect)
		{
			return iXROverridesGazeAutoSelect.timeToAutoDeselectGaze;
		}
		return base.GetTimeToAutoDeselect(interactable);
	}
}
