using System;

namespace Fusion;

[Flags]
internal enum NetworkObjectHeaderPlayerDataFlags
{
	InAreaOfInterest = 1,
	ForceInterest = 2,
	AllInterestFlags = 3
}
