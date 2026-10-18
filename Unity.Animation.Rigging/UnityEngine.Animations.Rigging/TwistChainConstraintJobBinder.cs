using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class TwistChainConstraintJobBinder<T> : AnimationJobBinder<TwistChainConstraintJob, T> where T : struct, IAnimationJobData, ITwistChainConstraintData
{
	public override TwistChainConstraintJob Create(Animator animator, ref T data, Component component)
	{
		Transform[] array = ConstraintsUtils.ExtractChain(data.root, data.tip);
		float[] array2 = ConstraintsUtils.ExtractSteps(array);
		TwistChainConstraintJob result = default(TwistChainConstraintJob);
		result.chain = new NativeArray<ReadWriteTransformHandle>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.steps = new NativeArray<float>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.weights = new NativeArray<float>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.rotations = new NativeArray<Quaternion>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.rootTarget = ReadWriteTransformHandle.Bind(animator, data.rootTarget);
		result.tipTarget = ReadWriteTransformHandle.Bind(animator, data.tipTarget);
		for (int i = 0; i < array.Length; i++)
		{
			result.chain[i] = ReadWriteTransformHandle.Bind(animator, array[i]);
			result.steps[i] = array2[i];
			result.weights[i] = Mathf.Clamp01(data.curve.Evaluate(array2[i]));
		}
		result.rotations[0] = Quaternion.identity;
		result.rotations[array.Length - 1] = Quaternion.identity;
		for (int j = 1; j < array.Length - 1; j++)
		{
			result.rotations[j] = Quaternion.Inverse(Quaternion.Lerp(array[0].rotation, array[^1].rotation, result.weights[j])) * array[j].rotation;
		}
		return result;
	}

	public override void Destroy(TwistChainConstraintJob job)
	{
		job.chain.Dispose();
		job.weights.Dispose();
		job.steps.Dispose();
		job.rotations.Dispose();
	}
}
