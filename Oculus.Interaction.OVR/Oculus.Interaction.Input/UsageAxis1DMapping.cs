namespace Oculus.Interaction.Input;

internal class UsageAxis1DMapping : IUsage
{
	public ControllerAxis1DUsage Usage { get; }

	public OVRInput.Axis1D Axis1D { get; }

	public UsageAxis1DMapping(ControllerAxis1DUsage usage, OVRInput.Axis1D axis1D)
	{
		Usage = usage;
		Axis1D = axis1D;
	}

	public void Apply(ControllerDataAsset controllerDataAsset, OVRInput.Controller controllerMask)
	{
		float value = OVRInput.Get(Axis1D, controllerMask);
		controllerDataAsset.Input.SetAxis1D(Usage, value);
	}
}
