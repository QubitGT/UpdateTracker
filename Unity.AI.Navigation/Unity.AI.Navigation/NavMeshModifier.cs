using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.AI.Navigation;

[ExecuteAlways]
[DefaultExecutionOrder(-103)]
[AddComponentMenu("Navigation/NavMesh Modifier", 32)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshModifier.html")]
public class NavMeshModifier : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private byte m_SerializedVersion;

	[SerializeField]
	private bool m_OverrideArea;

	[SerializeField]
	private int m_Area;

	[SerializeField]
	private bool m_OverrideGenerateLinks;

	[SerializeField]
	private bool m_GenerateLinks;

	[SerializeField]
	private bool m_IgnoreFromBuild;

	[SerializeField]
	private bool m_ApplyToChildren = true;

	[SerializeField]
	private List<int> m_AffectedAgents = new List<int>(new int[1] { -1 });

	private static bool s_RebuildNavMeshModifiers = true;

	private static List<NavMeshModifier> s_NavMeshModifiers = new List<NavMeshModifier>();

	private static readonly HashSet<NavMeshModifier> s_NavMeshModifiersSet = new HashSet<NavMeshModifier>();

	public bool overrideArea
	{
		get
		{
			return m_OverrideArea;
		}
		set
		{
			m_OverrideArea = value;
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

	public bool overrideGenerateLinks
	{
		get
		{
			return m_OverrideGenerateLinks;
		}
		set
		{
			m_OverrideGenerateLinks = value;
		}
	}

	public bool generateLinks
	{
		get
		{
			return m_GenerateLinks;
		}
		set
		{
			m_GenerateLinks = value;
		}
	}

	public bool ignoreFromBuild
	{
		get
		{
			return m_IgnoreFromBuild;
		}
		set
		{
			m_IgnoreFromBuild = value;
		}
	}

	public bool applyToChildren
	{
		get
		{
			return m_ApplyToChildren;
		}
		set
		{
			m_ApplyToChildren = value;
		}
	}

	public static List<NavMeshModifier> activeModifiers
	{
		get
		{
			if (s_RebuildNavMeshModifiers)
			{
				s_NavMeshModifiers = s_NavMeshModifiersSet.ToList();
				s_RebuildNavMeshModifiers = false;
			}
			return s_NavMeshModifiers;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ClearNavMeshModifiers()
	{
		s_NavMeshModifiers.Clear();
		s_NavMeshModifiersSet.Clear();
	}

	private void OnEnable()
	{
		RegisterModifier();
	}

	private void OnDisable()
	{
		UnregisterModifier();
	}

	private void RegisterModifier()
	{
		if (s_NavMeshModifiersSet.Add(this))
		{
			s_RebuildNavMeshModifiers = true;
		}
	}

	private void UnregisterModifier()
	{
		if (s_NavMeshModifiersSet.Remove(this))
		{
			s_RebuildNavMeshModifiers = true;
		}
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
