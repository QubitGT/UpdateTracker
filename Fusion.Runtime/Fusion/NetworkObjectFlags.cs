using System;

namespace Fusion;

[Flags]
public enum NetworkObjectFlags
{
	None = 0,
	MaskVersion = 0xFF,
	V1 = 1,
	Ignore = 0x10000,
	MasterClientObject = 0x20000,
	DestroyWhenStateAuthorityLeaves = 0x40000,
	AllowStateAuthorityOverride = 0x80000
}
