using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("RenderPass: {name} (Index:{index} Async:{enableAsyncCompute})")]
internal abstract class BaseRenderGraphPass<PassData, TRenderGraphContext> : RenderGraphPass where PassData : class, new()
{
	internal PassData data;

	internal BaseRenderFunc<PassData, TRenderGraphContext> renderFunc;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Initialize(int passIndex, PassData passData, string passName, RenderGraphPassType passType, ProfilingSampler sampler)
	{
		Clear();
		base.index = passIndex;
		data = passData;
		base.name = passName;
		base.type = passType;
		base.customSampler = sampler;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Release(RenderGraphObjectPool pool)
	{
		pool.Release(data);
		data = null;
		renderFunc = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool HasRenderFunc()
	{
		return renderFunc != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetRenderFuncHash()
	{
		if (renderFunc == null)
		{
			return 0;
		}
		return DelegateHashCodeUtils.GetFuncHashCode(renderFunc);
	}
}
