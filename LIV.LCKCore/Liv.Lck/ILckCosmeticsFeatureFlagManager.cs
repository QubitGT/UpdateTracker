using System.Threading.Tasks;

namespace Liv.Lck;

public interface ILckCosmeticsFeatureFlagManager
{
	Task<bool> IsEnabledAsync();
}
