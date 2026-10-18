using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct MultiRotationConstraintJob : IWeightedAnimationJob, IAnimationJob
{
	private const float k_Epsilon = 1E-05f;

	public ReadWriteTransformHandle driven;

	public ReadOnlyTransformHandle drivenParent;

	public Vector3Property drivenOffset;

	public NativeArray<ReadOnlyTransformHandle> sourceTransforms;

	public NativeArray<PropertyStreamHandle> sourceWeights;

	public NativeArray<Quaternion> sourceOffsets;

	public NativeArray<float> weightBuffer;

	public Vector3 axesMask;

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
			Quaternion quaternion = QuaternionExt.zero;
			for (int i = 0; i < sourceTransforms.Length; i++)
			{
				float num5 = weightBuffer[i] * num3;
				if (!(num5 < 1E-05f))
				{
					ReadOnlyTransformHandle value = sourceTransforms[i];
					quaternion = QuaternionExt.Add(quaternion, QuaternionExt.Scale(value.GetRotation(stream) * sourceOffsets[i], num5));
					sourceTransforms[i] = value;
					num4 += num5;
				}
			}
			quaternion = QuaternionExt.NormalizeSafe(quaternion);
			if (num4 < 1f)
			{
				quaternion = Quaternion.Lerp(driven.GetRotation(stream), quaternion, num4);
			}
			if (drivenParent.IsValid(stream))
			{
				quaternion = Quaternion.Inverse(drivenParent.GetRotation(stream)) * quaternion;
			}
			Quaternion localRotation = driven.GetLocalRotation(stream);
			if (Vector3.Dot(axesMask, axesMask) < 3f)
			{
				quaternion = Quaternion.Euler(AnimationRuntimeUtils.Lerp(localRotation.eulerAngles, quaternion.eulerAngles, axesMask));
			}
			Vector3 vector = drivenOffset.Get(stream);
			if (Vector3.Dot(vector, vector) > 0f)
			{
				quaternion *= Quaternion.Euler(vector);
			}
			driven.SetLocalRotation(stream, Quaternion.Lerp(localRotation, quaternion, num));
		}
		else
		{
			AnimationRuntimeUtils.PassThrough(stream, driven);
		}
	}
}
