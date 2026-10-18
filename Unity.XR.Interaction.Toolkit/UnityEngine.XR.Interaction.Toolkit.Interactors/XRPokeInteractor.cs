using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[AddComponentMenu("XR/Interactors/XR Poke Interactor", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor.html")]
public class XRPokeInteractor : XRBaseInteractor, IUIHoverInteractor, IUIInteractor, IPokeStateDataProvider, IAttachPointVelocityProvider
{
	private readonly struct PokeCollision
	{
		public readonly IXRInteractable interactable;

		public readonly IXRPokeFilter filter;

		public PokeCollision(IXRInteractable interactable, IXRPokeFilter filter)
		{
			this.interactable = interactable;
			this.filter = filter;
		}
	}

	private static readonly List<IXRInteractable> s_Results = new List<IXRInteractable>();

	[SerializeField]
	private float m_PokeDepth = 0.1f;

	[SerializeField]
	private float m_PokeWidth = 0.0075f;

	[SerializeField]
	private float m_PokeSelectWidth = 0.015f;

	[SerializeField]
	private float m_PokeHoverRadius = 0.015f;

	[SerializeField]
	private float m_PokeInteractionOffset = 0.005f;

	[SerializeField]
	private LayerMask m_PhysicsLayerMask = -1;

	[SerializeField]
	private QueryTriggerInteraction m_PhysicsTriggerInteraction = QueryTriggerInteraction.Ignore;

	[SerializeField]
	private QueryUIDocumentInteraction m_UIDocumentTriggerInteraction = QueryUIDocumentInteraction.Collide;

	[SerializeField]
	private bool m_RequirePokeFilter = true;

	[SerializeField]
	private bool m_EnableUIInteraction = true;

	[SerializeField]
	private bool m_ClickUIOnDown;

	[SerializeField]
	private bool m_DebugVisualizationsEnabled;

	[SerializeField]
	private UIHoverEnterEvent m_UIHoverEntered = new UIHoverEnterEvent();

	[SerializeField]
	private UIHoverExitEvent m_UIHoverExited = new UIHoverExitEvent();

	private BindableVariable<PokeStateData> m_PokeStateData = new BindableVariable<PokeStateData>();

	private GameObject m_HoverDebugSphere;

	private MeshRenderer m_HoverDebugRenderer;

	private Vector3 m_LastPokeInteractionPoint;

	private bool m_FirstFrame = true;

	private IXRSelectInteractable m_CurrentPokeTarget;

	private IXRPokeFilter m_CurrentPokeFilter;

	private readonly RaycastHit[] m_SphereCastHits = new RaycastHit[25];

	private readonly Collider[] m_OverlapSphereHits = new Collider[25];

	private readonly List<PokeCollision> m_PokeTargets = new List<PokeCollision>();

	private readonly List<IXRSelectFilter> m_InteractableSelectFilters = new List<IXRSelectFilter>();

	private readonly List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();

	private static readonly Dictionary<IXRInteractable, IXRPokeFilter> s_ValidTargetsScratchMap = new Dictionary<IXRInteractable, IXRPokeFilter>();

	private RegisteredUIInteractorCache m_RegisteredUIInteractorCache;

	private PhysicsScene m_LocalPhysicsScene;

	private Func<Vector3> m_PositionProvider;

	private XRUIToolkitPokeHandler m_UIToolkitPokeHandler;

	[HideInInspector]
	[SerializeField]
	[Tooltip("When enabled, multi-point sampling is used for more forgiving UI element detection. Off by default for performance.")]
	private bool m_EnableMultiPick;

	public float pokeDepth
	{
		get
		{
			return m_PokeDepth;
		}
		set
		{
			m_PokeDepth = value;
		}
	}

	public float pokeWidth
	{
		get
		{
			return m_PokeWidth;
		}
		set
		{
			m_PokeWidth = value;
		}
	}

	public float pokeSelectWidth
	{
		get
		{
			return m_PokeSelectWidth;
		}
		set
		{
			m_PokeSelectWidth = value;
		}
	}

	public float pokeHoverRadius
	{
		get
		{
			return m_PokeHoverRadius;
		}
		set
		{
			m_PokeHoverRadius = value;
		}
	}

	public float pokeInteractionOffset
	{
		get
		{
			return m_PokeInteractionOffset;
		}
		set
		{
			m_PokeInteractionOffset = value;
		}
	}

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

	public QueryUIDocumentInteraction uiDocumentTriggerInteraction
	{
		get
		{
			return m_UIDocumentTriggerInteraction;
		}
		set
		{
			m_UIDocumentTriggerInteraction = value;
		}
	}

	public bool requirePokeFilter
	{
		get
		{
			return m_RequirePokeFilter;
		}
		set
		{
			m_RequirePokeFilter = value;
		}
	}

	public bool enableUIInteraction
	{
		get
		{
			return m_EnableUIInteraction;
		}
		set
		{
			if (m_EnableUIInteraction != value)
			{
				m_EnableUIInteraction = value;
				m_RegisteredUIInteractorCache?.RegisterOrUnregisterXRUIInputModule(m_EnableUIInteraction);
			}
		}
	}

	public bool clickUIOnDown
	{
		get
		{
			return m_ClickUIOnDown;
		}
		set
		{
			m_ClickUIOnDown = value;
		}
	}

	public bool debugVisualizationsEnabled
	{
		get
		{
			return m_DebugVisualizationsEnabled;
		}
		set
		{
			m_DebugVisualizationsEnabled = value;
		}
	}

	public UIHoverEnterEvent uiHoverEntered
	{
		get
		{
			return m_UIHoverEntered;
		}
		set
		{
			m_UIHoverEntered = value;
		}
	}

	public UIHoverExitEvent uiHoverExited
	{
		get
		{
			return m_UIHoverExited;
		}
		set
		{
			m_UIHoverExited = value;
		}
	}

	public IReadOnlyBindableVariable<PokeStateData> pokeStateData => m_PokeStateData;

	protected IAttachPointVelocityTracker attachPointVelocityTracker { get; set; } = new AttachPointVelocityTracker();


	private bool canProcessUIToolkit
	{
		get
		{
			if (m_EnableUIInteraction)
			{
				return XRUIToolkitHandler.uiToolkitSupportEnabled;
			}
			return false;
		}
	}

	internal bool enableMultiPick
	{
		get
		{
			return m_EnableMultiPick;
		}
		set
		{
			m_EnableMultiPick = value;
		}
	}

	internal virtual void UpdateUIRegistration()
	{
		m_RegisteredUIInteractorCache?.UnregisterFromXRUIInputModule();
		XRUIToolkitHandler.Unregister(this);
		if (m_EnableUIInteraction)
		{
			m_RegisteredUIInteractorCache?.RegisterWithXRUIInputModule();
		}
		if (m_EnableUIInteraction)
		{
			XRUIToolkitHandler.Register(this);
			if (m_UIToolkitPokeHandler == null)
			{
				m_UIToolkitPokeHandler = new XRUIToolkitPokeHandler(this);
			}
			m_UIToolkitPokeHandler?.UpdateVisualizersState();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
		m_RegisteredUIInteractorCache = new RegisteredUIInteractorCache(this);
		m_PositionProvider = GetPokePosition;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDebugObjectVisibility(isVisible: true);
		m_FirstFrame = true;
		if (m_EnableUIInteraction)
		{
			UpdateUIRegistration();
		}
		if (this.attachPointVelocityTracker is AttachPointVelocityTracker attachPointVelocityTracker)
		{
			attachPointVelocityTracker.ResetVelocityTracking();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SetDebugObjectVisibility(isVisible: false);
		m_RegisteredUIInteractorCache?.UnregisterFromXRUIInputModule();
		if (canProcessUIToolkit)
		{
			m_UIToolkitPokeHandler?.ResetPointerState();
		}
		XRUIToolkitHandler.Unregister(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_UIToolkitPokeHandler?.Dispose();
	}

	public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.PreprocessInteractor(updatePhase);
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			if (TryGetXROrigin(out var origin))
			{
				attachPointVelocityTracker.UpdateAttachPointVelocityData(GetAttachTransform(null), origin);
			}
			else
			{
				attachPointVelocityTracker.UpdateAttachPointVelocityData(GetAttachTransform(null));
			}
			RegisterValidTargets(out m_CurrentPokeTarget, out m_CurrentPokeFilter);
			ProcessPokeStateData();
		}
	}

	public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractor(updatePhase);
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			UpdateDebugVisuals();
		}
	}

	private bool RegisterValidTargets(out IXRSelectInteractable currentTarget, out IXRPokeFilter pokeFilter)
	{
		bool flag = EvaluateSphereOverlap() > 0;
		m_ValidTargets.Clear();
		s_ValidTargetsScratchMap.Clear();
		if (flag)
		{
			int count = m_PokeTargets.Count;
			for (int i = 0; i < count; i++)
			{
				IXRInteractable interactable = m_PokeTargets[i].interactable;
				if (!m_ValidTargets.Contains(interactable) && interactable is IXRSelectInteractable iXRSelectInteractable && iXRSelectInteractable is IXRHoverInteractable iXRHoverInteractable && iXRHoverInteractable.IsHoverableBy(this))
				{
					m_ValidTargets.Add(m_PokeTargets[i].interactable);
					s_ValidTargetsScratchMap.Add(m_PokeTargets[i].interactable, m_PokeTargets[i].filter);
				}
			}
			if (m_ValidTargets.Count > 1)
			{
				SortingHelpers.SortByDistanceToInteractor(this, m_ValidTargets, SortingHelpers.squareDistanceAttachPointEvaluator);
			}
			IXRTargetFilter iXRTargetFilter = base.targetFilter;
			if (iXRTargetFilter != null && iXRTargetFilter.canProcess)
			{
				iXRTargetFilter.Process(this, m_ValidTargets, s_Results);
				m_ValidTargets.Clear();
				m_ValidTargets.AddRange(s_Results);
			}
			if (m_ValidTargets.Count == 0)
			{
				flag = false;
			}
		}
		currentTarget = (flag ? ((IXRSelectInteractable)m_ValidTargets[0]) : null);
		pokeFilter = (flag ? s_ValidTargetsScratchMap[currentTarget] : null);
		return flag;
	}

	private void ProcessPokeStateData()
	{
		if (TrackedDeviceGraphicRaycaster.TryGetPokeStateDataForInteractor(this, out var data))
		{
			m_PokeStateData.Value = data;
		}
		else if (m_CurrentPokeFilter is IPokeStateDataProvider pokeStateDataProvider)
		{
			m_PokeStateData.Value = pokeStateDataProvider.pokeStateData.Value;
		}
		else
		{
			m_PokeStateData.Value = default(PokeStateData);
		}
	}

	public override void GetValidTargets(List<IXRInteractable> targets)
	{
		targets.Clear();
		if (base.isActiveAndEnabled && m_ValidTargets.Count > 0)
		{
			targets.Add(m_ValidTargets[0]);
		}
	}

	private int EvaluateSphereOverlap()
	{
		m_PokeTargets.Clear();
		Vector3 position = GetAttachTransform(null).position;
		Vector3 overlapStart = m_LastPokeInteractionPoint;
		Vector3 overlapEnd = position;
		BurstPhysicsUtils.GetSphereOverlapParameters(in overlapStart, in overlapEnd, out var normalizedOverlapVector, out var overlapSqrMagnitude, out var overlapDistance);
		if (m_FirstFrame || overlapSqrMagnitude < 0.001f)
		{
			int num = m_LocalPhysicsScene.OverlapSphere(overlapEnd, m_PokeHoverRadius, m_OverlapSphereHits, m_PhysicsLayerMask, m_PhysicsTriggerInteraction);
			for (int i = 0; i < num; i++)
			{
				if (FindPokeTarget(m_OverlapSphereHits[i], out var newPokeCollision))
				{
					m_PokeTargets.Add(newPokeCollision);
				}
			}
		}
		else
		{
			int num2 = m_LocalPhysicsScene.SphereCast(overlapStart, m_PokeHoverRadius, normalizedOverlapVector, m_SphereCastHits, overlapDistance, m_PhysicsLayerMask, m_PhysicsTriggerInteraction);
			for (int j = 0; j < num2; j++)
			{
				if (FindPokeTarget(m_SphereCastHits[j].collider, out var newPokeCollision2))
				{
					m_PokeTargets.Add(newPokeCollision2);
				}
			}
		}
		m_LastPokeInteractionPoint = position;
		m_FirstFrame = false;
		return m_PokeTargets.Count;
	}

	private bool FindPokeTarget(Collider hitCollider, out PokeCollision newPokeCollision)
	{
		newPokeCollision = default(PokeCollision);
		if (base.interactionManager.TryGetInteractableForCollider(hitCollider, out var interactable))
		{
			if (TryGetPokeFilter(interactable, out var pokeFilter))
			{
				newPokeCollision = new PokeCollision(interactable, pokeFilter);
				ProcessValidInteraction(hitCollider, interactable, pokeFilter);
				return true;
			}
			if (!m_RequirePokeFilter)
			{
				newPokeCollision = new PokeCollision(interactable, null);
				ProcessValidInteraction(hitCollider, interactable, null);
				return true;
			}
		}
		return false;
	}

	private bool TryGetPokeFilter(IXRInteractable interactable, out IXRPokeFilter pokeFilter)
	{
		pokeFilter = null;
		if (interactable is XRBaseInteractable xRBaseInteractable)
		{
			xRBaseInteractable.selectFilters.GetAll(m_InteractableSelectFilters);
			foreach (IXRSelectFilter interactableSelectFilter in m_InteractableSelectFilters)
			{
				if (interactableSelectFilter is IXRPokeFilter iXRPokeFilter && interactableSelectFilter.canProcess)
				{
					pokeFilter = iXRPokeFilter;
					return true;
				}
			}
		}
		return false;
	}

	private void ProcessValidInteraction(Collider hitCollider, IXRInteractable interactable, IXRPokeFilter pokeFilter)
	{
		if (canProcessUIToolkit)
		{
			m_UIToolkitPokeHandler?.ProcessPokeInteraction(hitCollider, interactable.transform, interactable, m_EnableMultiPick, pokeFilter);
		}
	}

	private void SetDebugObjectVisibility(bool isVisible)
	{
		if (m_DebugVisualizationsEnabled && m_HoverDebugSphere == null)
		{
			m_HoverDebugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			m_HoverDebugSphere.name = "[Debug] Poke - HoverVisual: " + this;
			m_HoverDebugSphere.transform.SetParent(GetAttachTransform(null), worldPositionStays: false);
			m_HoverDebugSphere.transform.localScale = new Vector3(m_PokeHoverRadius, m_PokeHoverRadius, m_PokeHoverRadius);
			if (m_HoverDebugSphere.TryGetComponent<Collider>(out var component))
			{
				Object.Destroy(component);
			}
			m_HoverDebugRenderer = GetOrAddComponent<MeshRenderer>(m_HoverDebugSphere);
		}
		bool flag = m_DebugVisualizationsEnabled && isVisible;
		if (m_HoverDebugSphere != null && m_HoverDebugSphere.activeSelf != flag)
		{
			m_HoverDebugSphere.SetActive(flag);
		}
	}

	private void UpdateDebugVisuals()
	{
		SetDebugObjectVisibility(m_CurrentPokeTarget != null);
		if (m_DebugVisualizationsEnabled)
		{
			m_HoverDebugRenderer.material.color = ((m_PokeTargets.Count > 0) ? new Color(0f, 0.8f, 0f, 0.1f) : new Color(0.8f, 0f, 0f, 0.1f));
		}
	}

	private static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		if (!go.TryGetComponent<T>(out var component))
		{
			return go.AddComponent<T>();
		}
		return component;
	}

	public virtual void UpdateUIModel(ref TrackedDeviceModel model)
	{
		if (!base.isActiveAndEnabled || this.IsBlockedByInteractionWithinGroup())
		{
			model.Reset(resetImplementation: false);
			return;
		}
		Pose worldPose = GetAttachTransform(null).GetWorldPose();
		Vector3 position = worldPose.position;
		Vector3 vector = worldPose.rotation * Vector3.forward;
		Vector3 item = position + vector * m_PokeDepth;
		model.position = worldPose.position;
		model.orientation = worldPose.rotation;
		model.positionProvider = m_PositionProvider;
		model.raycastLayerMask = m_PhysicsLayerMask;
		model.pokeDepth = m_PokeDepth;
		model.interactionType = UIInteractionType.Poke;
		model.clickOnDown = m_ClickUIOnDown;
		model.UpdatePokeSelectState();
		List<Vector3> raycastPoints = model.raycastPoints;
		raycastPoints.Clear();
		raycastPoints.Add(position);
		raycastPoints.Add(item);
	}

	private Vector3 GetPokePosition()
	{
		return GetAttachTransform(null).position;
	}

	public bool TryGetUIModel(out TrackedDeviceModel model)
	{
		if (m_RegisteredUIInteractorCache == null)
		{
			model = TrackedDeviceModel.invalid;
			return false;
		}
		return m_RegisteredUIInteractorCache.TryGetUIModel(out model);
	}

	void IUIHoverInteractor.OnUIHoverEntered(UIHoverEventArgs args)
	{
		OnUIHoverEntered(args);
	}

	void IUIHoverInteractor.OnUIHoverExited(UIHoverEventArgs args)
	{
		OnUIHoverExited(args);
	}

	protected virtual void OnUIHoverEntered(UIHoverEventArgs args)
	{
		m_UIHoverEntered?.Invoke(args);
	}

	protected virtual void OnUIHoverExited(UIHoverEventArgs args)
	{
		m_UIHoverExited?.Invoke(args);
	}

	protected override void OnHoverExited(HoverExitEventArgs args)
	{
		base.OnHoverExited(args);
		if (args.interactableObject != null && canProcessUIToolkit && XRUIToolkitHandler.IsValidUIToolkitInteraction(args.interactableObject.colliders))
		{
			m_UIToolkitPokeHandler?.ResetPointerState();
		}
	}

	protected override void OnHoverEntering(HoverEnterEventArgs args)
	{
		base.OnHoverEntering(args);
	}

	public Vector3 GetAttachPointVelocity()
	{
		if (TryGetXROrigin(out var origin))
		{
			return attachPointVelocityTracker.GetAttachPointVelocity(origin);
		}
		return attachPointVelocityTracker.GetAttachPointVelocity();
	}

	public Vector3 GetAttachPointAngularVelocity()
	{
		if (TryGetXROrigin(out var origin))
		{
			return attachPointVelocityTracker.GetAttachPointAngularVelocity(origin);
		}
		return attachPointVelocityTracker.GetAttachPointAngularVelocity();
	}
}
