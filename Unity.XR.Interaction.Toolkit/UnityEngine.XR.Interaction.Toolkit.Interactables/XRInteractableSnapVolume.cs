using System.Diagnostics;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[AddComponentMenu("XR/XR Interactable Snap Volume", 11)]
[DefaultExecutionOrder(-99)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactables.XRInteractableSnapVolume.html")]
public class XRInteractableSnapVolume : MonoBehaviour
{
	[SerializeField]
	private XRInteractionManager m_InteractionManager;

	[SerializeField]
	[RequireInterface(typeof(IXRInteractable))]
	private Object m_InteractableObject;

	[SerializeField]
	private Collider m_SnapCollider;

	[SerializeField]
	private bool m_DisableSnapColliderWhenSelected = true;

	[SerializeField]
	private Collider m_SnapToCollider;

	private IXRInteractable m_Interactable;

	private IXRInteractable m_BoundInteractable;

	private IXRSelectInteractable m_BoundSelectInteractable;

	private XRInteractionManager m_RegisteredInteractionManager;

	public XRInteractionManager interactionManager
	{
		get
		{
			return m_InteractionManager;
		}
		set
		{
			m_InteractionManager = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				RegisterWithInteractionManager();
			}
		}
	}

	public Object interactableObject
	{
		get
		{
			return m_InteractableObject;
		}
		set
		{
			m_InteractableObject = value;
			interactable = value as IXRInteractable;
		}
	}

	public Collider snapCollider
	{
		get
		{
			return m_SnapCollider;
		}
		set
		{
			if (!(m_SnapCollider == value))
			{
				if (Application.isPlaying && base.isActiveAndEnabled)
				{
					UnregisterWithInteractionManager();
					m_SnapCollider = value;
					ValidateSnapCollider();
					RefreshSnapColliderEnabled();
					RegisterWithInteractionManager();
				}
				else
				{
					m_SnapCollider = value;
				}
			}
		}
	}

	public bool disableSnapColliderWhenSelected
	{
		get
		{
			return m_DisableSnapColliderWhenSelected;
		}
		set
		{
			m_DisableSnapColliderWhenSelected = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				RefreshSnapColliderEnabled();
			}
		}
	}

	public Collider snapToCollider
	{
		get
		{
			return m_SnapToCollider;
		}
		set
		{
			m_SnapToCollider = value;
		}
	}

	public IXRInteractable interactable
	{
		get
		{
			return m_Interactable;
		}
		set
		{
			m_Interactable = value;
			m_InteractableObject = value as Object;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				SetBoundInteractable(value);
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	protected virtual void Reset()
	{
	}

	protected virtual void Awake()
	{
		if (m_SnapCollider == null)
		{
			m_SnapCollider = FindSnapCollider(base.gameObject);
		}
		ValidateSnapCollider();
	}

	protected virtual void OnEnable()
	{
		FindCreateInteractionManager();
		RegisterWithInteractionManager();
		if (m_InteractableObject != null && m_InteractableObject is IXRInteractable iXRInteractable)
		{
			interactable = iXRInteractable;
		}
		else
		{
			interactable = m_Interactable ?? (m_Interactable = GetComponentInParent<IXRInteractable>());
		}
	}

	protected virtual void OnDisable()
	{
		UnregisterWithInteractionManager();
		SetBoundInteractable(null);
		SetSnapColliderEnabled(enable: false);
	}

	private void FindCreateInteractionManager()
	{
		if (!(m_InteractionManager != null))
		{
			m_InteractionManager = ComponentLocatorUtility<XRInteractionManager>.FindOrCreateComponent();
		}
	}

	private void RegisterWithInteractionManager()
	{
		if (!(m_RegisteredInteractionManager == m_InteractionManager))
		{
			UnregisterWithInteractionManager();
			if (m_InteractionManager != null)
			{
				m_InteractionManager.RegisterSnapVolume(this);
				m_RegisteredInteractionManager = m_InteractionManager;
			}
		}
	}

	private void UnregisterWithInteractionManager()
	{
		if (!(m_RegisteredInteractionManager == null))
		{
			m_RegisteredInteractionManager.UnregisterSnapVolume(this);
			m_RegisteredInteractionManager = null;
		}
	}

	protected static Collider FindSnapCollider(GameObject gameObject)
	{
		Collider collider = null;
		Collider[] components = gameObject.GetComponents<Collider>();
		foreach (Collider collider2 in components)
		{
			if (SupportsTriggerCollider(collider2))
			{
				if (collider2.isTrigger)
				{
					return collider2;
				}
				if (collider == null)
				{
					collider = collider2;
				}
			}
		}
		return collider;
	}

	internal static bool SupportsTriggerCollider(Collider col)
	{
		if (!(col is BoxCollider) && !(col is SphereCollider) && !(col is CapsuleCollider))
		{
			if (col is MeshCollider meshCollider)
			{
				return meshCollider.convex;
			}
			return false;
		}
		return true;
	}

	private void ValidateSnapCollider()
	{
		if (m_SnapCollider == null)
		{
			Debug.LogWarning("XR Interactable Snap Volume is missing a Snap Collider assignment.", this);
		}
		else if (!SupportsTriggerCollider(m_SnapCollider))
		{
			Debug.LogError("Snap Collider is set to a collider which does not support being a trigger collider. Set it to a Box Collider, Sphere Collider, Capsule Collider, or convex Mesh Collider.", this);
		}
		else if (!m_SnapCollider.isTrigger)
		{
			Debug.LogWarning($"Snap Collider must be trigger collider, updating {m_SnapCollider}.", this);
			m_SnapCollider.isTrigger = true;
		}
	}

	private void SetSnapColliderEnabled(bool enable)
	{
		if (m_SnapCollider != null)
		{
			m_SnapCollider.enabled = enable;
		}
	}

	public Vector3 GetClosestPoint(Vector3 point)
	{
		if (m_SnapToCollider == null || !m_SnapToCollider.gameObject.activeInHierarchy || !m_SnapToCollider.enabled)
		{
			if (m_Interactable == null || (m_Interactable is Object @object && !(@object != null)))
			{
				return base.transform.position;
			}
			return m_Interactable.transform.position;
		}
		return m_SnapToCollider.ClosestPoint(point);
	}

	public Vector3 GetClosestPointOfAttachTransform(IXRInteractor interactor)
	{
		Vector3 vector = ((m_Interactable != null && (!(m_Interactable is Object @object) || @object != null)) ? m_Interactable.GetAttachTransform(interactor).position : base.transform.position);
		if (m_SnapToCollider == null || !m_SnapToCollider.gameObject.activeInHierarchy || !m_SnapToCollider.enabled)
		{
			return vector;
		}
		return m_SnapToCollider.ClosestPoint(vector);
	}

	private void SetBoundInteractable(IXRInteractable source)
	{
		if (m_BoundInteractable != source)
		{
			if (m_BoundSelectInteractable != null)
			{
				m_BoundSelectInteractable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
				m_BoundSelectInteractable.lastSelectExited.RemoveListener(OnLastSelectExited);
			}
			m_BoundInteractable = source;
			m_BoundSelectInteractable = source as IXRSelectInteractable;
			if (m_BoundSelectInteractable != null)
			{
				m_BoundSelectInteractable.firstSelectEntered.AddListener(OnFirstSelectEntered);
				m_BoundSelectInteractable.lastSelectExited.AddListener(OnLastSelectExited);
			}
			RefreshSnapColliderEnabled();
		}
	}

	private void RefreshSnapColliderEnabled()
	{
		bool flag = m_BoundSelectInteractable != null && m_BoundSelectInteractable.isSelected;
		if (m_DisableSnapColliderWhenSelected)
		{
			SetSnapColliderEnabled(!flag);
		}
		else
		{
			SetSnapColliderEnabled(enable: true);
		}
	}

	private void OnFirstSelectEntered(SelectEnterEventArgs args)
	{
		if (m_DisableSnapColliderWhenSelected)
		{
			SetSnapColliderEnabled(enable: false);
		}
	}

	private void OnLastSelectExited(SelectExitEventArgs args)
	{
		if (m_DisableSnapColliderWhenSelected)
		{
			SetSnapColliderEnabled(enable: true);
		}
	}
}
