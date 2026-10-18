using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

[DebuggerDisplay("PassFragmentData: Res({resource.index}):{accessFlags}")]
internal readonly struct PassFragmentData
{
	public readonly ResourceHandle resource;

	public readonly AccessFlags accessFlags;

	public readonly int mipLevel;

	public readonly int depthSlice;

	public PassFragmentData(ResourceHandle handle, AccessFlags flags, int mipLevel, int depthSlice)
	{
		resource = handle;
		accessFlags = flags;
		this.mipLevel = mipLevel;
		this.depthSlice = depthSlice;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return ((resource.GetHashCode() * 23 + accessFlags.GetHashCode()) * 23 + mipLevel.GetHashCode()) * 23 + depthSlice.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SameSubResource(in PassFragmentData x, in PassFragmentData y)
	{
		if (x.resource.index == y.resource.index && x.mipLevel == y.mipLevel)
		{
			return x.depthSlice == y.depthSlice;
		}
		return false;
	}
}
