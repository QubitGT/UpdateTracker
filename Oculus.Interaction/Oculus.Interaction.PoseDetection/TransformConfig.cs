using System;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[Serializable]
public class TransformConfig
{
	public Vector3 PositionOffset;

	public Vector3 RotationOffset;

	public UpVectorType UpVectorType;

	public TransformFeatureStateThresholds FeatureThresholds;

	public int InstanceId { get; set; }

	public TransformConfig()
	{
		PositionOffset = Vector3.zero;
		RotationOffset = Vector3.zero;
		UpVectorType = UpVectorType.Head;
		FeatureThresholds = null;
		InstanceId = 0;
	}
}
