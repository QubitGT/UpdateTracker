using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct MultiParentConstraintJob : IWeightedAnimationJob, IAnimationJob
{
	private const float k_Epsilon = 1E-05f;

	public ReadWriteTransformHandle driven;

	public ReadOnlyTransformHandle drivenParent;

	public NativeArray<ReadOnlyTransformHandle> sourceTransforms;

	public NativeArray<PropertyStreamHandle> sourceWeights;

	public NativeArray<AffineTransform> sourceOffsets;

	public NativeArray<float> weightBuffer;

	public Vector3 positionAxesMask;

	public Vector3 rotationAxesMask;

	public FloatProperty jobWeight { get; set; }

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		float num = jobWeight.Get(stream);
		if (num > 0f)
		{
			AnimationStreamHandleUtility.ReadFloats(stream, sourceWeights, weightBuffer);
			float num2 = AnimationRuntimeUtils.Sum(weightBuffer);
			if (num2 < 1E-05f)
			{
				AnimationRuntimeUtils.PassThrough(stream, driven);
				return;
			}
			float num3 = ((num2 > 1f) ? (1f / num2) : 1f);
			float num4 = 0f;
			AffineTransform affineTransform = new AffineTransform(Vector3.zero, QuaternionExt.zero);
			for (int i = 0; i < sourceTransforms.Length; i++)
			{
				ReadOnlyTransformHandle value = sourceTransforms[i];
				float num5 = weightBuffer[i] * num3;
				if (!(num5 < 1E-05f))
				{
					value.GetGlobalTR(stream, out var position, out var rotation);
					AffineTransform affineTransform2 = new AffineTransform(position, rotation);
					affineTransform2 *= sourceOffsets[i];
					affineTransform.translation += affineTransform2.translation * num5;
					affineTransform.rotation = QuaternionExt.Add(affineTransform.rotation, QuaternionExt.Scale(affineTransform2.rotation, num5));
					sourceTransforms[i] = value;
					num4 += num5;
				}
			}
			driven.GetGlobalTR(stream, out var position2, out var rotation2);
			AffineTransform transform = new AffineTransform(position2, rotation2);
			affineTransform.rotation = QuaternionExt.NormalizeSafe(affineTransform.rotation);
			if (num4 < 1f)
			{
				affineTransform.translation += position2 * (1f - num4);
				affineTransform.rotation = Quaternion.Lerp(rotation2, affineTransform.rotation, num4);
			}
			AffineTransform affineTransform3 = AffineTransform.identity;
			if (drivenParent.IsValid(stream))
			{
				drivenParent.GetGlobalTR(stream, out var position3, out var rotation3);
				affineTransform3 = new AffineTransform(position3, rotation3);
				affineTransform = affineTransform3.InverseMul(affineTransform);
				transform = affineTransform3.InverseMul(transform);
			}
			if (Vector3.Dot(positionAxesMask, positionAxesMask) < 3f)
			{
				affineTransform.translation = AnimationRuntimeUtils.Lerp(transform.translation, affineTransform.translation, positionAxesMask);
			}
			if (Vector3.Dot(rotationAxesMask, rotationAxesMask) < 3f)
			{
				affineTransform.rotation = Quaternion.Euler(AnimationRuntimeUtils.Lerp(transform.rotation.eulerAngles, affineTransform.rotation.eulerAngles, rotationAxesMask));
			}
			affineTransform = affineTransform3 * affineTransform;
			driven.SetGlobalTR(stream, Vector3.Lerp(position2, affineTransform.translation, num), Quaternion.Lerp(rotation2, affineTransform.rotation, num));
		}
		else
		{
			AnimationRuntimeUtils.PassThrough(stream, driven);
		}
	}
}
