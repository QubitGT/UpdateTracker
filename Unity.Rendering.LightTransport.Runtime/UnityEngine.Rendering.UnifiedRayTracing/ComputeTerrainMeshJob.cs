using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.UnifiedRayTracing;

[BurstCompile]
internal struct ComputeTerrainMeshJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<float> heightmap;

	[ReadOnly]
	public NativeArray<bool> holes;

	public int width;

	public int height;

	public float3 heightmapScale;

	public NativeArray<float3> positions;

	public NativeArray<float2> uvs;

	public NativeArray<float3> normals;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> indices;

	public void DisposeArrays()
	{
		heightmap.Dispose();
		holes.Dispose();
		positions.Dispose();
		uvs.Dispose();
		normals.Dispose();
		indices.Dispose();
	}

	public void Execute(int index)
	{
		int num = index % width;
		int num2 = index / height;
		float3 @float = new float3(num, heightmap[num2 * width + num], num2);
		positions[index] = @float * heightmapScale;
		uvs[index] = @float.xz / new float2(width, height);
		normals[index] = CalculateTerrainNormal(heightmap, num, num2, width, height, heightmapScale);
		if (num < width - 1 && num2 < height - 1)
		{
			int num3 = num2 * width + num;
			int value = num3 + 1;
			int num4 = num3 + width;
			int value2 = num4 + 1;
			int num5 = num + num2 * (width - 1);
			if (!holes[num5])
			{
				num3 = (value = (num4 = (value2 = 0)));
			}
			indices[6 * num5] = num3;
			indices[6 * num5 + 1] = value2;
			indices[6 * num5 + 2] = value;
			indices[6 * num5 + 3] = num3;
			indices[6 * num5 + 4] = num4;
			indices[6 * num5 + 5] = value2;
		}
	}

	private static float3 CalculateTerrainNormal(NativeArray<float> heightmap, int x, int y, int width, int height, float3 scale)
	{
		float num = (SampleHeight(x - 1, y - 1, width, height, heightmap, scale.y) * -1f + SampleHeight(x - 1, y, width, height, heightmap, scale.y) * -2f + SampleHeight(x - 1, y + 1, width, height, heightmap, scale.y) * -1f + SampleHeight(x + 1, y - 1, width, height, heightmap, scale.y) * 1f + SampleHeight(x + 1, y, width, height, heightmap, scale.y) * 2f + SampleHeight(x + 1, y + 1, width, height, heightmap, scale.y) * 1f) / scale.x;
		float num2 = SampleHeight(x - 1, y - 1, width, height, heightmap, scale.y) * -1f;
		num2 += SampleHeight(x, y - 1, width, height, heightmap, scale.y) * -2f;
		num2 += SampleHeight(x + 1, y - 1, width, height, heightmap, scale.y) * -1f;
		num2 += SampleHeight(x - 1, y + 1, width, height, heightmap, scale.y) * 1f;
		num2 += SampleHeight(x, y + 1, width, height, heightmap, scale.y) * 2f;
		num2 += SampleHeight(x + 1, y + 1, width, height, heightmap, scale.y) * 1f;
		num2 /= scale.z;
		return math.normalize(new float3(0f - num, 8f, 0f - num2));
	}

	private static float SampleHeight(int x, int y, int width, int height, NativeArray<float> heightmap, float scale)
	{
		x = math.clamp(x, 0, width - 1);
		y = math.clamp(y, 0, height - 1);
		return heightmap[x + y * width] * scale;
	}
}
