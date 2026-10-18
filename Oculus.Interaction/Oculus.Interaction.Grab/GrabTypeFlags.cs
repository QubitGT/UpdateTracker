using System;

namespace Oculus.Interaction.Grab;

[Flags]
public enum GrabTypeFlags
{
	None = 0,
	Pinch = 1,
	Palm = 2,
	All = 3
}
