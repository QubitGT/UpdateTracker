namespace UnityEngine.Animations.Rigging;

public class OverrideTransformJobBinder<T> : AnimationJobBinder<OverrideTransformJob, T> where T : struct, IAnimationJobData, IOverrideTransformData
{
	public override OverrideTransformJob Create(Animator animator, ref T data, Component component)
	{
		OverrideTransformJob result = default(OverrideTransformJob);
		AnimationJobCacheBuilder animationJobCacheBuilder = new AnimationJobCacheBuilder();
		result.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
		if (data.sourceObject != null)
		{
			result.source = ReadOnlyTransformHandle.Bind(animator, data.sourceObject);
			result.sourceInvLocalBindTx = new AffineTransform(data.sourceObject.localPosition, data.sourceObject.localRotation).Inverse();
			AffineTransform affineTransform = new AffineTransform(data.sourceObject.position, data.sourceObject.rotation);
			AffineTransform transform = new AffineTransform(data.constrainedObject.position, data.constrainedObject.rotation);
			result.sourceToWorldRot = affineTransform.Inverse().rotation;
			result.sourceToPivotRot = affineTransform.InverseMul(transform).rotation;
			Transform parent = data.constrainedObject.parent;
			if (parent != null)
			{
				AffineTransform transform2 = new AffineTransform(parent.position, parent.rotation);
				result.sourceToLocalRot = affineTransform.InverseMul(transform2).rotation;
			}
			else
			{
				result.sourceToLocalRot = result.sourceToPivotRot;
			}
		}
		result.spaceIdx = animationJobCacheBuilder.Add(data.space);
		if (data.space == 2)
		{
			result.sourceToCurrSpaceRotIdx = animationJobCacheBuilder.Add(result.sourceToPivotRot);
		}
		else if (data.space == 1)
		{
			result.sourceToCurrSpaceRotIdx = animationJobCacheBuilder.Add(result.sourceToLocalRot);
		}
		else
		{
			result.sourceToCurrSpaceRotIdx = animationJobCacheBuilder.Add(result.sourceToWorldRot);
		}
		result.position = Vector3Property.Bind(animator, component, data.positionVector3Property);
		result.rotation = Vector3Property.Bind(animator, component, data.rotationVector3Property);
		result.positionWeight = FloatProperty.Bind(animator, component, data.positionWeightFloatProperty);
		result.rotationWeight = FloatProperty.Bind(animator, component, data.rotationWeightFloatProperty);
		result.cache = animationJobCacheBuilder.Build();
		return result;
	}

	public override void Destroy(OverrideTransformJob job)
	{
		job.cache.Dispose();
	}

	public override void Update(OverrideTransformJob job, ref T data)
	{
		job.UpdateSpace(data.space);
	}
}
