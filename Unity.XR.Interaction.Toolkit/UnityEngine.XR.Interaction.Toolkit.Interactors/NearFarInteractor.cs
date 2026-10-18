using System.Collections.Generic;
using Unity.Collections;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
[AddComponentMenu("XR/Interactors/Near-Far Interactor", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor.html")]
public class NearFarInteractor : XRBaseInputInteractor, IXRRayProvider, IUIHoverInteractor, IUIInteractor, ICurveInteractionDataProvider
{
	public enum Region
	{
		None,
		Near,
		Far
	}

	public enum NearCasterSortingStrategy
	{
		None,
		SquareDistance,
		InteractableBased,
		ClosestPointOnCollider
	}

	[SerializeField]
	[RequireInterface(typeof(IInteractionAttachController))]
	private Object m_InteractionAttachController;

	private readonly UnityObjectReferenceCache<IInteractionAttachController, Object> m_InteractionAttachControllerObjectRef = new UnityObjectReferenceCache<IInteractionAttachController, Object>();

	[SerializeField]
	private bool m_EnableNearCasting = true;

	[SerializeField]
	[RequireInterface(typeof(IInteractionCaster))]
	private Object m_NearInteractionCaster;

	private readonly UnityObjectReferenceCache<IInteractionCaster, Object> m_NearCasterObjectRef = new UnityObjectReferenceCache<IInteractionCaster, Object>();

	[SerializeField]
	private NearCasterSortingStrategy m_NearCasterSortingStrategy = NearCasterSortingStrategy.SquareDistance;

	[SerializeField]
	private bool m_SortNearTargetsAfterTargetFilter;

	[Space]
	[SerializeField]
	private bool m_EnableFarCasting = true;

	[SerializeField]
	[RequireInterface(typeof(ICurveInteractionCaster))]
	private Object m_FarInteractionCaster;

	private readonly UnityObjectReferenceCache<ICurveInteractionCaster, Object> m_FarCasterObjectRef = new UnityObjectReferenceCache<ICurveInteractionCaster, Object>();

	[SerializeField]
	private InteractorFarAttachMode m_FarAttachMode = InteractorFarAttachMode.Far;

	[SerializeField]
	private bool m_EnableUIInteraction = true;

	[SerializeField]
	private bool m_BlockUIOnInteractableSelection = true;

	[SerializeField]
	private UIHoverEnterEvent m_UIHoverEntered = new UIHoverEnterEvent();

	[SerializeField]
	private UIHoverExitEvent m_UIHoverExited = new UIHoverExitEvent();

	[SerializeField]
	private XRInputButtonReader m_UIPressInput = new XRInputButtonReader("UI Press");

	[SerializeField]
	private XRInputValueReader<Vector2> m_UIScrollInput = new XRInputValueReader<Vector2>("UI Scroll");

	private readonly BindableEnum<Region> m_SelectionRegion = new BindableEnum<Region>(Region.None);

	private Region m_ValidTargetCastSource;

	private Region m_SelectedTargetCastSource;

	private readonly List<Collider> m_TargetColliders = new List<Collider>();

	private readonly List<RaycastHit> m_FarRayCastHits = new List<RaycastHit>();

	private readonly List<IXRInteractable> m_InternalValidTargets = new List<IXRInteractable>();

	private readonly Dictionary<int, IXRInteractable> m_IndexToSnapVolumeMap = new Dictionary<int, IXRInteractable>();

	private readonly Dictionary<IXRInteractable, int> m_FarTargetToIndexMap = new Dictionary<IXRInteractable, int>();

	private readonly List<IXRInteractable> m_PreFilteredTargets = new List<IXRInteractable>();

	private bool m_ReleasedNearInteractionThisFrame;

	private RegisteredUIInteractorCache m_RegisteredUIInteractorCache;

	private readonly UnityObjectReferenceCache<IUIModelUpdater, Object> m_UIModelUpdaterReferenceCache = new UnityObjectReferenceCache<IUIModelUpdater, Object>();

	private bool m_HasValidRayHit;

	private bool m_LastValidHitIsUI;

	private Transform m_RayEndTransform;

	private Vector3 m_RayEndPoint;

	private Vector3 m_RayEndNormal;

	private Vector3 m_NormalRelativeToInteractable;

	private bool m_ValidHitIsUI;

	private bool m_ValidHitIsSnapVolume;

	private IXRInteractable m_ValidHitSnapVolumeInteractable;

	private readonly bool m_AllowMultipleValidTargets;

	public IInteractionAttachController interactionAttachController
	{
		get
		{
			return m_InteractionAttachControllerObjectRef.Get(m_InteractionAttachController);
		}
		set
		{
			m_InteractionAttachControllerObjectRef.Set(ref m_InteractionAttachController, value);
		}
	}

	public bool enableNearCasting
	{
		get
		{
			return m_EnableNearCasting;
		}
		set
		{
			m_EnableNearCasting = value;
		}
	}

	public IInteractionCaster nearInteractionCaster
	{
		get
		{
			return m_NearCasterObjectRef.Get(m_NearInteractionCaster);
		}
		set
		{
			m_NearCasterObjectRef.Set(ref m_NearInteractionCaster, value);
		}
	}

	public NearCasterSortingStrategy nearCasterSortingStrategy
	{
		get
		{
			return m_NearCasterSortingStrategy;
		}
		set
		{
			m_NearCasterSortingStrategy = value;
		}
	}

	public bool sortNearTargetsAfterTargetFilter
	{
		get
		{
			return m_SortNearTargetsAfterTargetFilter;
		}
		set
		{
			m_SortNearTargetsAfterTargetFilter = value;
		}
	}

	public bool enableFarCasting
	{
		get
		{
			return m_EnableFarCasting;
		}
		set
		{
			m_EnableFarCasting = value;
		}
	}

	public ICurveInteractionCaster farInteractionCaster
	{
		get
		{
			return m_FarCasterObjectRef.Get(m_FarInteractionCaster);
		}
		set
		{
			m_FarCasterObjectRef.Set(ref m_FarInteractionCaster, value);
		}
	}

	public InteractorFarAttachMode farAttachMode
	{
		get
		{
			return m_FarAttachMode;
		}
		set
		{
			m_FarAttachMode = value;
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

	public bool blockUIOnInteractableSelection
	{
		get
		{
			return m_BlockUIOnInteractableSelection;
		}
		set
		{
			m_BlockUIOnInteractableSelection = value;
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

	public XRInputButtonReader uiPressInput
	{
		get
		{
			return m_UIPressInput;
		}
		set
		{
			SetInputProperty(ref m_UIPressInput, value);
		}
	}

	public XRInputValueReader<Vector2> uiScrollInput
	{
		get
		{
			return m_UIScrollInput;
		}
		set
		{
			SetInputProperty(ref m_UIScrollInput, value);
		}
	}

	Transform IXRRayProvider.rayEndTransform => m_RayEndTransform;

	Vector3 IXRRayProvider.rayEndPoint
	{
		get
		{
			if (TryGetCurveEndPoint(out var endPoint) != 0)
			{
				return endPoint;
			}
			return farInteractionCaster.lastSamplePoint;
		}
	}

	public IReadOnlyBindableVariable<Region> selectionRegion => m_SelectionRegion;

	private IUIModelUpdater uiModelUpdater => m_UIModelUpdaterReferenceCache.Get(m_FarInteractionCaster);

	private bool isUiSelectInputActive => m_UIPressInput.ReadIsPerformed();

	private Vector2 uiScrollInputValue => m_UIScrollInput.ReadValue();

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

	private bool isCurveActive
	{
		get
		{
			bool flag = selectionRegion.Value == Region.Far;
			bool flag2 = m_EnableFarCasting || flag;
			if (!base.isActiveAndEnabled || !flag2 || !farInteractionCaster.isInitialized || m_ReleasedNearInteractionThisFrame)
			{
				return false;
			}
			if (base.hasSelection)
			{
				return flag;
			}
			if (m_ValidTargetCastSource != Region.Near)
			{
				return !this.IsBlockedByInteractionWithinGroup();
			}
			return false;
		}
	}

	bool ICurveInteractionDataProvider.isActive => isCurveActive;

	bool ICurveInteractionDataProvider.hasValidSelect
	{
		get
		{
			if (!m_ValidHitIsUI)
			{
				return base.hasSelection;
			}
			return isUiSelectInputActive;
		}
	}

	NativeArray<Vector3> ICurveInteractionDataProvider.samplePoints => farInteractionCaster.samplePoints;

	Vector3 ICurveInteractionDataProvider.lastSamplePoint => farInteractionCaster.lastSamplePoint;

	public Transform curveOrigin => farInteractionCaster.effectiveCastOrigin;

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
		}
	}

	protected override void Awake()
	{
		InitializeReferences();
		base.Awake();
		m_RegisteredUIInteractorCache = new RegisteredUIInteractorCache(this);
		base.buttonReaders.Add(m_UIPressInput);
		base.valueReaders.Add(m_UIScrollInput);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_EnableUIInteraction)
		{
			UpdateUIRegistration();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_RegisteredUIInteractorCache?.UnregisterFromXRUIInputModule();
		XRUIToolkitHandler.Unregister(this);
		InitializeInteractor();
	}

	protected virtual void InitializeReferences()
	{
		if (farInteractionCaster == null)
		{
			if (TryGetComponent<ICurveInteractionCaster>(out var component))
			{
				farInteractionCaster = component;
			}
			else
			{
				farInteractionCaster = base.gameObject.AddComponent<CurveInteractionCaster>();
			}
		}
		if (nearInteractionCaster == null)
		{
			IInteractionCaster[] components = GetComponents<IInteractionCaster>();
			IInteractionCaster interactionCaster = null;
			if (components.Length != 0)
			{
				foreach (IInteractionCaster interactionCaster2 in components)
				{
					if (!(interactionCaster2 is ICurveInteractionCaster))
					{
						interactionCaster = interactionCaster2;
						break;
					}
				}
			}
			nearInteractionCaster = interactionCaster ?? base.gameObject.AddComponent<SphereInteractionCaster>();
		}
		if (interactionAttachController == null)
		{
			if (TryGetComponent<IInteractionAttachController>(out var component2))
			{
				interactionAttachController = component2;
			}
			else
			{
				interactionAttachController = base.gameObject.AddComponent<InteractionAttachController>();
			}
		}
		base.attachTransform = interactionAttachController.GetOrCreateAnchorTransform();
	}

	public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.PreprocessInteractor(updatePhase);
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			InitializeInteractor();
			UpdateAnchor();
			Region newSelectionRegion = DetermineSelectionRegion();
			EvaluateNearInteraction();
			EvaluateFarInteraction(newSelectionRegion);
			UpdateSelectionRegion(newSelectionRegion);
			HandleUIToolkitEvents();
		}
	}

	private void InitializeInteractor()
	{
		m_InternalValidTargets.Clear();
		m_IndexToSnapVolumeMap.Clear();
		m_TargetColliders.Clear();
		m_FarRayCastHits.Clear();
		m_HasValidRayHit = false;
		m_ValidHitIsSnapVolume = false;
		m_ValidTargetCastSource = Region.None;
	}

	private void UpdateAnchor()
	{
		interactionAttachController.DoUpdate(Time.unscaledDeltaTime);
	}

	private Region DetermineSelectionRegion()
	{
		if (!base.hasSelection)
		{
			return Region.None;
		}
		if (!interactionAttachController.hasOffset)
		{
			return Region.Near;
		}
		return Region.Far;
	}

	private void UpdateSelectionRegion(Region newSelectionRegion)
	{
		m_ReleasedNearInteractionThisFrame = false;
		m_SelectionRegion.Value = newSelectionRegion;
	}

	private void EvaluateNearInteraction()
	{
		if (!m_EnableNearCasting || base.hasSelection || !nearInteractionCaster.TryGetColliderTargets(base.interactionManager, m_TargetColliders))
		{
			return;
		}
		int num = RegisterNearValidTargets(m_TargetColliders, m_InternalValidTargets);
		if (num > 0)
		{
			m_ValidTargetCastSource = Region.Near;
		}
		if (base.targetFilter == null && !m_SortNearTargetsAfterTargetFilter && num > 1)
		{
			IInteractorDistanceEvaluator evaluatorForSortingStrategy = GetEvaluatorForSortingStrategy(m_NearCasterSortingStrategy);
			if (evaluatorForSortingStrategy != null)
			{
				SortingHelpers.SortByDistanceToInteractor(this, m_InternalValidTargets, evaluatorForSortingStrategy);
			}
		}
	}

	private void EvaluateFarInteraction(Region newSelectionRegion)
	{
		if ((!m_EnableFarCasting && newSelectionRegion != Region.Far) || m_ValidTargetCastSource == Region.Near)
		{
			return;
		}
		ICurveInteractionCaster curveInteractionCaster = farInteractionCaster;
		bool flag = curveInteractionCaster.TryGetColliderTargets(base.interactionManager, m_TargetColliders, m_FarRayCastHits);
		bool flag2 = canProcessUIToolkit && XRUIToolkitHandler.IsValidUIToolkitInteraction(m_TargetColliders);
		if (flag && flag2)
		{
			ProcessUIToolkitHit(m_FarRayCastHits[0]);
			m_HasValidRayHit = true;
			m_ValidTargetCastSource = Region.Far;
			return;
		}
		RaycastResult raycastResult;
		bool flag3 = TryGetCurrentUIRaycastResult(out raycastResult);
		m_HasValidRayHit = flag3 || flag;
		m_ValidHitIsUI = false;
		if (m_HasValidRayHit)
		{
			m_ValidTargetCastSource = Region.Far;
			Vector3 farCasterOrigin = curveInteractionCaster.samplePoints[0];
			bool flag4 = false;
			if (flag && flag3)
			{
				RaycastHit raycastHit = m_FarRayCastHits[0];
				flag4 = raycastHit.collider != null && raycastResult.gameObject == raycastHit.collider.gameObject;
			}
			float num = (flag3 ? (raycastResult.worldPosition - farCasterOrigin).sqrMagnitude : float.MaxValue);
			float num2 = (flag ? (m_FarRayCastHits[0].point - farCasterOrigin).sqrMagnitude : float.MaxValue);
			bool shouldProcess2dHit = flag3 && (flag4 || !flag || num < num2);
			if (flag && (flag4 || !flag3 || num2 < num))
			{
				Process3dHit(in farCasterOrigin, flag3, num, ref shouldProcess2dHit);
			}
			if (shouldProcess2dHit)
			{
				Process2dHit(in raycastResult);
			}
		}
	}

	private void ProcessUIToolkitHit(RaycastHit raycastHit)
	{
		m_RayEndTransform = raycastHit.transform;
		m_RayEndPoint = raycastHit.point;
		m_RayEndNormal = raycastHit.normal;
		m_ValidHitIsUI = true;
	}

	private void HandleUIToolkitEvents()
	{
		if (canProcessUIToolkit)
		{
			bool validHitIsUI = m_ValidHitIsUI;
			bool lastValidHitIsUI = m_LastValidHitIsUI;
			m_LastValidHitIsUI = m_ValidHitIsUI;
			bool shouldReset = (!validHitIsUI && lastValidHitIsUI) || !isCurveActive;
			XRUIToolkitHandler.HandlePointerUpdate(this, farInteractionCaster.effectiveCastOrigin.position, farInteractionCaster.effectiveCastOrigin.rotation, isUiSelectInputActive, shouldReset);
		}
	}

	private void Process3dHit(in Vector3 farCasterOrigin, bool has2dHit, float uiHitSqDistance, ref bool shouldProcess2dHit)
	{
		if (!base.hasSelection)
		{
			if (RegisterFarValidTargets(m_TargetColliders, m_InternalValidTargets, out var firstRegisteredIndex) > 0)
			{
				if (m_IndexToSnapVolumeMap.TryGetValue(firstRegisteredIndex, out var value))
				{
					m_RayEndTransform = value.GetAttachTransform(this);
					m_RayEndPoint = m_RayEndTransform.position;
					m_RayEndNormal = m_RayEndTransform.up;
					m_ValidHitIsSnapVolume = true;
					m_ValidHitSnapVolumeInteractable = value;
				}
				else if (firstRegisteredIndex >= 0 && firstRegisteredIndex < m_FarRayCastHits.Count)
				{
					RaycastHit raycastHit = m_FarRayCastHits[firstRegisteredIndex];
					m_RayEndTransform = raycastHit.transform;
					m_RayEndPoint = raycastHit.point;
					m_RayEndNormal = raycastHit.normal;
				}
				if (has2dHit && firstRegisteredIndex > 0)
				{
					shouldProcess2dHit = uiHitSqDistance < (m_RayEndPoint - farCasterOrigin).sqrMagnitude;
				}
			}
			else if (has2dHit)
			{
				shouldProcess2dHit = true;
			}
			else
			{
				m_HasValidRayHit = false;
			}
		}
		else
		{
			RaycastHit raycastHit2 = m_FarRayCastHits[0];
			m_RayEndTransform = raycastHit2.transform;
			m_RayEndPoint = raycastHit2.point;
			m_RayEndNormal = raycastHit2.normal;
		}
	}

	private void Process2dHit(in RaycastResult uiHit)
	{
		m_RayEndTransform = uiHit.gameObject.transform;
		m_RayEndPoint = uiHit.worldPosition;
		m_RayEndNormal = uiHit.worldNormal;
		m_ValidHitIsUI = true;
	}

	protected virtual IInteractorDistanceEvaluator GetEvaluatorForSortingStrategy(NearCasterSortingStrategy strategy)
	{
		return strategy switch
		{
			NearCasterSortingStrategy.SquareDistance => SortingHelpers.squareDistanceAttachPointEvaluator, 
			NearCasterSortingStrategy.InteractableBased => SortingHelpers.interactableBasedEvaluator, 
			NearCasterSortingStrategy.ClosestPointOnCollider => SortingHelpers.closestPointOnColliderEvaluator, 
			_ => null, 
		};
	}

	private int RegisterNearValidTargets(List<Collider> targets, List<IXRInteractable> interactables)
	{
		foreach (Collider target in targets)
		{
			if (base.interactionManager.TryGetInteractableForCollider(target, out var interactable) && base.interactionManager.IsHoverPossible(this, interactable as IXRHoverInteractable))
			{
				interactables.Add(interactable);
			}
		}
		IXRTargetFilter iXRTargetFilter = base.targetFilter;
		if (iXRTargetFilter != null && iXRTargetFilter.canProcess)
		{
			m_PreFilteredTargets.Clear();
			m_PreFilteredTargets.AddRange(interactables);
			iXRTargetFilter.Process(this, m_PreFilteredTargets, interactables);
		}
		return interactables.Count;
	}

	private int RegisterFarValidTargets(List<Collider> targets, List<IXRInteractable> interactables, out int firstRegisteredIndex)
	{
		firstRegisteredIndex = -1;
		int count = targets.Count;
		bool flag = false;
		IXRTargetFilter iXRTargetFilter = base.targetFilter;
		bool flag2 = iXRTargetFilter?.canProcess ?? false;
		if (flag2)
		{
			m_FarTargetToIndexMap.Clear();
		}
		m_IndexToSnapVolumeMap.Clear();
		for (int i = 0; i < count; i++)
		{
			IXRInteractable interactable;
			XRInteractableSnapVolume snapVolume;
			bool num = base.interactionManager.TryGetInteractableForCollider(targets[i], out interactable, out snapVolume);
			bool flag3 = snapVolume != null;
			bool flag4 = num && base.interactionManager.IsHoverPossible(this, interactable as IXRHoverInteractable);
			if (flag4)
			{
				if (!flag)
				{
					firstRegisteredIndex = i;
					flag = true;
				}
				interactables.Add(interactable);
				if (flag3)
				{
					m_IndexToSnapVolumeMap.Add(i, interactable);
				}
				if (flag2)
				{
					m_FarTargetToIndexMap.TryAdd(interactable, i);
				}
			}
			if (!flag2 && (flag4 || !flag3))
			{
				break;
			}
		}
		if (flag2)
		{
			m_PreFilteredTargets.Clear();
			m_PreFilteredTargets.AddRange(interactables);
			iXRTargetFilter.Process(this, m_PreFilteredTargets, interactables);
			firstRegisteredIndex = ((interactables.Count > 0 && m_FarTargetToIndexMap.TryGetValue(interactables[0], out var value)) ? value : (-1));
		}
		return interactables.Count;
	}

	public override void GetValidTargets(List<IXRInteractable> targets)
	{
		targets.Clear();
		if (m_InternalValidTargets.Count != 0)
		{
			if (m_AllowMultipleValidTargets)
			{
				targets.AddRange(m_InternalValidTargets);
			}
			else
			{
				targets.Add(m_InternalValidTargets[0]);
			}
		}
	}

	protected override void OnSelectEntering(SelectEnterEventArgs args)
	{
		base.OnSelectEntering(args);
		if (base.interactablesSelected.Count != 1)
		{
			return;
		}
		m_SelectedTargetCastSource = m_ValidTargetCastSource;
		bool flag = false;
		Vector3 endPoint = Vector3.zero;
		if (m_SelectedTargetCastSource == Region.Far && TryGetCurveEndPoint(out endPoint) != 0)
		{
			flag = m_FarAttachMode == InteractorFarAttachMode.Far;
			if (args.interactableObject is IFarAttachProvider { farAttachMode: not InteractableFarAttachMode.DeferToInteractor } farAttachProvider)
			{
				flag = farAttachProvider.farAttachMode == InteractableFarAttachMode.Far;
			}
		}
		if (flag)
		{
			interactionAttachController.MoveTo(endPoint);
		}
		else
		{
			interactionAttachController.ResetOffset();
		}
	}

	protected override void OnSelectEntered(SelectEnterEventArgs args)
	{
		base.OnSelectEntered(args);
		if (m_SelectedTargetCastSource == Region.Far)
		{
			m_NormalRelativeToInteractable = base.firstInteractableSelected.GetAttachTransform(this).InverseTransformDirection(m_RayEndNormal);
		}
	}

	protected override void OnSelectExiting(SelectExitEventArgs args)
	{
		base.OnSelectExiting(args);
		if (!base.hasSelection)
		{
			m_ValidTargetCastSource = Region.None;
			m_SelectedTargetCastSource = Region.None;
			m_ReleasedNearInteractionThisFrame = !interactionAttachController.hasOffset;
			interactionAttachController.ResetOffset();
			m_SelectionRegion.SetValueWithoutNotify(Region.None);
		}
	}

	protected override void OnSelectExited(SelectExitEventArgs args)
	{
		base.OnSelectExited(args);
		if (!base.hasSelection)
		{
			m_SelectionRegion.BroadcastValue();
		}
	}

	Transform IXRRayProvider.GetOrCreateAttachTransform()
	{
		return interactionAttachController.transformToFollow;
	}

	void IXRRayProvider.SetAttachTransform(Transform newAttach)
	{
		interactionAttachController.transformToFollow = newAttach;
	}

	Transform IXRRayProvider.GetOrCreateRayOrigin()
	{
		return farInteractionCaster.castOrigin;
	}

	void IXRRayProvider.SetRayOrigin(Transform newOrigin)
	{
		farInteractionCaster.castOrigin = newOrigin;
	}

	public void UpdateUIModel(ref TrackedDeviceModel model)
	{
		if (!base.isActiveAndEnabled || !m_EnableFarCasting || !m_EnableUIInteraction || uiModelUpdater == null || m_ValidTargetCastSource == Region.Near || (m_BlockUIOnInteractableSelection && base.hasSelection) || this.IsBlockedByInteractionWithinGroup())
		{
			model.Reset(resetImplementation: false);
			return;
		}
		IUIModelUpdater iUIModelUpdater = uiModelUpdater;
		bool num = isUiSelectInputActive;
		Vector2 scrollDelta = uiScrollInputValue;
		if (!iUIModelUpdater.UpdateUIModel(ref model, num, in scrollDelta))
		{
			model.Reset(resetImplementation: false);
		}
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

	public bool TryGetCurrentUIRaycastResult(out RaycastResult raycastResult)
	{
		if (m_EnableUIInteraction && TryGetUIModel(out var model) && model.currentRaycast.isValid)
		{
			raycastResult = model.currentRaycast;
			return model.currentRaycastEndpointIndex > 0;
		}
		raycastResult = default(RaycastResult);
		return false;
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

	public EndPointType TryGetCurveEndPoint(out Vector3 endPoint, bool snapToSelectedAttachIfAvailable = false, bool snapToSnapVolumeIfAvailable = false)
	{
		bool flag = interactionAttachController.hasOffset && base.hasSelection;
		if (snapToSelectedAttachIfAvailable && flag)
		{
			Transform transform = base.firstInteractableSelected.GetAttachTransform(this);
			endPoint = transform.position;
			return EndPointType.AttachPoint;
		}
		if (snapToSnapVolumeIfAvailable && m_ValidHitIsSnapVolume)
		{
			endPoint = ((m_ValidHitSnapVolumeInteractable != null) ? m_ValidHitSnapVolumeInteractable.GetAttachTransform(this).position : m_RayEndPoint);
			return EndPointType.AttachPoint;
		}
		endPoint = m_RayEndPoint;
		if (!m_HasValidRayHit)
		{
			return EndPointType.None;
		}
		if (m_ValidHitIsUI)
		{
			return EndPointType.UI;
		}
		if (!(m_InternalValidTargets.Count > 0 || flag))
		{
			return EndPointType.EmptyCastHit;
		}
		return EndPointType.ValidCastHit;
	}

	public EndPointType TryGetCurveEndNormal(out Vector3 endNormal, bool snapToSelectedAttachIfAvailable = false)
	{
		bool flag = interactionAttachController.hasOffset && base.hasSelection;
		if (snapToSelectedAttachIfAvailable && flag)
		{
			Transform transform = base.firstInteractableSelected.GetAttachTransform(this);
			endNormal = transform.TransformDirection(m_NormalRelativeToInteractable);
			return EndPointType.AttachPoint;
		}
		endNormal = m_RayEndNormal;
		if (!m_HasValidRayHit)
		{
			return EndPointType.None;
		}
		if (m_ValidHitIsUI)
		{
			return EndPointType.UI;
		}
		if (!(m_InternalValidTargets.Count > 0 || flag))
		{
			return EndPointType.EmptyCastHit;
		}
		return EndPointType.ValidCastHit;
	}
}
