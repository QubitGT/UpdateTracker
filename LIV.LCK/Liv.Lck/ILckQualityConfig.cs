using System.Collections.Generic;

namespace Liv.Lck;

public interface ILckQualityConfig
{
	List<QualityOption> GetQualityOptionsForSystem();
}
