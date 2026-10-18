using System;

namespace Fusion.Protocol;

[Flags]
internal enum JoinRequests : uint
{
	None = 0u,
	NetworkConfig = 2u,
	ReflexiveInfo = 4u,
	DisableNATPunch = 8u
}
