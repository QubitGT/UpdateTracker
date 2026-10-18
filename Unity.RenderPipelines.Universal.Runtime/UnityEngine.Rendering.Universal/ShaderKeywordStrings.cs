namespace UnityEngine.Rendering.Universal;

public static class ShaderKeywordStrings
{
	public const string MainLightShadows = "_MAIN_LIGHT_SHADOWS";

	public const string MainLightShadowCascades = "_MAIN_LIGHT_SHADOWS_CASCADE";

	public const string MainLightShadowScreen = "_MAIN_LIGHT_SHADOWS_SCREEN";

	public const string CastingPunctualLightShadow = "_CASTING_PUNCTUAL_LIGHT_SHADOW";

	public const string AdditionalLightsVertex = "_ADDITIONAL_LIGHTS_VERTEX";

	public const string AdditionalLightsPixel = "_ADDITIONAL_LIGHTS";

	internal const string ClusterLightLoop = "_CLUSTER_LIGHT_LOOP";

	public const string AdditionalLightShadows = "_ADDITIONAL_LIGHT_SHADOWS";

	public const string ReflectionProbeBoxProjection = "_REFLECTION_PROBE_BOX_PROJECTION";

	public const string ReflectionProbeBlending = "_REFLECTION_PROBE_BLENDING";

	public const string ReflectionProbeAtlas = "_REFLECTION_PROBE_ATLAS";

	public const string SoftShadows = "_SHADOWS_SOFT";

	public const string SoftShadowsLow = "_SHADOWS_SOFT_LOW";

	public const string SoftShadowsMedium = "_SHADOWS_SOFT_MEDIUM";

	public const string SoftShadowsHigh = "_SHADOWS_SOFT_HIGH";

	public const string MixedLightingSubtractive = "_MIXED_LIGHTING_SUBTRACTIVE";

	public const string LightmapShadowMixing = "LIGHTMAP_SHADOW_MIXING";

	public const string ShadowsShadowMask = "SHADOWS_SHADOWMASK";

	public const string LightLayers = "_LIGHT_LAYERS";

	public const string RenderPassEnabled = "_RENDER_PASS_ENABLED";

	public const string BillboardFaceCameraPos = "BILLBOARD_FACE_CAMERA_POS";

	public const string LightCookies = "_LIGHT_COOKIES";

	public const string DepthNoMsaa = "_DEPTH_NO_MSAA";

	public const string DepthMsaa2 = "_DEPTH_MSAA_2";

	public const string DepthMsaa4 = "_DEPTH_MSAA_4";

	public const string DepthMsaa8 = "_DEPTH_MSAA_8";

	public const string LinearToSRGBConversion = "_LINEAR_TO_SRGB_CONVERSION";

	internal const string UseFastSRGBLinearConversion = "_USE_FAST_SRGB_LINEAR_CONVERSION";

	public const string DBufferMRT1 = "_DBUFFER_MRT1";

	public const string DBufferMRT2 = "_DBUFFER_MRT2";

	public const string DBufferMRT3 = "_DBUFFER_MRT3";

	public const string DecalNormalBlendLow = "_DECAL_NORMAL_BLEND_LOW";

	public const string DecalNormalBlendMedium = "_DECAL_NORMAL_BLEND_MEDIUM";

	public const string DecalNormalBlendHigh = "_DECAL_NORMAL_BLEND_HIGH";

	public const string DecalLayers = "_DECAL_LAYERS";

	public const string WriteRenderingLayers = "_WRITE_RENDERING_LAYERS";

	public const string SmaaLow = "_SMAA_PRESET_LOW";

	public const string SmaaMedium = "_SMAA_PRESET_MEDIUM";

	public const string SmaaHigh = "_SMAA_PRESET_HIGH";

	public const string PaniniGeneric = "_GENERIC";

	public const string PaniniUnitDistance = "_UNIT_DISTANCE";

	public const string BloomLQ = "_BLOOM_LQ";

	public const string BloomHQ = "_BLOOM_HQ";

	public const string BloomLQDirt = "_BLOOM_LQ_DIRT";

	public const string BloomHQDirt = "_BLOOM_HQ_DIRT";

	public const string Distortion = "_DISTORTION";

	public const string ChromaticAberration = "_CHROMATIC_ABERRATION";

	public const string HDRGrading = "_HDR_GRADING";

	public const string HDROverlay = "_HDR_OVERLAY";

	public const string TonemapACES = "_TONEMAP_ACES";

	public const string TonemapNeutral = "_TONEMAP_NEUTRAL";

	public const string FilmGrain = "_FILM_GRAIN";

	public const string Fxaa = "_FXAA";

	public const string Dithering = "_DITHERING";

