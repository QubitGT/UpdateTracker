using System.Collections.Generic;

namespace Fusion.LagCompensation;

internal interface ILagCompensationBroadphase
{
	void CopyFrom(ILagCompensationBroadphase other);

	void Traverse(IBoundsTraversalTest hitTest, HashSet<HitboxRoot> candidateRoots, int layerMask);

	void Add(HitboxRoot root);

	bool Remove(HitboxRoot root);

	void Update(HitboxRoot changed, int tick);
}
