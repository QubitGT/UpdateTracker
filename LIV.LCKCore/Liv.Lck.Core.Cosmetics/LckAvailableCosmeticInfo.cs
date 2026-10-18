using System;

namespace Liv.Lck.Core.Cosmetics;

[Serializable]
public struct LckAvailableCosmeticInfo
{
	public LckCosmeticInfo CosmeticInfo;

	public string[] PlayerIds;
}
