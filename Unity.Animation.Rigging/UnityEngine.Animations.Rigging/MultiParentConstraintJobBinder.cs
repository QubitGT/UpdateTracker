using System;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class MultiParentConstraintJobBinder<T> : AnimationJobBinder<MultiParentConstraintJob, T> where T : struct, IAnimationJobData, IMultiParentConstraintData
{
	public override MultiParentConstraintJob Create(Animator animator, ref T data, Component component)
	{
		MultiParentConstraintJob result = default(MultiParentConstraintJob);
		result.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
		result.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
		WeightedTransformArray sourceObjects = data.sourceObjects;
		WeightedTransformArrayBinder.BindReadOnlyTransforms(animator, component, sourceObjects, out result.sourceTransforms);
		WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out result.sourceWeights);
		result.sourceOffsets = new NativeArray<AffineTransform>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		AffineTransform transform = new AffineTransform(data.constrainedObject.position, data.constrainedObject.rotation);
		for (int i = 0; i < sourceObjects.Count; i++)
		{
			Transform transform2 = sourceObjects[i].transform;
			AffineTransform affineTransform = new AffineTransform(transform2.position, transform2.rotation);
			AffineTransform identity = AffineTransform.identity;
			AffineTransform affineTransform2 = affineTransform.InverseMul(transform);
			if (data.maintainPositionOffset)
			{
				identity.translation = affineTransform2.translation;
			}
			if (data.maintainRotationOffset)
			{
				identity.rotation = affineTransform2.rotation;
			}
			result.sourceOffsets[i] = identity;
		}
		result.positionAxesMask = new Vector3(Convert.ToSingle(data.constrainedPositionXAxis), Convert.ToSingle(data.constrainedPositionYAxis), Convert.ToSingle(data.constrainedPositionZAxis));
		result.rotationAxesMask = new Vector3(Convert.ToSingle(data.constrainedRotationXAxis), Convert.ToSingle(data.constrainedRotationYAxis), Convert.ToSingle(data.constrainedRotationZAxis));
		return result;
	}

	public override void Destroy(MultiParentConstraintJob job)
	{
		job.sourceTransforms.Dispose();
		job.sourceWeights.Dispose();
		job.sourceOffsets.Dispose();
		job.weightBuffer.Dispose();
	}
}
