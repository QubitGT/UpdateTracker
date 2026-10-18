using System.Threading.Tasks;

namespace Modio.Customizations;

public interface IOculusCredentialProvider
{
	Task<(Error, string)> GetOculusUserId();

	Task<string> GetOculusAccessToken();

	Task<string> GetOculusUserProof();

	string GetOculusDevice();
}
