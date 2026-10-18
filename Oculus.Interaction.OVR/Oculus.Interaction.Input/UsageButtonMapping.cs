namespace Oculus.Interaction.Input;

internal class UsageButtonMapping : IUsage
{
	public ControllerButtonUsage Usage { get; }

	public OVRInput.Button Button { get; }

	public UsageButtonMapping(ControllerButtonUsage usage, OVRInput.Button button)
	{
		Usage = usage;
		Button = button;
	}

	public void Apply(ControllerDataAsset controllerDataAsset, OVRInput.Controller controllerMask)
	{
		bool value = OVRInput.Get(Button, controllerMask);
		controllerDataAsset.Input.SetButton(Usage, value);
	}
}
