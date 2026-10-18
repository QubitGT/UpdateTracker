using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[ReloadGroup]
[ExcludeFromPreset]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.Universal", "Unity.RenderPipelines.Universal.Runtime", null)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/2DRendererData-overview.html")]
public class Renderer2DData : ScriptableRendererData
{
	internal enum Renderer2DDefaultMaterialType
	{
		Lit,
		Unlit,
		Custom
	}

	[SerializeField]
	private LayerMask m_LayerMask = -1;

	[SerializeField]
	private TransparencySortMode m_TransparencySortMode;

	[SerializeField]
	private Vector3 m_TransparencySortAxis = Vector3.up;

	[SerializeField]
	private float m_HDREmulationScale = 1f;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float m_LightRenderTextureScale = 0.5f;

	[SerializeField]
	[FormerlySerializedAs("m_LightOperations")]
	private Light2DBlendStyle[] m_LightBlendStyles;

	[SerializeField]
	private bool m_UseDepthStencilBuffer = true;

	[SerializeField]
	private bool m_UseCameraSortingLayersTexture;

	[SerializeField]
	private int m_CameraSortingLayersTextureBound;

	[SerializeField]
	private Downsampling m_CameraSortingLayerDownsamplingMethod;

	[SerializeField]
	private uint m_MaxLightRenderTextureCount = 16u;

	[SerializeField]
	private uint m_MaxShadowRenderTextureCount = 1u;

	[SerializeField]
	private PostProcessData m_PostProcessData;

	internal RTHandle normalsRenderTarget;

	internal RTHandle cameraSortingLayerRenderTarget;

	public float hdrEmulationScale => m_HDREmulationScale;

	internal float lightRenderTextureScale => m_LightRenderTextureScale;

	public Light2DBlendStyle[] lightBlendStyles => m_LightBlendStyles;

	internal bool useDepthStencilBuffer => m_UseDepthStencilBuffer;

	internal PostProcessData postProcessData
	{
		get
		{
			return m_PostProcessData;
		}
		set
		{
			m_PostProcessData = value;
		}
	}

	internal TransparencySortMode transparencySortMode => m_TransparencySortMode;

	internal Vector3 transparencySortAxis => m_TransparencySortAxis;

	internal uint lightRenderTextureMemoryBudget => m_MaxLightRenderTextureCount;

	internal uint shadowRenderTextureMemoryBudget => m_MaxShadowRenderTextureCount;

	internal bool useCameraSortingLayerTexture => m_UseCameraSortingLayersTexture;

	internal int cameraSortingLayerTextureBound => m_CameraSortingLayersTextureBound;

	internal Downsampling cameraSortingLayerDownsamplingMethod => m_CameraSortingLayerDownsamplingMethod;

	internal LayerMask layerMask => m_LayerMask;

	internal Dictionary<uint, Material> lightMaterials { get; } = new Dictionary<uint, Material>();


	internal Material spriteSelfShadowMaterial { get; set; }

	internal Material spriteUnshadowMaterial { get; set; }

	internal Material geometrySelfShadowMaterial { get; set; }

	internal Material geometryUnshadowMaterial { get; set; }

	internal Material projectedShadowMaterial { get; set; }

	internal Material projectedUnshadowMaterial { get; set; }

	internal ILight2DCullResult lightCullResult { get; set; }

	protected override ScriptableRenderer Create()
	{
		return new Renderer2D(this);
	}

	internal void Dispose()
	{
		for (int i = 0; i < m_LightBlendStyles.Length; i++)
		{
			m_LightBlendStyles[i].renderTargetHandle?.Release();
		}
		foreach (KeyValuePair<uint, Material> lightMaterial in lightMaterials)
		{
			CoreUtils.Destroy(lightMaterial.Value);
		}
		lightMaterials.Clear();
		CoreUtils.Destroy(spriteSelfShadowMaterial);
		CoreUtils.Destroy(spriteUnshadowMaterial);
		CoreUtils.Destroy(geometrySelfShadowMaterial);
		CoreUtils.Destroy(geometryUnshadowMaterial);
		CoreUtils.Destroy(projectedShadowMaterial);
		CoreUtils.Destroy(projectedUnshadowMaterial);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < m_LightBlendStyles.Length; i++)
		{
			m_LightBlendStyles[i].renderTargetHandleId = Shader.PropertyToID($"_ShapeLightTexture{i}");
			m_LightBlendStyles[i].renderTargetHandle = RTHandles.Alloc(m_LightBlendStyles[i].renderTargetHandleId, $"_ShapeLightTexture{i}");
		}
		geometrySelfShadowMaterial = null;
		geometryUnshadowMaterial = null;
		spriteSelfShadowMaterial = null;
		spriteUnshadowMaterial = null;
		projectedShadowMaterial = null;
		projectedUnshadowMaterial = null;
	}
}
