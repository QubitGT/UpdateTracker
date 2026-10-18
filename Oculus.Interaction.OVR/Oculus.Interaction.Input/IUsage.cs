namespace Oculus.Interaction.Input;

internal interface IUsage
{
	void Apply(ControllerDataAsset controllerDataAsset, OVRInput.Controller controllerMask);
}
