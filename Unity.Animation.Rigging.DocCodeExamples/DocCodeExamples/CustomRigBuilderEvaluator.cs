using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

namespace DocCodeExamples;

[RequireComponent(typeof(RigBuilder))]
public class CustomRigBuilderEvaluator : MonoBehaviour
{
	private RigBuilder m_RigBuilder;

	private void OnEnable()
	{
		m_RigBuilder = GetComponent<RigBuilder>();
		m_RigBuilder.enabled = false;
		if (m_RigBuilder.Build())
		{
			m_RigBuilder.graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
		}
	}

	private void LateUpdate()
	{
		m_RigBuilder.Evaluate(Time.deltaTime);
	}
}
