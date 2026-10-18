using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal static class TerrainToMesh
{
	private static AsyncTerrainToMeshRequest MakeAsyncTerrainToMeshRequest(int width, int height, Vector3 heightmapScale, float[,] heightmap, bool[,] holes)
	{
		int num = width * height;
		ComputeTerrainMeshJob computeTerrainMeshJob = default(ComputeTerrainMeshJob);
		computeTerrainMeshJob.heightmap = new NativeArray<float>(num, Allocator.Persistent);
		for (int i = 0; i < num; i++)
		{
			computeTerrainMeshJob.heightmap[i] = heightmap[i / width, i % width];
		}
		computeTerrainMeshJob.holes = new NativeArray<bool>((width - 1) * (height - 1), Allocator.Persistent);
		for (int j = 0; j < (width - 1) * (height - 1); j++)
		{
			computeTerrainMeshJob.holes[j] = holes[j / (width - 1), j % (width - 1)];
		}
		computeTerrainMeshJob.width = width;
		computeTerrainMeshJob.height = height;
		computeTerrainMeshJob.heightmapScale = heightmapScale;
		computeTerrainMeshJob.positions = new NativeArray<float3>(num, Allocator.Persistent);
		computeTerrainMeshJob.uvs = new NativeArray<float2>(num, Allocator.Persistent);
		computeTerrainMeshJob.normals = new NativeArray<float3>(num, Allocator.Persistent);
		computeTerrainMeshJob.indices = new NativeArray<int>((width - 1) * (height - 1) * 6, Allocator.Persistent);
		JobHandle jobHandle = IJobParallelForExtensions.Schedule(computeTerrainMeshJob, num, math.max(width, 128));
		return new AsyncTerrainToMeshRequest(computeTerrainMeshJob, jobHandle);
	}

	public static AsyncTerrainToMeshRequest ConvertAsync(Terrain terrain)
	{
		TerrainData terrainData = terrain.terrainData;
		int width = terrainData.heightmapTexture.width;
		int height = terrainData.heightmapTexture.height;
		float[,] heights = terrain.terrainData.GetHeights(0, 0, width, height);
		bool[,] holes = terrain.terrainData.GetHoles(0, 0, width - 1, height - 1);
		return MakeAsyncTerrainToMeshRequest(width, height, terrainData.heightmapScale, heights, holes);
	}

	public static AsyncTerrainToMeshRequest ConvertAsync(int heightmapWidth, int heightmapHeight, short[] heightmapData, Vector3 heightmapScale, int holeWidth, int holeHeight, byte[] holedata)
	{
		float[,] array = new float[heightmapWidth, heightmapHeight];
		for (int i = 0; i < heightmapHeight; i++)
		{
			for (int j = 0; j < heightmapWidth; j++)
			{
				array[i, j] = (float)heightmapData[i * heightmapWidth + j] / 32766f;
			}
		}
		bool[,] array2 = new bool[heightmapWidth - 1, heightmapHeight - 1];
		if (holedata != null)
		{
			for (int k = 0; k < heightmapHeight - 1; k++)
			{
				for (int l = 0; l < heightmapWidth - 1; l++)
				{
					array2[k, l] = holedata[k * holeWidth + l] != 0;
				}
			}
		}
		else
		{
			for (int m = 0; m < heightmapHeight - 1; m++)
			{
				for (int n = 0; n < heightmapWidth - 1; n++)
				{
					array2[n, m] = true;
				}
			}
		}
		return MakeAsyncTerrainToMeshRequest(heightmapWidth, heightmapHeight, heightmapScale, array, array2);
	}

	public static Mesh Convert(Terrain terrain)
	{
		AsyncTerrainToMeshRequest asyncTerrainToMeshRequest = ConvertAsync(terrain);
		asyncTerrainToMeshRequest.WaitForCompletion();
		return asyncTerrainToMeshRequest.GetMesh();
	}

	public static Mesh Convert(int heightmapWidth, int heightmapHeight, short[] heightmapData, Vector3 heightmapScale, int holeWidth, int holeHeight, byte[] holedata)
	{
		AsyncTerrainToMeshRequest asyncTerrainToMeshRequest = ConvertAsync(heightmapWidth, heightmapHeight, heightmapData, heightmapScale, holeWidth, holeHeight, holedata);
		asyncTerrainToMeshRequest.WaitForCompletion();
		return asyncTerrainToMeshRequest.GetMesh();
	}
}
