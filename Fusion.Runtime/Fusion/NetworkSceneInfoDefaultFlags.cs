using System;

namespace Fusion;

[Flags]
public enum NetworkSceneInfoDefaultFlags : uint
{
	SceneCountMask = 0xFu,
	ConterMask = 0xFFFF0u
}
