using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Drawing.Examples;

public class BurstExample : MonoBehaviour
{
	[BurstCompile]
	private struct DrawingJob : IJob
	{
		public float2 offset;

		public CommandBuilder builder;

		private Color Colormap(float x)
		{
			float r = math.clamp(2.6666667f * x, 0f, 1f);
			float g = math.clamp(2.6666667f * x - 1f, 0f, 1f);
			float b = math.clamp(4f * x - 3f, 0f, 1f);
			return new Color(r, g, b, 1f);
		}

		public void Execute(int index)
		{
			int num = index / 100;
			int num2 = index % 100;
			float num3 = Mathf.PerlinNoise((float)num * 0.05f + offset.x, (float)num2 * 0.05f + offset.y);
			Bounds bounds = new Bounds(new float3(num, 0f, num2), new float3(1f, 14f * num3, 1f));
			builder.SolidBox(bounds, Colormap(num3));
		}

		public void Execute()
		{
			for (int i = 0; i < 10000; i++)
			{
				Execute(i);
			}
		}
	}

	public void Update()
	{
		CommandBuilder builder = DrawingManager.GetBuilder(renderInGame: true);
		DrawingJob jobData = default(DrawingJob);
		jobData.builder = builder;
		jobData.offset = new float2(Time.time * 0.2f, Time.time * 0.2f);
		JobHandle dependency = jobData.Schedule();
		builder.DisposeAfter(dependency);
		dependency.Complete();
	}
}
