using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct AnimateCrossFadeJob : IJobParallelFor
{
	public const int k_BatchSize = 512;

	public const byte k_MeshLODTransitionToLowerLODBit = 128;

	private const byte k_LODFadeOff = byte.MaxValue;

	private const float k_CrossfadeAnimationTimeS = 0.333f;

	[ReadOnly]
	public float deltaTime;

	public UnsafeList<byte> crossFadeArray;

	public void Execute(int instanceIndex)
	{
		ref byte reference = ref crossFadeArray.ElementAt(instanceIndex);
		if (reference != byte.MaxValue)
		{
			int num = reference & 0x80;
			reference += (byte)(deltaTime / 0.333f * 127f);
			if (num != ((reference + 1) & 0x80))
			{
				reference = byte.MaxValue;
			}
		}
	}
}
