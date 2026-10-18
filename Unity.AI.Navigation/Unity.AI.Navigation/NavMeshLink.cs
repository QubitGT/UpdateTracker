using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.AI.Navigation;

[ExecuteAlways]
[DefaultExecutionOrder(-101)]
[AddComponentMenu("Navigation/NavMesh Link", 33)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshLink.html")]
public class NavMeshLink : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private byte m_SerializedVersion;

	[SerializeField]
	private int m_AgentTypeID;

	[SerializeField]
	private Vector3 m_StartPoint = new Vector3(0f, 0f, -2.5f);

	[SerializeField]
	private Vector3 m_EndPoint = new Vector3(0f, 0f, 2.5f);

	[SerializeField]
	private Transform m_StartTransform;

	[SerializeField]
	private Transform m_EndTransform;

	[SerializeField]
	private bool m_Activated = true;

	[SerializeField]
	private float m_Width;

	[SerializeField]
	[Min(0f)]
	private float m_CostModifier = -1f;

	[SerializeField]
	private bool m_IsOverridingCost;

	[SerializeField]
	private bool m_Bidirectional = true;

	[SerializeField]
	private bool m_AutoUpdatePosition;

	[SerializeField]
	private int m_Area;

	private NavMeshLinkInstance m_LinkInstance;

	private bool m_StartTransformWasEmpty = true;

	private bool m_EndTransformWasEmpty = true;

	private Vector3 m_LastStartWorldPosition = Vector3.positiveInfinity;

	private Vector3 m_LastEndWorldPosition = Vector3.positiveInfinity;

	private Vector3 m_LastPosition = Vector3.positiveInfinity;

	private Quaternion m_LastRotation = Quaternion.identity;

	private static readonly List<NavMeshLink> s_Tracked = new List<NavMeshLink>();

	public int agentTypeID
	{
		get
		{
			return m_AgentTypeID;
		}
		set
		{
			if (value != m_AgentTypeID)
			{
				m_AgentTypeID = value;
				UpdateLink();
			}
		}
	}

	public Vector3 startPoint
	{
		get
		{
			return m_StartPoint;
		}
		set
		{
			if (!(value == m_StartPoint))
			{
				m_StartPoint = value;
				UpdateLink();
			}
		}
	}

	public Vector3 endPoint
	{
		get
		{
			return m_EndPoint;
		}
		set
		{
			if (!(value == m_EndPoint))
			{
				m_EndPoint = value;
				UpdateLink();
			}
		}
	}

	public Transform startTransform
	{
		get
		{
			return m_StartTransform;
		}
		set
		{
			if (!(value == m_StartTransform))
			{
				m_StartTransform = value;
				UpdateLink();
			}
		}
	}

	public Transform endTransform
	{
		get
		{
			return m_EndTransform;
		}
		set
		{
			if (!(value == m_EndTransform))
			{
				m_EndTransform = value;
				UpdateLink();
			}
		}
	}

	public float width
	{
		get
		{
			return m_Width;
		}
		set
		{
			if (!value.Equals(m_Width))
			{
				m_Width = value;
				UpdateLink();
			}
		}
	}

	public float costModifier
	{
		get
		{
			if (!m_IsOverridingCost)
			{
				return 0f - m_CostModifier;
			}
			return m_CostModifier;
		}
		set
		{
			bool flag = value >= 0f;
			if (!value.Equals(costModifier) || flag != m_IsOverridingCost)
			{
				m_IsOverridingCost = flag;
				m_CostModifier = Mathf.Abs(value);
				UpdateLink();
			}
		}
	}

	public bool bidirectional
	{
		get
		{
			return m_Bidirectional;
		}
		set
		{
			if (value != m_Bidirectional)
			{
				m_Bidirectional = value;
				UpdateLink();
			}
		}
	}

	public bool autoUpdate
	{
		get
		{
			return m_AutoUpdatePosition;
		}
		set
		{
			if (value != m_AutoUpdatePosition)
			{
				m_AutoUpdatePosition = value;
				if (m_AutoUpdatePosition)
				{
					AddTracking(this);
				}
				else
				{
					RemoveTracking(this);
				}
			}
		}
	}

	public int area
	{
		get
		{
			return m_Area;
		}
		set
		{
			if (value != m_Area)
			{
				m_Area = value;
				UpdateLink();
			}
		}
	}

	public bool activated
	{
		get
		{
			return m_Activated;
		}
		set
		{
			m_Activated = value;
			NavMesh.SetLinkActive(m_LinkInstance, m_Activated);
		}
	}

	public bool occupied => NavMesh.IsLinkOccupied(m_LinkInstance);

	[Obsolete("autoUpdatePositions has been deprecated. Use autoUpdate instead. (UnityUpgradable) -> autoUpdate")]
	public bool autoUpdatePositions
	{
		get
		{
			return autoUpdate;
		}
		set
		{
			autoUpdate = value;
		}
	}

	[Obsolete("biDirectional has been deprecated. Use bidirectional instead. (UnityUpgradable) -> bidirectional")]
	public bool biDirectional
	{
		get
		{
			return bidirectional;
		}
		set
		{
			bidirectional = value;
		}
	}

	[Obsolete("costOverride has been deprecated. Use costModifier instead. (UnityUpgradable) -> costModifier")]
	public float costOverride
	{
		get
		{
			return costModifier;
		}
		set
		{
			costModifier = value;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ClearTrackedList()
	{
		s_Tracked.Clear();
	}

	private void UpgradeSerializedVersion()
	{
		if (m_SerializedVersion < 1)
		{
			m_SerializedVersion = 1;
			m_IsOverridingCost = m_CostModifier >= 0f;
			m_CostModifier = Mathf.Abs(m_CostModifier);
			if (m_StartTransform == base.gameObject.transform)
			{
				m_StartTransform = null;
			}
			if (m_EndTransform == base.gameObject.transform)
			{
				m_EndTransform = null;
			}
		}
	}

	private void Awake()
	{
		UpgradeSerializedVersion();
	}

	private void OnEnable()
	{
		AddLink();
		if (m_AutoUpdatePosition && NavMesh.IsLinkValid(m_LinkInstance))
		{
			AddTracking(this);
		}
	}

	private void OnDisable()
	{
		RemoveTracking(this);
		NavMesh.RemoveLink(m_LinkInstance);
	}

	public void UpdateLink()
	{
		if (base.isActiveAndEnabled)
		{
			NavMesh.RemoveLink(m_LinkInstance);
			AddLink();
		}
	}

	private static void AddTracking(NavMeshLink link)
	{
		if (s_Tracked.Count == 0)
		{
			NavMesh.onPreUpdate = (NavMesh.OnNavMeshPreUpdate)Delegate.Combine(NavMesh.onPreUpdate, new NavMesh.OnNavMeshPreUpdate(UpdateTrackedInstances));
		}
		s_Tracked.Add(link);
		link.RecordEndpointTransforms();
	}

	private static void RemoveTracking(NavMeshLink link)
	{
		s_Tracked.Remove(link);
		if (s_Tracked.Count == 0)
		{
			NavMesh.onPreUpdate = (NavMesh.OnNavMeshPreUpdate)Delegate.Remove(NavMesh.onPreUpdate, new NavMesh.OnNavMeshPreUpdate(UpdateTrackedInstances));
		}
	}

	internal void GetWorldPositions(out Vector3 worldStartPosition, out Vector3 worldEndPosition)
	{
		bool flag = m_StartTransform == null;
		bool flag2 = m_EndTransform == null;
		Matrix4x4 matrix4x = ((flag || flag2) ? LocalToWorldUnscaled() : Matrix4x4.identity);
		worldStartPosition = (flag ? matrix4x.MultiplyPoint3x4(m_StartPoint) : m_StartTransform.position);
		worldEndPosition = (flag2 ? matrix4x.MultiplyPoint3x4(m_EndPoint) : m_EndTransform.position);
	}

	internal void GetLocalPositions(out Vector3 localStartPosition, out Vector3 localEndPosition)
	{
		bool flag = m_StartTransform == null;
		bool flag2 = m_EndTransform == null;
		Matrix4x4 matrix4x = ((flag && flag2) ? Matrix4x4.identity : LocalToWorldUnscaled().inverse);
		localStartPosition = (flag ? m_StartPoint : matrix4x.MultiplyPoint3x4(m_StartTransform.position));
		localEndPosition = (flag2 ? m_EndPoint : matrix4x.MultiplyPoint3x4(m_EndTransform.position));
	}

	private void AddLink()
	{
		GetLocalPositions(out var localStartPosition, out var localEndPosition);
		NavMeshLinkData navMeshLinkData = default(NavMeshLinkData);
		navMeshLinkData.startPosition = localStartPosition;
		navMeshLinkData.endPosition = localEndPosition;
		navMeshLinkData.width = m_Width;
		navMeshLinkData.costModifier = costModifier;
		navMeshLinkData.bidirectional = m_Bidirectional;
		navMeshLinkData.area = m_Area;
		navMeshLinkData.agentTypeID = m_AgentTypeID;
		NavMeshLinkData link = navMeshLinkData;
		m_LinkInstance = NavMesh.AddLink(link, base.transform.position, base.transform.rotation);
		if (NavMesh.IsLinkValid(m_LinkInstance))
		{
			NavMesh.SetLinkOwner(m_LinkInstance, this);
			NavMesh.SetLinkActive(m_LinkInstance, m_Activated);
		}
		m_LastPosition = base.transform.position;
		m_LastRotation = base.transform.rotation;
		RecordEndpointTransforms();
		GetWorldPositions(out m_LastStartWorldPosition, out m_LastEndWorldPosition);
	}

	internal void RecordEndpointTransforms()
	{
		m_StartTransformWasEmpty = m_StartTransform == null;
		m_EndTransformWasEmpty = m_EndTransform == null;
	}

	internal bool HaveTransformsChanged()
	{
		bool flag = m_StartTransform == null;
		bool flag2 = m_EndTransform == null;
		if (flag && flag2 && m_StartTransformWasEmpty && m_EndTransformWasEmpty && base.transform.position == m_LastPosition && base.transform.rotation == m_LastRotation)
		{
			return false;
		}
		Matrix4x4 matrix4x = ((flag || flag2) ? LocalToWorldUnscaled() : Matrix4x4.identity);
		if ((flag ? matrix4x.MultiplyPoint3x4(m_StartPoint) : m_StartTransform.position) != m_LastStartWorldPosition)
		{
			return true;
		}
		return (flag2 ? matrix4x.MultiplyPoint3x4(m_EndPoint) : m_EndTransform.position) != m_LastEndWorldPosition;
	}

	internal Matrix4x4 LocalToWorldUnscaled()
	{
		return Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
	}

	private void OnDidApplyAnimationProperties()
	{
		UpdateLink();
	}

	private static void UpdateTrackedInstances()
	{
		foreach (NavMeshLink item in s_Tracked)
		{
			if (item.HaveTransformsChanged())
			{
				item.UpdateLink();
			}
			item.RecordEndpointTransforms();
		}
	}

	[Obsolete("UpdatePositions() has been deprecated. Use UpdateLink() instead. (UnityUpgradable) -> UpdateLink(*)")]
	public void UpdatePositions()
	{
		UpdateLink();
	}
}
