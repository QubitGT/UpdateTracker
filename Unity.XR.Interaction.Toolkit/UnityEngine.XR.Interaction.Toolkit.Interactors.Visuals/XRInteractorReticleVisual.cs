using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine.EventSystems;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[AddComponentMenu("XR/Visual/XR Interactor Reticle Visual", 11)]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorReticleVisual.html")]
public class XRInteractorReticleVisual : MonoBehaviour
{
	private const int k_MaxRaycastHits = 10;

	[SerializeField]
	private float m_MaxRaycastDistance = 10f;

	[SerializeField]
	private GameObject m_ReticlePrefab;

	[SerializeField]
	private float m_PrefabScalingFactor = 1f;

	[SerializeField]
	private bool m_UndoDistanceScaling = true;

	[SerializeField]
	private bool m_AlignPrefabWithSurfaceNormal = true;

	[SerializeField]
	private float m_EndpointSmoothingTime = 0.02f;

	[SerializeField]
	private bool m_DrawWhileSelecting;

	[SerializeField]
	private bool m_DrawOnNoHit;

	[SerializeField]
	private LayerMask m_RaycastMask = -1;

	private bool m_ReticleActive;

	private NativeArray<Vector3> m_InteractorLinePoints;

	private XROrigin m_XROrigin;

	private GameObject m_ReticleInstance;

	private XRBaseInteractor m_Interactor;

	private Vector3 m_TargetEndPoint;

	private Vector3 m_TargetEndNormal;

	private PhysicsScene m_LocalPhysicsScene;

	private bool m_HasRaycastHit;

	private readonly RaycastHit[] m_RaycastHits = new RaycastHit[10];

	public float maxRaycastDistance
	{
		get
		{
			return m_MaxRaycastDistance;
		}
		set
		{
			m_MaxRaycastDistance = value;
		}
	}

	public GameObject reticlePrefab
	{
		get
		{
			return m_ReticlePrefab;
		}
		set
		{
			m_ReticlePrefab = value;
			SetupReticlePrefab();
		}
	}

	public float prefabScalingFactor
	{
		get
		{
			return m_PrefabScalingFactor;
		}
		set
		{
			m_PrefabScalingFactor = value;
		}
	}

	public bool undoDistanceScaling
	{
		get
		{
			return m_UndoDistanceScaling;
		}
		set
		{
			m_UndoDistanceScaling = value;
		}
	}

	public bool alignPrefabWithSurfaceNormal
	{
		get
		{
			return m_AlignPrefabWithSurfaceNormal;
		}
		set
		{
			m_AlignPrefabWithSurfaceNormal = value;
		}
	}

	public float endpointSmoothingTime
	{
		get
		{
			return m_EndpointSmoothingTime;
		}
		set
		{
			m_EndpointSmoothingTime = value;
		}
	}

	public bool drawWhileSelecting
	{
		get
		{
			return m_DrawWhileSelecting;
		}
		set
		{
			m_DrawWhileSelecting = value;
		}
	}

	public bool drawOnNoHit
	{
		get
		{
			return m_DrawOnNoHit;
		}
		set
		{
			m_DrawOnNoHit = value;
		}
	}

	public LayerMask raycastMask
	{
		get
		{
			return m_RaycastMask;
		}
		set
		{
			m_RaycastMask = value;
		}
	}

	public bool reticleActive
	{
		get
		{
			return m_ReticleActive;
		}
		set
		{
			m_ReticleActive = value;
			if (m_ReticleInstance != null)
			{
				m_ReticleInstance.SetActive(value);
			}
		}
	}

	protected void Awake()
	{
		m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
		if (TryGetComponent<XRBaseInteractor>(out m_Interactor))
		{
			m_Interactor.selectEntered.AddListener(OnSelectEntered);
		}
		FindXROrigin();
		SetupReticlePrefab();
		reticleActive = false;
	}

	protected void OnDisable()
	{
		reticleActive = false;
	}

	protected void Update()
	{
		if (m_Interactor != null && UpdateReticleTarget())
		{
			ActivateReticleAtTarget();
		}
		else
		{
			reticleActive = false;
		}
	}

	protected void OnDestroy()
	{
		if (m_InteractorLinePoints.IsCreated)
		{
			m_InteractorLinePoints.Dispose();
		}
		if (m_Interactor != null)
		{
			m_Interactor.selectEntered.RemoveListener(OnSelectEntered);
		}
	}

	private void FindXROrigin()
	{
		if (m_XROrigin == null)
		{
			ComponentLocatorUtility<XROrigin>.TryFindComponent(out m_XROrigin);
		}
	}

	private void SetupReticlePrefab()
	{
		if (m_ReticleInstance != null)
		{
			Object.Destroy(m_ReticleInstance);
		}
		if (m_ReticlePrefab != null)
		{
			m_ReticleInstance = Object.Instantiate(m_ReticlePrefab);
		}
	}

