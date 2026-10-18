using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal class DecalDrawCallChunk : DecalChunk
{
	public NativeArray<float4x4> decalToWorlds;

	public NativeArray<float4x4> normalToDecals;

	public NativeArray<float> renderingLayerMasks;

	public NativeArray<DecalSubDrawCall> subCalls;

	public NativeArray<int> subCallCounts;

	public int subCallCount
	{
		get
		{
			return subCallCounts[0];
		}
		set
		{
			subCallCounts[0] = value;
		}
	}

	public override void RemoveAtSwapBack(int entityIndex)
	{
		RemoveAtSwapBack(ref decalToWorlds, entityIndex, base.count);
		RemoveAtSwapBack(ref normalToDecals, entityIndex, base.count);
		RemoveAtSwapBack(ref renderingLayerMasks, entityIndex, base.count);
		RemoveAtSwapBack(ref subCalls, entityIndex, base.count);
		base.count--;
	}

	public override void SetCapacity(int newCapacity)
	{
		ArrayExtensions.ResizeArray(ref decalToWorlds, newCapacity);
		ArrayExtensions.ResizeArray(ref normalToDecals, newCapacity);
		ArrayExtensions.ResizeArray(ref renderingLayerMasks, newCapacity);
		ArrayExtensions.ResizeArray(ref subCalls, newCapacity);
		base.capacity = newCapacity;
	}

	public override void Dispose()
	{
		subCallCounts.Dispose();
		if (base.capacity != 0)
		{
			decalToWorlds.Dispose();
			normalToDecals.Dispose();
			renderingLayerMasks.Dispose();
			subCalls.Dispose();
			base.count = 0;
			base.capacity = 0;
		}
	}
}
