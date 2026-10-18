namespace UnityEngine.Rendering.Universal;

[SupportedOnRenderer(typeof(UniversalRendererData))]
[DisallowMultipleRendererFeature("Screen Space Ambient Occlusion")]
[Tooltip("The Ambient Occlusion effect darkens creases, holes, intersections and surfaces that are close to each other.")]
public class ScreenSpaceAmbientOcclusion : ScriptableRendererFeature
{
	[SerializeField]
	private ScreenSpaceAmbientOcclusionSettings m_Settings = new ScreenSpaceAmbientOcclusionSettings();

	private Material m_Material;

	private ScreenSpaceAmbientOcclusionPass m_SSAOPass;

	private Shader m_Shader;

	private Texture2D[] m_BlueNoise256Textures;

	internal const string k_AOInterleavedGradientKeyword = "_INTERLEAVED_GRADIENT";

	internal const string k_AOBlueNoiseKeyword = "_BLUE_NOISE";

	internal const string k_OrthographicCameraKeyword = "_ORTHOGRAPHIC";

	internal const string k_SourceDepthLowKeyword = "_SOURCE_DEPTH_LOW";

	internal const string k_SourceDepthMediumKeyword = "_SOURCE_DEPTH_MEDIUM";

	internal const string k_SourceDepthHighKeyword = "_SOURCE_DEPTH_HIGH";

	internal const string k_SourceDepthNormalsKeyword = "_SOURCE_DEPTH_NORMALS";

	internal const string k_SampleCountLowKeyword = "_SAMPLE_COUNT_LOW";

	internal const string k_SampleCountMediumKeyword = "_SAMPLE_COUNT_MEDIUM";

	internal const string k_SampleCountHighKeyword = "_SAMPLE_COUNT_HIGH";

	internal ref ScreenSpaceAmbientOcclusionSettings settings => ref m_Settings;

	public override void Create()
	{
		if (m_SSAOPass == null)
		{
			m_SSAOPass = new ScreenSpaceAmbientOcclusionPass();
		}
		if (m_Settings.SampleCount > 0)
		{
			m_Settings.AOMethod = ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.InterleavedGradient;
			if (m_Settings.SampleCount > 11)
			{
				m_Settings.Samples = ScreenSpaceAmbientOcclusionSettings.AOSampleOption.High;
			}
			else if (m_Settings.SampleCount > 8)
			{
				m_Settings.Samples = ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Medium;
			}
			else
			{
				m_Settings.Samples = ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Low;
			}
			m_Settings.SampleCount = -1;
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData) && TryPrepareResources() && m_SSAOPass.Setup(ref m_Settings, ref renderer, ref m_Material, ref m_BlueNoise256Textures))
		{
			renderer.EnqueuePass(m_SSAOPass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		m_SSAOPass?.Dispose();
		m_SSAOPass = null;
		CoreUtils.Destroy(m_Material);
	}

	private bool TryPrepareResources()
	{
		if (m_Shader == null)
		{
			if (!GraphicsSettings.TryGetRenderPipelineSettings<ScreenSpaceAmbientOcclusionPersistentResources>(out var screenSpaceAmbientOcclusionPersistentResources))
			{
				Debug.LogErrorFormat("Couldn't find the required resources for the ScreenSpaceAmbientOcclusion render feature. If this exception appears in the Player, make sure at least one ScreenSpaceAmbientOcclusion render feature is enabled or adjust your stripping settings.");
				return false;
			}
			m_Shader = screenSpaceAmbientOcclusionPersistentResources.Shader;
		}
		if (m_Settings.AOMethod == ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.BlueNoise && (m_BlueNoise256Textures == null || m_BlueNoise256Textures.Length == 0))
		{
			if (!GraphicsSettings.TryGetRenderPipelineSettings<ScreenSpaceAmbientOcclusionDynamicResources>(out var screenSpaceAmbientOcclusionDynamicResources))
			{
				Debug.LogErrorFormat("Couldn't load BlueNoise256Textures. If this exception appears in the Player, please check the SSAO options for ScreenSpaceAmbientOcclusion or adjust your stripping settings");
				return false;
			}
			m_BlueNoise256Textures = screenSpaceAmbientOcclusionDynamicResources.BlueNoise256Textures;
		}
		if (m_Material == null && m_Shader != null)
		{
			m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
		}
		if (m_Material == null)
		{
			Debug.LogError(GetType().Name + ".AddRenderPasses(): Missing material. " + base.name + " render pass will not be added.");
			return false;
		}
		return true;
	}
}
