using System.Diagnostics;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

[DebuggerDisplay("PassRandomWriteData: Res({resource.index}):{index}:{preserveCounterValue}")]
internal readonly struct PassRandomWriteData
{
	public readonly ResourceHandle resource;

	public readonly int index;

	public readonly bool preserveCounterValue;

	public PassRandomWriteData(ResourceHandle resource, int index, bool preserveCounterValue)
	{
		this.resource = resource;
		this.index = index;
		this.preserveCounterValue = preserveCounterValue;
	}

	public override int GetHashCode()
	{
		return resource.GetHashCode() * 23 + index.GetHashCode();
	}
}
