namespace UnityEngine.Rendering;

public interface IUnsafeCommandBuffer : IBaseCommandBuffer, IRasterCommandBuffer, IComputeCommandBuffer
{
	void Clear();

	void SetRenderTarget(RenderTargetIdentifier rt);

	void SetRenderTarget(RenderTargetIdentifier rt, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction);

	void SetRenderTarget(RenderTargetIdentifier rt, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction);

	void SetRenderTarget(RenderTargetIdentifier rt, int mipLevel);

	void SetRenderTarget(RenderTargetIdentifier rt, int mipLevel, CubemapFace cubemapFace);

	void SetRenderTarget(RenderTargetIdentifier rt, int mipLevel, CubemapFace cubemapFace, int depthSlice);

	void SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth);

	void SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth, int mipLevel);

	void SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth, int mipLevel, CubemapFace cubemapFace);

	void SetRenderTarget(RenderTargetIdentifier color, RenderTargetIdentifier depth, int mipLevel, CubemapFace cubemapFace, int depthSlice);

	void SetRenderTarget(RenderTargetIdentifier color, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderTargetIdentifier depth, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction);

	void SetRenderTarget(RenderTargetIdentifier[] colors, RenderTargetIdentifier depth);

	void SetRenderTarget(RenderTargetIdentifier[] colors, RenderTargetIdentifier depth, int mipLevel, CubemapFace cubemapFace, int depthSlice);

	void SetRenderTarget(RenderTargetBinding binding, int mipLevel, CubemapFace cubemapFace, int depthSlice);

	void SetRenderTarget(RenderTargetBinding binding);
}
