using System;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace UnityEngine.Experimental.Rendering;

public struct XRPassCreateInfo
{
	internal RenderTargetIdentifier renderTarget;

	internal RenderTextureDescriptor renderTargetDesc;

	internal RenderTargetIdentifier motionVectorRenderTarget;

	internal RenderTextureDescriptor motionVectorRenderTargetDesc;

	internal ScriptableCullingParameters cullingParameters;

	internal Material occlusionMeshMaterial;

	internal float occlusionMeshScale;

	internal int renderTargetScaledWidth;

	internal int renderTargetScaledHeight;

	internal IntPtr foveatedRenderingInfo;

	internal int multipassId;

	internal int cullingPassId;

	internal bool copyDepth;

	internal bool hasMotionVectorPass;

	internal bool spaceWarpRightHandedNDC;

	internal XRDisplaySubsystem.XRRenderPass xrSdkRenderPass;
}
