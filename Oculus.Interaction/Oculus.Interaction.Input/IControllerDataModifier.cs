using System;

namespace Oculus.Interaction.Input;

[Obsolete]
public interface IControllerDataModifier
{
	void Apply(ControllerDataAsset controllerDataAsset, Handedness handedness);
}
