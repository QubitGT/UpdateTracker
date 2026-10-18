using System;

namespace Fusion;

[Flags]
public enum RpcTargets
{
	StateAuthority = 1,
	InputAuthority = 2,
	Proxies = 4,
	All = 7
}
