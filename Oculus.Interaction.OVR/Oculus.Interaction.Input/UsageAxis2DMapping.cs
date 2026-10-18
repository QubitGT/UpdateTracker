using UnityEngine;

namespace Oculus.Interaction.Input;

internal class UsageAxis2DMapping : IUsage
{
	public ControllerAxis2DUsage Usage { get; }

	public OVRInput.Axis2D Axis2D { get; }

	public UsageAxis2DMapping(ControllerAxis2DUsage usage, OVRInput.Axis2D axis2D)
	{
		Usage = usage;
		Axis2D = axis2D;
	}

	public void Apply(ControllerDataAsset controllerDataAsset, OVRInput.Controller controllerMask)
	{
		Vector2 value = OVRInput.Get(Axis2D, controllerMask);
		controllerDataAsset.Input.SetAxis2D(Usage, value);
	}
}
