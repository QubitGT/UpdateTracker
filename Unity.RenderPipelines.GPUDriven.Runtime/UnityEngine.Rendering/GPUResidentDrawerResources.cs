using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "R: GPU Resident Drawers", Order = 1000)]
[HideInInspector]
internal class GPUResidentDrawerResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
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
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/InstanceDataBufferCopyKernels.compute", SearchType.ProjectPath)]
	private ComputeShader m_InstanceDataBufferCopyKernels;

	[SerializeField]
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/InstanceDataBufferUploadKernels.compute", SearchType.ProjectPath)]
	private ComputeShader m_InstanceDataBufferUploadKernels;

	[SerializeField]
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/InstanceTransformUpdateKernels.compute", SearchType.ProjectPath)]
	private ComputeShader m_TransformUpdaterKernels;

	[SerializeField]
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/InstanceWindDataUpdateKernels.compute", SearchType.ProjectPath)]
	public ComputeShader m_WindDataUpdaterKernels;

	[SerializeField]
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/OccluderDepthPyramidKernels.compute", SearchType.ProjectPath)]
	private ComputeShader m_OccluderDepthPyramidKernels;

	[SerializeField]
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/InstanceOcclusionCullingKernels.compute", SearchType.ProjectPath)]
	private ComputeShader m_InstanceOcclusionCullingKernels;

	[SerializeField]
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/OcclusionCullingDebug.compute", SearchType.ProjectPath)]
	private ComputeShader m_OcclusionCullingDebugKernels;

	[SerializeField]
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/DebugOcclusionTest.shader", SearchType.ProjectPath)]
	private Shader m_DebugOcclusionTestPS;

	[SerializeField]
	[ResourcePath("Runtime/RenderPipelineResources/GPUDriven/DebugOccluder.shader", SearchType.ProjectPath)]
	private Shader m_DebugOccluderPS;

	int IRenderPipelineGraphicsSettings.version => (int)m_Version;

	public ComputeShader instanceDataBufferCopyKernels
	{
		get
		{
			return m_InstanceDataBufferCopyKernels;
		}
		set
		{
			this.SetValueAndNotify(ref m_InstanceDataBufferCopyKernels, value, "m_InstanceDataBufferCopyKernels");
		}
	}

	public ComputeShader instanceDataBufferUploadKernels
	{
		get
		{
			return m_InstanceDataBufferUploadKernels;
		}
		set
		{
			this.SetValueAndNotify(ref m_InstanceDataBufferUploadKernels, value, "m_InstanceDataBufferUploadKernels");
		}
	}

	public ComputeShader transformUpdaterKernels
	{
		get
		{
			return m_TransformUpdaterKernels;
		}
		set
		{
			this.SetValueAndNotify(ref m_TransformUpdaterKernels, value, "m_TransformUpdaterKernels");
		}
	}

	public ComputeShader windDataUpdaterKernels
	{
		get
		{
			return m_WindDataUpdaterKernels;
		}
		set
		{
			this.SetValueAndNotify(ref m_WindDataUpdaterKernels, value, "m_WindDataUpdaterKernels");
		}
	}

	public ComputeShader occluderDepthPyramidKernels
	{
		get
		{
			return m_OccluderDepthPyramidKernels;
		}
		set
		{
			this.SetValueAndNotify(ref m_OccluderDepthPyramidKernels, value, "m_OccluderDepthPyramidKernels");
		}
	}

	public ComputeShader instanceOcclusionCullingKernels
	{
		get
		{
			return m_InstanceOcclusionCullingKernels;
		}
		set
		{
			this.SetValueAndNotify(ref m_InstanceOcclusionCullingKernels, value, "m_InstanceOcclusionCullingKernels");
		}
	}

	public ComputeShader occlusionCullingDebugKernels
	{
		get
		{
			return m_OcclusionCullingDebugKernels;
		}
		set
		{
			this.SetValueAndNotify(ref m_OcclusionCullingDebugKernels, value, "m_OcclusionCullingDebugKernels");
		}
	}

	public Shader debugOcclusionTestPS
	{
		get
		{
			return m_DebugOcclusionTestPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_DebugOcclusionTestPS, value, "m_DebugOcclusionTestPS");
		}
	}

	public Shader debugOccluderPS
	{
		get
		{
			return m_DebugOccluderPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_DebugOccluderPS, value, "m_DebugOccluderPS");
		}
	}
}
