using System.Threading.Tasks;
using Modio.API;

namespace Modio.Authentication;

public interface IModioAuthService
{
	ModioAPI.Portal Portal { get; }

	Task<Error> Authenticate(bool displayedTerms, string thirdPartyEmail = null);
}
