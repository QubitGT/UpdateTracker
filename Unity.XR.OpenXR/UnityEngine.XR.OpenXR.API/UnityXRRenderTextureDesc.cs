namespace UnityEngine.XR.OpenXR.API;

public struct UnityXRRenderTextureDesc
{
	public UnityXRRenderTextureFormat colorFormat;

	public UnityXRTextureData color;

	public UnityXRDepthTextureFormat depthFormat;

	public UnityXRTextureData depth;

	public UnityXRShadingRateFormat shadingRateFormat;

	public UnityXRTextureData shadingRate;

	public uint width;

	public uint height;

	public uint textureArrayLength;

	public uint flags;
}
