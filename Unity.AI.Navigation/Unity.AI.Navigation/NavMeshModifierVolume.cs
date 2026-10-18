using System.Collections.Generic;
using UnityEngine;

namespace Unity.AI.Navigation;

[ExecuteAlways]
[AddComponentMenu("Navigation/NavMesh Modifier Volume", 31)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshModifierVolume.html")]
public class NavMeshModifierVolume : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private byte m_SerializedVersion;

	[SerializeField]
	private Vector3 m_Size = new Vector3(4f, 3f, 4f);

	[SerializeField]
	private Vector3 m_Center = new Vector3(0f, 1f, 0f);

	[SerializeField]
	private int m_Area;

	[SerializeField]
	private List<int> m_AffectedAgents = new List<int>(new int[1] { -1 });

	private static readonly List<NavMeshModifierVolume> s_NavMeshModifiers = new List<NavMeshModifierVolume>();

	public Vector3 size
	{
		get
		{
			return m_Size;
		}
		set
		{
			m_Size = value;
		}
	}

	public Vector3 center
	{
		get
		{
			return m_Center;
		}
		set
		{
			m_Center = value;
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
			m_Area = value;
		}
	}

	public static List<NavMeshModifierVolume> activeModifiers => s_NavMeshModifiers;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ClearNavMeshModifiers()
	{
		s_NavMeshModifiers.Clear();
	}

	private void OnEnable()
	{
		if (!s_NavMeshModifiers.Contains(this))
		{
			s_NavMeshModifiers.Add(this);
		}
	}

	private void OnDisable()
	{
		s_NavMeshModifiers.Remove(this);
	}

	public bool AffectsAgentType(int agentTypeID)
	{
		if (m_AffectedAgents.Count == 0)
		{
			return false;
		}
		if (m_AffectedAgents[0] == -1)
		{
			return true;
		}
		return m_AffectedAgents.IndexOf(agentTypeID) != -1;
	}
}
