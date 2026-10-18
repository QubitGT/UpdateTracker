namespace UnityEngine.Rendering.Universal;

internal static class ShaderPropertyId
{
	public static readonly int glossyEnvironmentColor = Shader.PropertyToID("_GlossyEnvironmentColor");

	public static readonly int subtractiveShadowColor = Shader.PropertyToID("_SubtractiveShadowColor");

	public static readonly int glossyEnvironmentCubeMap = Shader.PropertyToID("_GlossyEnvironmentCubeMap");

	public static readonly int glossyEnvironmentCubeMapHDR = Shader.PropertyToID("_GlossyEnvironmentCubeMap_HDR");

	public static readonly int ambientSkyColor = Shader.PropertyToID("unity_AmbientSky");

	public static readonly int ambientEquatorColor = Shader.PropertyToID("unity_AmbientEquator");

	public static readonly int ambientGroundColor = Shader.PropertyToID("unity_AmbientGround");

	public static readonly int time = Shader.PropertyToID("_Time");

	public static readonly int sinTime = Shader.PropertyToID("_SinTime");

	public static readonly int cosTime = Shader.PropertyToID("_CosTime");

	public static readonly int deltaTime = Shader.PropertyToID("unity_DeltaTime");

	public static readonly int timeParameters = Shader.PropertyToID("_TimeParameters");

	public static readonly int lastTimeParameters = Shader.PropertyToID("_LastTimeParameters");

	public static readonly int scaledScreenParams = Shader.PropertyToID("_ScaledScreenParams");

	public static readonly int worldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");

	public static readonly int screenParams = Shader.PropertyToID("_ScreenParams");

	public static readonly int alphaToMaskAvailable = Shader.PropertyToID("_AlphaToMaskAvailable");

	public static readonly int projectionParams = Shader.PropertyToID("_ProjectionParams");

	public static readonly int zBufferParams = Shader.PropertyToID("_ZBufferParams");

	public static readonly int orthoParams = Shader.PropertyToID("unity_OrthoParams");

	public static readonly int globalMipBias = Shader.PropertyToID("_GlobalMipBias");

	public static readonly int screenSize = Shader.PropertyToID("_ScreenSize");

	public static readonly int screenCoordScaleBias = Shader.PropertyToID("_ScreenCoordScaleBias");

	public static readonly int screenSizeOverride = Shader.PropertyToID("_ScreenSizeOverride");

	public static readonly int viewMatrix = Shader.PropertyToID("unity_MatrixV");

	public static readonly int projectionMatrix = Shader.PropertyToID("glstate_matrix_projection");

	public static readonly int viewAndProjectionMatrix = Shader.PropertyToID("unity_MatrixVP");

	public static readonly int inverseViewMatrix = Shader.PropertyToID("unity_MatrixInvV");

	public static readonly int inverseProjectionMatrix = Shader.PropertyToID("unity_MatrixInvP");

	public static readonly int inverseViewAndProjectionMatrix = Shader.PropertyToID("unity_MatrixInvVP");

	public static readonly int cameraProjectionMatrix = Shader.PropertyToID("unity_CameraProjection");

	public static readonly int inverseCameraProjectionMatrix = Shader.PropertyToID("unity_CameraInvProjection");

	public static readonly int worldToCameraMatrix = Shader.PropertyToID("unity_WorldToCamera");

	public static readonly int cameraToWorldMatrix = Shader.PropertyToID("unity_CameraToWorld");

	public static readonly int shadowBias = Shader.PropertyToID("_ShadowBias");

	public static readonly int lightDirection = Shader.PropertyToID("_LightDirection");

	public static readonly int lightPosition = Shader.PropertyToID("_LightPosition");

	public static readonly int cameraWorldClipPlanes = Shader.PropertyToID("unity_CameraWorldClipPlanes");

	public static readonly int billboardNormal = Shader.PropertyToID("unity_BillboardNormal");

	public static readonly int billboardTangent = Shader.PropertyToID("unity_BillboardTangent");

	public static readonly int billboardCameraParams = Shader.PropertyToID("unity_BillboardCameraParams");

	public static readonly int previousViewProjectionNoJitter = Shader.PropertyToID("_PrevViewProjMatrix");

	public static readonly int viewProjectionNoJitter = Shader.PropertyToID("_NonJitteredViewProjMatrix");

	public static readonly int previousViewProjectionNoJitterStereo = Shader.PropertyToID("_PrevViewProjMatrixStereo");

	public static readonly int viewProjectionNoJitterStereo = Shader.PropertyToID("_NonJitteredViewProjMatrixStereo");

	public static readonly int blitTexture = Shader.PropertyToID("_BlitTexture");

	public static readonly int blitScaleBias = Shader.PropertyToID("_BlitScaleBias");

	public static readonly int sourceTex = Shader.PropertyToID("_SourceTex");

	public static readonly int scaleBias = Shader.PropertyToID("_ScaleBias");

	public static readonly int scaleBiasRt = Shader.PropertyToID("_ScaleBiasRt");

	public static readonly int rtHandleScale = Shader.PropertyToID("_RTHandleScale");

	public static readonly int rendererColor = Shader.PropertyToID("_RendererColor");

	public static readonly int ditheringTexture = Shader.PropertyToID("_DitheringTexture");

	public static readonly int ditheringTextureInvSize = Shader.PropertyToID("_DitheringTextureInvSize");

	public static readonly int renderingLayerMaxInt = Shader.PropertyToID("_RenderingLayerMaxInt");

	public static readonly int overlayUITexture = Shader.PropertyToID("_OverlayUITexture");

	public static readonly int hdrOutputLuminanceParams = Shader.PropertyToID("_HDROutputLuminanceParams");

	public static readonly int hdrOutputGradingParams = Shader.PropertyToID("_HDROutputGradingParams");
}
