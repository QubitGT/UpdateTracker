using System;

namespace Fusion;

[Flags]
internal enum NetworkLoadSceneParametersFlags : byte
{
	Single = 1,
	LocalPhysics2D = 2,
	LocalPhysics3D = 4,
	ActiveOnLoad = 8
}
