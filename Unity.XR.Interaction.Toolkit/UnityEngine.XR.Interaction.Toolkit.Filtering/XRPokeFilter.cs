using System.Diagnostics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[AddComponentMenu("XR/XR Poke Filter", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Filtering.XRPokeFilter.html")]
public class XRPokeFilter : MonoBehaviour, IXRPokeFilter, IXRSelectFilter, IXRInteractionStrengthFilter, IPokeStateDataProvider
{
	[SerializeField]
	[Tooltip("The interactable associated with this poke filter.")]
	private XRBaseInteractable m_Interactable;

	[SerializeField]
	[Tooltip("The collider used to compute bounds of the poke interaction.")]
	private Collider m_PokeCollider;

	[SerializeField]
	[Tooltip("The settings used to fine tune the vector and offsets which dictate how the poke interaction will be evaluated.")]
	private PokeThresholdDatumProperty m_PokeConfiguration = new PokeThresholdDatumProperty(new PokeThresholdData());

	private XRPokeLogic m_PokeLogic = new XRPokeLogic();

	private XRBaseInteractable m_SubscribedInteractable;

	public XRBaseInteractable pokeInteractable
	{
		get
		{
			return m_Interactable;
		}
		set
		{
			m_Interactable = value;
			Setup();
		}
	}

	public Collider pokeCollider
	{
		get
		{
			return m_PokeCollider;
		}
		set
		{
			m_PokeCollider = value;
			Setup();
		}
	}

	public PokeThresholdDatumProperty pokeConfiguration
	{
		get
		{
			return m_PokeConfiguration;
		}
		set
		{
			m_PokeConfiguration = value;
			Setup();
		}
	}

	public IReadOnlyBindableVariable<PokeStateData> pokeStateData => m_PokeLogic?.pokeStateData;

	public virtual bool canProcess
	{
		get
		{
			if (base.isActiveAndEnabled)
			{
				return m_PokeLogic != null;
			}
			return false;
		}
	}

	[Conditional("UNITY_EDITOR")]
	protected virtual void Reset()
	{
	}

	[Conditional("UNITY_EDITOR")]
	protected void OnValidate()
	{
	}

	protected void Awake()
	{
		if (m_Interactable == null)
		{
			m_Interactable = FindPokeInteractable();
		}
		if (m_PokeCollider == null)
		{
			m_PokeCollider = FindPokeCollider();
		}
	}

	protected void Start()
	{
		bool flag = false;
		if (m_Interactable == null)
		{
			m_Interactable = FindPokeInteractable();
			if (m_Interactable == null)
			{
				Debug.LogWarning("Could not find associated XRBaseInteractable in scene.This XRPokeFilter will be disabled.", this);
				flag = true;
			}
		}
		if (m_PokeCollider == null)
		{
			m_PokeCollider = FindPokeCollider();
			if (m_PokeCollider == null)
			{
				Debug.LogWarning("Could not find a Collider associated with this filter in the scene.This XRPokeFilter will be disabled.", this);
				flag = true;
			}
		}
		if (m_PokeConfiguration.Value == null)
		{
			Debug.LogWarning("Poke Data property has been improperly configured. Please assign a Poke Threshold Datum asset if configured to Use Asset.", this);
			flag = true;
		}
		if (flag)
		{
			base.enabled = false;
		}
		else
		{
			Setup();
		}
	}

	protected void OnDestroy()
	{
		Unsubscribe();
		m_PokeLogic?.Dispose();
	}

	[Conditional("UNITY_EDITOR")]
	protected void OnDrawGizmosSelected()
	{
	}

	public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		if (interactor is IPokeStateDataProvider interactor2)
		{
			float pokeInteractionOffset = 0f;
			if (interactor is XRPokeInteractor xRPokeInteractor)
			{
				pokeInteractionOffset = xRPokeInteractor.pokeInteractionOffset;
			}
			Transform attachTransform = interactable.GetAttachTransform(interactor);
			return m_PokeLogic.MeetsRequirementsForSelectAction(interactor2, attachTransform.position, interactor.GetAttachTransform(interactable).position, pokeInteractionOffset, attachTransform);
		}
		return true;
	}

	public float Process(IXRInteractor interactor, IXRInteractable interactable, float interactionStrength)
	{
		float b = 0f;
		if (interactor is IPokeStateDataProvider)
		{
			b = pokeStateData?.Value.interactionStrength ?? 0f;
		}
		return Mathf.Max(interactionStrength, b);
	}

	private void OnHoverEntered(HoverEnterEventArgs args)
	{
		if (m_PokeLogic != null)
		{
			IXRHoverInteractor interactorObject = args.interactorObject;
			IXRHoverInteractable interactableObject = args.interactableObject;
			Transform attachTransform = interactorObject.GetAttachTransform(interactableObject);
			Transform attachTransform2 = interactableObject.GetAttachTransform(interactorObject);
			m_PokeLogic.OnHoverEntered(interactorObject, attachTransform.GetWorldPose(), attachTransform2);
		}
	}

	private void OnHoverExited(HoverExitEventArgs args)
	{
		if (m_PokeLogic != null)
		{
			m_PokeLogic.OnHoverExited(args.interactorObject);
		}
	}

	private XRBaseInteractable FindPokeInteractable()
	{
		if (!(m_Interactable != null))
		{
			return GetComponentInParent<XRBaseInteractable>();
		}
		return m_Interactable;
	}

	private Collider FindPokeCollider()
	{
		if (!(m_PokeCollider != null))
		{
			return GetComponentInChildren<Collider>();
		}
		return m_PokeCollider;
	}

	private void Setup()
	{
		if (m_PokeLogic == null)
		{
			m_PokeLogic = new XRPokeLogic();
		}
		XRBaseInteractable xRBaseInteractable = FindPokeInteractable();
		Collider collider = FindPokeCollider();
		PokeThresholdData value = m_PokeConfiguration.Value;
		if (xRBaseInteractable != null && collider != null && value != null)
		{
			m_PokeLogic.Initialize(xRBaseInteractable.GetAttachTransform(null), value, collider);
			if (Application.isPlaying)
			{
				Subscribe(xRBaseInteractable);
			}
		}
	}

	private void Subscribe(XRBaseInteractable interactable)
	{
		if (!(m_SubscribedInteractable == interactable))
		{
			Unsubscribe();
			interactable.selectFilters.Add(this);
			interactable.interactionStrengthFilters.Add(this);
			interactable.hoverEntered.AddListener(OnHoverEntered);
			interactable.hoverExited.AddListener(OnHoverExited);
			m_SubscribedInteractable = interactable;
		}
	}

	private void Unsubscribe()
	{
		if (!(m_SubscribedInteractable == null))
		{
			m_SubscribedInteractable.selectFilters.Remove(this);
			m_SubscribedInteractable.interactionStrengthFilters.Remove(this);
			m_SubscribedInteractable.hoverEntered.RemoveListener(OnHoverEntered);
			m_SubscribedInteractable.hoverExited.RemoveListener(OnHoverExited);
			m_SubscribedInteractable = null;
		}
	}
}
