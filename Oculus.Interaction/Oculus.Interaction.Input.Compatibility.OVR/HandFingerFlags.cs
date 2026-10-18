using System;

namespace Oculus.Interaction.Input.Compatibility.OVR;

[Flags]
public enum HandFingerFlags
{
	None = 0,
	Thumb = 1,
	Index = 2,
	Middle = 4,
	Ring = 8,
	Pinky = 0x10,
	All = 0x1F
}
