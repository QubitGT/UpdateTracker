using System;

namespace g3;

[Flags]
public enum EdgeRefineFlags
{
	NoConstraint = 0,
	NoFlip = 1,
	NoSplit = 2,
	NoCollapse = 4,
	FullyConstrained = 7,
	PreserveTopology = 8
}
