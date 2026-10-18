using System;
using System.Collections.Generic;

namespace Liv.Lck;

[Serializable]
public struct QualityOptionOverride
{
	public DeviceModel DeviceModel;

	public List<QualityOption> QualityOptions;
}
