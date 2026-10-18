using Unity.Burst;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct BlendConstraintJob : IWeightedAnimationJob, IAnimationJob
{
	private const int k_BlendTranslationMask = 1;

	private const int k_BlendRotationMask = 2;

	public ReadWriteTransformHandle driven;

	public ReadOnlyTransformHandle sourceA;

	public ReadOnlyTransformHandle sourceB;

	public AffineTransform sourceAOffset;

	public AffineTransform sourceBOffset;

	public BoolProperty blendPosition;

	public BoolProperty blendRotation;

	public FloatProperty positionWeight;

	public FloatProperty rotationWeight;

	public FloatProperty jobWeight { get; set; }

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		float num = jobWeight.Get(stream);
		if (num > 0f)
		{
			if (blendPosition.Get(stream))
			{
				Vector3 b = Vector3.Lerp(sourceA.GetPosition(stream) + sourceAOffset.translation, sourceB.GetPosition(stream) + sourceBOffset.translation, positionWeight.Get(stream));
				driven.SetPosition(stream, Vector3.Lerp(driven.GetPosition(stream), b, num));
			}
			else
			{
				driven.SetLocalPosition(stream, driven.GetLocalPosition(stream));
			}
			if (blendRotation.Get(stream))
			{
				Quaternion b2 = Quaternion.Lerp(sourceA.GetRotation(stream) * sourceAOffset.rotation, sourceB.GetRotation(stream) * sourceBOffset.rotation, rotationWeight.Get(stream));
				driven.SetRotation(stream, Quaternion.Lerp(driven.GetRotation(stream), b2, num));
			}
			else
			{
				driven.SetLocalRotation(stream, driven.GetLocalRotation(stream));
			}
		}
		else
		{
			AnimationRuntimeUtils.PassThrough(stream, driven);
		}
	}
}
