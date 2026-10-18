using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Liv.Lck.Core.Cosmetics;

public interface ILckCosmeticsCoordinator
{
	event Action<LckAvailableCosmeticInfo> OnCosmeticAvailable;

	Task InitializeLocalCosmeticsAsync();

	Task<Result<bool>> GetUserCosmeticsForSessionAsync(IEnumerable<string> playerIds, string sessionId);

	Task<Result<bool>> AnnouncePlayerPresenceForSessionAsync(string playerId, string sessionId);
}