	public const string ScreenSpaceOcclusion = "_SCREEN_SPACE_OCCLUSION";

	public const string PointSampling = "_POINT_SAMPLING";

	public const string Rcas = "_RCAS";

	public const string EasuRcasAndHDRInput = "_EASU_RCAS_AND_HDR_INPUT";

	public const string Gamma20 = "_GAMMA_20";

	public const string Gamma20AndHDRInput = "_GAMMA_20_AND_HDR_INPUT";

	public const string HighQualitySampling = "_HIGH_QUALITY_SAMPLING";

	public const string _SPOT = "_SPOT";

	public const string _DIRECTIONAL = "_DIRECTIONAL";

	public const string _POINT = "_POINT";

	public const string _DEFERRED_STENCIL = "_DEFERRED_STENCIL";

	public const string _DEFERRED_FIRST_LIGHT = "_DEFERRED_FIRST_LIGHT";

	public const string _DEFERRED_MAIN_LIGHT = "_DEFERRED_MAIN_LIGHT";

	public const string _GBUFFER_NORMALS_OCT = "_GBUFFER_NORMALS_OCT";

	public const string _DEFERRED_MIXED_LIGHTING = "_DEFERRED_MIXED_LIGHTING";

	public const string LIGHTMAP_ON = "LIGHTMAP_ON";

	public const string DYNAMICLIGHTMAP_ON = "DYNAMICLIGHTMAP_ON";

	public const string _ALPHATEST_ON = "_ALPHATEST_ON";

	public const string DIRLIGHTMAP_COMBINED = "DIRLIGHTMAP_COMBINED";

	public const string _DETAIL_MULX2 = "_DETAIL_MULX2";

	public const string _DETAIL_SCALED = "_DETAIL_SCALED";

	public const string _CLEARCOAT = "_CLEARCOAT";

	public const string _CLEARCOATMAP = "_CLEARCOATMAP";

	public const string DEBUG_DISPLAY = "DEBUG_DISPLAY";

	public const string LOD_FADE_CROSSFADE = "LOD_FADE_CROSSFADE";

	public const string USE_UNITY_CROSSFADE = "USE_UNITY_CROSSFADE";

	public const string _EMISSION = "_EMISSION";

	public const string _RECEIVE_SHADOWS_OFF = "_RECEIVE_SHADOWS_OFF";

	public const string _SURFACE_TYPE_TRANSPARENT = "_SURFACE_TYPE_TRANSPARENT";

	public const string _ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";

	public const string _ALPHAMODULATE_ON = "_ALPHAMODULATE_ON";

	public const string _NORMALMAP = "_NORMALMAP";

	public const string _ADD_PRECOMPUTED_VELOCITY = "_ADD_PRECOMPUTED_VELOCITY";

	public const string EDITOR_VISUALIZATION = "EDITOR_VISUALIZATION";

	public const string FoveatedRenderingNonUniformRaster = "_FOVEATED_RENDERING_NON_UNIFORM_RASTER";

	public const string DisableTexture2DXArray = "DISABLE_TEXTURE2D_X_ARRAY";

	public const string BlitSingleSlice = "BLIT_SINGLE_SLICE";

	public const string XROcclusionMeshCombined = "XR_OCCLUSION_MESH_COMBINED";

	public const string SCREEN_COORD_OVERRIDE = "SCREEN_COORD_OVERRIDE";

	public const string DOWNSAMPLING_SIZE_2 = "DOWNSAMPLING_SIZE_2";

	public const string DOWNSAMPLING_SIZE_4 = "DOWNSAMPLING_SIZE_4";

	public const string DOWNSAMPLING_SIZE_8 = "DOWNSAMPLING_SIZE_8";

	public const string DOWNSAMPLING_SIZE_16 = "DOWNSAMPLING_SIZE_16";

	public const string EVALUATE_SH_MIXED = "EVALUATE_SH_MIXED";

	public const string EVALUATE_SH_VERTEX = "EVALUATE_SH_VERTEX";

	public const string ProbeVolumeL1 = "PROBE_VOLUMES_L1";

	public const string ProbeVolumeL2 = "PROBE_VOLUMES_L2";

	public const string LIGHTMAP_BICUBIC_SAMPLING = "LIGHTMAP_BICUBIC_SAMPLING";

	public const string USE_LEGACY_LIGHTMAPS = "USE_LEGACY_LIGHTMAPS";

	public const string _OUTPUT_DEPTH = "_OUTPUT_DEPTH";

	public const string _ENABLE_ALPHA_OUTPUT = "_ENABLE_ALPHA_OUTPUT";

	internal const string ForwardPlus = "_FORWARD_PLUS";
}
