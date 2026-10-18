using System;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class MultiAimConstraintJobBinder<T> : AnimationJobBinder<MultiAimConstraintJob, T> where T : struct, IAnimationJobData, IMultiAimConstraintData
{
	public override MultiAimConstraintJob Create(Animator animator, ref T data, Component component)
	{
		MultiAimConstraintJob result = default(MultiAimConstraintJob);
		result.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
		result.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
		result.aimAxis = data.aimAxis;
		result.upAxis = data.upAxis;
		result.worldUpType = (MultiAimConstraintJob.WorldUpType)data.worldUpType;
		result.worldUpAxis = data.worldUpAxis;
		if (data.worldUpObject != null)
		{
			result.worldUpObject = ReadOnlyTransformHandle.Bind(animator, data.worldUpObject);
		}
		WeightedTransformArray sourceObjects = data.sourceObjects;
		WeightedTransformArrayBinder.BindReadOnlyTransforms(animator, component, sourceObjects, out result.sourceTransforms);
		WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out result.sourceWeights);
		result.sourceOffsets = new NativeArray<Quaternion>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < sourceObjects.Count; i++)
		{
			if (data.maintainOffset)
			{
				Vector3 to = data.constrainedObject.rotation * data.aimAxis;
				result.sourceOffsets[i] = QuaternionExt.FromToRotation(sourceObjects[i].transform.position - data.constrainedObject.position, to);
			}
			else
			{
				result.sourceOffsets[i] = Quaternion.identity;
			}
		}
		result.minLimit = FloatProperty.Bind(animator, component, data.minLimitFloatProperty);
		result.maxLimit = FloatProperty.Bind(animator, component, data.maxLimitFloatProperty);
		result.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);
		result.axesMask = new Vector3(Convert.ToSingle(data.constrainedXAxis), Convert.ToSingle(data.constrainedYAxis), Convert.ToSingle(data.constrainedZAxis));
		return result;
	}

	public override void Destroy(MultiAimConstraintJob job)
	{
		job.sourceTransforms.Dispose();
		job.sourceWeights.Dispose();
		job.sourceOffsets.Dispose();
		job.weightBuffer.Dispose();
	}
}
