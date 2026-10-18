using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Voxels;

[Serializable]
public class PerlinVoxelGenerator : VoxelGenerator
{
	[Serializable]
	public struct NoiseParameters
	{
		public float NoiseScale;

		public float GroundLevel;

		public float HeightScale;

		public float HeightCompensation;

		public int Octaves;

		public float Persistence;
	}

	[BurstCompile]
	public struct VoxelDataJob : IJobParallelFor
	{
		public int3 chunkPosition;

		public int chunkSize;

		public int dimension;

		public float noiseScale;

		public float groundLevel;

		public float heightScale;

		public float heightCompensation;

		public int octaves;

		public float persistence;

		public int seed;

		[WriteOnly]
		public NativeArray<byte> voxels;

		[WriteOnly]
		public NativeArray<byte> materials;

		public void Execute(int index)
		{
			int num = index % dimension;
			int num2 = index / dimension % dimension;
			int num3 = index / (dimension * dimension);
			float3 @float = new float3(chunkPosition.x * chunkSize + num, chunkPosition.y * chunkSize + num2, chunkPosition.z * chunkSize + num3);
			float3 float2 = new float3((float)seed * 1.7f, (float)seed * 2.3f, (float)seed * 3.1f);
			float3 float3 = @float + float2;
			float num4 = noise.snoise((new float3(@float.x, 0f, @float.z) + float2) * noiseScale) + (groundLevel - @float.y) / heightScale;
			num4 = math.clamp(num4 * heightCompensation, -1f, 1f);
			float num5 = noiseScale;
			float num6 = 1f;
			for (int i = 0; i < octaves; i++)
			{
				num5 *= 2f;
				num6 *= persistence;
				num4 += noise.snoise(float3 * num5) * num6;
			}
			if (noise.snoise(float3 * 0.05f) > 0.6f && num4 >= 0f)
			{
				materials[index] = 1;
			}
			voxels[index] = num4.ToByte();
		}
	}

	public NoiseParameters noiseParameters = new NoiseParameters
	{
		NoiseScale = 0.01f,
		GroundLevel = 0f,
		HeightScale = 0.01f,
		Octaves = 4,
		Persistence = 0.5f
	};

	public override ChunkTask CreateVoxelDataJob(Chunk chunk)
	{
		if (!chunk.Density.IsCreated)
		{
			chunk.Density = new NativeArray<byte>(chunk.VoxelCount, Allocator.Persistent);
		}
		if (!chunk.Material.IsCreated)
		{
			chunk.Material = new NativeArray<byte>(chunk.VoxelCount, Allocator.Persistent);
		}
		VoxelDataJob jobData = default(VoxelDataJob);
		jobData.chunkPosition = chunk.Id;
		jobData.chunkSize = chunk.Size.x;
		jobData.dimension = chunk.Dimensions.x;
		jobData.noiseScale = noiseParameters.NoiseScale;
		jobData.groundLevel = noiseParameters.GroundLevel;
		jobData.heightCompensation = noiseParameters.HeightCompensation;
		jobData.octaves = noiseParameters.Octaves;
		jobData.persistence = noiseParameters.Persistence;
		jobData.heightScale = noiseParameters.HeightScale;
		jobData.seed = Seed;
		jobData.voxels = chunk.Density;
		jobData.materials = chunk.Material;
		JobHandle handle = IJobParallelForExtensions.Schedule(jobData, chunk.VoxelCount, 64);
		Action onComplete = delegate
		{
			chunk.IsDataGenerated = true;
			chunk.IsMeshGenerated = false;
			chunk.IsDirty = true;
		};
		return new ChunkTask(chunk, handle, onComplete);
	}
}
