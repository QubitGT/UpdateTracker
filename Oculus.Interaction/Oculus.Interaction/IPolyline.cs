using UnityEngine;

namespace Oculus.Interaction;

public interface IPolyline
{
	int PointsCount { get; }

	Vector3 PointAtIndex(int index);
}
