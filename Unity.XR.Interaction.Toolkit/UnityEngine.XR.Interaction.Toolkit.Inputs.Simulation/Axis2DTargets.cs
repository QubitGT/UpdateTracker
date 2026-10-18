using System;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[Flags]
public enum Axis2DTargets
{
	None = 0,
	Position = 1,
	Primary2DAxis = 2,
	Secondary2DAxis = 4
}
