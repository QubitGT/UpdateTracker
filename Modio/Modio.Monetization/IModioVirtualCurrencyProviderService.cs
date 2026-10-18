using System.Threading.Tasks;

namespace Modio.Monetization;

public interface IModioVirtualCurrencyProviderService
{
	Task<(Error error, PortalSku[] skus)> GetCurrencyPackSkus();

	Task<Error> OpenCheckoutFlow(PortalSku sku);
}
