using System.Threading.Tasks;

namespace Modio.Monetization;

public interface IModioEntitlementService
{
	Task<Error> SyncEntitlements();
}
