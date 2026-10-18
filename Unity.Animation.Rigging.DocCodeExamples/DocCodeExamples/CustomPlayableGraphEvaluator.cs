using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

namespace DocCodeExamples;

[RequireComponent(typeof(RigBuilder))]
public class CustomPlayableGraphEvaluator : MonoBehaviour
{
	private RigBuilder m_RigBuilder;

	private PlayableGraph m_PlayableGraph;

	private void OnEnable()
	{
		m_RigBuilder = GetComponent<RigBuilder>();
		m_PlayableGraph = PlayableGraph.Create();
		m_PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
		m_RigBuilder.Build(m_PlayableGraph);
	}

	private void OnDisable()
	{
		if (m_PlayableGraph.IsValid())
		{
			m_PlayableGraph.Destroy();
		}
	}

	private void LateUpdate()
	{
		m_RigBuilder.SyncLayers();
		m_PlayableGraph.Evaluate(Time.deltaTime);
	}
}
