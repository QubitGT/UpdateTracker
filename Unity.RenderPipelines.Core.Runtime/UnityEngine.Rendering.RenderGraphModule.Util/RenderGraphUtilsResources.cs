using System;
using System.ComponentModel;

namespace UnityEngine.Rendering.RenderGraphModule.Util;

[Serializable]
[HideInInspector]
[Category("Resources/Render Graph Helper Function Resources")]
[SupportedOnRenderPipeline(new Type[] { })]
internal class RenderGraphUtilsResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	public enum Version
	{
		Initial = 0,
		Count = 1,
		Latest = 0
	}

	[SerializeField]
	[HideInInspector]
	private Version m_Version;

	[SerializeField]
	[ResourcePath("Shaders/CoreCopy.shader", SearchType.ProjectPath)]
	internal Shader m_CoreCopyPS;

	int IRenderPipelineGraphicsSettings.version => (int)m_Version;

	public Shader coreCopyPS
	{
		get
		{
			return m_CoreCopyPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_CoreCopyPS, value, "m_CoreCopyPS");
		}
	}
}
