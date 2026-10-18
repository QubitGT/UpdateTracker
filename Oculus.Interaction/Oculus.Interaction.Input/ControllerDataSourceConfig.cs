namespace Oculus.Interaction.Input;

public class ControllerDataSourceConfig
{
	public Handedness Handedness { get; set; }

	public ITrackingToWorldTransformer TrackingToWorldTransformer { get; set; }
}
