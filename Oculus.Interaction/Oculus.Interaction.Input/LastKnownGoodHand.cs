namespace Oculus.Interaction.Input;

public class LastKnownGoodHand : Hand
{
	private readonly HandDataAsset _lastState = new HandDataAsset();

	protected override void Apply(HandDataAsset data)
	{
		bool flag = data.IsHighConfidence || data.RootPoseOrigin == PoseOrigin.FilteredTrackedPose || data.RootPoseOrigin == PoseOrigin.SyntheticPose;
		if (data.IsDataValid && data.IsTracked && flag)
		{
			_lastState.CopyFrom(data);
		}
		else if (_lastState.IsDataValid && data.IsConnected)
		{
			data.CopyPosesFrom(_lastState);
			data.RootPoseOrigin = PoseOrigin.SyntheticPose;
			data.IsDataValid = true;
			data.IsTracked = true;
			data.IsHighConfidence = true;
		}
		else
		{
			data.IsTracked = false;
			data.IsHighConfidence = false;
			data.RootPoseOrigin = PoseOrigin.None;
		}
	}
}
