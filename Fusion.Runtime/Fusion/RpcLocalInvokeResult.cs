using System;

namespace Fusion;

public enum RpcLocalInvokeResult
{
	Invoked = 0,
	NotInvokableLocally = 1,
	NotInvokableDuringResim = 2,
	InsufficientSourceAuthority = 3,
	InsufficientTargetAuthority = 4,
	TargetPlayerIsNotLocal = 5,
	PayloadSizeExceeded = 6,
	[Obsolete("Use TargetPlayerIsNotLocal instead")]
	TagetPlayerIsNotLocal = 5
}
