using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct TwistChainConstraintJob : IWeightedAnimationJob, IAnimationJob
{
	public ReadWriteTransformHandle rootTarget;

	public ReadWriteTransformHandle tipTarget;

	public NativeArray<ReadWriteTransformHandle> chain;

	public NativeArray<float> steps;

	public NativeArray<float> weights;

	public NativeArray<Quaternion> rotations;

	public FloatProperty jobWeight { get; set; }

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		float num = jobWeight.Get(stream);
		if (num > 0f)
		{
			Quaternion rotation = rootTarget.GetRotation(stream);
			Quaternion rotation2 = tipTarget.GetRotation(stream);
			chain[0].SetRotation(stream, Quaternion.Lerp(chain[0].GetRotation(stream), rotation, num));
			for (int i = 1; i < chain.Length - 1; i++)
			{
				chain[i].SetRotation(stream, Quaternion.Lerp(chain[i].GetRotation(stream), rotations[i] * Quaternion.Lerp(rotation, rotation2, weights[i]), num));
			}
			chain[chain.Length - 1].SetRotation(stream, Quaternion.Lerp(chain[chain.Length - 1].GetRotation(stream), rotation2, num));
		}
		else
		{
			for (int j = 0; j < chain.Length; j++)
			{
				AnimationRuntimeUtils.PassThrough(stream, chain[j]);
			}
		}
	}
}
