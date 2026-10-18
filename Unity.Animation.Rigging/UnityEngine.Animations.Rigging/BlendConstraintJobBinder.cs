namespace UnityEngine.Animations.Rigging;

public class BlendConstraintJobBinder<T> : AnimationJobBinder<BlendConstraintJob, T> where T : struct, IAnimationJobData, IBlendConstraintData
{
	public override BlendConstraintJob Create(Animator animator, ref T data, Component component)
	{
		BlendConstraintJob result = default(BlendConstraintJob);
		result.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
		result.sourceA = ReadOnlyTransformHandle.Bind(animator, data.sourceObjectA);
		result.sourceB = ReadOnlyTransformHandle.Bind(animator, data.sourceObjectB);
		result.sourceAOffset = (result.sourceBOffset = AffineTransform.identity);
		if (data.maintainPositionOffsets)
		{
			Vector3 position = data.constrainedObject.position;
			result.sourceAOffset.translation = position - data.sourceObjectA.position;
			result.sourceBOffset.translation = position - data.sourceObjectB.position;
		}
		if (data.maintainRotationOffsets)
		{
			Quaternion rotation = data.constrainedObject.rotation;
			result.sourceAOffset.rotation = Quaternion.Inverse(data.sourceObjectA.rotation) * rotation;
			result.sourceBOffset.rotation = Quaternion.Inverse(data.sourceObjectB.rotation) * rotation;
		}
		result.blendPosition = BoolProperty.Bind(animator, component, data.blendPositionBoolProperty);
		result.blendRotation = BoolProperty.Bind(animator, component, data.blendRotationBoolProperty);
		result.positionWeight = FloatProperty.Bind(animator, component, data.positionWeightFloatProperty);
		result.rotationWeight = FloatProperty.Bind(animator, component, data.rotationWeightFloatProperty);
		return result;
	}

	public override void Destroy(BlendConstraintJob job)
	{
	}
}
