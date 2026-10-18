using System;

namespace UnityEngine.XR.OpenXR.NativeTypes;

[Flags]
public enum XrSpaceLocationFlags
{
	None = 0,
	OrientationValid = 1,
	PositionValid = 2,
	OrientationTracked = 4,
	PositionTracked = 8
}
