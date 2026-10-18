using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace UnityEngine.Rendering.Universal;

internal class DecalUpdateCachedSystem
{
	[BurstCompile]
	public struct UpdateTransformsJob : IJobParallelForTransform
	{
		private static readonly quaternion k_MinusYtoZRotation = quaternion.EulerXYZ(-MathF.PI / 2f, 0f, 0f);

		public NativeArray<float3> positions;

		public NativeArray<quaternion> rotations;

		public NativeArray<float3> scales;

		public NativeArray<bool> dirty;

		[ReadOnly]
		public NativeArray<DecalScaleMode> scaleModes;

		[ReadOnly]
		public NativeArray<float4x4> sizeOffsets;

		[WriteOnly]
		public NativeArray<float4x4> decalToWorlds;

		[WriteOnly]
		public NativeArray<float4x4> normalToWorlds;

		[WriteOnly]
		public NativeArray<BoundingSphere> boundingSpheres;

		public float minDistance;

		private float DistanceBetweenQuaternions(quaternion a, quaternion b)
		{
			return math.distancesq(a.value, b.value);
		}

		public void Execute(int index, TransformAccess transform)
		{
			bool num = math.distancesq(transform.position, positions[index]) > minDistance;
			if (num)
			{
				positions[index] = transform.position;
			}
			bool flag = DistanceBetweenQuaternions(transform.rotation, rotations[index]) > minDistance;
			if (flag)
			{
				rotations[index] = transform.rotation;
			}
			bool flag2 = math.distancesq(transform.localScale, scales[index]) > minDistance;
			if (flag2)
			{
				scales[index] = transform.localScale;
			}
			if (num || flag || flag2 || dirty[index])
			{
				float4x4 a;
				if (scaleModes[index] == DecalScaleMode.InheritFromHierarchy)
				{
					a = transform.localToWorldMatrix;
					a = math.mul(a, new float4x4(k_MinusYtoZRotation, float3.zero));
				}
				else
				{
					quaternion rotation = math.mul(transform.rotation, k_MinusYtoZRotation);
					a = float4x4.TRS(positions[index], rotation, new float3(1f, 1f, 1f));
				}
				float4x4 value = a;
				float4 c = value.c1;
				value.c1 = value.c2;
				value.c2 = c;
				normalToWorlds[index] = value;
				float4x4 b = sizeOffsets[index];
				float4x4 float4x = math.mul(a, b);
				decalToWorlds[index] = float4x;
				boundingSpheres[index] = GetDecalProjectBoundingSphere(float4x);
				dirty[index] = false;
			}
		}

		private BoundingSphere GetDecalProjectBoundingSphere(Matrix4x4 decalToWorld)
		{
			float4 b = new float4(-0.5f, -0.5f, -0.5f, 1f);
			float4 b2 = new float4(0.5f, 0.5f, 0.5f, 1f);
			b = math.mul(decalToWorld, b);
			b2 = math.mul(decalToWorld, b2);
			float3 xyz = ((b2 + b) / 2f).xyz;
			float radius = math.length(b2 - b) / 2f;
			BoundingSphere result = default(BoundingSphere);
			result.position = xyz;
			result.radius = radius;
			return result;
		}
	}

	private DecalEntityManager m_EntityManager;

	private ProfilingSampler m_Sampler;

	private ProfilingSampler m_SamplerJob;

	public DecalUpdateCachedSystem(DecalEntityManager entityManager)
	{
		m_EntityManager = entityManager;
		m_Sampler = new ProfilingSampler("DecalUpdateCachedSystem.Execute");
		m_SamplerJob = new ProfilingSampler("DecalUpdateCachedSystem.ExecuteJob");
	}

	public void Execute()
	{
		using (new ProfilingScope(m_Sampler))
		{
			for (int i = 0; i < m_EntityManager.chunkCount; i++)
			{
				Execute(m_EntityManager.entityChunks[i], m_EntityManager.cachedChunks[i], m_EntityManager.entityChunks[i].count);
			}
		}
	}

	private void Execute(DecalEntityChunk entityChunk, DecalCachedChunk cachedChunk, int count)
	{
		if (count == 0)
		{
			return;
		}
		cachedChunk.currentJobHandle.Complete();
		Material material = entityChunk.material;
		if (material.HasProperty("_DrawOrder"))
		{
			cachedChunk.drawOrder = material.GetInt("_DrawOrder");
		}
		if (!cachedChunk.isCreated)
		{
			int passIndexDBuffer = material.FindPass("DBufferProjector");
			cachedChunk.passIndexDBuffer = passIndexDBuffer;
			int passIndexEmissive = material.FindPass("DecalProjectorForwardEmissive");
			cachedChunk.passIndexEmissive = passIndexEmissive;
			int passIndexScreenSpace = material.FindPass("DecalScreenSpaceProjector");
			cachedChunk.passIndexScreenSpace = passIndexScreenSpace;
			int passIndexGBuffer = material.FindPass("DecalGBufferProjector");
			cachedChunk.passIndexGBuffer = passIndexGBuffer;
			cachedChunk.isCreated = true;
		}
		using (new ProfilingScope(m_SamplerJob))
		{
			UpdateTransformsJob jobData = default(UpdateTransformsJob);
			jobData.positions = cachedChunk.positions;
			jobData.rotations = cachedChunk.rotation;
			jobData.scales = cachedChunk.scales;
			jobData.dirty = cachedChunk.dirty;
			jobData.scaleModes = cachedChunk.scaleModes;
			jobData.sizeOffsets = cachedChunk.sizeOffsets;
			jobData.decalToWorlds = cachedChunk.decalToWorlds;
			jobData.normalToWorlds = cachedChunk.normalToWorlds;
			jobData.boundingSpheres = cachedChunk.boundingSpheres;
			jobData.minDistance = float.Epsilon;
			JobHandle currentJobHandle = jobData.Schedule(entityChunk.transformAccessArray);
			cachedChunk.currentJobHandle = currentJobHandle;
		}
	}
}
