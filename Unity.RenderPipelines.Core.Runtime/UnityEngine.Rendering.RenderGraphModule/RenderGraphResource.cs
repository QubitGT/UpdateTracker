using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule;

[DebuggerDisplay("Resource ({GetType().Name}:{GetName()})")]
internal abstract class RenderGraphResource<DescType, ResType> : IRenderGraphResource where DescType : struct where ResType : class
{
	public DescType desc;

	public bool validDesc;

	public ResType graphicsResource;

	protected RenderGraphResourcePool<ResType> m_Pool;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void Reset(IRenderGraphResourcePool pool = null)
	{
		base.Reset();
		m_Pool = pool as RenderGraphResourcePool<ResType>;
		graphicsResource = null;
		validDesc = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool IsCreated()
	{
		return graphicsResource != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void ReleaseGraphicsResource()
	{
		graphicsResource = null;
	}

	public override void CreatePooledGraphicsResource()
	{
		int descHashCode = GetDescHashCode();
		if (graphicsResource != null)
		{
			throw new InvalidOperationException("RenderGraphResource: Trying to create an already created resource (" + GetName() + "). Resource was probably declared for writing more than once in the same pass.");
		}
		if (!m_Pool.TryGetResource(descHashCode, out graphicsResource))
		{
			CreateGraphicsResource();
		}
		else
		{
			UpdateGraphicsResource();
		}
		cachedHash = descHashCode;
	}

	public override void ReleasePooledGraphicsResource(int frameIndex)
	{
		if (graphicsResource == null)
		{
			throw new InvalidOperationException("RenderGraphResource: Tried to release a resource (" + GetName() + ") that was never created. Check that there is at least one pass writing to it first.");
		}
		if (m_Pool != null)
		{
			m_Pool.ReleaseResource(cachedHash, graphicsResource, frameIndex);
		}
		Reset();
	}
}
