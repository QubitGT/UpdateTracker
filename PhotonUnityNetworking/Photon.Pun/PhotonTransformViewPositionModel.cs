using System;

namespace Photon.Pun;

[Serializable]
public class PhotonTransformViewPositionModel
{
	public enum InterpolateOptions
	{
		Disabled,
		FixedSpeed,
		EstimatedSpeed,
		SynchronizeValues,
		Lerp
	}

	public enum ExtrapolateOptions
	{
		Disabled,
		SynchronizeValues,
		EstimateSpeedAndTurn,
		FixedSpeed
	}

	public bool SynchronizeEnabled;

	public bool TeleportEnabled = true;

	public float TeleportIfDistanceGreaterThan = 3f;

	public InterpolateOptions InterpolateOption = InterpolateOptions.EstimatedSpeed;

	public float InterpolateMoveTowardsSpeed = 1f;

	public float InterpolateLerpSpeed = 1f;

	public ExtrapolateOptions ExtrapolateOption;

	public float ExtrapolateSpeed = 1f;

	public bool ExtrapolateIncludingRoundTripTime = true;

	public int ExtrapolateNumberOfStoredPositions = 1;
}
