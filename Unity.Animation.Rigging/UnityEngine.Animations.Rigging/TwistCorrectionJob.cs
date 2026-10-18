using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct TwistCorrectionJob : IWeightedAnimationJob, IAnimationJob
{
	public ReadOnlyTransformHandle source;

	public Quaternion sourceInverseBindRotation;

	public Vector3 axisMask;

	public NativeArray<ReadWriteTransformHandle> twistTransforms;

	public NativeArray<PropertyStreamHandle> twistWeights;

	public NativeArray<Quaternion> twistBindRotations;

	public NativeArray<float> weightBuffer;

	public FloatProperty jobWeight { get; set; }

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		float num = jobWeight.Get(stream);
		if (num > 0f)
		{
			if (twistTransforms.Length != 0)
			{
				AnimationStreamHandleUtility.ReadFloats(stream, twistWeights, weightBuffer);
				Quaternion quaternion = TwistRotation(axisMask, sourceInverseBindRotation * source.GetLocalRotation(stream));
				Quaternion quaternion2 = Quaternion.Inverse(quaternion);
				for (int i = 0; i < twistTransforms.Length; i++)
				{
					ReadWriteTransformHandle value = twistTransforms[i];
					float f = Mathf.Clamp(weightBuffer[i], -1f, 1f);
					Quaternion b = Quaternion.Lerp(Quaternion.identity, (Mathf.Sign(f) < 0f) ? quaternion2 : quaternion, Mathf.Abs(f));
					value.SetLocalRotation(stream, Quaternion.Lerp(twistBindRotations[i], b, num));
					twistTransforms[i] = value;
				}
			}
		}
		else
		{
			for (int j = 0; j < twistTransforms.Length; j++)
			{
				AnimationRuntimeUtils.PassThrough(stream, twistTransforms[j]);
			}
		}
	}

	private static Quaternion TwistRotation(Vector3 axis, Quaternion rot)
	{
		return new Quaternion(axis.x * rot.x, axis.y * rot.y, axis.z * rot.z, rot.w);
	}
}
