using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal struct ResourceVersionedData
{
	public bool written;

	public int writePassId;

	public int numReaders;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetWritingPass(CompilerContextData ctx, ResourceHandle h, int passId)
	{
		writePassId = passId;
		written = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RegisterReadingPass(CompilerContextData ctx, ResourceHandle h, int passId, int index)
	{
		ctx.resources.readerData[h.iType][ctx.resources.IndexReader(h, numReaders)] = new ResourceReaderData
		{
			passId = passId,
			inputSlot = index
		};
		numReaders++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RemoveReadingPass(CompilerContextData ctx, ResourceHandle h, int passId)
	{
		int num = 0;
		while (num < numReaders)
		{
			ref ResourceReaderData reference = ref ctx.resources.readerData[h.iType].ElementAt(ctx.resources.IndexReader(h, num));
			if (reference.passId == passId)
			{
				if (num < numReaders - 1)
				{
					reference = ctx.resources.readerData[h.iType][ctx.resources.IndexReader(h, numReaders - 1)];
				}
				numReaders--;
			}
			else
			{
				num++;
			}
		}
	}
}
