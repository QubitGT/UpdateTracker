using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct ChainIKConstraintJob : IWeightedAnimationJob, IAnimationJob
{
	public NativeArray<ReadWriteTransformHandle> chain;

	public ReadOnlyTransformHandle target;

	public AffineTransform targetOffset;

	public NativeArray<float> linkLengths;

	public NativeArray<Vector3> linkPositions;

	public FloatProperty chainRotationWeight;

	public FloatProperty tipRotationWeight;

	public CacheIndex toleranceIdx;

	public CacheIndex maxIterationsIdx;

	public AnimationJobCache cache;

	public float maxReach;

	public FloatProperty jobWeight { get; set; }

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		float num = jobWeight.Get(stream);
		if (num > 0f)
		{
			for (int i = 0; i < chain.Length; i++)
			{
				ReadWriteTransformHandle value = chain[i];
				linkPositions[i] = value.GetPosition(stream);
				chain[i] = value;
			}
			int num2 = chain.Length - 1;
			if (AnimationRuntimeUtils.SolveFABRIK(ref linkPositions, ref linkLengths, target.GetPosition(stream) + targetOffset.translation, cache.GetRaw(toleranceIdx), maxReach, (int)cache.GetRaw(maxIterationsIdx)))
			{
				float t = chainRotationWeight.Get(stream) * num;
				for (int j = 0; j < num2; j++)
				{
					Vector3 from = chain[j + 1].GetPosition(stream) - chain[j].GetPosition(stream);
					Vector3 to = linkPositions[j + 1] - linkPositions[j];
					Quaternion rotation = chain[j].GetRotation(stream);
					chain[j].SetRotation(stream, Quaternion.Lerp(rotation, QuaternionExt.FromToRotation(from, to) * rotation, t));
				}
			}
			chain[num2].SetRotation(stream, Quaternion.Lerp(chain[num2].GetRotation(stream), target.GetRotation(stream) * targetOffset.rotation, tipRotationWeight.Get(stream) * num));
		}
		else
		{
			for (int k = 0; k < chain.Length; k++)
			{
				AnimationRuntimeUtils.PassThrough(stream, chain[k]);
			}
		}
	}
}
