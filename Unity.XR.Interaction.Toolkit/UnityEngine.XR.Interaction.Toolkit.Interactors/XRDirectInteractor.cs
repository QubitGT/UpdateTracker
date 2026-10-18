using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[DisallowMultipleComponent]
[AddComponentMenu("XR/Interactors/XR Direct Interactor", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor.html")]
public class XRDirectInteractor : XRBaseInputInteractor
{
	[SerializeField]
	private bool m_ImproveAccuracyWithSphereCollider;

	[SerializeField]
	private LayerMask m_PhysicsLayerMask = 1;

	[SerializeField]
	private QueryTriggerInteraction m_PhysicsTriggerInteraction = QueryTriggerInteraction.Ignore;

	private readonly HashSet<Collider> m_StayedColliders = new HashSet<Collider>();

	private readonly TriggerContactMonitor m_TriggerContactMonitor = new TriggerContactMonitor();

	private static readonly WaitForFixedUpdate s_WaitForFixedUpdate = new WaitForFixedUpdate();

	private IEnumerator m_UpdateCollidersAfterTriggerStay;

	private bool m_UsingSphereColliderAccuracyImprovement;

	private SphereCollider m_SphereCollider;

	private PhysicsScene m_LocalPhysicsScene;

	private Vector3 m_LastSphereCastOrigin = Vector3.zero;

	private readonly Collider[] m_OverlapSphereHits = new Collider[25];

	private readonly RaycastHit[] m_SphereCastHits = new RaycastHit[25];

	private bool m_FirstFrame = true;

	private bool m_ContactsSortedThisFrame;

	private readonly List<IXRInteractable> m_SortedValidTargets = new List<IXRInteractable>();

	public bool improveAccuracyWithSphereCollider
	{
		get
		{
			return m_ImproveAccuracyWithSphereCollider;
		}
		set
		{
			m_ImproveAccuracyWithSphereCollider = value;
		}
	}

	public bool usingSphereColliderAccuracyImprovement => m_UsingSphereColliderAccuracyImprovement;

	public LayerMask physicsLayerMask
	{
		get
		{
			return m_PhysicsLayerMask;
		}
		set
		{
			m_PhysicsLayerMask = value;
		}
	}

	public QueryTriggerInteraction physicsTriggerInteraction
	{
		get
		{
			return m_PhysicsTriggerInteraction;
		}
		set
		{
			m_PhysicsTriggerInteraction = value;
		}
	}

	protected List<IXRInteractable> unsortedValidTargets { get; } = new List<IXRInteractable>();


	protected override void Awake()
	{
		base.Awake();
		m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
		m_TriggerContactMonitor.interactionManager = base.interactionManager;
		m_UpdateCollidersAfterTriggerStay = UpdateCollidersAfterOnTriggerStay();
		ValidateColliderConfiguration();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_TriggerContactMonitor.contactAdded += OnContactAdded;
		m_TriggerContactMonitor.contactRemoved += OnContactRemoved;
		ResetCollidersAndValidTargets();
		if (!m_UsingSphereColliderAccuracyImprovement)
		{
			StartCoroutine(m_UpdateCollidersAfterTriggerStay);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_TriggerContactMonitor.contactAdded -= OnContactAdded;
		m_TriggerContactMonitor.contactRemoved -= OnContactRemoved;
		ResetCollidersAndValidTargets();
		if (!m_UsingSphereColliderAccuracyImprovement)
		{
			StopCoroutine(m_UpdateCollidersAfterTriggerStay);
		}
	}

	protected void OnTriggerEnter(Collider other)
	{
		if (!m_UsingSphereColliderAccuracyImprovement)
		{
			m_TriggerContactMonitor.AddCollider(other);
		}
	}

	protected void OnTriggerStay(Collider other)
	{
		if (!m_UsingSphereColliderAccuracyImprovement)
		{
			m_StayedColliders.Add(other);
		}
	}

	protected void OnTriggerExit(Collider other)
	{
		if (!m_UsingSphereColliderAccuracyImprovement)
		{
			m_TriggerContactMonitor.RemoveCollider(other);
		}
	}

	private IEnumerator UpdateCollidersAfterOnTriggerStay()
	{
		while (true)
		{
			yield return s_WaitForFixedUpdate;
			m_TriggerContactMonitor.UpdateStayedColliders(m_StayedColliders);
		}
	}

	public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.PreprocessInteractor(updatePhase);
		if (m_UsingSphereColliderAccuracyImprovement && updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			EvaluateSphereOverlap();
		}
	}

	public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractor(updatePhase);
		if (!m_UsingSphereColliderAccuracyImprovement && updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
		{
			m_StayedColliders.Clear();
		}
	}

	private void EvaluateSphereOverlap()
	{
		m_ContactsSortedThisFrame = false;
		m_StayedColliders.Clear();
		Vector3 vector = GetAttachTransform(null).TransformPoint(m_SphereCollider.center);
		Vector3 overlapStart = m_LastSphereCastOrigin;
		Vector3 overlapEnd = vector;
		float radius = m_SphereCollider.radius * m_SphereCollider.transform.lossyScale.x;
		BurstPhysicsUtils.GetSphereOverlapParameters(in overlapStart, in overlapEnd, out var normalizedOverlapVector, out var overlapSqrMagnitude, out var overlapDistance);
		if (m_FirstFrame || overlapSqrMagnitude < 0.001f)
		{
			int num = m_LocalPhysicsScene.OverlapSphere(overlapEnd, radius, m_OverlapSphereHits, m_PhysicsLayerMask, m_PhysicsTriggerInteraction);
			for (int i = 0; i < num; i++)
			{
				m_StayedColliders.Add(m_OverlapSphereHits[i]);
			}
		}
		else
		{
			int num2 = m_LocalPhysicsScene.SphereCast(overlapStart, radius, normalizedOverlapVector, m_SphereCastHits, overlapDistance, m_PhysicsLayerMask, m_PhysicsTriggerInteraction);
			for (int j = 0; j < num2; j++)
			{
				m_StayedColliders.Add(m_SphereCastHits[j].collider);
			}
		}
		m_TriggerContactMonitor.UpdateStayedColliders(m_StayedColliders);
		m_LastSphereCastOrigin = vector;
		m_FirstFrame = false;
	}

	private void ValidateColliderConfiguration()
	{
		if (TryGetComponent<Rigidbody>(out var _))
		{
			return;
		}
		Collider[] components = GetComponents<Collider>();
		if (m_ImproveAccuracyWithSphereCollider && components.Length == 1 && components[0] is SphereCollider sphereCollider)
		{
			m_SphereCollider = sphereCollider;
			m_SphereCollider.enabled = false;
			m_UsingSphereColliderAccuracyImprovement = true;
			return;
		}
		bool flag = false;
		Collider[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isTrigger)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.LogWarning("Direct Interactor does not have required Collider set as a trigger.", this);
		}
	}

	public override void GetValidTargets(List<IXRInteractable> targets)
	{
		targets.Clear();
		if (base.isActiveAndEnabled)
		{
			IXRTargetFilter iXRTargetFilter = base.targetFilter;
			if (iXRTargetFilter != null && iXRTargetFilter.canProcess)
			{
				iXRTargetFilter.Process(this, unsortedValidTargets, targets);
				return;
			}
			if (m_ContactsSortedThisFrame)
			{
				targets.AddRange(m_SortedValidTargets);
				return;
			}
			SortingHelpers.SortByDistanceToInteractor(this, unsortedValidTargets, m_SortedValidTargets);
			targets.AddRange(m_SortedValidTargets);
			m_ContactsSortedThisFrame = true;
		}
	}

	public override bool CanHover(IXRHoverInteractable interactable)
	{
		if (base.CanHover(interactable))
		{
			if (base.hasSelection)
			{
				return IsSelecting(interactable);
			}
			return true;
		}
		return false;
	}

	public override bool CanSelect(IXRSelectInteractable interactable)
	{
		if (base.CanSelect(interactable))
		{
			if (base.hasSelection)
			{
				return IsSelecting(interactable);
			}
			return true;
		}
		return false;
	}

	protected override void OnRegistered(InteractorRegisteredEventArgs args)
	{
		base.OnRegistered(args);
		args.manager.interactableRegistered += OnInteractableRegistered;
		args.manager.interactableUnregistered += OnInteractableUnregistered;
		m_TriggerContactMonitor.interactionManager = args.manager;
		if (!m_UsingSphereColliderAccuracyImprovement)
		{
			m_TriggerContactMonitor.ResolveUnassociatedColliders();
			XRInteractionManager.RemoveAllUnregistered(args.manager, unsortedValidTargets);
		}
	}

	protected override void OnUnregistered(InteractorUnregisteredEventArgs args)
	{
		base.OnUnregistered(args);
		args.manager.interactableRegistered -= OnInteractableRegistered;
		args.manager.interactableUnregistered -= OnInteractableUnregistered;
	}

	private void OnInteractableRegistered(InteractableRegisteredEventArgs args)
	{
		IXRInteractable interactableObject = args.interactableObject;
		m_TriggerContactMonitor.ResolveUnassociatedColliders(interactableObject);
		if (m_TriggerContactMonitor.IsContacting(interactableObject))
		{
			OnContactAdded(interactableObject);
		}
	}

	private void OnInteractableUnregistered(InteractableUnregisteredEventArgs args)
	{
		OnContactRemoved(args.interactableObject);
	}

	private void OnContactAdded(IXRInteractable interactable)
	{
		if (!unsortedValidTargets.Contains(interactable))
		{
			unsortedValidTargets.Add(interactable);
			m_ContactsSortedThisFrame = false;
		}
	}

	private void OnContactRemoved(IXRInteractable interactable)
	{
		if (unsortedValidTargets.Remove(interactable))
		{
			m_ContactsSortedThisFrame = false;
		}
	}

	private void ResetCollidersAndValidTargets()
	{
		unsortedValidTargets.Clear();
		m_SortedValidTargets.Clear();
		m_ContactsSortedThisFrame = false;
		m_FirstFrame = true;
		m_StayedColliders.Clear();
		m_TriggerContactMonitor.UpdateStayedColliders(m_StayedColliders);
	}
}
