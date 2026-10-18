namespace UnityEngine.Animations.Rigging;

public class DampedTransformJobBinder<T> : AnimationJobBinder<DampedTransformJob, T> where T : struct, IAnimationJobData, IDampedTransformData
{
	public override DampedTransformJob Create(Animator animator, ref T data, Component component)
	{
		DampedTransformJob result = default(DampedTransformJob);
		result.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
		result.source = ReadOnlyTransformHandle.Bind(animator, data.sourceObject);
		AffineTransform affineTransform = new AffineTransform(data.constrainedObject.position, data.constrainedObject.rotation);
		AffineTransform affineTransform2 = new AffineTransform(data.sourceObject.position, data.sourceObject.rotation);
		result.localBindTx = affineTransform2.InverseMul(affineTransform);
		result.prevDrivenTx = affineTransform;
		result.dampPosition = FloatProperty.Bind(animator, component, data.dampPositionFloatProperty);
		result.dampRotation = FloatProperty.Bind(animator, component, data.dampRotationFloatProperty);
		if (data.maintainAim && AnimationRuntimeUtils.SqrDistance(data.constrainedObject.position, data.sourceObject.position) > 0f)
		{
			result.aimBindAxis = Quaternion.Inverse(data.constrainedObject.rotation) * (affineTransform2.translation - affineTransform.translation).normalized;
		}
		else
		{
			result.aimBindAxis = Vector3.zero;
		}
		return result;
	}

	public override void Destroy(DampedTransformJob job)
	{
	}
}
