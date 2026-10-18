using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct MultiPositionConstraintJob : IWeightedAnimationJob, IAnimationJob
{
	private const float k_Epsilon = 1E-05f;

	public ReadWriteTransformHandle driven;

	public ReadOnlyTransformHandle drivenParent;

	public Vector3Property drivenOffset;

	public NativeArray<ReadOnlyTransformHandle> sourceTransforms;

	public NativeArray<PropertyStreamHandle> sourceWeights;

	public NativeArray<Vector3> sourceOffsets;

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
			Vector3 position = driven.GetPosition(stream);
			Vector3 vector = position;
			Vector3 vector2 = position;
			for (int i = 0; i < sourceTransforms.Length; i++)
			{
				float num4 = weightBuffer[i] * num3;
				if (!(num4 < 1E-05f))
				{
					ReadOnlyTransformHandle value = sourceTransforms[i];
					vector2 += (value.GetPosition(stream) + sourceOffsets[i] - position) * num4;
					sourceTransforms[i] = value;
				}
			}
			AffineTransform affineTransform = AffineTransform.identity;
			if (drivenParent.IsValid(stream))
			{
				drivenParent.GetGlobalTR(stream, out var position2, out var rotation);
				affineTransform = new AffineTransform(position2, rotation);
				vector2 = affineTransform.InverseTransform(vector2);
				vector = affineTransform.InverseTransform(vector);
			}
			if (Vector3.Dot(axesMask, axesMask) < 3f)
			{
				vector2 = AnimationRuntimeUtils.Lerp(vector, vector2, axesMask);
			}
			vector2 = affineTransform * (vector2 + drivenOffset.Get(stream));
			driven.SetPosition(stream, Vector3.Lerp(position, vector2, num));
		}
		else
		{
			AnimationRuntimeUtils.PassThrough(stream, driven);
		}
	}
}
