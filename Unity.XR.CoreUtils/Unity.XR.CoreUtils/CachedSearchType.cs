using System;

namespace Unity.XR.CoreUtils;

[Flags]
public enum CachedSearchType
{
	Children = 1,
	Self = 2,
	Parents = 4
}
