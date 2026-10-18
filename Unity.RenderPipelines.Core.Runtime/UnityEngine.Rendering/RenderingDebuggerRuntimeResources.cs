using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[HideInInspector]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "R : Rendering Debugger Resources", Order = 100)]
[ElementInfo(Order = 0)]
internal class RenderingDebuggerRuntimeResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	private enum Version
	{
		Initial = 0,
		Count = 1,
		Last = 0
	}

	[SerializeField]
	[HideInInspector]
	private Version m_version;

	int IRenderPipelineGraphicsSettings.version => (int)m_version;
}
