using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[CategoryInfo(Name = "R: Universal Renderer Shaders", Order = 1000)]
[HideInInspector]
public class UniversalRendererResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Shaders/Utils/CopyDepth.shader", SearchType.ProjectPath)]
	private Shader m_CopyDepthPS;

	[SerializeField]
	[ResourcePath("Shaders/CameraMotionVectors.shader", SearchType.ProjectPath)]
	private Shader m_CameraMotionVector;

	[SerializeField]
	[ResourcePath("Shaders/Utils/StencilDeferred.shader", SearchType.ProjectPath)]
	private Shader m_StencilDeferredPS;

	[SerializeField]
	[ResourcePath("Shaders/Utils/ClusterDeferred.shader", SearchType.ProjectPath)]
	private Shader m_ClusterDeferred;

	[SerializeField]
	[ResourcePath("Shaders/Utils/StencilDitherMaskSeed.shader", SearchType.ProjectPath)]
	private Shader m_StencilDitherMaskSeedPS;

	[Header("Decal Renderer Feature Specific")]
	[SerializeField]
	[ResourcePath("Runtime/Decal/DBuffer/DBufferClear.shader", SearchType.ProjectPath)]
	private Shader m_DBufferClear;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public Shader copyDepthPS
	{
		get
		{
			return m_CopyDepthPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_CopyDepthPS, value, "m_CopyDepthPS");
		}
	}

	public Shader cameraMotionVector
	{
		get
		{
			return m_CameraMotionVector;
		}
		set
		{
			this.SetValueAndNotify(ref m_CameraMotionVector, value, "m_CameraMotionVector");
		}
	}

	public Shader stencilDeferredPS
	{
		get
		{
			return m_StencilDeferredPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_StencilDeferredPS, value, "m_StencilDeferredPS");
		}
	}

	public Shader clusterDeferred
	{
		get
		{
			return m_ClusterDeferred;
		}
		set
		{
			this.SetValueAndNotify(ref m_ClusterDeferred, value, "m_ClusterDeferred");
		}
	}

	public Shader stencilDitherMaskSeedPS
	{
		get
		{
			return m_StencilDitherMaskSeedPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_StencilDitherMaskSeedPS, value, "m_StencilDitherMaskSeedPS");
		}
	}

	public Shader decalDBufferClear
	{
		get
		{
			return m_DBufferClear;
		}
		set
		{
			this.SetValueAndNotify(ref m_DBufferClear, value, "m_DBufferClear");
		}
	}
}
