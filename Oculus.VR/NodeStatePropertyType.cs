using System;

public enum NodeStatePropertyType
{
	[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
	Acceleration,
	[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
	AngularAcceleration,
	Velocity,
	AngularVelocity,
	Position,
	Orientation
}
