using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

public abstract class ScriptableRendererData : ScriptableObject
{
	[Serializable]
	[Obsolete("Moved to UniversalRenderPipelineDebugShaders on GraphicsSettings. #from(2023.3)", false)]
	[ReloadGroup]
	public sealed class DebugShaderResources
	{
		[Obsolete("Moved to UniversalRenderPipelineDebugShaders on GraphicsSettings. #from(2023.3)", false)]
		[Reload("Shaders/Debug/DebugReplacement.shader", ReloadAttribute.Package.Root)]
		public Shader debugReplacementPS;

		[Obsolete("Moved to UniversalRenderPipelineDebugShaders on GraphicsSettings. #from(2023.3)", false)]
		[Reload("Shaders/Debug/HDRDebugView.shader", ReloadAttribute.Package.Root)]
		public Shader hdrDebugViewPS;
	}

	[Serializable]
	[ReloadGroup]
	[Obsolete("Probe volume debug resource are now in the ProbeVolumeDebugResources class.")]
	public sealed class ProbeVolumeResources
	{
		[Obsolete("This shader is now in the ProbeVolumeDebugResources class.")]
		public Shader probeVolumeDebugShader;

		[Obsolete("This shader is now in the ProbeVolumeDebugResources class.")]
		public Shader probeVolumeFragmentationDebugShader;

		[Obsolete("This shader is now in the ProbeVolumeDebugResources class.")]
		public Shader probeVolumeOffsetDebugShader;

		[Obsolete("This shader is now in the ProbeVolumeDebugResources class.")]
		public Shader probeVolumeSamplingDebugShader;

		[Obsolete("This shader is now in the ProbeVolumeDebugResources class.")]
		public Mesh probeSamplingDebugMesh;

		[Obsolete("This shader is now in the ProbeVolumeDebugResources class.")]
		public Texture2D probeSamplingDebugTexture;

		[Obsolete("This shader is now in the ProbeVolumeRuntimeResources class.")]
		public ComputeShader probeVolumeBlendStatesCS;
	}

	[Obsolete("Moved to UniversalRenderPipelineDebugShaders on GraphicsSettings. #from(2023.3)", false)]
	public DebugShaderResources debugShaders;

	[Obsolete("Probe volume debug resource are now in the ProbeVolumeDebugResources class.")]
	public ProbeVolumeResources probeVolumeResources;

	[SerializeField]
	internal List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

	[SerializeField]
	internal List<long> m_RendererFeatureMap = new List<long>(10);

	[SerializeField]
	private bool m_UseNativeRenderPass;

	[NonSerialized]
	private bool m_StripShadowsOffVariants;

	[NonSerialized]
	private bool m_StripAdditionalLightOffVariants;

	internal bool isInvalidated { get; set; }

	internal virtual bool stripShadowsOffVariants
	{
		get
		{
			return m_StripShadowsOffVariants;
		}
		set
		{
			m_StripShadowsOffVariants = value;
		}
	}

	internal virtual bool stripAdditionalLightOffVariants
	{
		get
		{
			return m_StripAdditionalLightOffVariants;
		}
		set
		{
			m_StripAdditionalLightOffVariants = value;
		}
	}

	public List<ScriptableRendererFeature> rendererFeatures => m_RendererFeatures;

	public bool useNativeRenderPass
	{
		get
		{
			return m_UseNativeRenderPass;
		}
		set
		{
			SetDirty();
			m_UseNativeRenderPass = value;
		}
	}

	protected abstract ScriptableRenderer Create();

	public new void SetDirty()
	{
		isInvalidated = true;
	}

	internal ScriptableRenderer InternalCreateRenderer()
	{
		isInvalidated = false;
		return Create();
	}

	protected virtual void OnValidate()
	{
		SetDirty();
	}

	protected virtual void OnEnable()
	{
		SetDirty();
	}

	public bool TryGetRendererFeature<T>(out T rendererFeature) where T : ScriptableRendererFeature
	{
		foreach (ScriptableRendererFeature rendererFeature2 in rendererFeatures)
		{
			if (rendererFeature2.GetType() == typeof(T))
			{
				rendererFeature = rendererFeature2 as T;
				return true;
			}
		}
		rendererFeature = null;
		return false;
	}
}
