using System;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[Flags]
public enum TargetedDevices
{
	None = 0,
	FPS = 1,
	LeftDevice = 2,
	RightDevice = 4,
	HMD = 8
}
