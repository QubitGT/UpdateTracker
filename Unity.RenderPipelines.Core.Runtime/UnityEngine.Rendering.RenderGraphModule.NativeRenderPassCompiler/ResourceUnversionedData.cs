using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal struct ResourceUnversionedData
{
	public readonly bool isImported;

	public bool isShared;

	public int tag;

	public int lastUsePassID;

	public int lastWritePassID;

	public int firstUsePassID;

	public bool memoryLess;

	public readonly int width;

	public readonly int height;

	public readonly int volumeDepth;

	public readonly int msaaSamples;

	public int latestVersionNumber;

	public readonly bool clear;

	public readonly bool discard;

	public readonly bool bindMS;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string GetName(CompilerContextData ctx, ResourceHandle h)
	{
		return ctx.GetResourceName(h);
	}

	public ResourceUnversionedData(IRenderGraphResource rll, ref RenderTargetInfo info, ref TextureDesc desc, bool isResourceShared)
	{
		isImported = rll.imported;
		isShared = isResourceShared;
		tag = 0;
		firstUsePassID = -1;
		lastUsePassID = -1;
		lastWritePassID = -1;
		memoryLess = false;
		width = info.width;
		height = info.height;
		volumeDepth = info.volumeDepth;
		msaaSamples = info.msaaSamples;
		latestVersionNumber = rll.version;
		clear = desc.clearBuffer;
		discard = desc.discardBuffer;
		bindMS = info.bindMS;
	}

	public ResourceUnversionedData(IRenderGraphResource rll, ref BufferDesc _, bool isResourceShared)
	{
		isImported = rll.imported;
		isShared = isResourceShared;
		tag = 0;
		firstUsePassID = -1;
		lastUsePassID = -1;
		lastWritePassID = -1;
		memoryLess = false;
		width = -1;
		height = -1;
		volumeDepth = -1;
		msaaSamples = -1;
		latestVersionNumber = rll.version;
		clear = false;
		discard = false;
		bindMS = false;
	}

	public ResourceUnversionedData(IRenderGraphResource rll, ref RayTracingAccelerationStructureDesc _, bool isResourceShared)
	{
		isImported = rll.imported;
		isShared = isResourceShared;
		tag = 0;
		firstUsePassID = -1;
		lastUsePassID = -1;
		lastWritePassID = -1;
		memoryLess = false;
		width = -1;
		height = -1;
		volumeDepth = -1;
		msaaSamples = -1;
		latestVersionNumber = rll.version;
		clear = false;
		discard = false;
		bindMS = false;
	}

	public void InitializeNullResource()
	{
		firstUsePassID = -1;
		lastUsePassID = -1;
		lastWritePassID = -1;
	}
}
