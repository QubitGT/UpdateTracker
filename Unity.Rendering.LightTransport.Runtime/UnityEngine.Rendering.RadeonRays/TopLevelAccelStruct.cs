using System;

namespace UnityEngine.Rendering.RadeonRays;

internal struct TopLevelAccelStruct : IDisposable
{
	public const GraphicsBuffer.Target topLevelBvhTarget = GraphicsBuffer.Target.Structured;

	public const GraphicsBuffer.Target instanceInfoTarget = GraphicsBuffer.Target.Structured;

	public GraphicsBuffer topLevelBvh;

	public GraphicsBuffer bottomLevelBvhs;

	public GraphicsBuffer instanceInfos;

	public uint instanceCount;

	public void Dispose()
	{
		topLevelBvh?.Dispose();
		instanceInfos?.Dispose();
	}
}
