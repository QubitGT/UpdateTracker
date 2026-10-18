using System;

namespace Fusion;

[Flags]
internal enum RuntimeFlagsDotNetVersion
{
	NONE = 0,
	NET_4_6 = 2,
	NETFX_CORE = 4,
	NET_STANDARD_2_0 = 8
}
