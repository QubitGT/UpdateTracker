using System.Collections.Generic;

namespace g3;

public interface IBinaryVoxelGrid
{
	AxisAlignedBox3i GridBounds { get; }

	bool Get(Vector3i i);

	IEnumerable<Vector3i> NonZeros();
}
