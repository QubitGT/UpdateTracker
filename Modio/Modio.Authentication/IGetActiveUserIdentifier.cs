using System.Threading.Tasks;

namespace Modio.Authentication;

public interface IGetActiveUserIdentifier
{
	Task<string> GetActiveUserIdentifier();
}
