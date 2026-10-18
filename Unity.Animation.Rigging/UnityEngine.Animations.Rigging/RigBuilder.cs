using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
[AddComponentMenu("Animation Rigging/Setup/Rig Builder")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/RiggingWorkflow.html#rig-builder-component")]
public class RigBuilder : MonoBehaviour, IAnimationWindowPreview, IRigEffectorHolder
{
	public delegate void OnAddRigBuilderCallback(RigBuilder rigBuilder);

	public delegate void OnRemoveRigBuilderCallback(RigBuilder rigBuilder);

	[SerializeField]
	private List<RigLayer> m_RigLayers;

	private IRigLayer[] m_RuntimeRigLayers;

	private SyncSceneToStreamLayer m_SyncSceneToStreamLayer;

	[SerializeField]
	private List<RigEffectorData> m_Effectors = new List<RigEffectorData>();

	private bool m_IsInPreview;

	public static OnAddRigBuilderCallback onAddRigBuilder;

	public static OnRemoveRigBuilderCallback onRemoveRigBuilder;

	public List<RigLayer> layers
	{
		get
		{
			if (m_RigLayers == null)
			{
				m_RigLayers = new List<RigLayer>();
			}
			return m_RigLayers;
		}
		set
		{
			m_RigLayers = value;
		}
	}

	private SyncSceneToStreamLayer syncSceneToStreamLayer
	{
		get
		{
			if (m_SyncSceneToStreamLayer == null)
			{
				m_SyncSceneToStreamLayer = new SyncSceneToStreamLayer();
			}
			return m_SyncSceneToStreamLayer;
		}
		set
		{
			m_SyncSceneToStreamLayer = value;
		}
	}

	public PlayableGraph graph { get; private set; }

	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			Build();
		}
		onAddRigBuilder?.Invoke(this);
	}

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			Clear();
		}
		onRemoveRigBuilder?.Invoke(this);
	}

	private void OnDestroy()
	{
		Clear();
	}

	public void Evaluate(float deltaTime)
	{
		if (graph.IsValid())
		{
			SyncLayers();
			graph.Evaluate(deltaTime);
		}
	}

	private void Update()
	{
		if (graph.IsValid())
		{
			SyncLayers();
		}
	}

	public void SyncLayers()
	{
		if (m_RuntimeRigLayers == null)
		{
			return;
		}
		syncSceneToStreamLayer.Update(m_RuntimeRigLayers);
		int i = 0;
		for (int num = m_RuntimeRigLayers.Length; i < num; i++)
		{
			if (m_RuntimeRigLayers[i].IsValid() && m_RuntimeRigLayers[i].active)
			{
				m_RuntimeRigLayers[i].Update();
			}
		}
	}

	public bool Build()
	{
		if (m_IsInPreview)
		{
			return false;
		}
		Clear();
		Animator component = GetComponent<Animator>();
		if (component == null || layers.Count == 0)
		{
			return false;
		}
		IRigLayer[] runtimeRigLayers = layers.ToArray();
		m_RuntimeRigLayers = runtimeRigLayers;
		graph = RigBuilderUtils.BuildPlayableGraph(component, m_RuntimeRigLayers, syncSceneToStreamLayer);
		if (!graph.IsValid())
		{
			return false;
		}
		graph.Play();
		return true;
	}

	public bool Build(PlayableGraph graph)
	{
		if (m_IsInPreview)
		{
			return false;
		}
		Clear();
		Animator component = GetComponent<Animator>();
		if (component == null || layers.Count == 0)
		{
			return false;
		}
		IRigLayer[] runtimeRigLayers = layers.ToArray();
		m_RuntimeRigLayers = runtimeRigLayers;
		RigBuilderUtils.BuildPlayableGraph(graph, component, m_RuntimeRigLayers, syncSceneToStreamLayer);
		return true;
	}

	public void Clear()
	{
		if (m_IsInPreview)
		{
			return;
		}
		if (graph.IsValid())
		{
			graph.Destroy();
		}
		if (m_RuntimeRigLayers != null)
		{
			IRigLayer[] runtimeRigLayers = m_RuntimeRigLayers;
			for (int i = 0; i < runtimeRigLayers.Length; i++)
			{
				runtimeRigLayers[i].Reset();
			}
			m_RuntimeRigLayers = null;
		}
		syncSceneToStreamLayer.Reset();
	}

	public void StartPreview()
	{
		m_IsInPreview = true;
		if (!base.enabled)
		{
			return;
		}
		if (m_RuntimeRigLayers == null)
		{
			IRigLayer[] runtimeRigLayers = layers.ToArray();
			m_RuntimeRigLayers = runtimeRigLayers;
		}
		Animator component = GetComponent<Animator>();
		if (component != null)
		{
			IRigLayer[] runtimeRigLayers = m_RuntimeRigLayers;
			for (int i = 0; i < runtimeRigLayers.Length; i++)
			{
				runtimeRigLayers[i].Initialize(component);
			}
		}
	}

	public void StopPreview()
	{
		m_IsInPreview = false;
		if (base.enabled && !Application.isPlaying)
		{
			Clear();
		}
	}

	public void UpdatePreviewGraph(PlayableGraph graph)
	{
		if (!base.enabled || !graph.IsValid() || m_RuntimeRigLayers == null)
		{
			return;
		}
		syncSceneToStreamLayer.Update(m_RuntimeRigLayers);
		IRigLayer[] runtimeRigLayers = m_RuntimeRigLayers;
		foreach (IRigLayer rigLayer in runtimeRigLayers)
		{
			if (rigLayer.IsValid() && rigLayer.active)
			{
				rigLayer.Update();
			}
		}
	}

	public Playable BuildPreviewGraph(PlayableGraph graph, Playable inputPlayable)
	{
		if (!base.enabled)
		{
			return inputPlayable;
		}
		if (m_RuntimeRigLayers == null)
		{
			StartPreview();
		}
		Animator component = GetComponent<Animator>();
		if (component == null || m_RuntimeRigLayers == null || m_RuntimeRigLayers.Length == 0)
		{
			return inputPlayable;
		}
		foreach (RigBuilderUtils.PlayableChain item in RigBuilderUtils.BuildPlayables(component, graph, m_RuntimeRigLayers, syncSceneToStreamLayer))
		{
			if (item.playables != null && item.playables.Length != 0)
			{
				item.playables[0].AddInput(inputPlayable, 0, 1f);
				inputPlayable = item.playables[item.playables.Length - 1];
			}
		}
		return inputPlayable;
	}
}
