using System;
using UnityEngine;

namespace Oculus.Interaction.Throw;

[Obsolete]
public struct TransformSample
{
	public readonly Vector3 Position;

	public readonly Quaternion Rotation;

	public readonly float SampleTime;

	public readonly int FrameIndex;

	public TransformSample(Vector3 position, Quaternion rotation, float time, int frameIndex)
	{
		Position = position;
		Rotation = rotation;
		SampleTime = time;
		FrameIndex = frameIndex;
	}

	public static TransformSample Interpolate(TransformSample start, TransformSample fin, float time)
	{
		float t = Mathf.Clamp01(Mathf.InverseLerp(start.SampleTime, fin.SampleTime, time));
		return new TransformSample(Vector3.Lerp(start.Position, fin.Position, t), Quaternion.Slerp(start.Rotation, fin.Rotation, t), time, (int)Mathf.Lerp(start.FrameIndex, fin.FrameIndex, t));
	}
}
