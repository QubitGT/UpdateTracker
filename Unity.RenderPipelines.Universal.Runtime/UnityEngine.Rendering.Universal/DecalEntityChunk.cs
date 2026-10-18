using Unity.Collections;
using UnityEngine.Jobs;

namespace UnityEngine.Rendering.Universal;

internal class DecalEntityChunk : DecalChunk
{
	public Material material;

	public NativeArray<DecalEntity> decalEntities;

	public DecalProjector[] decalProjectors;

	public TransformAccessArray transformAccessArray;

	public override void Push()
	{
		base.count++;
	}

	public override void RemoveAtSwapBack(int entityIndex)
	{
		RemoveAtSwapBack(ref decalEntities, entityIndex, base.count);
		RemoveAtSwapBack(ref decalProjectors, entityIndex, base.count);
		transformAccessArray.RemoveAtSwapBack(entityIndex);
		base.count--;
	}

	public override void SetCapacity(int newCapacity)
	{
		ArrayExtensions.ResizeArray(ref decalEntities, newCapacity);
		ResizeNativeArray(ref transformAccessArray, decalProjectors, newCapacity);
		ArrayExtensions.ResizeArray(ref decalProjectors, newCapacity);
		base.capacity = newCapacity;
	}

	public override void Dispose()
	{
		if (base.capacity != 0)
		{
			decalEntities.Dispose();
			transformAccessArray.Dispose();
			decalProjectors = null;
			base.count = 0;
			base.capacity = 0;
		}
	}
}
