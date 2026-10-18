using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class ChainIKConstraintJobBinder<T> : AnimationJobBinder<ChainIKConstraintJob, T> where T : struct, IAnimationJobData, IChainIKConstraintData
{
	public override ChainIKConstraintJob Create(Animator animator, ref T data, Component component)
	{
		Transform[] array = ConstraintsUtils.ExtractChain(data.root, data.tip);
		ChainIKConstraintJob result = default(ChainIKConstraintJob);
		result.chain = new NativeArray<ReadWriteTransformHandle>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.linkLengths = new NativeArray<float>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.linkPositions = new NativeArray<Vector3>(array.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.maxReach = 0f;
		int num = array.Length - 1;
		for (int i = 0; i < array.Length; i++)
		{
			result.chain[i] = ReadWriteTransformHandle.Bind(animator, array[i]);
			result.linkLengths[i] = ((i != num) ? Vector3.Distance(array[i].position, array[i + 1].position) : 0f);
			result.maxReach += result.linkLengths[i];
		}
		result.target = ReadOnlyTransformHandle.Bind(animator, data.target);
		result.targetOffset = AffineTransform.identity;
		if (data.maintainTargetPositionOffset)
		{
			result.targetOffset.translation = data.tip.position - data.target.position;
		}
		if (data.maintainTargetRotationOffset)
		{
			result.targetOffset.rotation = Quaternion.Inverse(data.target.rotation) * data.tip.rotation;
		}
		result.chainRotationWeight = FloatProperty.Bind(animator, component, data.chainRotationWeightFloatProperty);
		result.tipRotationWeight = FloatProperty.Bind(animator, component, data.tipRotationWeightFloatProperty);
		AnimationJobCacheBuilder animationJobCacheBuilder = new AnimationJobCacheBuilder();
		result.maxIterationsIdx = animationJobCacheBuilder.Add(data.maxIterations);
		result.toleranceIdx = animationJobCacheBuilder.Add(data.tolerance);
		result.cache = animationJobCacheBuilder.Build();
		return result;
	}

	public override void Destroy(ChainIKConstraintJob job)
	{
		job.chain.Dispose();
		job.linkLengths.Dispose();
		job.linkPositions.Dispose();
		job.cache.Dispose();
	}

	public override void Update(ChainIKConstraintJob job, ref T data)
	{
		job.cache.SetRaw(data.maxIterations, job.maxIterationsIdx);
		job.cache.SetRaw(data.tolerance, job.toleranceIdx);
	}
}
