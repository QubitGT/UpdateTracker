using System;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class MultiPositionConstraintJobBinder<T> : AnimationJobBinder<MultiPositionConstraintJob, T> where T : struct, IAnimationJobData, IMultiPositionConstraintData
{
	public override MultiPositionConstraintJob Create(Animator animator, ref T data, Component component)
	{
		MultiPositionConstraintJob result = default(MultiPositionConstraintJob);
		result.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
		result.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
		result.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);
		WeightedTransformArray sourceObjects = data.sourceObjects;
		WeightedTransformArrayBinder.BindReadOnlyTransforms(animator, component, sourceObjects, out result.sourceTransforms);
		WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out result.sourceWeights);
		result.sourceOffsets = new NativeArray<Vector3>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Vector3 position = data.constrainedObject.position;
		for (int i = 0; i < sourceObjects.Count; i++)
		{
			result.sourceOffsets[i] = (data.maintainOffset ? (position - sourceObjects[i].transform.position) : Vector3.zero);
		}
		result.axesMask = new Vector3(Convert.ToSingle(data.constrainedXAxis), Convert.ToSingle(data.constrainedYAxis), Convert.ToSingle(data.constrainedZAxis));
		return result;
	}

	public override void Destroy(MultiPositionConstraintJob job)
	{
		job.sourceTransforms.Dispose();
		job.sourceWeights.Dispose();
		job.sourceOffsets.Dispose();
		job.weightBuffer.Dispose();
	}
}
