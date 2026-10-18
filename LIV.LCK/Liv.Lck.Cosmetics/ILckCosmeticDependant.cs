using System.Collections.Generic;
using UnityEngine;

namespace Liv.Lck.Cosmetics;

public interface ILckCosmeticDependant
{
	string PlayerId { get; }

	string GetCosmeticType();

	void OnCosmeticLoaded(List<Object> assets);
}
