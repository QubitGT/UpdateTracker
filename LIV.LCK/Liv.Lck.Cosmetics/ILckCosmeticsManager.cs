using System;

namespace Liv.Lck.Cosmetics;

public interface ILckCosmeticsManager : IDisposable
{
	void RegisterDependant(ILckCosmeticDependant dependant);

	void UnregisterDependant(ILckCosmeticDependant dependant);
}
