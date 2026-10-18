namespace UnityEngine.XR.OpenXR.API;

public enum UnityXRRenderTextureFlags
{
	kUnityXRRenderTextureFlagsUVDirectionTopToBottom = 1,
	kUnityXRRenderTextureFlagsMultisampleAutoResolve = 2,
	kUnityXRRenderTextureFlagsLockedWidthHeight = 4,
	kUnityXRRenderTextureFlagsWriteOnly = 8,
	kUnityXRRenderTextureFlagsSRGB = 0x10,
	kUnityXRRenderTextureFlagsOptimizeBufferDiscards = 0x20,
	kUnityXRRenderTextureFlagsMotionVectorTexture = 0x40,
	kUnityXRRenderTextureFlagsFoveationOffset = 0x80,
	kUnityXRRenderTextureFlagsViewportAsRenderArea = 0x100,
	kUnityXRRenderTextureFlagsHDR = 0x200
}
