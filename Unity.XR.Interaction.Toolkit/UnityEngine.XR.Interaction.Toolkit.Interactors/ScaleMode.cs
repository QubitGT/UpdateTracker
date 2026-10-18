using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public enum ScaleMode
{
	None = 0,
	ScaleOverTime = 1,
	[Obsolete("Input has been renamed in version 3.0.0. Use ScaleOverTime instead. (UnityUpgradable) -> ScaleOverTime")]
	Input = 1,
	DistanceDelta = 2,
	[Obsolete("Distance has been renamed in version 3.0.0. Use DistanceDelta instead. (UnityUpgradable) -> DistanceDelta")]
	Distance = 2
}
