using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace Liv.Lck.Core;

[Preserve]
public class LckCoreWrapper : ILckCore
{
	[Preserve]
	public LckCoreWrapper()
	{
	}

	Task<Result<bool>> ILckCore.CheckLoginCompletedAsync()
	{
		return LckCore.CheckLoginCompletedAsync();
	}

	Task<Result<bool>> ILckCore.HasUserConfiguredStreaming()
	{
		return LckCore.HasUserConfiguredStreaming();
	}

	Task<Result<string>> ILckCore.StartLoginAttemptAsync()
	{
		return LckCore.StartLoginAttemptAsync();
	}

	Task<Result<bool>> ILckCore.IsUserSubscribed()
	{
		return LckCore.IsUserSubscribed();
	}

	Task<Result<float>> ILckCore.GetRemainingBackoffTimeSeconds()
	{
		return LckCore.GetRemainingBackoffTimeSeconds();
	}
}
