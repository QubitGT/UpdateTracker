using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[DisallowMultipleComponent]
[AddComponentMenu("XR/Interactors/XR Socket Interactor", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor.html")]
public class XRSocketInteractor : XRBaseInteractor
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct ShaderPropertyLookup
	{
		public static readonly int surface = Shader.PropertyToID("_Surface");

		public static readonly int mode = Shader.PropertyToID("_Mode");

		public static readonly int srcBlend = Shader.PropertyToID("_SrcBlend");

		public static readonly int dstBlend = Shader.PropertyToID("_DstBlend");

		public static readonly int zWrite = Shader.PropertyToID("_ZWrite");

		public static readonly int baseColor = Shader.PropertyToID("_BaseColor");

		public static readonly int color = Shader.PropertyToID("_Color");
	}

	[SerializeField]
	private bool m_ShowInteractableHoverMeshes = true;

	[SerializeField]
	private Material m_InteractableHoverMeshMaterial;

	[SerializeField]
	private Material m_InteractableCantHoverMeshMaterial;

	[SerializeField]
	private bool m_SocketActive = true;

	[SerializeField]
	private float m_InteractableHoverScale = 1f;

	[SerializeField]
	private float m_RecycleDelayTime = 1f;

	private float m_LastRemoveTime = -1f;

	[SerializeField]
	private bool m_HoverSocketSnapping;

	[SerializeField]
	private float m_SocketSnappingRadius = 0.1f;

	[SerializeField]
	private SocketScaleMode m_SocketScaleMode;

	[SerializeField]
	private Vector3 m_FixedScale = Vector3.one;

	[SerializeField]
	private Vector3 m_TargetBoundsSize = Vector3.one;

	private readonly HashSet<Collider> m_StayedColliders = new HashSet<Collider>();

	private readonly TriggerContactMonitor m_TriggerContactMonitor = new TriggerContactMonitor();

	private readonly Dictionary<IXRInteractable, (MeshFilter, Renderer)[]> m_MeshFilterCache = new Dictionary<IXRInteractable, (MeshFilter, Renderer)[]>();

	private static readonly List<MeshFilter> s_MeshFilters = new List<MeshFilter>();

	private static readonly WaitForFixedUpdate s_WaitForFixedUpdate = new WaitForFixedUpdate();

	private IEnumerator m_UpdateCollidersAfterTriggerStay;

	private readonly XRSocketGrabTransformer m_SocketGrabTransformer = new XRSocketGrabTransformer();

	private readonly HashSetList<XRGrabInteractable> m_InteractablesWithSocketTransformer = new HashSetList<XRGrabInteractable>();

	public bool showInteractableHoverMeshes
	{
		get
		{
			return m_ShowInteractableHoverMeshes;
		}
		set
		{
			m_ShowInteractableHoverMeshes = value;
		}
	}

	public Material interactableHoverMeshMaterial
	{
		get
		{
			return m_InteractableHoverMeshMaterial;
		}
		set
		{
			m_InteractableHoverMeshMaterial = value;
		}
	}

	public Material interactableCantHoverMeshMaterial
	{
		get
		{
			return m_InteractableCantHoverMeshMaterial;
		}
		set
		{
			m_InteractableCantHoverMeshMaterial = value;
		}
	}

	public bool socketActive
	{
		get
		{
			return m_SocketActive;
		}
		set
		{
			m_SocketActive = value;
			m_SocketGrabTransformer.canProcess = value && base.isActiveAndEnabled;
		}
	}

	public float interactableHoverScale
	{
		get
		{
			return m_InteractableHoverScale;
		}
		set
		{
			m_InteractableHoverScale = value;
		}
	}

	public float recycleDelayTime
	{
		get
		{
			return m_RecycleDelayTime;
		}
		set
		{
			m_RecycleDelayTime = value;
		}
	}

	public bool hoverSocketSnapping
	{
		get
		{
			return m_HoverSocketSnapping;
		}
		set
		{
			m_HoverSocketSnapping = value;
		}
	}

	public float socketSnappingRadius
	{
		get
		{
			return m_SocketSnappingRadius;
		}
		set
		{
			m_SocketSnappingRadius = value;
			m_SocketGrabTransformer.socketSnappingRadius = value;
		}
	}

	public SocketScaleMode socketScaleMode
	{
		get
		{
			return m_SocketScaleMode;
		}
		set
		{
			m_SocketScaleMode = value;
			m_SocketGrabTransformer.scaleMode = value;
		}
	}

	public Vector3 fixedScale
	{
		get
		{
			return m_FixedScale;
		}
		set
		{
			m_FixedScale = value;
			m_SocketGrabTransformer.fixedScale = value;
		}
	}

	public Vector3 targetBoundsSize
	{
		get
		{
			return m_TargetBoundsSize;
		}
		set
		{
			m_TargetBoundsSize = value;
			m_SocketGrabTransformer.targetBoundsSize = value;
		}
	}

	protected List<IXRInteractable> unsortedValidTargets { get; } = new List<IXRInteractable>();


	protected virtual int socketSnappingLimit => 1;

	protected virtual bool ejectExistingSocketsWhenSnapping => true;

	public override bool isHoverActive
	{
		get
		{
			if (base.isHoverActive)
			{
				return m_SocketActive;
			}
			return false;
		}
	}

	public override bool isSelectActive
	{
		get
		{
			if (base.isSelectActive)
			{
				return m_SocketActive;
			}
			return false;
		}
	}

	public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride => XRBaseInteractable.MovementType.Instantaneous;

	private bool isHoverRecycleAllowed
	{
		get
		{
			if (!m_HoverSocketSnapping)
			{
				if (!(m_LastRemoveTime < 0f) && !(m_RecycleDelayTime <= 0f))
				{
					return Time.time > m_LastRemoveTime + m_RecycleDelayTime;
				}
				return true;
			}
			return true;
		}
	}

	protected virtual void OnValidate()
	{
		SyncTransformerParams();
	}

	protected override void Awake()
	{
		base.Awake();
		m_TriggerContactMonitor.interactionManager = base.interactionManager;
		m_UpdateCollidersAfterTriggerStay = UpdateCollidersAfterOnTriggerStay();
		SyncTransformerParams();
		CreateDefaultHoverMaterials();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_TriggerContactMonitor.contactAdded += OnContactAdded;
		m_TriggerContactMonitor.contactRemoved += OnContactRemoved;
		SyncTransformerParams();
		ResetCollidersAndValidTargets();
		StartCoroutine(m_UpdateCollidersAfterTriggerStay);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_SocketGrabTransformer.canProcess = false;
		m_TriggerContactMonitor.contactAdded -= OnContactAdded;
		m_TriggerContactMonitor.contactRemoved -= OnContactRemoved;
		ResetCollidersAndValidTargets();
		StopCoroutine(m_UpdateCollidersAfterTriggerStay);
	}

	protected void OnTriggerEnter(Collider other)
	{
		m_TriggerContactMonitor.AddCollider(other);
	}

	protected void OnTriggerStay(Collider other)
	{
		m_StayedColliders.Add(other);
	}

	protected void OnTriggerExit(Collider other)
	{
		m_TriggerContactMonitor.RemoveCollider(other);
	}

	private IEnumerator UpdateCollidersAfterOnTriggerStay()
	{
		while (true)
		{
			yield return s_WaitForFixedUpdate;
			m_TriggerContactMonitor.UpdateStayedColliders(m_StayedColliders);
		}
	}

	public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractor(updatePhase);
		switch (updatePhase)
		{
		case XRInteractionUpdateOrder.UpdatePhase.Fixed:
			m_StayedColliders.Clear();
			break;
		case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
			if (m_ShowInteractableHoverMeshes && base.hasHover && isHoverRecycleAllowed)
			{
				DrawHoveredInteractables();
			}
			break;
		}
	}

	protected virtual void CreateDefaultHoverMaterials()
	{
		if (m_InteractableHoverMeshMaterial != null && m_InteractableCantHoverMeshMaterial != null)
		{
			return;
		}
		string text = (GraphicsSettings.currentRenderPipeline ? "Universal Render Pipeline/Lit" : "Standard");
		Shader shader = Shader.Find(text);
		if (shader == null)
		{
			Debug.LogWarning("Failed to create default materials for Socket Interactor, was unable to find \"" + text + "\" Shader. Make sure the shader is included into the game build.", this);
			return;
		}
		if (m_InteractableHoverMeshMaterial == null)
		{
			m_InteractableHoverMeshMaterial = new Material(shader);
			SetMaterialFade(m_InteractableHoverMeshMaterial, new Color(0f, 0f, 1f, 0.6f));
		}
		if (m_InteractableCantHoverMeshMaterial == null)
		{
			m_InteractableCantHoverMeshMaterial = new Material(shader);
			SetMaterialFade(m_InteractableCantHoverMeshMaterial, new Color(1f, 0f, 0f, 0.6f));
		}
	}

	private static void SetMaterialFade(Material material, Color color)
	{
		material.SetOverrideTag("RenderType", "Transparent");
		bool flag = GraphicsSettings.currentRenderPipeline != null;
		if (flag)
		{
			material.SetFloat(ShaderPropertyLookup.surface, 1f);
		}
		material.SetFloat(ShaderPropertyLookup.mode, 2f);
		material.SetInt(ShaderPropertyLookup.srcBlend, 5);
		material.SetInt(ShaderPropertyLookup.dstBlend, 10);
		material.SetInt(ShaderPropertyLookup.zWrite, 0);
		material.DisableKeyword("_ALPHATEST_ON");
		material.EnableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		material.renderQueue = 3000;
		material.SetColor(flag ? ShaderPropertyLookup.baseColor : ShaderPropertyLookup.color, color);
	}

	protected override void OnHoverEntering(HoverEnterEventArgs args)
	{
		base.OnHoverEntering(args);
		if (!m_ShowInteractableHoverMeshes)
		{
			return;
		}
		IXRHoverInteractable interactableObject = args.interactableObject;
		s_MeshFilters.Clear();
		interactableObject.transform.GetComponentsInChildren(includeInactive: true, s_MeshFilters);
		if (s_MeshFilters.Count != 0)
		{
			(MeshFilter, Renderer)[] array = new(MeshFilter, Renderer)[s_MeshFilters.Count];
			for (int i = 0; i < s_MeshFilters.Count; i++)
			{
				MeshFilter meshFilter = s_MeshFilters[i];
				array[i] = (meshFilter, meshFilter.GetComponent<Renderer>());
			}
			m_MeshFilterCache.Add(interactableObject, array);
		}
	}

	protected override void OnHoverEntered(HoverEnterEventArgs args)
	{
		base.OnHoverEntered(args);
		if (CanHoverSnap(args.interactableObject) && args.interactableObject is XRGrabInteractable grabInteractable)
		{
			StartSocketSnapping(grabInteractable);
		}
	}

	protected virtual bool CanHoverSnap(IXRInteractable interactable)
	{
		if (m_HoverSocketSnapping)
		{
			if (base.hasSelection)
			{
				return IsSelecting(interactable);
			}
			return true;
		}
		return false;
	}

	protected override void OnHoverExiting(HoverExitEventArgs args)
	{
		base.OnHoverExiting(args);
		IXRHoverInteractable interactableObject = args.interactableObject;
		m_MeshFilterCache.Remove(interactableObject);
		if (interactableObject is XRGrabInteractable grabInteractable)
		{
			EndSocketSnapping(grabInteractable);
		}
	}

	protected override void OnSelectEntered(SelectEnterEventArgs args)
	{
		base.OnSelectEntered(args);
		if (args.interactableObject is XRGrabInteractable grabInteractable)
		{
			StartSocketSnapping(grabInteractable);
		}
	}

	protected override void OnSelectExiting(SelectExitEventArgs args)
	{
		base.OnSelectExiting(args);
		m_LastRemoveTime = Time.time;
	}

	protected override void OnSelectExited(SelectExitEventArgs args)
	{
		base.OnSelectExited(args);
		if (args.interactableObject is XRGrabInteractable grabInteractable)
		{
			if (IsHovering(args.interactableObject))
			{
				m_SocketGrabTransformer.scaleOnlyMode = true;
			}
			else
			{
				EndSocketSnapping(grabInteractable);
			}
		}
	}

	private Matrix4x4 GetHoverMeshMatrix(IXRInteractable interactable, MeshFilter meshFilter, float hoverScale)
	{
		Transform transform = interactable.GetAttachTransform(this);
		XRGrabInteractable xRGrabInteractable = interactable as XRGrabInteractable;
		Pose pose;
		if (xRGrabInteractable != null && !xRGrabInteractable.useDynamicAttach && xRGrabInteractable.isSelected && transform != interactable.transform && transform.IsChildOf(interactable.transform))
		{
			Pose localAttachPoseOnSelect = xRGrabInteractable.GetLocalAttachPoseOnSelect(xRGrabInteractable.firstInteractorSelecting);
			Transform parent = transform.parent;
			pose = new Pose(parent.TransformPoint(localAttachPoseOnSelect.position), parent.rotation * localAttachPoseOnSelect.rotation);
		}
		else
		{
			pose = new Pose(transform.position, transform.rotation);
		}
		Vector3 direction = meshFilter.transform.position - pose.position;
		Vector3 vector = InverseTransformDirection(pose, direction) * hoverScale;
		Quaternion quaternion = Quaternion.Inverse(Quaternion.Inverse(meshFilter.transform.rotation) * pose.rotation);
		Transform transform2 = GetAttachTransform(interactable);
		Pose pose2 = new Pose(transform2.position, transform2.rotation);
		Vector3 pos;
		Quaternion q;
		if (xRGrabInteractable == null || xRGrabInteractable.trackRotation)
		{
			pos = pose2.rotation * vector + pose2.position;
			q = pose2.rotation * quaternion;
		}
		else
		{
			pos = pose.rotation * vector + pose2.position;
			q = meshFilter.transform.rotation;
		}
		if (xRGrabInteractable != null && !xRGrabInteractable.trackPosition)
		{
			pos = meshFilter.transform.position;
		}
		Vector3 s = meshFilter.transform.lossyScale * hoverScale;
		return Matrix4x4.TRS(pos, q, s);
	}

	private static Vector3 InverseTransformDirection(Pose pose, Vector3 direction)
	{
		return Quaternion.Inverse(pose.rotation) * direction;
	}

	protected virtual void DrawHoveredInteractables()
	{
		if (!m_ShowInteractableHoverMeshes || m_InteractableHoverScale <= 0f)
		{
			return;
		}
		Camera main = Camera.main;
		if (main == null)
		{
			return;
		}
		foreach (IXRHoverInteractable item in base.interactablesHovered)
		{
			if (item == null || IsSelecting(item) || !m_MeshFilterCache.TryGetValue(item, out var value) || value == null || value.Length == 0)
			{
				continue;
			}
			Material hoveredInteractableMaterial = GetHoveredInteractableMaterial(item);
			if (hoveredInteractableMaterial == null)
			{
				continue;
			}
			(MeshFilter, Renderer)[] array = value;
			for (int i = 0; i < array.Length; i++)
			{
				var (meshFilter, meshRenderer) = array[i];
				if (ShouldDrawHoverMesh(meshFilter, meshRenderer, main))
				{
					Matrix4x4 hoverMeshMatrix = GetHoverMeshMatrix(item, meshFilter, m_InteractableHoverScale);
					Mesh sharedMesh = meshFilter.sharedMesh;
					for (int j = 0; j < sharedMesh.subMeshCount; j++)
					{
						Graphics.DrawMesh(sharedMesh, hoverMeshMatrix, hoveredInteractableMaterial, base.gameObject.layer, null, j);
					}
				}
			}
		}
	}

	protected virtual Material GetHoveredInteractableMaterial(IXRHoverInteractable interactable)
	{
		if (!base.hasSelection)
		{
			return m_InteractableHoverMeshMaterial;
		}
		return m_InteractableCantHoverMeshMaterial;
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
			}
			else
			{
				SortingHelpers.SortByDistanceToInteractor(this, unsortedValidTargets, targets);
			}
		}
	}

	public override bool CanHover(IXRHoverInteractable interactable)
	{
		if (base.CanHover(interactable))
		{
			return isHoverRecycleAllowed;
		}
		return false;
	}

	public override bool CanSelect(IXRSelectInteractable interactable)
	{
		if (base.CanSelect(interactable))
		{
			if (base.hasSelection || interactable.isSelected)
			{
				if (IsSelecting(interactable))
				{
					return interactable.interactorsSelecting.Count == 1;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected virtual bool ShouldDrawHoverMesh(MeshFilter meshFilter, Renderer meshRenderer, Camera mainCamera)
	{
		int cullingMask = mainCamera.cullingMask;
		if (meshFilter != null && (cullingMask & (1 << meshFilter.gameObject.layer)) != 0 && meshRenderer != null)
		{
			return meshRenderer.enabled;
		}
		return false;
	}

	protected override void OnRegistered(InteractorRegisteredEventArgs args)
	{
		base.OnRegistered(args);
		args.manager.interactableRegistered += OnInteractableRegistered;
		args.manager.interactableUnregistered += OnInteractableUnregistered;
		m_TriggerContactMonitor.interactionManager = args.manager;
		m_TriggerContactMonitor.ResolveUnassociatedColliders();
		XRInteractionManager.RemoveAllUnregistered(args.manager, unsortedValidTargets);
	}

	protected override void OnUnregistered(InteractorUnregisteredEventArgs args)
	{
		base.OnUnregistered(args);
		args.manager.interactableRegistered -= OnInteractableRegistered;
		args.manager.interactableUnregistered -= OnInteractableUnregistered;
	}

	private void OnInteractableRegistered(InteractableRegisteredEventArgs args)
	{
		m_TriggerContactMonitor.ResolveUnassociatedColliders(args.interactableObject);
		if (m_TriggerContactMonitor.IsContacting(args.interactableObject) && !unsortedValidTargets.Contains(args.interactableObject))
		{
			unsortedValidTargets.Add(args.interactableObject);
		}
	}

	private void OnInteractableUnregistered(InteractableUnregisteredEventArgs args)
	{
		unsortedValidTargets.Remove(args.interactableObject);
	}

	private void OnContactAdded(IXRInteractable interactable)
	{
		if (!unsortedValidTargets.Contains(interactable))
		{
			unsortedValidTargets.Add(interactable);
		}
	}

	private void OnContactRemoved(IXRInteractable interactable)
	{
		unsortedValidTargets.Remove(interactable);
	}

	private void ResetCollidersAndValidTargets()
	{
		unsortedValidTargets.Clear();
		m_StayedColliders.Clear();
		m_TriggerContactMonitor.UpdateStayedColliders(m_StayedColliders);
	}

	protected virtual bool StartSocketSnapping(XRGrabInteractable grabInteractable)
	{
		m_SocketGrabTransformer.scaleOnlyMode = false;
		int count = m_InteractablesWithSocketTransformer.Count;
		if (count >= socketSnappingLimit || m_InteractablesWithSocketTransformer.Contains(grabInteractable))
		{
			return false;
		}
		if (count > 0 && ejectExistingSocketsWhenSnapping)
		{
			foreach (XRGrabInteractable item in m_InteractablesWithSocketTransformer.AsList())
			{
				item.RemoveSingleGrabTransformer(m_SocketGrabTransformer);
			}
			m_InteractablesWithSocketTransformer.Clear();
		}
		grabInteractable.AddSingleGrabTransformer(m_SocketGrabTransformer);
		m_InteractablesWithSocketTransformer.Add(grabInteractable);
		return true;
	}

	protected virtual bool EndSocketSnapping(XRGrabInteractable grabInteractable)
	{
		grabInteractable.RemoveSingleGrabTransformer(m_SocketGrabTransformer);
		return m_InteractablesWithSocketTransformer.Remove(grabInteractable);
	}

	private void SyncTransformerParams()
	{
		m_SocketGrabTransformer.canProcess = m_SocketActive && base.isActiveAndEnabled;
		m_SocketGrabTransformer.socketInteractor = this;
		m_SocketGrabTransformer.socketSnappingRadius = socketSnappingRadius;
		m_SocketGrabTransformer.scaleMode = socketScaleMode;
		m_SocketGrabTransformer.fixedScale = fixedScale;
		m_SocketGrabTransformer.targetBoundsSize = targetBoundsSize;
	}
}
