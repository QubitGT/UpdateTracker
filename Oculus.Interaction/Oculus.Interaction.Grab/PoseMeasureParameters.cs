using System;
using UnityEngine;

namespace Oculus.Interaction.Grab;

[Serializable]
public struct PoseMeasureParameters
{
	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Weights the scoring of the pose based more in the amount of translationor rotation needed to align the interactor with the desired pose.")]
	private float _positionRotationWeight;

	public static readonly PoseMeasureParameters DEFAULT = new PoseMeasureParameters(0f);

	public float PositionRotationWeight => _positionRotationWeight;

	public PoseMeasureParameters(float positionRotationWeight)
	{
		_positionRotationWeight = positionRotationWeight;
	}

	public static PoseMeasureParameters Lerp(in PoseMeasureParameters from, in PoseMeasureParameters to, float t)
	{
		return new PoseMeasureParameters(Mathf.Lerp(from._positionRotationWeight, to._positionRotationWeight, t));
	}
}
