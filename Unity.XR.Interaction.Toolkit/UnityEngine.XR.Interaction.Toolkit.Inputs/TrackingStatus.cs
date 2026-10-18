namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

public struct TrackingStatus
{
	public bool isConnected { get; set; }

	public bool isTracked { get; set; }

	public InputTrackingState trackingState { get; set; }
}
