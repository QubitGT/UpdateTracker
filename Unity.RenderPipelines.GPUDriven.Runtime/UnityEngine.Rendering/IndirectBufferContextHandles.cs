using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering;

internal struct IndirectBufferContextHandles
{
	public BufferHandle instanceBuffer;

	public BufferHandle instanceInfoBuffer;

	public BufferHandle argsBuffer;

	public BufferHandle drawInfoBuffer;

	public void UseForOcclusionTest(IBaseRenderGraphBuilder builder)
	{
		instanceBuffer = builder.UseBuffer(in instanceBuffer, AccessFlags.ReadWrite);
		instanceInfoBuffer = builder.UseBuffer(in instanceInfoBuffer);
		argsBuffer = builder.UseBuffer(in argsBuffer, AccessFlags.ReadWrite);
		drawInfoBuffer = builder.UseBuffer(in drawInfoBuffer);
	}
}
