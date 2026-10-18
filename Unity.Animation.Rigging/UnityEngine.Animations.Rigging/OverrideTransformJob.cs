using Unity.Burst;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
public struct OverrideTransformJob : IWeightedAnimationJob, IAnimationJob
{
	public enum Space
	{
		World,
		Local,
		Pivot
	}

	public ReadWriteTransformHandle driven;

	public ReadOnlyTransformHandle source;

	public AffineTransform sourceInvLocalBindTx;

	public Quaternion sourceToWorldRot;

	public Quaternion sourceToLocalRot;

	public Quaternion sourceToPivotRot;

	public CacheIndex spaceIdx;

	public CacheIndex sourceToCurrSpaceRotIdx;

	public Vector3Property position;

	public Vector3Property rotation;

	public FloatProperty positionWeight;

	public FloatProperty rotationWeight;

	public AnimationJobCache cache;

	public FloatProperty jobWeight { get; set; }

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		float num = jobWeight.Get(stream);
		if (num > 0f)
		{
			AffineTransform affineTransform2;
			if (source.IsValid(stream))
			{
				source.GetLocalTRS(stream, out var t, out var r, out var _);
				AffineTransform affineTransform = new AffineTransform(t, r);
				Quaternion quaternion = cache.Get<Quaternion>(sourceToCurrSpaceRotIdx);
				affineTransform2 = Quaternion.Inverse(quaternion) * (sourceInvLocalBindTx * affineTransform) * quaternion;
			}
			else
			{
				affineTransform2 = new AffineTransform(position.Get(stream), Quaternion.Euler(rotation.Get(stream)));
			}
			Space space = (Space)cache.GetRaw(spaceIdx);
			float t2 = positionWeight.Get(stream) * num;
			float t3 = rotationWeight.Get(stream) * num;
			switch (space)
			{
			case Space.World:
			{
				driven.GetGlobalTR(stream, out var a3, out var a4);
				driven.SetGlobalTR(stream, Vector3.Lerp(a3, affineTransform2.translation, t2), Quaternion.Lerp(a4, affineTransform2.rotation, t3));
				break;
			}
			case Space.Local:
			{
				driven.GetLocalTRS(stream, out var a, out var a2, out var scale3);
				driven.SetLocalTRS(stream, Vector3.Lerp(a, affineTransform2.translation, t2), Quaternion.Lerp(a2, affineTransform2.rotation, t3), scale3);
				break;
			}
			case Space.Pivot:
			{
				driven.GetLocalTRS(stream, out var t4, out var r2, out var scale2);
				AffineTransform affineTransform3 = new AffineTransform(t4, r2);
				affineTransform2 = affineTransform3 * affineTransform2;
				driven.SetLocalTRS(stream, Vector3.Lerp(affineTransform3.translation, affineTransform2.translation, t2), Quaternion.Lerp(affineTransform3.rotation, affineTransform2.rotation, t3), scale2);
				break;
			}
			}
		}
		else
		{
			AnimationRuntimeUtils.PassThrough(stream, driven);
		}
	}

	internal void UpdateSpace(int space)
	{
		if ((int)cache.GetRaw(spaceIdx) != space)
		{
			cache.SetRaw(space, spaceIdx);
			switch ((Space)space)
			{
			case Space.Pivot:
				cache.Set(sourceToPivotRot, sourceToCurrSpaceRotIdx);
				break;
			case Space.Local:
				cache.Set(sourceToLocalRot, sourceToCurrSpaceRotIdx);
				break;
			default:
				cache.Set(sourceToWorldRot, sourceToCurrSpaceRotIdx);
				break;
			}
		}
	}
}
