using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class MultiReferentialConstraintJobBinder<T> : AnimationJobBinder<MultiReferentialConstraintJob, T> where T : struct, IAnimationJobData, IMultiReferentialConstraintData
{
	public override MultiReferentialConstraintJob Create(Animator animator, ref T data, Component component)
	{
		MultiReferentialConstraintJob result = default(MultiReferentialConstraintJob);
		Transform[] sourceObjects = data.sourceObjects;
		result.driver = IntProperty.Bind(animator, component, data.driverIntProperty);
		result.sources = new NativeArray<ReadWriteTransformHandle>(sourceObjects.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.sourceBindTx = new NativeArray<AffineTransform>(sourceObjects.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.offsetTx = new NativeArray<AffineTransform>(sourceObjects.Length - 1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < sourceObjects.Length; i++)
		{
			result.sources[i] = ReadWriteTransformHandle.Bind(animator, sourceObjects[i].transform);
			result.sourceBindTx[i] = new AffineTransform(sourceObjects[i].position, sourceObjects[i].rotation);
		}
		result.UpdateOffsets(data.driverValue);
		return result;
	}

	public override void Destroy(MultiReferentialConstraintJob job)
	{
		job.sources.Dispose();
		job.sourceBindTx.Dispose();
		job.offsetTx.Dispose();
	}
}
