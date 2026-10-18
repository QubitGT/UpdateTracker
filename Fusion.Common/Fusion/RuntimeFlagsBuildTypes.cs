using System;

namespace Fusion;

[Flags]
internal enum RuntimeFlagsBuildTypes
{
	NONE = 0,
	ENABLE_MONO = 2,
	ENABLE_IL2CPP = 4
}
