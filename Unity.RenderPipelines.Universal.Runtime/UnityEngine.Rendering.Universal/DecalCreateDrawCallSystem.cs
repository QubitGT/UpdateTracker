using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal class DecalCreateDrawCallSystem
{
	[BurstCompile]
	private struct DrawCallJob : IJob
	{
		[ReadOnly]
		public NativeArray<float4x4> decalToWorlds;

		[ReadOnly]
		public NativeArray<float4x4> normalToWorlds;

		[ReadOnly]
		public NativeArray<float4x4> sizeOffsets;

		[ReadOnly]
		public NativeArray<float2> drawDistances;

		[ReadOnly]
		public NativeArray<float2> angleFades;

		[ReadOnly]
		public NativeArray<float4> uvScaleBiases;

		[ReadOnly]
		public NativeArray<int> layerMasks;

		[ReadOnly]
		public NativeArray<ulong> sceneLayerMasks;

		[ReadOnly]
		public NativeArray<float> fadeFactors;

		[ReadOnly]
		public NativeArray<BoundingSphere> boundingSpheres;

		[ReadOnly]
		public NativeArray<uint> renderingLayerMasks;

		public Vector3 cameraPosition;

		public ulong sceneCullingMask;

		public int cullingMask;

		[ReadOnly]
		public NativeArray<int> visibleDecalIndices;

		public int visibleDecalCount;

		public float maxDrawDistance;

		[WriteOnly]
		public NativeArray<float4x4> decalToWorldsDraw;

		[WriteOnly]
		public NativeArray<float4x4> normalToDecalsDraw;

		[WriteOnly]
		public NativeArray<float> renderingLayerMasksDraw;

		[WriteOnly]
		public NativeArray<DecalSubDrawCall> subCalls;

		[WriteOnly]
		public NativeArray<int> subCallCount;

		public void Execute()
		{
			int value = 0;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < visibleDecalCount; i++)
			{
				int index = visibleDecalIndices[i];
				int num3 = 1 << layerMasks[index];
				if ((cullingMask & num3) == 0)
				{
					continue;
				}
				BoundingSphere boundingSphere = boundingSpheres[index];
				float2 @float = drawDistances[index];
				float magnitude = (cameraPosition - boundingSphere.position).magnitude;
				float num4 = math.min(@float.x, maxDrawDistance) + boundingSphere.radius;
				if (!(magnitude > num4))
				{
					decalToWorldsDraw[num] = decalToWorlds[index];
					float num5 = fadeFactors[index];
					float2 float2 = angleFades[index];
					float4 float3 = uvScaleBiases[index];
					float4x4 value2 = normalToWorlds[index];
					float num6 = num5 * math.clamp((num4 - magnitude) / (num4 * (1f - @float.y)), 0f, 1f);
					value2.c0.w = float3.x;
					value2.c1.w = float3.y;
					value2.c2.w = float3.z;
					value2.c3 = new float4(num6 * 1f, float2.x, float2.y, float3.w);
					normalToDecalsDraw[num] = value2;
					renderingLayerMasksDraw[num] = renderingLayerMasks[index];
					num++;
					if (num - num2 >= DecalDrawSystem.MaxBatchSize)
					{
						subCalls[value++] = new DecalSubDrawCall
						{
							start = num2,
							end = num
						};
						num2 = num;
					}
				}
			}
			if (num - num2 != 0)
			{
				subCalls[value++] = new DecalSubDrawCall
				{
					start = num2,
					end = num
				};
			}
			subCallCount[0] = value;
		}
	}

	private DecalEntityManager m_EntityManager;

	private ProfilingSampler m_Sampler;

	private float m_MaxDrawDistance;

	public float maxDrawDistance
	{
		get
		{
			return m_MaxDrawDistance;
		}
		set
		{
			m_MaxDrawDistance = value;
		}
	}

	public DecalCreateDrawCallSystem(DecalEntityManager entityManager, float maxDrawDistance)
	{
		m_EntityManager = entityManager;
		m_Sampler = new ProfilingSampler("DecalCreateDrawCallSystem.Execute");
		m_MaxDrawDistance = maxDrawDistance;
	}

	public void Execute()
	{
		using (new ProfilingScope(m_Sampler))
		{
			for (int i = 0; i < m_EntityManager.chunkCount; i++)
			{
				Execute(m_EntityManager.cachedChunks[i], m_EntityManager.culledChunks[i], m_EntityManager.drawCallChunks[i], m_EntityManager.cachedChunks[i].count);
			}
		}
	}

	private void Execute(DecalCachedChunk cachedChunk, DecalCulledChunk culledChunk, DecalDrawCallChunk drawCallChunk, int count)
	{
		if (count != 0)
		{
			DrawCallJob jobData = default(DrawCallJob);
			jobData.decalToWorlds = cachedChunk.decalToWorlds;
			jobData.normalToWorlds = cachedChunk.normalToWorlds;
			jobData.sizeOffsets = cachedChunk.sizeOffsets;
			jobData.drawDistances = cachedChunk.drawDistances;
			jobData.angleFades = cachedChunk.angleFades;
			jobData.uvScaleBiases = cachedChunk.uvScaleBias;
			jobData.layerMasks = cachedChunk.layerMasks;
			jobData.sceneLayerMasks = cachedChunk.sceneLayerMasks;
			jobData.fadeFactors = cachedChunk.fadeFactors;
			jobData.boundingSpheres = cachedChunk.boundingSpheres;
			jobData.renderingLayerMasks = cachedChunk.renderingLayerMasks;
			jobData.cameraPosition = culledChunk.cameraPosition;
			jobData.sceneCullingMask = culledChunk.sceneCullingMask;
			jobData.cullingMask = culledChunk.cullingMask;
			jobData.visibleDecalIndices = culledChunk.visibleDecalIndices;
			jobData.visibleDecalCount = culledChunk.visibleDecalCount;
			jobData.maxDrawDistance = m_MaxDrawDistance;
			jobData.decalToWorldsDraw = drawCallChunk.decalToWorlds;
			jobData.normalToDecalsDraw = drawCallChunk.normalToDecals;
			jobData.renderingLayerMasksDraw = drawCallChunk.renderingLayerMasks;
			jobData.subCalls = drawCallChunk.subCalls;
			jobData.subCallCount = drawCallChunk.subCallCounts;
			JobHandle currentJobHandle = (drawCallChunk.currentJobHandle = jobData.Schedule(cachedChunk.currentJobHandle));
			cachedChunk.currentJobHandle = currentJobHandle;
		}
	}
}
