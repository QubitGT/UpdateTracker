using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct MultiReferentialConstraintJob : IWeightedAnimationJob, IAnimationJob
{
	public IntProperty driver;

	public NativeArray<ReadWriteTransformHandle> sources;

	public NativeArray<AffineTransform> sourceBindTx;

	public NativeArray<AffineTransform> offsetTx;

	private int m_PrevDriverIdx;

	public FloatProperty jobWeight { get; set; }

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		float num = jobWeight.Get(stream);
		if (num > 0f)
		{
			int num2 = driver.Get(stream);
			if (num2 != m_PrevDriverIdx)
			{
				UpdateOffsets(num2);
			}
			sources[num2].GetGlobalTR(stream, out var position, out var rotation);
			AffineTransform affineTransform = new AffineTransform(position, rotation);
			int num3 = 0;
			for (int i = 0; i < sources.Length; i++)
			{
				if (i != num2)
				{
					AffineTransform affineTransform2 = affineTransform * offsetTx[num3];
					ReadWriteTransformHandle value = sources[i];
					value.GetGlobalTR(stream, out var position2, out var rotation2);
					value.SetGlobalTR(stream, Vector3.Lerp(position2, affineTransform2.translation, num), Quaternion.Lerp(rotation2, affineTransform2.rotation, num));
					num3++;
					sources[i] = value;
				}
			}
			AnimationRuntimeUtils.PassThrough(stream, sources[num2]);
		}
		else
		{
			for (int j = 0; j < sources.Length; j++)
			{
				AnimationRuntimeUtils.PassThrough(stream, sources[j]);
			}
		}
	}

	internal void UpdateOffsets(int driver)
	{
		driver = Mathf.Clamp(driver, 0, sources.Length - 1);
		int num = 0;
		AffineTransform affineTransform = sourceBindTx[driver].Inverse();
		for (int i = 0; i < sourceBindTx.Length; i++)
		{
			if (i != driver)
			{
				offsetTx[num] = affineTransform * sourceBindTx[i];
				num++;
			}
		}
		m_PrevDriverIdx = driver;
	}
}
