using System;

namespace Fusion;

[Flags]
public enum NetworkSceneInfoChangeSource
{
	None = 0,
	Initial = 1,
	Remote = 2
}
