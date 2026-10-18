using System;

namespace UnityEngine.Rendering;

public interface IPostProcessComponent
{
	bool IsActive();

	[Obsolete("Unused #from(2023.1)", false)]
	bool IsTileCompatible()
	{
		return false;
	}
}
