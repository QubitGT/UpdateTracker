namespace Oculus.Interaction.Input;

internal class UsageTouchMapping : IUsage
{
	public ControllerButtonUsage Usage { get; }

	public OVRInput.Touch Touch { get; }

	public UsageTouchMapping(ControllerButtonUsage usage, OVRInput.Touch touch)
	{
		Usage = usage;
		Touch = touch;
	}

	public void Apply(ControllerDataAsset controllerDataAsset, OVRInput.Controller controllerMask)
	{
		bool value = OVRInput.Get(Touch, controllerMask);
		controllerDataAsset.Input.SetButton(Usage, value);
	}
}
