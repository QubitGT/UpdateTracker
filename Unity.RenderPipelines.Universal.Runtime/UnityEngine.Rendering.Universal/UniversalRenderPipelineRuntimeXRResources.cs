using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[CategoryInfo(Name = "R: Runtime XR", Order = 1000)]
[HideInInspector]
public class UniversalRenderPipelineRuntimeXRResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[ResourcePath("Shaders/XR/XROcclusionMesh.shader", SearchType.ProjectPath)]
	private Shader m_xrOcclusionMeshPS;

	[SerializeField]
	[ResourcePath("Shaders/XR/XRMirrorView.shader", SearchType.ProjectPath)]
	private Shader m_xrMirrorViewPS;

	[SerializeField]
	[ResourcePath("Shaders/XR/XRMotionVector.shader", SearchType.ProjectPath)]
	private Shader m_xrMotionVector;

	public int version => 0;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public Shader xrOcclusionMeshPS
	{
		get
		{
			return m_xrOcclusionMeshPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_xrOcclusionMeshPS, value, "m_xrOcclusionMeshPS");
		}
	}

	public Shader xrMirrorViewPS
	{
		get
		{
			return m_xrMirrorViewPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_xrMirrorViewPS, value, "m_xrMirrorViewPS");
		}
	}

	public Shader xrMotionVector
	{
		get
		{
			return m_xrMotionVector;
		}
		set
		{
			this.SetValueAndNotify(ref m_xrMotionVector, value, "m_xrMotionVector");
		}
	}

	internal bool valid
	{
		get
		{
			if (xrOcclusionMeshPS == null)
			{
				return false;
			}
			if (xrMirrorViewPS == null)
			{
				return false;
			}
			if (m_xrMotionVector == null)
			{
				return false;
			}
			return true;
		}
	}
}
