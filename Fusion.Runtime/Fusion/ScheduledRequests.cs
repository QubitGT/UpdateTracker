using System;

namespace Fusion;

[Flags]
internal enum ScheduledRequests : uint
{
	None = 0u,
	ReflexiveInfo = 2u
}