	private static RaycastHit FindClosestHit(RaycastHit[] hits, int hitCount)
	{
		int num = 0;
		float num2 = float.MaxValue;
		for (int i = 0; i < hitCount; i++)
		{
			if (hits[i].distance < num2)
			{
				num2 = hits[i].distance;
				num = i;
			}
		}
		return hits[num];
	}

	private bool TryGetRaycastPoint(ref Vector3 raycastPos, ref Vector3 raycastNormal)
	{
		bool result = false;
		int num = m_LocalPhysicsScene.Raycast(m_Interactor.attachTransform.position, m_Interactor.attachTransform.forward, m_RaycastHits, m_MaxRaycastDistance, m_RaycastMask);
		if (num != 0)
		{
			RaycastHit raycastHit = FindClosestHit(m_RaycastHits, num);
			raycastPos = raycastHit.point;
			raycastNormal = raycastHit.normal;
			result = true;
		}
		return result;
	}

	private bool UpdateReticleTarget()
	{
		if (!m_DrawWhileSelecting && m_Interactor.hasSelection)
		{
			return false;
		}
		if (m_Interactor.disableVisualsWhenBlockedInGroup && m_Interactor.IsBlockedByInteractionWithinGroup())
		{
			return false;
		}
		bool flag = false;
		Vector3 raycastPos = Vector3.zero;
		Vector3 raycastNormal = Vector3.zero;
		if (m_Interactor is XRRayInteractor xRRayInteractor)
		{
			if (xRRayInteractor.TryGetCurrentRaycast(out var raycastHit, out var _, out var uiRaycastHit, out var uiRaycastHitIndex, out var isUIHitClosest))
			{
				if (isUIHitClosest)
				{
					RaycastResult value = uiRaycastHit.Value;
					raycastPos = value.worldPosition;
					raycastNormal = value.worldNormal;
					if (Vector3.Dot(xRRayInteractor.rayOriginTransform.forward, raycastNormal) > 0f)
					{
						raycastNormal *= -1f;
					}
					flag = true;
				}
				else if (raycastHit.HasValue)
				{
					RaycastHit value2 = raycastHit.Value;
					raycastPos = value2.point;
					raycastNormal = value2.normal;
					flag = true;
				}
			}
			else if (m_DrawOnNoHit && xRRayInteractor.GetLinePoints(ref m_InteractorLinePoints, out uiRaycastHitIndex))
			{
				_ = m_InteractorLinePoints;
				raycastPos = ((m_InteractorLinePoints.Length > 0) ? m_InteractorLinePoints[m_InteractorLinePoints.Length - 1] : Vector3.zero);
			}
		}
		else if (TryGetRaycastPoint(ref raycastPos, ref raycastNormal))
		{
			flag = true;
		}
		m_HasRaycastHit = flag;
		if (flag || m_DrawOnNoHit)
		{
			Vector3 currentVelocity = Vector3.zero;
			m_TargetEndPoint = Vector3.SmoothDamp(m_TargetEndPoint, raycastPos, ref currentVelocity, m_EndpointSmoothingTime);
			m_TargetEndNormal = Vector3.SmoothDamp(m_TargetEndNormal, raycastNormal, ref currentVelocity, m_EndpointSmoothingTime);
			return true;
		}
		return false;
	}

	private void ActivateReticleAtTarget()
	{
		if (!(m_ReticleInstance != null))
		{
			return;
		}
		Vector3 vector = ((m_XROrigin != null && m_XROrigin.Origin != null) ? m_XROrigin.Origin.transform.up : Vector3.up);
		if (m_AlignPrefabWithSurfaceNormal && m_HasRaycastHit)
		{
			Vector3 vector2 = vector;
			float num = Vector3.Dot(m_TargetEndNormal, vector2);
			if (Mathf.Approximately(Mathf.Abs(num), 1f))
			{
				vector2 = m_Interactor.transform.forward * num;
			}
			Vector3 vector3 = Vector3.ProjectOnPlane(vector2, m_TargetEndNormal);
			if (vector3 != Vector3.zero)
			{
				m_ReticleInstance.transform.SetWorldPose(new Pose(m_TargetEndPoint, Quaternion.LookRotation(vector3, m_TargetEndNormal)));
			}
			else
			{
				m_ReticleInstance.transform.position = m_TargetEndPoint;
			}
		}
		else
		{
			m_ReticleInstance.transform.SetWorldPose(new Pose(m_TargetEndPoint, Quaternion.LookRotation(vector, (m_Interactor.attachTransform.position - m_TargetEndPoint).normalized)));
		}
		float num2 = m_PrefabScalingFactor;
		if (m_UndoDistanceScaling)
		{
			num2 *= Vector3.Distance(m_Interactor.attachTransform.position, m_TargetEndPoint);
		}
		m_ReticleInstance.transform.localScale = new Vector3(num2, num2, num2);
		reticleActive = true;
	}

	private void OnSelectEntered(SelectEnterEventArgs args)
	{
		reticleActive = false;
	}
}
