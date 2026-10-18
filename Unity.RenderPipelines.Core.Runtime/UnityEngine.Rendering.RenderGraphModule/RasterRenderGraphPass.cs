using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("RenderPass: {name} (Index:{index} Async:{enableAsyncCompute})")]
internal sealed class RasterRenderGraphPass<PassData> : BaseRenderGraphPass<PassData, RasterGraphContext> where PassData : class, new()
{
	internal static RasterGraphContext c;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Execute(InternalRenderGraphContext renderGraphContext)
	{
		c.FromInternalContext(renderGraphContext);
		renderFunc(data, c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Release(RenderGraphObjectPool pool)
	{
		base.Release(pool);
		pool.Release(this);
	}
}
