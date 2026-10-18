namespace Oculus.Interaction.Input;

public class HandDataSourceConfig
{
	public Handedness Handedness { get; set; }

	public ITrackingToWorldTransformer TrackingToWorldTransformer { get; set; }

	public HandSkeleton HandSkeleton { get; set; }
}
