using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Voxels;

public static class SurfaceNets
{
	public static JobHandle Generate(NativeArray<byte> sdf, int3 shape, int3 min, int3 max, SurfaceNetsBuffer buffer, JobHandle dependency = default(JobHandle))
	{
		if (sdf.Length != shape.x * shape.y * shape.z)
		{
			throw new ArgumentException("SDF size does not match shape dimensions.");
		}
		SurfaceNetsJob jobData = default(SurfaceNetsJob);
		jobData.sdf = sdf;
		jobData.shape = shape;
		jobData.min = min;
		jobData.max = max;
		jobData.buffer = buffer;
		jobData.isoLevel = 0f.ToByte();
		return jobData.Schedule(dependency);
	}
}
