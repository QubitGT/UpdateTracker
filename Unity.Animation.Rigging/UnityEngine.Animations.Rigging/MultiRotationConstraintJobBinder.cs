using System;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class MultiRotationConstraintJobBinder<T> : AnimationJobBinder<MultiRotationConstraintJob, T> where T : struct, IAnimationJobData, IMultiRotationConstraintData
{
	public override MultiRotationConstraintJob Create(Animator animator, ref T data, Component component)
	{
		MultiRotationConstraintJob result = default(MultiRotationConstraintJob);
		result.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
		result.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
		result.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);
		WeightedTransformArray sourceObjects = data.sourceObjects;
		WeightedTransformArrayBinder.BindReadOnlyTransforms(animator, component, sourceObjects, out result.sourceTransforms);
		WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out result.sourceWeights);
		result.sourceOffsets = new NativeArray<Quaternion>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Quaternion rotation = data.constrainedObject.rotation;
		for (int i = 0; i < sourceObjects.Count; i++)
		{
			result.sourceOffsets[i] = (data.maintainOffset ? (Quaternion.Inverse(sourceObjects[i].transform.rotation) * rotation) : Quaternion.identity);
		}
		result.axesMask = new Vector3(Convert.ToSingle(data.constrainedXAxis), Convert.ToSingle(data.constrainedYAxis), Convert.ToSingle(data.constrainedZAxis));
		return result;
	}

	public override void Destroy(MultiRotationConstraintJob job)
	{
		job.sourceTransforms.Dispose();
		job.sourceWeights.Dispose();
		job.sourceOffsets.Dispose();
		job.weightBuffer.Dispose();
	}
}
