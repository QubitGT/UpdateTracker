using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public interface IRasterRenderGraphBuilder : IBaseRenderGraphBuilder, IDisposable
{
	void SetRenderAttachment(TextureHandle tex, int index, AccessFlags flags = AccessFlags.Write)
	{
		SetRenderAttachment(tex, index, flags, 0, -1);
	}

	void SetRenderAttachment(TextureHandle tex, int index, AccessFlags flags, int mipLevel, int depthSlice);

	void SetInputAttachment(TextureHandle tex, int index, AccessFlags flags = AccessFlags.Read)
	{
		SetInputAttachment(tex, index, flags, 0, -1);
	}

	void SetInputAttachment(TextureHandle tex, int index, AccessFlags flags, int mipLevel, int depthSlice);

	void SetRenderAttachmentDepth(TextureHandle tex, AccessFlags flags = AccessFlags.Write)
	{
		SetRenderAttachmentDepth(tex, flags, 0, -1);
	}

	void SetRenderAttachmentDepth(TextureHandle tex, AccessFlags flags, int mipLevel, int depthSlice);

	TextureHandle SetRandomAccessAttachment(TextureHandle tex, int index, AccessFlags flags = AccessFlags.ReadWrite);

	BufferHandle UseBufferRandomAccess(BufferHandle tex, int index, AccessFlags flags = AccessFlags.Read);

	BufferHandle UseBufferRandomAccess(BufferHandle tex, int index, bool preserveCounterValue, AccessFlags flags = AccessFlags.Read);

	void SetShadingRateImageAttachment(in TextureHandle tex);

	void SetShadingRateFragmentSize(ShadingRateFragmentSize shadingRateFragmentSize);

	void SetShadingRateCombiner(ShadingRateCombinerStage stage, ShadingRateCombiner combiner);

	void SetRenderFunc<PassData>(BaseRenderFunc<PassData, RasterGraphContext> renderFunc) where PassData : class, new();
}
