using System;

namespace Fusion.Protocol;

[Flags]
internal enum StartRequests : uint
{
	None = 0u,
	ConnectToShared = 2u,
	WaitForReflexiveInfo = 4u
}
