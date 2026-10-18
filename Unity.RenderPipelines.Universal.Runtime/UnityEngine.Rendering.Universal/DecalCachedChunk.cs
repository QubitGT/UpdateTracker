using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal class DecalCachedChunk : DecalChunk
{
	public MaterialPropertyBlock propertyBlock;

	public int passIndexDBuffer;

	public int passIndexEmissive;

	public int passIndexScreenSpace;

	public int passIndexGBuffer;

	public int drawOrder;

	public bool isCreated;

	public NativeArray<float4x4> decalToWorlds;

	public NativeArray<float4x4> normalToWorlds;

	public NativeArray<float4x4> sizeOffsets;

	public NativeArray<float2> drawDistances;

	public NativeArray<float2> angleFades;

	public NativeArray<float4> uvScaleBias;

	public NativeArray<int> layerMasks;

	public NativeArray<ulong> sceneLayerMasks;

	public NativeArray<float> fadeFactors;

	public NativeArray<BoundingSphere> boundingSpheres;

	public NativeArray<DecalScaleMode> scaleModes;

	public NativeArray<uint> renderingLayerMasks;

	public NativeArray<float3> positions;

	public NativeArray<quaternion> rotation;

	public NativeArray<float3> scales;

	public NativeArray<bool> dirty;

	public BoundingSphere[] boundingSphereArray;

	public override void RemoveAtSwapBack(int entityIndex)
	{
		RemoveAtSwapBack(ref decalToWorlds, entityIndex, base.count);
		RemoveAtSwapBack(ref normalToWorlds, entityIndex, base.count);
		RemoveAtSwapBack(ref sizeOffsets, entityIndex, base.count);
		RemoveAtSwapBack(ref drawDistances, entityIndex, base.count);
		RemoveAtSwapBack(ref angleFades, entityIndex, base.count);
		RemoveAtSwapBack(ref uvScaleBias, entityIndex, base.count);
		RemoveAtSwapBack(ref layerMasks, entityIndex, base.count);
		RemoveAtSwapBack(ref sceneLayerMasks, entityIndex, base.count);
		RemoveAtSwapBack(ref fadeFactors, entityIndex, base.count);
		RemoveAtSwapBack(ref boundingSphereArray, entityIndex, base.count);
		RemoveAtSwapBack(ref boundingSpheres, entityIndex, base.count);
		RemoveAtSwapBack(ref scaleModes, entityIndex, base.count);
		RemoveAtSwapBack(ref renderingLayerMasks, entityIndex, base.count);
		RemoveAtSwapBack(ref positions, entityIndex, base.count);
		RemoveAtSwapBack(ref rotation, entityIndex, base.count);
		RemoveAtSwapBack(ref scales, entityIndex, base.count);
		RemoveAtSwapBack(ref dirty, entityIndex, base.count);
		base.count--;
	}

	public override void SetCapacity(int newCapacity)
	{
		ArrayExtensions.ResizeArray(ref decalToWorlds, newCapacity);
		ArrayExtensions.ResizeArray(ref normalToWorlds, newCapacity);
		ArrayExtensions.ResizeArray(ref sizeOffsets, newCapacity);
		ArrayExtensions.ResizeArray(ref drawDistances, newCapacity);
		ArrayExtensions.ResizeArray(ref angleFades, newCapacity);
		ArrayExtensions.ResizeArray(ref uvScaleBias, newCapacity);
		ArrayExtensions.ResizeArray(ref layerMasks, newCapacity);
		ArrayExtensions.ResizeArray(ref sceneLayerMasks, newCapacity);
		ArrayExtensions.ResizeArray(ref fadeFactors, newCapacity);
		ArrayExtensions.ResizeArray(ref boundingSpheres, newCapacity);
		ArrayExtensions.ResizeArray(ref scaleModes, newCapacity);
		ArrayExtensions.ResizeArray(ref renderingLayerMasks, newCapacity);
		ArrayExtensions.ResizeArray(ref positions, newCapacity);
		ArrayExtensions.ResizeArray(ref rotation, newCapacity);
		ArrayExtensions.ResizeArray(ref scales, newCapacity);
		ArrayExtensions.ResizeArray(ref dirty, newCapacity);
		ArrayExtensions.ResizeArray(ref boundingSphereArray, newCapacity);
		base.capacity = newCapacity;
	}

	public override void Dispose()
	{
		if (base.capacity != 0)
		{
			decalToWorlds.Dispose();
			normalToWorlds.Dispose();
			sizeOffsets.Dispose();
			drawDistances.Dispose();
			angleFades.Dispose();
			uvScaleBias.Dispose();
			layerMasks.Dispose();
			sceneLayerMasks.Dispose();
			fadeFactors.Dispose();
			boundingSpheres.Dispose();
			scaleModes.Dispose();
			renderingLayerMasks.Dispose();
			positions.Dispose();
			rotation.Dispose();
			scales.Dispose();
			dirty.Dispose();
			base.count = 0;
			base.capacity = 0;
		}
	}
}
