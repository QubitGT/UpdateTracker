using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.RenderGraphModule;

[MovedFrom(true, "UnityEngine.Experimental.Rendering.RenderGraphModule", "UnityEngine.Rendering.RenderGraphModule", null)]
public interface IBaseRenderGraphBuilder : IDisposable
{
	void UseTexture(in TextureHandle input, AccessFlags flags = AccessFlags.Read);

	void UseGlobalTexture(int propertyId, AccessFlags flags = AccessFlags.Read);

	void UseAllGlobalTextures(bool enable);

	void SetGlobalTextureAfterPass(in TextureHandle input, int propertyId);

	BufferHandle UseBuffer(in BufferHandle input, AccessFlags flags = AccessFlags.Read);

	TextureHandle CreateTransientTexture(in TextureDesc desc);

	TextureHandle CreateTransientTexture(in TextureHandle texture);

	BufferHandle CreateTransientBuffer(in BufferDesc desc);

	BufferHandle CreateTransientBuffer(in BufferHandle computebuffer);

	void UseRendererList(in RendererListHandle input);

	void EnableAsyncCompute(bool value);

	void AllowPassCulling(bool value);

	void AllowGlobalStateModification(bool value);

	void EnableFoveatedRasterization(bool value);
}
