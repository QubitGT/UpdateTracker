using System.Threading.Tasks;

namespace Liv.Lck.Core;

public interface ILckCore
{
	Task<Result<bool>> HasUserConfiguredStreaming();

	Task<Result<bool>> IsUserSubscribed();

	Task<Result<string>> StartLoginAttemptAsync();

	Task<Result<bool>> CheckLoginCompletedAsync();

	Task<Result<float>> GetRemainingBackoffTimeSeconds();
}
