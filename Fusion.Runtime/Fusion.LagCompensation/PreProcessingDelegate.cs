using System.Collections.Generic;

namespace Fusion.LagCompensation;

public delegate void PreProcessingDelegate(Query query, HashSet<HitboxRoot> rootCandidates, HashSet<int> processedColliderIndices);
