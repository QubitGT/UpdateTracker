using System;
using System.Collections.Generic;

namespace Liv.Lck.Core.Cosmetics;

[Serializable]
public struct LckCosmeticInfo
{
	public string CosmeticId;

	public string CosmeticFilepath;

	public Dictionary<string, object> CosmeticMetadata;
}
