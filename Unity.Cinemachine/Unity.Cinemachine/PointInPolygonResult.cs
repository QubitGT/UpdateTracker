using System;

namespace Unity.Cinemachine;

[Flags]
internal enum PointInPolygonResult
{
	IsOn = 0,
	IsInside = 1,
	IsOutside = 2
}
