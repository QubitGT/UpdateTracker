namespace UnityEngine.Animations.Rigging;

public class TwoBoneIKConstraintJobBinder<T> : AnimationJobBinder<TwoBoneIKConstraintJob, T> where T : struct, IAnimationJobData, ITwoBoneIKConstraintData
{
	public override TwoBoneIKConstraintJob Create(Animator animator, ref T data, Component component)
	{
		TwoBoneIKConstraintJob result = default(TwoBoneIKConstraintJob);
		result.root = ReadWriteTransformHandle.Bind(animator, data.root);
		result.mid = ReadWriteTransformHandle.Bind(animator, data.mid);
		result.tip = ReadWriteTransformHandle.Bind(animator, data.tip);
		result.target = ReadOnlyTransformHandle.Bind(animator, data.target);
		if (data.hint != null)
		{
			result.hint = ReadOnlyTransformHandle.Bind(animator, data.hint);
		}
		result.targetOffset = AffineTransform.identity;
		if (data.maintainTargetPositionOffset)
		{
			result.targetOffset.translation = data.tip.position - data.target.position;
		}
		if (data.maintainTargetRotationOffset)
		{
			result.targetOffset.rotation = Quaternion.Inverse(data.target.rotation) * data.tip.rotation;
		}
		result.targetPositionWeight = FloatProperty.Bind(animator, component, data.targetPositionWeightFloatProperty);
		result.targetRotationWeight = FloatProperty.Bind(animator, component, data.targetRotationWeightFloatProperty);
		result.hintWeight = FloatProperty.Bind(animator, component, data.hintWeightFloatProperty);
		return result;
	}

	public override void Destroy(TwoBoneIKConstraintJob job)
	{
	}
}
