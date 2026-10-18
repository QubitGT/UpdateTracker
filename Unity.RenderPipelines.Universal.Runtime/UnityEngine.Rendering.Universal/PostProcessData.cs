using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
public class PostProcessData : ScriptableObject
{
	[Serializable]
	[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
	[CategoryInfo(Name = "R: Default PostProcess Shaders", Order = 1000)]
	[ElementInfo(Order = 0)]
	[HideInInspector]
	public sealed class ShaderResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
	{
		[ResourcePath("Shaders/PostProcessing/StopNaN.shader", SearchType.ProjectPath)]
		public Shader stopNanPS;

		[ResourcePath("Shaders/PostProcessing/SubpixelMorphologicalAntialiasing.shader", SearchType.ProjectPath)]
		public Shader subpixelMorphologicalAntialiasingPS;

		[ResourcePath("Shaders/PostProcessing/GaussianDepthOfField.shader", SearchType.ProjectPath)]
		public Shader gaussianDepthOfFieldPS;

		[ResourcePath("Shaders/PostProcessing/BokehDepthOfField.shader", SearchType.ProjectPath)]
		public Shader bokehDepthOfFieldPS;

		[ResourcePath("Shaders/PostProcessing/CameraMotionBlur.shader", SearchType.ProjectPath)]
		public Shader cameraMotionBlurPS;

		[ResourcePath("Shaders/PostProcessing/PaniniProjection.shader", SearchType.ProjectPath)]
		public Shader paniniProjectionPS;

		[ResourcePath("Shaders/PostProcessing/LutBuilderLdr.shader", SearchType.ProjectPath)]
		public Shader lutBuilderLdrPS;

		[ResourcePath("Shaders/PostProcessing/LutBuilderHdr.shader", SearchType.ProjectPath)]
		public Shader lutBuilderHdrPS;

		[ResourcePath("Shaders/PostProcessing/Bloom.shader", SearchType.ProjectPath)]
		public Shader bloomPS;

		[ResourcePath("Shaders/PostProcessing/TemporalAA.shader", SearchType.ProjectPath)]
		public Shader temporalAntialiasingPS;

		[ResourcePath("Shaders/PostProcessing/LensFlareDataDriven.shader", SearchType.ProjectPath)]
		public Shader LensFlareDataDrivenPS;

		[ResourcePath("Shaders/PostProcessing/LensFlareScreenSpace.shader", SearchType.ProjectPath)]
		public Shader LensFlareScreenSpacePS;

		[ResourcePath("Shaders/PostProcessing/ScalingSetup.shader", SearchType.ProjectPath)]
		public Shader scalingSetupPS;

		[ResourcePath("Shaders/PostProcessing/EdgeAdaptiveSpatialUpsampling.shader", SearchType.ProjectPath)]
		public Shader easuPS;

		[ResourcePath("Shaders/PostProcessing/UberPost.shader", SearchType.ProjectPath)]
		public Shader uberPostPS;

		[ResourcePath("Shaders/PostProcessing/FinalPost.shader", SearchType.ProjectPath)]
		public Shader finalPostPassPS;

		[SerializeField]
		[HideInInspector]
		private int m_ShaderResourcesVersion;

		public int version => m_ShaderResourcesVersion;

		public bool isAvailableInPlayerBuild => false;
	}

	[Serializable]
	[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
	[CategoryInfo(Name = "R: Default PostProcess Textures", Order = 1000)]
	[ElementInfo(Order = 0)]
	[HideInInspector]
	public sealed class TextureResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
	{
		[ResourceFormattedPaths("Textures/BlueNoise16/L/LDR_LLL1_{0}.png", 0, 32, SearchType.ProjectPath)]
		public Texture2D[] blueNoise16LTex;

		[ResourcePaths(new string[] { "Textures/FilmGrain/Thin01.png", "Textures/FilmGrain/Thin02.png", "Textures/FilmGrain/Medium01.png", "Textures/FilmGrain/Medium02.png", "Textures/FilmGrain/Medium03.png", "Textures/FilmGrain/Medium04.png", "Textures/FilmGrain/Medium05.png", "Textures/FilmGrain/Medium06.png", "Textures/FilmGrain/Large01.png", "Textures/FilmGrain/Large02.png" }, SearchType.ProjectPath)]
		public Texture2D[] filmGrainTex;

		[ResourcePath("Textures/SMAA/AreaTex.tga", SearchType.ProjectPath)]
		public Texture2D smaaAreaTex;

		[ResourcePath("Textures/SMAA/SearchTex.tga", SearchType.ProjectPath)]
		public Texture2D smaaSearchTex;

		[SerializeField]
		[HideInInspector]
		private int m_TexturesResourcesVersion;

		public int version => m_TexturesResourcesVersion;

		public bool isAvailableInPlayerBuild => false;
	}

	public ShaderResources shaders;

	public TextureResources textures;
}
