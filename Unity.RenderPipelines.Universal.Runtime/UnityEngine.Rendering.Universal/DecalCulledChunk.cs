using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

internal class DecalCulledChunk : DecalChunk
{
	public Vector3 cameraPosition;

	public ulong sceneCullingMask;

	public int cullingMask;

	public CullingGroup cullingGroups;

	public int[] visibleDecalIndexArray;

	public NativeArray<int> visibleDecalIndices;

	public int visibleDecalCount;

	public override void RemoveAtSwapBack(int entityIndex)
	{
		RemoveAtSwapBack(ref visibleDecalIndexArray, entityIndex, base.count);
		RemoveAtSwapBack(ref visibleDecalIndices, entityIndex, base.count);
		base.count--;
	}

	public override void SetCapacity(int newCapacity)
	{
		ArrayExtensions.ResizeArray(ref visibleDecalIndexArray, newCapacity);
		ArrayExtensions.ResizeArray(ref visibleDecalIndices, newCapacity);
		if (cullingGroups == null)
		{
			cullingGroups = new CullingGroup();
		}
		base.capacity = newCapacity;
	}

	public override void Dispose()
	{
		if (base.capacity != 0)
		{
			visibleDecalIndices.Dispose();
			visibleDecalIndexArray = null;
			base.count = 0;
			base.capacity = 0;
			cullingGroups.Dispose();
			cullingGroups = null;
		}
	}
}
