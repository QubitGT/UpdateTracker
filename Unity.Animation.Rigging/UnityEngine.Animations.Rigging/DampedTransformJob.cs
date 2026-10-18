using Unity.Burst;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct DampedTransformJob : IWeightedAnimationJob, IAnimationJob
{
	private const float k_FixedDt = 0.01667f;

	private const float k_DampFactor = 40f;

	public ReadWriteTransformHandle driven;

	public ReadOnlyTransformHandle source;

	public AffineTransform localBindTx;

	public Vector3 aimBindAxis;

	public AffineTransform prevDrivenTx;

	public FloatProperty dampPosition;

	public FloatProperty dampRotation;

	public FloatProperty jobWeight { get; set; }

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		float num = jobWeight.Get(stream);
		float num2 = Mathf.Abs(stream.deltaTime);
		driven.GetGlobalTR(stream, out var position, out var rotation);
		if (num > 0f && num2 > 0f)
		{
			source.GetGlobalTR(stream, out var position2, out var rotation2);
			AffineTransform affineTransform = new AffineTransform(position2, rotation2);
			AffineTransform affineTransform2 = affineTransform * localBindTx;
			affineTransform2.translation = Vector3.Lerp(position, affineTransform2.translation, num);
			affineTransform2.rotation = Quaternion.Lerp(rotation, affineTransform2.rotation, num);
			float num3 = AnimationRuntimeUtils.Square(1f - dampPosition.Get(stream));
			float num4 = AnimationRuntimeUtils.Square(1f - dampRotation.Get(stream));
			bool flag = Vector3.Dot(aimBindAxis, aimBindAxis) > 0f;
			while (num2 > 0f)
			{
				float num5 = 40f * Mathf.Min(0.01667f, num2);
				prevDrivenTx.translation += (affineTransform2.translation - prevDrivenTx.translation) * num3 * num5;
				prevDrivenTx.rotation *= Quaternion.Lerp(Quaternion.identity, Quaternion.Inverse(prevDrivenTx.rotation) * affineTransform2.rotation, num4 * num5);
				if (flag)
				{
					Vector3 vector = prevDrivenTx.rotation * aimBindAxis;
					Vector3 vector2 = affineTransform.translation - prevDrivenTx.translation;
					prevDrivenTx.rotation = Quaternion.AngleAxis(Vector3.Angle(vector, vector2), Vector3.Cross(vector, vector2).normalized) * prevDrivenTx.rotation;
				}
				num2 -= 0.01667f;
			}
			driven.SetGlobalTR(stream, prevDrivenTx.translation, prevDrivenTx.rotation);
		}
		else
		{
			prevDrivenTx.Set(position, rotation);
			AnimationRuntimeUtils.PassThrough(stream, driven);
		}
	}
}
