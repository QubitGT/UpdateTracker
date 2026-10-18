using System;

namespace Fusion;

[Flags]
public enum RpcSources
{
	StateAuthority = 1,
	InputAuthority = 2,
	Proxies = 4,
	All = 7
}
