using System;

namespace Fusion;

[Flags]
internal enum RuntimeFlagsBuildFlags
{
	NONE = 0,
	UNITY_WEBGL = 2,
	UNITY_XBOXONE = 4,
	UNITY_GAMECORE = 8,
	UNITY_EDITOR = 0x10,
	UNITY_SWITCH = 0x20,
	UNITY_2019_4_OR_NEWER = 0x40
}
